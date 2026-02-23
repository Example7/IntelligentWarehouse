/*
    Demo seed for IntelligentWarehouse (SQL Server / T-SQL)
    - idempotent inserts (IF NOT EXISTS)
    - focuses on warehouse/domain tables used by CRUD views
    - does not delete existing data
*/

SET NOCOUNT ON;

BEGIN TRY
    BEGIN TRAN;

    DECLARE @NowUtc datetime2 = SYSUTCDATETIME();

    /* =========================
       1) Slowniki i konta
       ========================= */

    IF NOT EXISTS (SELECT 1 FROM Roles WHERE [Name] = N'Admin')
        INSERT INTO Roles ([Name]) VALUES (N'Admin');
    IF NOT EXISTS (SELECT 1 FROM Roles WHERE [Name] = N'Magazynier')
        INSERT INTO Roles ([Name]) VALUES (N'Magazynier');

    IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = N'admin')
        INSERT INTO Users (Username, PasswordHash, Email, IsActive)
        VALUES (N'admin', N'demo-hash-admin', N'admin@demo.local', 1);

    IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = N'mag1')
        INSERT INTO Users (Username, PasswordHash, Email, IsActive)
        VALUES (N'mag1', N'demo-hash-mag1', N'mag1@demo.local', 1);

    DECLARE @UserAdminId int = (SELECT TOP 1 UserId FROM Users WHERE Username = N'admin');
    DECLARE @UserMag1Id int = (SELECT TOP 1 UserId FROM Users WHERE Username = N'mag1');
    DECLARE @RoleAdminId int = (SELECT TOP 1 RoleId FROM Roles WHERE [Name] = N'Admin');
    DECLARE @RoleWorkerId int = (SELECT TOP 1 RoleId FROM Roles WHERE [Name] = N'Magazynier');

    IF @UserAdminId IS NOT NULL AND @RoleAdminId IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = @UserAdminId AND RoleId = @RoleAdminId)
        INSERT INTO UserRoles (UserId, RoleId) VALUES (@UserAdminId, @RoleAdminId);

    IF @UserMag1Id IS NOT NULL AND @RoleWorkerId IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = @UserMag1Id AND RoleId = @RoleWorkerId)
        INSERT INTO UserRoles (UserId, RoleId) VALUES (@UserMag1Id, @RoleWorkerId);

    IF NOT EXISTS (SELECT 1 FROM UnitsOfMeasure WHERE Code = N'szt')
        INSERT INTO UnitsOfMeasure (Code, [Name], IsActive) VALUES (N'szt', N'Sztuka', 1);
    IF NOT EXISTS (SELECT 1 FROM UnitsOfMeasure WHERE Code = N'op')
        INSERT INTO UnitsOfMeasure (Code, [Name], IsActive) VALUES (N'op', N'Opakowanie', 1);
    IF NOT EXISTS (SELECT 1 FROM UnitsOfMeasure WHERE Code = N'kg')
        INSERT INTO UnitsOfMeasure (Code, [Name], IsActive) VALUES (N'kg', N'Kilogram', 1);

    DECLARE @UomSzt int = (SELECT TOP 1 UomId FROM UnitsOfMeasure WHERE Code = N'szt');
    DECLARE @UomOp int = (SELECT TOP 1 UomId FROM UnitsOfMeasure WHERE Code = N'op');
    DECLARE @UomKg int = (SELECT TOP 1 UomId FROM UnitsOfMeasure WHERE Code = N'kg');

    IF NOT EXISTS (SELECT 1 FROM Warehouses WHERE [Name] = N'Magazyn Glowny')
        INSERT INTO Warehouses ([Name], [Address], IsActive)
        VALUES (N'Magazyn Glowny', N'Poznan, ul. Magazynowa 1', 1);
    IF NOT EXISTS (SELECT 1 FROM Warehouses WHERE [Name] = N'Magazyn Bufor')
        INSERT INTO Warehouses ([Name], [Address], IsActive)
        VALUES (N'Magazyn Bufor', N'Poznan, ul. Logistyczna 12', 1);

    DECLARE @WhMain int = (SELECT TOP 1 WarehouseId FROM Warehouses WHERE [Name] = N'Magazyn Glowny');
    DECLARE @WhBuffer int = (SELECT TOP 1 WarehouseId FROM Warehouses WHERE [Name] = N'Magazyn Bufor');

    IF NOT EXISTS (SELECT 1 FROM Categories WHERE [Name] = N'Elektronika')
        INSERT INTO Categories (ParentCategoryId, [Name], [Path]) VALUES (NULL, N'Elektronika', N'/Elektronika');

    DECLARE @CatElec int = (SELECT TOP 1 CategoryId FROM Categories WHERE [Name] = N'Elektronika');

    IF NOT EXISTS (SELECT 1 FROM Categories WHERE [Name] = N'Skanery')
        INSERT INTO Categories (ParentCategoryId, [Name], [Path]) VALUES (@CatElec, N'Skanery', N'/Elektronika/Skanery');
    IF NOT EXISTS (SELECT 1 FROM Categories WHERE [Name] = N'Akcesoria')
        INSERT INTO Categories (ParentCategoryId, [Name], [Path]) VALUES (@CatElec, N'Akcesoria', N'/Elektronika/Akcesoria');
    IF NOT EXISTS (SELECT 1 FROM Categories WHERE [Name] = N'Chemia')
        INSERT INTO Categories (ParentCategoryId, [Name], [Path]) VALUES (NULL, N'Chemia', N'/Chemia');

    DECLARE @CatScanners int = (SELECT TOP 1 CategoryId FROM Categories WHERE [Name] = N'Skanery');
    DECLARE @CatAcc int = (SELECT TOP 1 CategoryId FROM Categories WHERE [Name] = N'Akcesoria');
    DECLARE @CatChem int = (SELECT TOP 1 CategoryId FROM Categories WHERE [Name] = N'Chemia');

    /* =========================
       2) Produkty i powiazania
       ========================= */

    IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = N'SCN-1000')
        INSERT INTO Products (SKU, [Name], [Description], CategoryId, DefaultUomId, MinStock, ReorderPoint, ReorderQty, IsActive, CreatedAt)
        VALUES (N'SCN-1000', N'Skaner kodow 2D', N'Reczny skaner magazynowy', @CatScanners, @UomSzt, 5.000, 8.000, 20.000, 1, DATEADD(day, -30, @NowUtc));

    IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = N'LAB-050')
        INSERT INTO Products (SKU, [Name], [Description], CategoryId, DefaultUomId, MinStock, ReorderPoint, ReorderQty, IsActive, CreatedAt)
        VALUES (N'LAB-050', N'Etykieta termiczna 50x30', N'Rolki etykiet do drukarki', @CatAcc, @UomOp, 10.000, 15.000, 40.000, 1, DATEADD(day, -25, @NowUtc));

    IF NOT EXISTS (SELECT 1 FROM Products WHERE SKU = N'CLN-001')
        INSERT INTO Products (SKU, [Name], [Description], CategoryId, DefaultUomId, MinStock, ReorderPoint, ReorderQty, IsActive, CreatedAt)
        VALUES (N'CLN-001', N'Plyn do czyszczenia skanerow', N'Srodek serwisowy', @CatChem, @UomKg, 2.000, 3.000, 10.000, 1, DATEADD(day, -20, @NowUtc));

    DECLARE @ProdScan int = (SELECT TOP 1 ProductId FROM Products WHERE SKU = N'SCN-1000');
    DECLARE @ProdLabel int = (SELECT TOP 1 ProductId FROM Products WHERE SKU = N'LAB-050');
    DECLARE @ProdClean int = (SELECT TOP 1 ProductId FROM Products WHERE SKU = N'CLN-001');

    IF @ProdScan IS NOT NULL AND NOT EXISTS (SELECT 1 FROM ProductCodes WHERE CodeValue = N'5901234123457')
        INSERT INTO ProductCodes (ProductId, CodeValue, CodeType, IsPrimary) VALUES (@ProdScan, N'5901234123457', N'EAN', 1);
    IF @ProdLabel IS NOT NULL AND NOT EXISTS (SELECT 1 FROM ProductCodes WHERE CodeValue = N'5901234123458')
        INSERT INTO ProductCodes (ProductId, CodeValue, CodeType, IsPrimary) VALUES (@ProdLabel, N'5901234123458', N'EAN', 1);
    IF @ProdScan IS NOT NULL AND NOT EXISTS (SELECT 1 FROM ProductCodes WHERE CodeValue = N'SCN-1000-QR')
        INSERT INTO ProductCodes (ProductId, CodeValue, CodeType, IsPrimary) VALUES (@ProdScan, N'SCN-1000-QR', N'QR', 0);

    IF @ProdLabel IS NOT NULL AND @UomSzt IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM ProductUoms WHERE ProductId = @ProdLabel AND UomId = @UomSzt)
        INSERT INTO ProductUoms (ProductId, UomId, FactorToDefault) VALUES (@ProdLabel, @UomSzt, 500.000000);

    IF @ProdScan IS NOT NULL AND @UomOp IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM ProductUoms WHERE ProductId = @ProdScan AND UomId = @UomOp)
        INSERT INTO ProductUoms (ProductId, UomId, FactorToDefault) VALUES (@ProdScan, @UomOp, 5.000000);

    /* =========================
       3) Lokacje / kontrahenci
       ========================= */

    IF @WhMain IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Locations WHERE WarehouseId = @WhMain AND Code = N'A-01-01')
        INSERT INTO Locations (WarehouseId, Code, [Description], IsActive) VALUES (@WhMain, N'A-01-01', N'Regal A, poziom 1', 1);
    IF @WhMain IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Locations WHERE WarehouseId = @WhMain AND Code = N'A-01-02')
        INSERT INTO Locations (WarehouseId, Code, [Description], IsActive) VALUES (@WhMain, N'A-01-02', N'Regal A, poziom 2', 1);
    IF @WhMain IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Locations WHERE WarehouseId = @WhMain AND Code = N'STG-01')
        INSERT INTO Locations (WarehouseId, Code, [Description], IsActive) VALUES (@WhMain, N'STG-01', N'Strefa przyjec', 1);
    IF @WhBuffer IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Locations WHERE WarehouseId = @WhBuffer AND Code = N'BUF-01')
        INSERT INTO Locations (WarehouseId, Code, [Description], IsActive) VALUES (@WhBuffer, N'BUF-01', N'Strefa buforowa', 1);

    DECLARE @LocA0101 int = (SELECT TOP 1 LocationId FROM Locations WHERE WarehouseId = @WhMain AND Code = N'A-01-01');
    DECLARE @LocA0102 int = (SELECT TOP 1 LocationId FROM Locations WHERE WarehouseId = @WhMain AND Code = N'A-01-02');
    DECLARE @LocStg01 int = (SELECT TOP 1 LocationId FROM Locations WHERE WarehouseId = @WhMain AND Code = N'STG-01');
    DECLARE @LocBuf01 int = (SELECT TOP 1 LocationId FROM Locations WHERE WarehouseId = @WhBuffer AND Code = N'BUF-01');

    IF NOT EXISTS (SELECT 1 FROM Suppliers WHERE [Name] = N'ABC Supply Sp. z o.o.')
        INSERT INTO Suppliers ([Name], TaxId, Email, Phone, [Address], IsActive, CreatedAt)
        VALUES (N'ABC Supply Sp. z o.o.', N'7810001112', N'kontakt@abcsupply.pl', N'+48 600 100 200', N'Poznan, ul. Dostawcza 5', 1, DATEADD(day, -90, @NowUtc));

    IF NOT EXISTS (SELECT 1 FROM Suppliers WHERE [Name] = N'LabelTech SA')
        INSERT INTO Suppliers ([Name], TaxId, Email, Phone, [Address], IsActive, CreatedAt)
        VALUES (N'LabelTech SA', N'7820002223', N'biuro@labeltech.pl', N'+48 600 300 400', N'Warszawa, ul. Papierowa 10', 1, DATEADD(day, -70, @NowUtc));

    IF NOT EXISTS (SELECT 1 FROM Customers WHERE [Name] = N'Sklep Partner 1')
        INSERT INTO Customers ([Name], Email, Phone, [Address], IsActive, CreatedAt)
        VALUES (N'Sklep Partner 1', N'zakupy@partner1.pl', N'+48 500 111 222', N'Gdansk, ul. Handlowa 9', 1, DATEADD(day, -60, @NowUtc));

    IF NOT EXISTS (SELECT 1 FROM Customers WHERE [Name] = N'Serwis Mobile')
        INSERT INTO Customers ([Name], Email, Phone, [Address], IsActive, CreatedAt)
        VALUES (N'Serwis Mobile', N'serwis@mobile.pl', N'+48 500 333 444', N'Wroclaw, ul. Serwisowa 2', 1, DATEADD(day, -40, @NowUtc));

    DECLARE @SupplierAbc int = (SELECT TOP 1 SupplierId FROM Suppliers WHERE [Name] = N'ABC Supply Sp. z o.o.');
    DECLARE @SupplierLabel int = (SELECT TOP 1 SupplierId FROM Suppliers WHERE [Name] = N'LabelTech SA');
    DECLARE @Customer1 int = (SELECT TOP 1 CustomerId FROM Customers WHERE [Name] = N'Sklep Partner 1');

    /* =========================
       4) Partie / stany / ruchy
       ========================= */

    IF @ProdScan IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Batches WHERE ProductId = @ProdScan AND BatchNumber = N'SCN-2026-01')
        INSERT INTO Batches (ProductId, BatchNumber, ProductionDate, ExpiryDate, SupplierId)
        VALUES (@ProdScan, N'SCN-2026-01', '2026-01-05', '2028-01-05', @SupplierAbc);

    IF @ProdLabel IS NOT NULL AND NOT EXISTS (SELECT 1 FROM Batches WHERE ProductId = @ProdLabel AND BatchNumber = N'LAB-2026-02')
        INSERT INTO Batches (ProductId, BatchNumber, ProductionDate, ExpiryDate, SupplierId)
        VALUES (@ProdLabel, N'LAB-2026-02', '2026-02-01', '2027-02-01', @SupplierLabel);

    IF @ProdScan IS NOT NULL AND @LocA0101 IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM Stock WHERE ProductId = @ProdScan AND LocationId = @LocA0101)
        INSERT INTO Stock (ProductId, LocationId, Quantity) VALUES (@ProdScan, @LocA0101, 12.000);

    IF @ProdLabel IS NOT NULL AND @LocA0102 IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM Stock WHERE ProductId = @ProdLabel AND LocationId = @LocA0102)
        INSERT INTO Stock (ProductId, LocationId, Quantity) VALUES (@ProdLabel, @LocA0102, 18.000);

    IF @ProdClean IS NOT NULL AND @LocBuf01 IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM Stock WHERE ProductId = @ProdClean AND LocationId = @LocBuf01)
        INSERT INTO Stock (ProductId, LocationId, Quantity) VALUES (@ProdClean, @LocBuf01, 1.500);

    IF @ProdScan IS NOT NULL AND @LocStg01 IS NOT NULL AND @UserMag1Id IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM StockMovements WHERE MovementType = 1 AND ProductId = @ProdScan AND Reference = N'PZ/2026/0001')
        INSERT INTO StockMovements (MovementType, ProductId, FromLocationId, ToLocationId, Quantity, Reference, Note, CreatedAt, CreatedByUserId)
        VALUES (1, @ProdScan, NULL, @LocStg01, 10.000, N'PZ/2026/0001', N'Przyjecie demo', DATEADD(day, -5, @NowUtc), @UserMag1Id);

    IF @ProdScan IS NOT NULL AND @LocA0101 IS NOT NULL AND @LocBuf01 IS NOT NULL AND @UserMag1Id IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM StockMovements WHERE MovementType = 3 AND ProductId = @ProdScan AND Reference = N'MM/2026/0001')
        INSERT INTO StockMovements (MovementType, ProductId, FromLocationId, ToLocationId, Quantity, Reference, Note, CreatedAt, CreatedByUserId)
        VALUES (3, @ProdScan, @LocA0101, @LocBuf01, 2.000, N'MM/2026/0001', N'Przesuniecie demo', DATEADD(day, -2, @NowUtc), @UserMag1Id);

    /* =========================
       5) Dokumenty PZ/WZ/MM + pozycje
       ========================= */

    IF @WhMain IS NOT NULL AND @SupplierAbc IS NOT NULL AND @UserMag1Id IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM GoodsReceipts WHERE DocumentNo = N'PZ/2026/0001')
        INSERT INTO GoodsReceipts (DocumentNo, WarehouseId, SupplierId, Status, ReceivedAt, CreatedByUserId, PostedAt, Note)
        VALUES (N'PZ/2026/0001', @WhMain, @SupplierAbc, N'Posted', DATEADD(day, -5, @NowUtc), @UserMag1Id, DATEADD(day, -5, DATEADD(minute, 20, @NowUtc)), N'Dostawa skanerow');

    DECLARE @Pz1 int = (SELECT TOP 1 ReceiptId FROM GoodsReceipts WHERE DocumentNo = N'PZ/2026/0001');

    IF @Pz1 IS NOT NULL AND @ProdScan IS NOT NULL AND @LocStg01 IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM GoodsReceiptItems WHERE ReceiptId = @Pz1 AND [LineNo] = 1)
        INSERT INTO GoodsReceiptItems (ReceiptId, [LineNo], ProductId, LocationId, Quantity, UnitPrice)
        VALUES (@Pz1, 1, @ProdScan, @LocStg01, 10.000, 799.00);

    IF @Pz1 IS NOT NULL AND @ProdLabel IS NOT NULL AND @LocA0102 IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM GoodsReceiptItems WHERE ReceiptId = @Pz1 AND [LineNo] = 2)
        INSERT INTO GoodsReceiptItems (ReceiptId, [LineNo], ProductId, LocationId, Quantity, UnitPrice)
        VALUES (@Pz1, 2, @ProdLabel, @LocA0102, 5.000, 129.50);

    IF @WhMain IS NOT NULL AND @UserMag1Id IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM GoodsIssues WHERE DocumentNo = N'WZ/2026/0001')
        INSERT INTO GoodsIssues (DocumentNo, WarehouseId, CustomerId, Status, IssuedAt, CreatedByUserId, PostedAt, Note)
        VALUES (N'WZ/2026/0001', @WhMain, @Customer1, N'Posted', DATEADD(day, -3, @NowUtc), @UserMag1Id, DATEADD(day, -3, DATEADD(minute, 10, @NowUtc)), N'Wydanie do klienta demo');

    DECLARE @Wz1 int = (SELECT TOP 1 IssueId FROM GoodsIssues WHERE DocumentNo = N'WZ/2026/0001');

    IF @Wz1 IS NOT NULL AND @ProdLabel IS NOT NULL AND @LocA0102 IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM GoodsIssueItems WHERE IssueId = @Wz1 AND [LineNo] = 1)
        INSERT INTO GoodsIssueItems (IssueId, [LineNo], ProductId, LocationId, Quantity)
        VALUES (@Wz1, 1, @ProdLabel, @LocA0102, 2.000);

    IF @WhMain IS NOT NULL AND @UserMag1Id IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM StockTransfers WHERE DocumentNo = N'MM/2026/0001')
        INSERT INTO StockTransfers (DocumentNo, WarehouseId, Status, TransferredAt, CreatedByUserId, PostedAt, Note)
        VALUES (N'MM/2026/0001', @WhMain, N'Posted', DATEADD(day, -2, @NowUtc), @UserMag1Id, DATEADD(day, -2, DATEADD(minute, 5, @NowUtc)), N'Przesuniecie miedzy lokacjami');

    DECLARE @Mm1 int = (SELECT TOP 1 TransferId FROM StockTransfers WHERE DocumentNo = N'MM/2026/0001');

    IF @Mm1 IS NOT NULL AND @ProdScan IS NOT NULL AND @LocStg01 IS NOT NULL AND @LocA0101 IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM StockTransferItems WHERE TransferId = @Mm1 AND [LineNo] = 1)
        INSERT INTO StockTransferItems (TransferId, [LineNo], ProductId, FromLocationId, ToLocationId, Quantity)
        VALUES (@Mm1, 1, @ProdScan, @LocStg01, @LocA0101, 4.000);

    /* =========================
       6) Rezerwacje / inwentaryzacje + pozycje
       ========================= */

    IF @WhMain IS NOT NULL AND @UserAdminId IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM Reservations WHERE DocumentNo = N'REZ/2026/0001')
        INSERT INTO Reservations (DocumentNo, WarehouseId, Status, CreatedAt, ExpiresAt, CreatedByUserId, Note)
        VALUES (N'REZ/2026/0001', @WhMain, N'Active', DATEADD(day, -1, @NowUtc), DATEADD(day, 2, @NowUtc), @UserAdminId, N'Rezerwacja pod zamowienie klienta');

    DECLARE @Rez1 int = (SELECT TOP 1 ReservationId FROM Reservations WHERE DocumentNo = N'REZ/2026/0001');

    IF @Rez1 IS NOT NULL AND @ProdScan IS NOT NULL AND @LocA0101 IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM ReservationItems WHERE ReservationId = @Rez1 AND [LineNo] = 1)
        INSERT INTO ReservationItems (ReservationId, [LineNo], ProductId, LocationId, Quantity)
        VALUES (@Rez1, 1, @ProdScan, @LocA0101, 2.000);

    IF @Rez1 IS NOT NULL AND @ProdClean IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM ReservationItems WHERE ReservationId = @Rez1 AND [LineNo] = 2)
        INSERT INTO ReservationItems (ReservationId, [LineNo], ProductId, LocationId, Quantity)
        VALUES (@Rez1, 2, @ProdClean, NULL, 0.500);

    IF @WhMain IS NOT NULL AND @UserAdminId IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM StockCounts WHERE DocumentNo = N'INV/2026/0001')
        INSERT INTO StockCounts (DocumentNo, WarehouseId, Status, StartedAt, ClosedAt, CreatedByUserId, Note)
        VALUES (N'INV/2026/0001', @WhMain, N'Closed', DATEADD(day, -4, @NowUtc), DATEADD(day, -4, DATEADD(hour, 2, @NowUtc)), @UserAdminId, N'Inwentaryzacja testowa strefy A');

    DECLARE @Inv1 int = (SELECT TOP 1 StockCountId FROM StockCounts WHERE DocumentNo = N'INV/2026/0001');

    IF @Inv1 IS NOT NULL AND @ProdScan IS NOT NULL AND @LocA0101 IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM StockCountItems WHERE StockCountId = @Inv1 AND [LineNo] = 1)
        INSERT INTO StockCountItems (StockCountId, [LineNo], ProductId, LocationId, ExpectedQty, CountedQty)
        VALUES (@Inv1, 1, @ProdScan, @LocA0101, 12.000, 11.000);

    IF @Inv1 IS NOT NULL AND @ProdLabel IS NOT NULL AND @LocA0102 IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM StockCountItems WHERE StockCountId = @Inv1 AND [LineNo] = 2)
        INSERT INTO StockCountItems (StockCountId, [LineNo], ProductId, LocationId, ExpectedQty, CountedQty)
        VALUES (@Inv1, 2, @ProdLabel, @LocA0102, 18.000, 18.000);

    /* =========================
       7) Alerty / reguly / log / ustawienia
       ========================= */

    IF @WhMain IS NOT NULL AND @ProdScan IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM AlertRules WHERE WarehouseId = @WhMain AND ProductId = @ProdScan AND RuleType = N'LowStock')
        INSERT INTO AlertRules (WarehouseId, ProductId, RuleType, ThresholdValue, IsEnabled, CreatedAt)
        VALUES (@WhMain, @ProdScan, N'LowStock', 8.000, 1, DATEADD(day, -15, @NowUtc));

    IF @WhMain IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM AlertRules WHERE WarehouseId = @WhMain AND ProductId IS NULL AND RuleType = N'InventoryMismatch')
        INSERT INTO AlertRules (WarehouseId, ProductId, RuleType, ThresholdValue, IsEnabled, CreatedAt)
        VALUES (@WhMain, NULL, N'InventoryMismatch', 1.000, 1, DATEADD(day, -12, @NowUtc));

    DECLARE @RuleLowStock int = (
        SELECT TOP 1 AlertRuleId FROM AlertRules
        WHERE WarehouseId = @WhMain AND ProductId = @ProdScan AND RuleType = N'LowStock'
    );
    DECLARE @RuleInvMismatch int = (
        SELECT TOP 1 AlertRuleId FROM AlertRules
        WHERE WarehouseId = @WhMain AND ProductId IS NULL AND RuleType = N'InventoryMismatch'
    );

    IF @RuleLowStock IS NOT NULL AND @WhMain IS NOT NULL AND @ProdScan IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM Alerts WHERE AlertRuleId = @RuleLowStock AND [Message] = N'Niski stan skanera SCN-1000')
        INSERT INTO Alerts (AlertRuleId, WarehouseId, ProductId, Severity, [Message], CreatedAt, IsAcknowledged, AcknowledgedByUserId, AcknowledgedAt)
        VALUES (@RuleLowStock, @WhMain, @ProdScan, N'WARN', N'Niski stan skanera SCN-1000', DATEADD(hour, -6, @NowUtc), 0, NULL, NULL);

    IF @RuleInvMismatch IS NOT NULL AND @WhMain IS NOT NULL AND @ProdScan IS NOT NULL AND @UserAdminId IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM Alerts WHERE AlertRuleId = @RuleInvMismatch AND [Message] = N'Roznica po inwentaryzacji dla SCN-1000')
        INSERT INTO Alerts (AlertRuleId, WarehouseId, ProductId, Severity, [Message], CreatedAt, IsAcknowledged, AcknowledgedByUserId, AcknowledgedAt)
        VALUES (@RuleInvMismatch, @WhMain, @ProdScan, N'CRIT', N'Roznica po inwentaryzacji dla SCN-1000', DATEADD(hour, -4, @NowUtc), 1, @UserAdminId, DATEADD(hour, -3, @NowUtc));

    IF NOT EXISTS (SELECT 1 FROM AppSettings WHERE [Key] = N'Warehouse.DefaultTimezone')
        INSERT INTO AppSettings ([Key], [Value], [Description], UpdatedAt, UpdatedByUserId)
        VALUES (N'Warehouse.DefaultTimezone', N'Europe/Warsaw', N'Strefa czasowa dla dat magazynowych', DATEADD(day, -10, @NowUtc), @UserAdminId);

    IF NOT EXISTS (SELECT 1 FROM AppSettings WHERE [Key] = N'Alerts.EmailEnabled')
        INSERT INTO AppSettings ([Key], [Value], [Description], UpdatedAt, UpdatedByUserId)
        VALUES (N'Alerts.EmailEnabled', N'true', N'Czy wysylac powiadomienia e-mail o alertach', DATEADD(day, -8, @NowUtc), @UserAdminId);

    IF NOT EXISTS (SELECT 1 FROM AppSettings WHERE [Key] = N'Inventory.CountTolerance')
        INSERT INTO AppSettings ([Key], [Value], [Description], UpdatedAt, UpdatedByUserId)
        VALUES (N'Inventory.CountTolerance', N'1', N'Tolerancja roznicy ilosci w inwentaryzacji', DATEADD(day, -7, @NowUtc), @UserAdminId);

    IF @UserAdminId IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM AuditLog WHERE [Action] = N'LOGIN' AND EntityName = N'User' AND EntityId = CONVERT(nvarchar(80), @UserAdminId))
        INSERT INTO AuditLog (UserId, [Action], EntityName, EntityId, [At], OldValuesJson, NewValuesJson)
        VALUES (@UserAdminId, N'LOGIN', N'User', CONVERT(nvarchar(80), @UserAdminId), DATEADD(hour, -12, @NowUtc), NULL, N'{"status":"success"}');

    IF @UserMag1Id IS NOT NULL AND @Pz1 IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM AuditLog WHERE [Action] = N'CREATE' AND EntityName = N'DokumentPZ' AND EntityId = CONVERT(nvarchar(80), @Pz1))
        INSERT INTO AuditLog (UserId, [Action], EntityName, EntityId, [At], OldValuesJson, NewValuesJson)
        VALUES (@UserMag1Id, N'CREATE', N'DokumentPZ', CONVERT(nvarchar(80), @Pz1), DATEADD(day, -5, @NowUtc), NULL, N'{"Numer":"PZ/2026/0001","Status":"Posted"}');

    IF @UserAdminId IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM AuditLog WHERE [Action] = N'UPDATE' AND EntityName = N'AppSettings' AND EntityId = N'Alerts.EmailEnabled')
        INSERT INTO AuditLog (UserId, [Action], EntityName, EntityId, [At], OldValuesJson, NewValuesJson)
        VALUES (@UserAdminId, N'UPDATE', N'AppSettings', N'Alerts.EmailEnabled', DATEADD(day, -8, @NowUtc), N'{"Value":"false"}', N'{"Value":"true"}');

    /* =========================
       8) CMS i pliki (pozostale tabele)
       ========================= */

    IF NOT EXISTS (SELECT 1 FROM Aktualnosc WHERE TytulLinku = N'NowaDostawa')
        INSERT INTO Aktualnosc (TytulLinku, Nazwa, Tresc, Pozycja)
        VALUES (
            N'NowaDostawa',
            N'Nowa dostawa skanerow',
            N'Do magazynu glownego przyjeto nowa partie skanerow 2D. Towar zostal rozlokowany i jest dostepny do kompletacji.',
            1
        );

    IF NOT EXISTS (SELECT 1 FROM Aktualnosc WHERE TytulLinku = N'Inwentaryzacja')
        INSERT INTO Aktualnosc (TytulLinku, Nazwa, Tresc, Pozycja)
        VALUES (
            N'Inwentaryzacja',
            N'Planowana inwentaryzacja strefy A',
            N'W czwartek od 18:00 bedzie prowadzona inwentaryzacja lokacji A-01. Prosba o ograniczenie ruchow w tej strefie.',
            2
        );

    IF NOT EXISTS (SELECT 1 FROM Strona WHERE TytulLinku = N'OFirmie')
        INSERT INTO Strona (TytulLinku, Nazwa, Tresc, Pozycja)
        VALUES (
            N'OFirmie',
            N'O systemie magazynowym',
            N'IntelligentWarehouse wspiera procesy przyjec, wydan, przesuniec, rezerwacji i inwentaryzacji wraz z audytem zmian.',
            1
        );

    IF NOT EXISTS (SELECT 1 FROM Strona WHERE TytulLinku = N'Kontakt')
        INSERT INTO Strona (TytulLinku, Nazwa, Tresc, Pozycja)
        VALUES (
            N'Kontakt',
            N'Kontakt z administratorem',
            N'W sprawach konfiguracji systemu i uprawnien skontaktuj sie z administratorem pod adresem admin@demo.local.',
            2
        );

    IF @UserAdminId IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM MediaFiles WHERE FilePath = N'/uploads/media/magazyn-glowny-plan.png')
        INSERT INTO MediaFiles (FileName, ContentType, FilePath, SizeBytes, [Description], UploadedAt, UploadedByUserId)
        VALUES (
            N'magazyn-glowny-plan.png',
            N'image/png',
            N'/uploads/media/magazyn-glowny-plan.png',
            248576,
            N'Plan magazynu glownego - wersja demonstracyjna',
            DATEADD(day, -6, @NowUtc),
            @UserAdminId
        );

    IF @UserAdminId IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM MediaFiles WHERE FilePath = N'/uploads/media/instrukcja-przyjecie.pdf')
        INSERT INTO MediaFiles (FileName, ContentType, FilePath, SizeBytes, [Description], UploadedAt, UploadedByUserId)
        VALUES (
            N'instrukcja-przyjecie.pdf',
            N'application/pdf',
            N'/uploads/media/instrukcja-przyjecie.pdf',
            812340,
            N'Instrukcja procesu przyjecia towaru (demo)',
            DATEADD(day, -5, @NowUtc),
            @UserAdminId
        );

    IF @UserAdminId IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM PrintTemplates WHERE DocType = N'PZ' AND [Version] = N'1.0')
        INSERT INTO PrintTemplates (DocType, [Name], [Version], FileName, FilePath, IsActive, UploadedAt, UploadedByUserId)
        VALUES (
            N'PZ',
            N'Szablon przyjecia standard',
            N'1.0',
            N'PZ_standard_v1.docx',
            N'/templates/pz/PZ_standard_v1.docx',
            1,
            DATEADD(day, -14, @NowUtc),
            @UserAdminId
        );

    IF @UserAdminId IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM PrintTemplates WHERE DocType = N'WZ' AND [Version] = N'1.0')
        INSERT INTO PrintTemplates (DocType, [Name], [Version], FileName, FilePath, IsActive, UploadedAt, UploadedByUserId)
        VALUES (
            N'WZ',
            N'Szablon wydania standard',
            N'1.0',
            N'WZ_standard_v1.docx',
            N'/templates/wz/WZ_standard_v1.docx',
            1,
            DATEADD(day, -13, @NowUtc),
            @UserAdminId
        );

    IF @UserAdminId IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM PrintTemplates WHERE DocType = N'MM' AND [Version] = N'1.0')
        INSERT INTO PrintTemplates (DocType, [Name], [Version], FileName, FilePath, IsActive, UploadedAt, UploadedByUserId)
        VALUES (
            N'MM',
            N'Szablon przesuniecia',
            N'1.0',
            N'MM_standard_v1.docx',
            N'/templates/mm/MM_standard_v1.docx',
            1,
            DATEADD(day, -12, @NowUtc),
            @UserAdminId
        );

    IF @UserAdminId IS NOT NULL AND @Pz1 IS NOT NULL
       AND NOT EXISTS (
           SELECT 1 FROM DocumentAttachments
           WHERE DocumentType = N'PZ' AND DocumentId = @Pz1 AND FilePath = N'/uploads/docs/pz-2026-0001-spec.pdf'
       )
        INSERT INTO DocumentAttachments (DocumentType, DocumentId, FileName, ContentType, FilePath, UploadedAt, UploadedByUserId)
        VALUES (
            N'PZ',
            @Pz1,
            N'pz-2026-0001-spec.pdf',
            N'application/pdf',
            N'/uploads/docs/pz-2026-0001-spec.pdf',
            DATEADD(day, -5, DATEADD(minute, 5, @NowUtc)),
            @UserAdminId
        );

    IF @UserAdminId IS NOT NULL AND @Wz1 IS NOT NULL
       AND NOT EXISTS (
           SELECT 1 FROM DocumentAttachments
           WHERE DocumentType = N'WZ' AND DocumentId = @Wz1 AND FilePath = N'/uploads/docs/wz-2026-0001-confirmation.pdf'
       )
        INSERT INTO DocumentAttachments (DocumentType, DocumentId, FileName, ContentType, FilePath, UploadedAt, UploadedByUserId)
        VALUES (
            N'WZ',
            @Wz1,
            N'wz-2026-0001-confirmation.pdf',
            N'application/pdf',
            N'/uploads/docs/wz-2026-0001-confirmation.pdf',
            DATEADD(day, -3, DATEADD(minute, 20, @NowUtc)),
            @UserAdminId
        );

    IF @UserAdminId IS NOT NULL AND @Mm1 IS NOT NULL
       AND NOT EXISTS (
           SELECT 1 FROM DocumentAttachments
           WHERE DocumentType = N'MM' AND DocumentId = @Mm1 AND FilePath = N'/uploads/docs/mm-2026-0001-photo.jpg'
       )
        INSERT INTO DocumentAttachments (DocumentType, DocumentId, FileName, ContentType, FilePath, UploadedAt, UploadedByUserId)
        VALUES (
            N'MM',
            @Mm1,
            N'mm-2026-0001-photo.jpg',
            N'image/jpeg',
            N'/uploads/docs/mm-2026-0001-photo.jpg',
            DATEADD(day, -2, DATEADD(minute, 30, @NowUtc)),
            @UserAdminId
        );

    COMMIT TRAN;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRAN;

    THROW;
END CATCH;
