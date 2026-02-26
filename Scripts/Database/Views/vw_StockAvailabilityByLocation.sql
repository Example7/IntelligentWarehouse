/*
Widok dostępności stanów na poziomie lokacji.
Uwzględnia:
- stan fizyczny (Stock)
- aktywne rezerwacje (Reservations = Active)
- pozycje WZ w statusie Draft (GoodsIssues = Draft)
*/

IF OBJECT_ID(N'dbo.vw_StockAvailabilityByLocation', N'V') IS NOT NULL
    DROP VIEW dbo.vw_StockAvailabilityByLocation;
GO

CREATE VIEW dbo.vw_StockAvailabilityByLocation
AS
WITH ActiveReservations AS
(
    SELECT
        ri.ProductId,
        ri.LocationId,
        SUM(ri.Quantity) AS ReservedActiveQty
    FROM dbo.ReservationItems ri
    INNER JOIN dbo.Reservations r ON r.ReservationId = ri.ReservationId
    WHERE r.Status = 'Active'
      AND ri.LocationId IS NOT NULL
    GROUP BY ri.ProductId, ri.LocationId
),
DraftWz AS
(
    SELECT
        gii.ProductId,
        gii.LocationId,
        SUM(gii.Quantity) AS ReservedDraftWzQty
    FROM dbo.GoodsIssueItems gii
    INNER JOIN dbo.GoodsIssues gi ON gi.IssueId = gii.IssueId
    WHERE gi.Status = 'Draft'
      AND gii.LocationId IS NOT NULL
    GROUP BY gii.ProductId, gii.LocationId
)
SELECT
    s.StockId,
    p.ProductId,
    p.SKU,
    p.Name AS ProductName,
    p.CategoryId,
    c.Name AS CategoryName,
    p.DefaultUomId AS UomId,
    uom.Code AS UomCode,
    w.WarehouseId,
    w.Name AS WarehouseName,
    l.LocationId,
    l.Code AS LocationCode,
    CAST(s.Quantity AS decimal(18,3)) AS PhysicalQty,
    CAST(ISNULL(ar.ReservedActiveQty, 0) AS decimal(18,3)) AS ReservedActiveQty,
    CAST(ISNULL(dw.ReservedDraftWzQty, 0) AS decimal(18,3)) AS ReservedDraftWzQty,
    CAST(ISNULL(ar.ReservedActiveQty, 0) + ISNULL(dw.ReservedDraftWzQty, 0) AS decimal(18,3)) AS ReservedTotalQty,
    CAST(s.Quantity - (ISNULL(ar.ReservedActiveQty, 0) + ISNULL(dw.ReservedDraftWzQty, 0)) AS decimal(18,3)) AS AvailableQty
FROM dbo.Stock s
INNER JOIN dbo.Products p ON p.ProductId = s.ProductId
INNER JOIN dbo.Locations l ON l.LocationId = s.LocationId
INNER JOIN dbo.Warehouses w ON w.WarehouseId = l.WarehouseId
LEFT JOIN dbo.Categories c ON c.CategoryId = p.CategoryId
LEFT JOIN dbo.UnitsOfMeasure uom ON uom.UomId = p.DefaultUomId
LEFT JOIN ActiveReservations ar ON ar.ProductId = s.ProductId AND ar.LocationId = s.LocationId
LEFT JOIN DraftWz dw ON dw.ProductId = s.ProductId AND dw.LocationId = s.LocationId;
GO

