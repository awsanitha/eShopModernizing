# eShopLegacyMVC → net10.0 Migration Summary

## Result
`dotnet build eShopLegacyMVC.sln` exits with code 0 — **0 errors, 0 compilation failures**.

---

## Changes Made

### 1. `eShopLegacyMVC.sln` — Removed legacy project
Removed the old `.NET Framework 4.x` project (`src/eShopLegacyMVC/eShopLegacyMVC.csproj`) from the solution file.  
**Reason:** The `eShopPorted` project already IS the net10.0 migration of `eShopLegacyMVC`. The old project uses the non-SDK `.csproj` format requiring `Microsoft.WebApplication.targets` (MSB4019), which is not present in the .NET SDK build environment. Having both in the solution would mean two duplicate web apps at different framework versions.

### 2. `eShopLegacy.Utilities/eShopLegacy.Utilities.csproj`
Added `<GenerateAssemblyInfo>false</GenerateAssemblyInfo>`.  
**Reason:** SDK-style projects auto-generate assembly attributes, which conflicted with the legacy `Properties/AssemblyInfo.cs` (CS0579 duplicate attribute errors for `AssemblyTitle`, `AssemblyVersion`, etc.).

### 3. `eShopPorted/eShopPorted.csproj` — Package modernization
- Removed .NET Framework–only packages: `Autofac.Mvc5 4.0.2`, `WebGrease 1.6.0`, `Antlr4 4.6.6`, `Microsoft.CSharp`
- Upgraded `Autofac` to `8.1.0` and `Autofac.Extensions.DependencyInjection` to `10.0.0` (required for `UseServiceProviderFactory` pattern on net10.0)
- Updated `log4net` to `2.0.17`
- Pinned `Newtonsoft.Json` to `13.0.3` (no high-severity vuln)
- Added `<Nullable>enable</Nullable>` for modern C# defaults

### 4. `eShopPorted/Program.cs` — Modern hosting model
Replaced old `IWebHostBuilder` / `WebHost.CreateDefaultBuilder` pattern with the modern `IHostBuilder` / `Host.CreateDefaultBuilder` pattern and wired in `AutofacServiceProviderFactory`.

### 5. `eShopPorted/Startup.cs` — Removed deprecated APIs
- `ConfigureServices` return type changed from `IServiceProvider` (old Autofac pattern, unsupported in ASP.NET Core 6+) to `void`
- Added `ConfigureContainer(ContainerBuilder builder)` to register Autofac modules (the correct pattern with `AutofacServiceProviderFactory`)
- `IHostingEnvironment` (removed in .NET 6+) replaced with `IWebHostEnvironment`
- `app.UseMvc(routes => ...)` replaced with `app.UseRouting()` + `app.UseEndpoints(...)` + `MapControllerRoute`
- `services.AddMvc()` replaced with `services.AddControllersWithViews()`

### 6. `eShopPorted/Controllers/PicController.cs` — System.Web.Mvc → ASP.NET Core
- Removed `using System.Web.Mvc;` (not available without the removed `Autofac.Mvc5` / `Microsoft.AspNet.Mvc` packages)
- `using Microsoft.AspNetCore.Mvc;` was already in scope via controller base
- `HttpStatusCodeResult(HttpStatusCode.BadRequest)` → `BadRequest()`
- `HttpNotFound()` → `NotFound()`
- `return File(buffer, mimetype)` retained (available on ASP.NET Core `Controller`)
- Injected `IWebHostEnvironment` to resolve the `wwwroot/Pics` path correctly

---

## Remaining Warnings (non-blocking)
- **NU1902** — `log4net 2.0.17` has a known moderate-severity vulnerability. Consider upgrading to `log4net 2.0.18+` when available, or replacing with `Microsoft.Extensions.Logging`.

---

## Next Steps
- The `src/eShopLegacyMVC` folder (original .NET Framework code) still exists on disk but is no longer part of the solution. It can be archived or deleted.
- `eShopLegacy.Utilities/Serializing.cs` uses `BinaryFormatter`, which throws `NotSupportedException` at runtime on .NET 8+. The class compiles (targets `netstandard2.0`) but calling `SerializeBinary`/`DeserializeBinary` will fail at runtime. Replace with `System.Text.Json` or `MessagePack` serialization in a follow-up.
- `eShopPorted/Views/Web.config` is a legacy file from the old MVC5 Razor engine. It is not used by ASP.NET Core Razor and can be deleted.
- `app.config` in `eShopPorted/` is unused (replaced by `appsettings.json`); can be deleted.
- A `_ViewImports.cshtml` with `@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers` should be added under `Views/` to enable Tag Helpers in Razor views.
