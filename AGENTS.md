# AGENTS.md

This file captures practical lessons learned while bootstrapping the desktop client and test infrastructure.

## Lessons learned

1. Keep generated code generated
- Do not edit `src/Desktop/ConferRecovery.Desktop.Infrastructure/Generated/ConferApiClient.g.cs` directly.
- Put human-authored wrappers in separate files (for example `ConferApiClient.FriendlyExtensions.cs` and feature adapters).

2. Use a stable API surface for app code
- Application code should depend on interfaces in `src/Desktop/ConferRecovery.Desktop.Application`.
- Infrastructure adapters map generated DTOs to immutable contract records in `src/Desktop/ConferRecovery.Desktop.Contracts`.

3. Optimize for contributor readability
- Generated names can occasionally be awkward (for example `Status2Async`).
- Hide those names behind friendly extension methods and adapter interfaces.

4. Keep DI composition predictable
- Register app services in `AddDesktopApplication(...)`.
- Register infrastructure and generated clients in `AddDesktopInfrastructure(...)`.
- Keep WPF `App` startup thin and declarative.

5. NSwag generation gotchas
- In this environment, NSwag executable resolution is sensitive; use package-provided MSBuild properties.
- Build-time generation should first build server OpenAPI output, then generate desktop client.

6. WPF template nuance
- `dotnet new wpf` accepts `-f net10.0` in this environment; generated project still targets `net10.0-windows`.

7. Test strategy that saves volunteer time
- Prioritize adapter mapping tests and branch handling (`404`, `401`, success path).
- Test DI extension methods so registration regressions are caught quickly.
- Keep tests focused and fast; avoid over-coupling tests to generated implementation details.

8. Consistency rules
- Prefer immutable `record` types for contracts and command-style inputs.
- Keep business logic out of UI code-behind; route through application services.
- Favor one-way mapping: generated DTO -> contract model.

## Contributor checklist

1. Add or change server endpoints in controllers/DTOs.
2. Build solution to regenerate OpenAPI client.
3. Update friendly extensions/adapters if signatures changed.
4. Add tests for new branches and mappings.
5. Run full tests before PR.
