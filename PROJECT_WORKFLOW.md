# Precheck Product API — Workflow & Core Modules

## 1. Architecture (Layered)

```
Controller (Host layer)
        │
        ▼
Service (Service layer)
        │
        ▼
Repository (Repository layer)  — Dapper / EF Core
        │
        ▼
SQL Server (ApplicationDbContext + BackupDbContext)
```

| Project | Responsibility |
|---|---|
| **Host** | ASP.NET Core Web API entry point (`Program.cs`). Controllers, JWT auth, Swagger, Serilog logging, CORS, DinkToPdf (HTML→PDF) setup. |
| **Service** | Business logic. One `Service/<Domain>Service/` folder per domain (Precheck, QRCode, Sop, Common, DrawingNumber, MaterialRequisition, ProductionOrder, Auth, Archive, Identifier, Testing, Script). Also `Cache/`, `Helper/`, `MapperSetup/` (Mapster), `Resources/`. |
| **Repository** | Data access — mostly Dapper raw SQL, one `<Domain>Repository` per service, plus `Queries/`. |
| **Models** | Shared DTOs (`DTOs/<Domain>/…`) and persistence models (`DataModel/…`). |

**Bootstrapping (`Program.cs`)**
- JWT bearer auth — most endpoints `[Authorize]`; user id/role/department read from claims (`id`, `deptid`, `roleid`, `department`, `username`).
- Two DB contexts: `ApplicationDbContext` (live/`DefaultConnection`) and `BackupDbContext` (`BackupConnection`, used by Archive).
- CORS: `AllowAll`.
- Swagger UI `v9.5` with Bearer auth.
- Serilog → `Logs/yyyy-MM-dd/HH/INFO.txt` & `ERROR.txt`.
- JSON `MaxDepth = 256` (raised specifically for the SOP service's `GetSop` recursive BOM tree, which can nest past the 32-level default).
- DinkToPdf (`wkhtmltopdf`) registered as **singleton** (native lib isn't thread-safe) — used for Precheck export, MSN Memo, Inspection export.
- Mapster (`MappingSetup.Init()`) used for DTO↔model projection (`.Adapt<T>()`).

---

## 2. Core Modules (Controllers)

| Module | Purpose |
|---|---|
| **AuthController** | Login/Register/Reset (JWT), security questions, departments, roles, user-role CRUD, plants. |
| **CommonController** | Reference-data hub (modules, component types, document types, prod series, shapes, units, assemblies, IR/MSN stages, LnItemCode) **and** Drawing Number master data + Assembly↔Drawing mapping CRUD. |
| **DrawingNumberController** | Focused drawing-number mapping: view (`GetDrawingMappings/{id}`) and insert/update (`InsertDrawingMappings`, admin, resolves-or-creates). |
| **SopController** | SOP/BOM tree: `GetSop`/`GetSopExcludingRawMaterial` (recursive BOM per assembly), `GetBomDetails`, `SearchAssembly`, exports. |
| **PrecheckController** | **Core module** — precheck template retrieval, execution (`MakePrecheck`/`BulkPrecheck`/`MakePrecheckFromExcel`), order creation (`MakePrecheckOrder`), status/monitoring views, rework flow, exports. |
| **QRCodeController** | QR/barcode lifecycle: generate, update, disable, store-in, consumed-in lookup, exports. |
| **ProductionOrderController** | Production order master data: upload (Excel), update, get/list (paginated, role/date/status filters), counts, export, delete. |
| **MaterialRequisitionController** | Material requisition + component swap workflow (create/update/cancel, `SwapComponents`, export). |
| **IdentifierController** (`api/reports`) | IR Number / MSN Number identifiers, combined view/export, MSN memo PDF download. |
| **TestingController** | QC inspection: template fields, insert inspection values, 3-stage data entry, inspection PDF export. |
| **UserController** | User admin, prod-series/unit/shape/stage CRUD, page-role access matrix, signature upload. |
| **ArchiveController** | Searches backup DB for historical "consumed-in" drawing-number records. |
| **ScriptController** | Uploads Excel and runs external Python scripts (QR generation, QR import, master data) via `Process.Start`. |

---

## 3. Domain Model

- **Drawing Number** — master item record (a component/part or an assembly): `DrawingNumber`, `LnItemCode`, `Nomenclature`, `Location`, `ComponentType`, `DocumentType`, `Unit`, `AvailableSeries` (which production series it's valid for).
- **Assembly Drawing Mapping** — the BOM edge: "drawing X is a child of parent assembly Y, quantity N, find-no F".
- **Production Order (PO)** — a build order for a target assembly + quantity.
- **Precheck** — the per-unit checklist materialized from the BOM for a PO; each line = one required component that must be scanned/consumed.
- **QR/Barcode** — physical identifier tying a scanned unit to a Drawing Number, production series, batch.

---

## 4. End-to-End Workflow

### Step 1 — Master data setup
Admin creates **Drawing Numbers** (parts/assemblies) and **Assembly-Drawing Mappings** (BOM edges with qty/find-no/valid series) via `CommonController` / `DrawingNumberController`. This defines the BOM/SOP tree.

### Step 2 — SOP / BOM view
`SopController.GetSop` returns the recursive BOM tree for an assembly (optionally excluding raw materials) — the "spec" against which precheck compliance is measured.

### Step 3 — QR/Barcode generation
`QRCodeController.GenerateQRCode` (and batch/standard variants) issues QR codes for physical components. `ComponentStoreIn` marks a scanned QR as received into store.

### Step 4 — Production Order creation
`ProductionOrderController` creates/imports a PO for a target assembly quantity. `PrecheckController.MakePrecheckOrder` → `PrecheckService.MakeOrder` then walks the BOM for each unit and creates one Precheck checklist row per required component (multiplying quantities for "ID"-type components per unit).

### Step 5 — Precheck execution
`MakePrecheck` / `BulkPrecheck` / `MakePrecheckFromExcel` → `PrecheckService.ProcessSinglePrecheckItem`:
1. Validate the scanned QR code exists and isn't already consumed.
2. Validate the QR's drawing matches what the BOM expects for that line.
3. Update QR status (consumed) and quantity.
4. Dispatch by component type — `ID` (serialized), `BATCH` (batch-tracked), `FIM`/`SI` (raw material) — updating component consumption + precheck detail row + disabling the QR code.
5. Recompute overall status: **1 = NotStarted → 2 = InProgress → 3 = Complete**.

### Step 6 — Monitoring
`ViewPrecheck` / `ViewPrechekByParameters` (paginated/filterable by date/status/series), `PendingPrecheck`, `GetStoreAvailablComponents` — planners/QC track progress and pending items.

### Step 7 — Exceptions / rework
`RejectAndDuplicate` (reject + duplicate row for re-attempt), `RemainingPrecheck` (new row for leftover qty), `ResetQrQuantity` (undo consumption).

### Step 8 — Downstream QC / Testing
Once precheck completes for a drawing (`GetPrecheckCompletedComponents`), `TestingController` records 3-stage inspection data and exports inspection/MSN memo PDFs (using uploaded user signatures for sign-off).

### Step 9 — Reporting
Nearly every module supports Excel/PDF export (SOP, BOM, Precheck details, Pending Precheck, QR lists, Consumed-In, Material Requisition, IR/MSN, Production Orders) for audits.

### Step 10 — Archive
`ArchiveController` + `BackupDbContext` support historical "consumed-in" search once data ages out of live tables.

---

## 5. Pipeline Summary

```
Drawing Number + Assembly Mapping (BOM/SOP master data)
        │
        ▼
Production Order created/imported
        │
        ▼
MakePrecheckOrder materializes per-unit Precheck checklist rows from BOM
        │
        ▼
Components get QR/Barcodes generated → scanned into store (ComponentStoreIn)
        │
        ▼
MakePrecheck / BulkPrecheck:
  scan QR → validate vs expected drawing → consume QR → update quantity
  → recompute Precheck status (NotStarted / InProgress / Complete)
        │
        ▼
ViewPrecheck / PendingPrecheck dashboards + exports
        │
        ▼
Testing/Inspection stages → MSN/IR memo sign-off documents
        │
        ▼
Archive (historical consumed-in search, backup DB)
```
