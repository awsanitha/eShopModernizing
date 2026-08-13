# Migration Summary – eShopLegacyNTier → .NET 10

## Result

`dotnet build eShopLegacyNTier.sln` exits **0** with **0 errors, 0 warnings**.

---

## What Was Migrated

### Solution File
- Added the missing `eShopWinForms` project entry (GUID `{AE32909C...}`) to the `.sln` file — it was referenced in the config sections but had no `Project` block.

### eShopWCFService (WCF server)

| Before | After |
|--------|-------|
| .NET Framework 4.6.1, legacy MSBuild `.csproj` | `net10.0`, SDK-style `Microsoft.NET.Sdk.Web` |
| `System.ServiceModel` (framework-shipped) | `CoreWCF.Primitives 1.9.1` + `CoreWCF.Http 1.9.1` |
| Entity Framework 6 (`System.Data.Entity`) | `Microsoft.EntityFrameworkCore.SqlServer 10.0.0` |
| `[ServiceContract]` / `[OperationContract]` from `System.ServiceModel` | Same attributes from `CoreWCF` namespace |
| `DbContext` constructed with connection-string name, EF6 initializer | `DbContextOptions<EntityModel>` + DI registration; `CatalogDBInitializer.Seed()` replaces `CreateDatabaseIfNotExists<T>` |
| IIS / `.svc` hosting + `web.config` | Kestrel / ASP.NET Core hosting in `Program.cs` + `appsettings.json` |
| `System.Web` in models and infrastructure | All `using System.Web` removed |
| `System.Data.Entity.Spatial` in models | Removed (no geography columns in schema) |

**New files:**
- `Program.cs` — CoreWCF ASP.NET Core host, EF Core DI registration, startup seeding
- `appsettings.json` — connection string replacing `web.config`

**Modified files:**
- `ICatalogService.cs` — `using CoreWCF` replaces `using System.ServiceModel`
- `CatalogService.svc.cs` — DI constructor only, `Microsoft.EntityFrameworkCore.EntityState`
- `EntityModel.cs` — EF Core `ModelBuilder`, `DbContextOptions<EntityModel>`
- All model files (`CatalogItem`, `CatalogBrand`, `CatalogType`, `CatalogItemsStock`, `DiscountItem`) — `System.Data.Entity.Spatial` removed
- `CatalogConfiguration.cs`, `PreconfiguredData.cs` — `System.Web` removed
- `CatalogDBInitializer.cs` — replaced EF6 `CreateDatabaseIfNotExists<T>` with `static void Seed(EntityModel)`
- `CatalogServiceMock.cs` — nullable annotations, null-safe `GetAvailableStock`

**Excluded from compilation:**
- `CatalogServiceClient.cs` — a misplaced WCF client proxy in the server project; excluded via `<Compile Remove="…" />`
- `Properties/AssemblyInfo.cs` — SDK auto-generates assembly attributes

### eShopWinForms (WinForms client)

| Before | After |
|--------|-------|
| .NET Framework 4.7, legacy MSBuild `.csproj` | `net10.0-windows`, SDK-style `UseWindowsForms` |
| `System.ServiceModel` (framework-shipped) | `System.ServiceModel.Http 8.1.0` + `System.ServiceModel.Primitives 8.1.0` |
| Config-file WCF client endpoint | Explicit `BasicHttpBinding` + `EndpointAddress` constructor in `Program.cs` |
| `Reference.cs` string-based `ClientBase<T>` constructors | Removed; only `(Binding, EndpointAddress)` and parameterless constructors kept |

**New / modified files:**
- `eShopWinForms.csproj` — SDK-style, `net10.0-windows`, `EnableWindowsTargeting` for Linux CI
- `Program.cs` — explicit binding constructor replaces config-file endpoint
- `CatalogController.cs` — nullable annotations
- `ICatalogView.cs` — cleaned imports
- `Views/CatalogView.cs` — nullable annotations (`null!`, null-conditional events)
- `Connected Services/eShopServiceReference/Reference.cs` — unsupported `string`-arg constructors removed

**Excluded from compilation (UWP leftover helpers — not referenced by WinForms code):**
- `Helpers/ResourceExtensions.cs` (Windows.ApplicationModel.Resources)
- `Helpers/UploadImageHelper.cs` (Windows.Storage)
- `Helpers/NotificationsHelper.cs` (Windows.UI.Notifications / Microsoft.Toolkit.Uwp)
- `Helpers/DependencyObjectExtensions.cs` (Windows.UI.Xaml)
- `Helpers/SettingsStorageExtensions.cs` (Windows.Storage)
- `Helpers/Json.cs` (eShop.UWP.Helpers namespace — not consumed by WinForms)

---

## Next Steps / Assumptions / Risks

1. **EF Core migrations not created.** The app uses `Database.EnsureCreated()` + `CatalogDBInitializer.Seed()` at startup — this recreates the schema from scratch if the DB does not exist. For a production upgrade against an existing database, run `dotnet ef migrations add InitialCreate` and `dotnet ef database update` instead.

2. **WCF service URL hardcoded in `Program.cs`** (`http://localhost:62314/CatalogService.svc`). Move this to `appsettings.json` or an environment variable for deployment flexibility.

3. **UWP helpers excluded, not deleted.** Files in `Helpers/` with the `eShop.UWP.Helpers` namespace are excluded via `<Compile Remove="…"/>` but still exist on disk. They can be safely deleted once the team confirms they are no longer needed in any other context.

4. **WinForms runs on Windows only.** Targeting `net10.0-windows` is correct for a WinForms application. `EnableWindowsTargeting` is set to allow the build to succeed on Linux CI agents; the resulting binary must run on Windows.

5. **SOAP contract mismatch (`int` vs `Nullable<int>` on `GetCatalogItems`).** The server contract uses `int`, the auto-generated proxy uses `Nullable<int>`. This is a legacy discrepancy in the original code. At runtime the server will receive `0` for `null`; this matches the existing `brandFilterIsNull = brandIdFilter == 0` logic. No functional change.

6. **CoreWCF 1.9.1 vulnerability advisory.** Upgraded from 1.6.0 (multiple advisories including critical) to 1.9.1 (the current stable release) to address NU1901–NU1904 advisories.
