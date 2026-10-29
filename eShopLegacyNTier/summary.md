# Migration Summary — eShopLegacyNTier (.NET Framework → net10.0)

## Final Build Status

```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

`dotnet build eShopLegacyNTier.sln` exits 0 with zero errors and zero warnings.

---

## Scope

The solution file (`eShopLegacyNTier.sln`) references one project:

| Project | Original framework | Migrated to |
|---|---|---|
| `src/eShopWCFService/eShopWCFService.csproj` | .NET Framework 4.6.1 (`net461`) | `net10.0` |

`src/eShopWinForms/eShopWinForms.csproj` exists on disk but is **not referenced by the
solution file** and was therefore out of scope for this migration cycle.

---

## What was migrated (eShopWCFService)

The AWS Transform Custom CLI completed the migration in full. This cycle assessed the output
and confirmed zero build errors or warnings remain.

### Project file

- Old-style `.csproj` → SDK-style `Microsoft.NET.Sdk.Web`
- `TargetFrameworkVersion v4.6.1` → `<TargetFramework>net10.0</TargetFramework>`
- `packages.config` removed; all references converted to `<PackageReference>`
- `Properties/AssemblyInfo.cs` removed (SDK generates assembly attributes automatically)

### WCF → CoreWCF

| Before | After |
|---|---|
| `System.ServiceModel` (WCF) hosted by IIS/IIS Express | `CoreWCF.Http` + `CoreWCF.Primitives` 1.9.1 self-hosted on Kestrel |
| `Web.config` `<system.serviceModel>` | `Program.cs` `UseServiceModel(...)` |
| `<endpoint ... binding="basicHttpBinding">` | `AddServiceEndpoint<CatalogService, ICatalogService>(new BasicHttpBinding(), "/CatalogService.svc")` |
| `<serviceMetadata httpGetEnabled="true"/>` | `ServiceMetadataBehavior.HttpGetEnabled = true` |
| `<endpoint address="mex" ...>` | **No CoreWCF 1.9.1 equivalent** — WSDL HTTP GET (`?wsdl`) preserved instead |

The SOAP wire contract is unchanged: all 10 `[OperationContract]` methods on `ICatalogService`
are intact. Existing WCF clients (`eShopWinForms`) need no proxy regeneration.

### Entity Framework 6 → EF Core 10

| Before | After |
|---|---|
| `EntityFramework` 6.1.3 (`packages.config`) | `Microsoft.EntityFrameworkCore.SqlServer` 10.0.0 |
| `DbContext` from `System.Data.Entity` | `DbContext` from `Microsoft.EntityFrameworkCore` |
| `Database.SetInitializer(new CatalogDBInitializer())` | `CatalogDBInitializer.EnsureInitialized` + `Database.EnsureCreated()` |
| EF6 `HasPrecision(19, 4)` on `money` column | Removed; `[Column(TypeName = "money")]` alone is correct for EF Core |
| Parameterless `EntityModel()` constructor | Single `DbContextOptions<EntityModel>` constructor; DI-only |
| Static `ConfigurationManager` connection string | `IConfiguration` injected in `Program.cs` |

### Configuration

| Before | After |
|---|---|
| `Web.config` `<connectionStrings>` | `appsettings.json` `ConnectionStrings:EntityModel` |
| `Web.Debug.config` / `Web.Release.config` (scaffold only) | Removed; use `appsettings.{Environment}.json` instead |
| `<system.web>`, `<system.webServer>`, `<entityFramework>` | Removed |
| `Global.asax` app lifecycle | `Program.cs` ASP.NET Core host builder |

### Host / listen address

The legacy IIS Express port (`62314`) is preserved in `appsettings.json`
`Kestrel:Endpoints:Http:Url` so that the `eShopWinForms` client endpoint configuration
(`App.config`) continues to work without changes.

### Packages

| Package | Version |
|---|---|
| `CoreWCF.Http` | 1.9.1 |
| `CoreWCF.Primitives` | 1.9.1 |
| `Microsoft.EntityFrameworkCore.SqlServer` | 10.0.0 |
| `System.Security.Cryptography.Pkcs` | 10.0.14 (pinned; lifts transitive dep above CVE-2026-84008 / CVE-2026-70162) |

---

## Known limitations / Next steps

- **`mex` endpoint absent**: CoreWCF 1.9.1 cannot host a WS-MetadataExchange endpoint.
  Metadata discovery is fully functional via HTTP GET (`?wsdl`). If a future CoreWCF release
  adds mex support, add the endpoint back in `Program.cs`.
- **`httpsGetEnabled` is `false`**: The host listens on HTTP only. To enable HTTPS metadata,
  add an `Https` Kestrel endpoint in `appsettings.json` and flip `HttpsGetEnabled = true`
  in `Program.cs`.
- **`eShopWinForms` not migrated**: The WinForms project targets `v4.7` and uses EF6, legacy
  WCF client proxies, and old-style `.csproj`. It is not in the solution file for this cycle.
  A future cycle should add it to the solution, convert it to SDK-style `net10.0-windows`, and
  replace the `System.ServiceModel` client proxy with a CoreWCF or `HttpClient`-based client.
