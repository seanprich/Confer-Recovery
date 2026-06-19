# Desktop API Client (Volunteer Guide)

This project generates the desktop API client from the server OpenAPI document at build time.

## Source of truth

- Server endpoints and DTOs in src/Server
- Generated OpenAPI document in src/Server/obj/<Configuration>/net10.0/EndpointInfo/ConferRecovery.Server.json
- Generated client code in src/Desktop/ConferRecovery.Desktop.Infrastructure/Generated/ConferApiClient.g.cs

## Important rules

1. Do not hand-edit Generated/ConferApiClient.g.cs.
2. Add or modify endpoints in server controllers, then rebuild.
3. Keep endpoint names explicit via the Name property on Http* attributes so generated method names stay readable and stable.
4. Prefer friendly extension methods from Generated/ConferApiClient.FriendlyExtensions.cs when calling generated clients from app code.

## Build behavior

Building the solution runs NSwag in the Desktop Infrastructure project and refreshes the generated client automatically.

Command:

dotnet build ConferRecovery.slnx

## Where to write custom code

- Wrap generated client calls in adapters under src/Desktop/ConferRecovery.Desktop.Infrastructure/Auth and future feature folders.
- Keep app-facing interfaces in src/Desktop/ConferRecovery.Desktop.Application.
- Keep immutable app contracts in src/Desktop/ConferRecovery.Desktop.Contracts.

## Friendly method names

Generated names can occasionally include suffixes (for example Status2Async) due to OpenAPI operation naming collisions.

Use friendly aliases from ConferApiClient.FriendlyExtensions.cs instead:

- IMembersClient.UpdateMemberStatusAsync(...)
- IMembersClient.UpdateMemberRoleAsync(...)
- IRoomsClient.StartRoomAsync(...)
- IRoomsClient.JoinRoomAsync(...)
