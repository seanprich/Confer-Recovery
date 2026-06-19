# Desktop Application Plan for Confer Recovery

## Recommendation Summary

Based on the current solution and product direction, the recommended first desktop client is:

1. WPF on .NET 10 for v1
2. Keep the app architecture UI-framework-agnostic so migration to WinUI 3 remains possible later

## Why WPF First

1. Fastest path to a stable Windows desktop app for chapter and admin workflows
2. Mature tooling and ecosystem with lower delivery risk than newer stacks
3. Strong fit for role-driven screens, consent workflows, and operational UX
4. Aligns with backend-first security model already implemented in src/Server

## Platform Comparison

### WPF (.NET 10) - Recommended

- Best blend of maturity, maintainability, and MVVM architecture support
- Good for complex admin and policy-heavy workflows
- Lowest implementation risk for v1

### WinUI 3 (Windows App SDK) - Good Future Option

- Newer Microsoft desktop direction with modern Windows UI
- Better visual modernity, but higher risk and smaller ecosystem for fast line-of-business delivery

### WinForms - Not Recommended for v1

- Fast for basic CRUD, but harder to scale into richer, role-sensitive UX
- Less ideal for long-term architecture and modern interaction patterns

### UWP - Do Not Choose

- Legacy path, superseded by Windows App SDK / WinUI 3

### .NET MAUI - Only if Mobile Is Required Soon

- Adds complexity if desktop-only is the immediate goal
- Better as a phase 2 if iOS/Android becomes a requirement

## Integration Plan with src/Server Backend

The backend in src/Server should remain the source of truth for all security and policy decisions.

### Desktop Client Responsibilities

1. Authentication and token lifecycle UX
2. Role-appropriate chapter/member/room management screens
3. Consent acknowledgment UX before meeting join
4. Connectivity and environment diagnostics

### Server Responsibilities (unchanged)

1. Role and permission enforcement
2. Consent gating and meeting join token issuance
3. Audit immutability
4. Secret protection and telemetry policy

## A/V Strategy for v1

Use WebView2 for the meeting surface (LiveKit web client path), while WPF handles native shell and secure workflows.

Benefits:

1. Avoids heavy native WebRTC complexity in v1
2. Leverages existing LiveKit token model from the API
3. Speeds delivery while preserving meeting quality and policy enforcement

## Suggested New Projects in This Solution

1. src/Desktop/ConferRecovery.Desktop
- WPF app shell, navigation, and views

2. src/Desktop/ConferRecovery.Desktop.Application
- Use cases, orchestration, application services

3. src/Desktop/ConferRecovery.Desktop.Infrastructure
- HTTP clients, token handling, retries, local persistence boundaries

4. src/Desktop/ConferRecovery.Desktop.Contracts
- Client-side contracts and model mapping support

## Privacy and Security UX Requirements

The desktop app should explicitly support:

1. Session expiration and re-authentication behavior
2. Consent-before-join flow visibility
3. Role-based action visibility (show/hide/disable by role)
4. No plaintext sensitive secret storage
5. Clear messaging that deployment is local and privacy-first

## Final Direction

Build the first desktop release using WPF on .NET 10, MVVM architecture, and a thin-client model backed by src/Server.

Then evaluate WinUI 3 only after v1 stabilization, if modern Windows-native styling becomes a strategic priority.
