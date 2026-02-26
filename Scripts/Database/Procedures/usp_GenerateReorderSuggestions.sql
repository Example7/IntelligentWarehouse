/*
Procedura generująca propozycje zamówień wg parametrów produktu:
- MinStock
- ReorderPoint (ROP)
- ReorderQty (ROQ)

Zwraca tylko pozycje poniżej progu ROP (lub MinStock, jeśli ROP = NULL).
Wykorzystuje:
- widok dbo.vw_StockAvailabilityByLocation
- funkcję dbo.fn_ProductAvailabilityInWarehouse
*/

IF OBJECT_ID(N'dbo.usp_GenerateReorderSuggestions', N'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_GenerateReorderSuggestions;
GO

CREATE PROCEDURE dbo.usp_GenerateReorderSuggestions
    @WarehouseId INT = NULL,
    @SearchTerm NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Term NVARCHAR(200) = NULLIF(LTRIM(RTRIM(@SearchTerm)), N'');

    ;WITH WarehouseScope AS
    (
        SELECT w.WarehouseId, w.Name AS WarehouseName
        FROM dbo.Warehouses w
        WHERE w.IsActive = 1
          AND (@WarehouseId IS NULL OR w.WarehouseId = @WarehouseId)
    ),
    ProductScope AS
    (
        SELECT
            p.ProductId,
            p.SKU,
            p.Name AS ProductName,
            p.MinStock,
            CAST(ISNULL(p.ReorderPoint, p.MinStock) AS decimal(18,3)) AS EffectiveReorderPoint,
            CAST(COALESCE(NULLIF(p.ReorderQty, 0), NULLIF(p.ReorderPoint, 0), NULLIF(p.MinStock, 0), 1) AS decimal(18,3)) AS EffectiveReorderQty,
            c.Name AS CategoryName,
            uom.Code AS UomCode
        FROM dbo.Products p
        LEFT JOIN dbo.Categories c ON c.CategoryId = p.CategoryId
        LEFT JOIN dbo.UnitsOfMeasure uom ON uom.UomId = p.DefaultUomId
        WHERE p.IsActive = 1
          AND (
                @Term IS NULL
                OR p.SKU LIKE N'%' + @Term + N'%'
                OR p.Name LIKE N'%' + @Term + N'%'
                OR c.Name LIKE N'%' + @Term + N'%'
              )
    )
    SELECT
        p.ProductId,
        p.SKU,
        p.ProductName,
        p.CategoryName,
        p.UomCode,
        w.WarehouseId,
        w.WarehouseName,
        CAST(ISNULL(SUM(fa.PhysicalQty), 0) AS decimal(18,3)) AS PhysicalQty,
        CAST(ISNULL(SUM(fa.ReservedActiveQty), 0) AS decimal(18,3)) AS ReservedActiveQty,
        CAST(ISNULL(SUM(fa.ReservedDraftWzQty), 0) AS decimal(18,3)) AS ReservedDraftWzQty,
        CAST(ISNULL(SUM(fa.AvailableQty), 0) AS decimal(18,3)) AS AvailableQty,
        CAST(p.MinStock AS decimal(18,3)) AS MinStock,
        CAST(p.EffectiveReorderPoint AS decimal(18,3)) AS ReorderPoint,
        CAST(p.EffectiveReorderQty AS decimal(18,3)) AS ReorderQty,
        CAST(CASE 
            WHEN ISNULL(SUM(fa.AvailableQty), 0) < p.EffectiveReorderPoint THEN p.EffectiveReorderQty
            ELSE 0
        END AS decimal(18,3)) AS SuggestedOrderQty,
        CAST(CASE
            WHEN ISNULL(SUM(fa.AvailableQty), 0) < p.EffectiveReorderPoint THEN p.EffectiveReorderPoint - ISNULL(SUM(fa.AvailableQty), 0)
            ELSE 0
        END AS decimal(18,3)) AS ShortageToRop
    FROM ProductScope p
    CROSS JOIN WarehouseScope w
    OUTER APPLY dbo.fn_ProductAvailabilityInWarehouse(p.ProductId, w.WarehouseId) fa
    GROUP BY
        p.ProductId, p.SKU, p.ProductName, p.CategoryName, p.UomCode,
        w.WarehouseId, w.WarehouseName,
        p.MinStock, p.EffectiveReorderPoint, p.EffectiveReorderQty
    HAVING ISNULL(SUM(fa.AvailableQty), 0) < p.EffectiveReorderPoint
    ORDER BY
        ShortageToRop DESC,
        p.ProductName,
        p.SKU,
        w.WarehouseName;
END
GO
