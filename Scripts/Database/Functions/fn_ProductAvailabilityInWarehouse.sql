/*
Funkcja tabelaryczna: dostępność produktu w magazynie na poziomie lokacji.
Zwraca dane z widoku vw_StockAvailabilityByLocation.
*/

IF OBJECT_ID(N'dbo.fn_ProductAvailabilityInWarehouse', N'IF') IS NOT NULL
    DROP FUNCTION dbo.fn_ProductAvailabilityInWarehouse;
GO

CREATE FUNCTION dbo.fn_ProductAvailabilityInWarehouse
(
    @ProductId INT,
    @WarehouseId INT
)
RETURNS TABLE
AS
RETURN
(
    SELECT
        v.ProductId,
        v.SKU,
        v.ProductName,
        v.WarehouseId,
        v.WarehouseName,
        v.LocationId,
        v.LocationCode,
        v.UomId,
        v.UomCode,
        v.PhysicalQty,
        v.ReservedActiveQty,
        v.ReservedDraftWzQty,
        v.ReservedTotalQty,
        v.AvailableQty
    FROM dbo.vw_StockAvailabilityByLocation v
    WHERE v.ProductId = @ProductId
      AND v.WarehouseId = @WarehouseId
);
GO

