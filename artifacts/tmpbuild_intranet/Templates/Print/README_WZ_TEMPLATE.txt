WZ template placeholders (used by WydrukDokumentuService)

Header/document placeholders:
- {{TemplateName}}
- {{TemplateVersion}}
- {{GeneratedAt}}
- {{GeneratedAtUtc}}
- {{DocType}}
- {{DocumentId}}
- {{DocumentNo}}
- {{Status}}
- {{IssuedAt}}
- {{PostedAt}}
- {{WarehouseName}}
- {{WarehouseId}}
- {{CustomerName}}
- {{CustomerEmail}}
- {{CustomerPhone}}
- {{CreatedBy}}
- {{Note}}
- {{ItemsCount}}
- {{TotalQty}}

Item loop syntax:
- {{#Items}} ... {{/Items}}

Placeholders available inside item loop:
- {{ItemId}}
- {{LineNo}}
- {{ProductId}}
- {{ProductCode}}
- {{ProductName}}
- {{Quantity}}
- {{Unit}}
- {{LocationCode}}
- {{LocationName}}
- {{LocationWarehouse}}
- {{BatchNo}}

Notes:
- Supported template file extensions: .html, .htm, .txt, .md, .docx
- For .docx placeholder replacement is best-effort (placeholders should be plain text without mixed formatting).
