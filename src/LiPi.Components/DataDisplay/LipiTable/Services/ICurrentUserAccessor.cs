// SPEC: docs/00-COMPONENTS/2.8/01-LipiTable-Spec.md
//   §25.3.4 (Consuming-app-provides-implementation — auth resolution)
// CROSS-REF: CHANGE-LOG.md A38 (Option C architecture)
// PHASE: 2.8 Data Display — Stage 1B (services foundation)
// COMPONENT: LipiTable persistence
//
// Auth resolution abstraction. The LiPi component library can never know about the
// consuming app's authentication scheme (cookie, JWT, mTLS, claims layout, etc.), but
// the persistence default impl needs the current user's GUID to scope preferences.
//
// Consumers implement this against their auth framework:
//   • LiPi HIS (Stage 1C): BlazorCurrentUserAccessor injects AuthenticationStateProvider,
//     extracts the user GUID from the "sub" or NameIdentifier claim.
//   • Other Blazor apps: same pattern; different claim shape.
//   • Non-Blazor consumers: implement against their own auth context.
//
// Method semantics:
//   • Returns null when no user is authenticated (LipiTable treats this as "no persistence
//     possible" and falls back to in-memory-only behavior — never throws).
//   • Returns a stable GUID per user (the same user must always resolve to the same GUID
//     so preferences round-trip).
//   • Uses ValueTask because most consumer implementations can resolve synchronously
//     from cached claims after the first call.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace LiPi.Components.DataDisplay;

/// <summary>
/// Resolves the current request/circuit user's stable GUID identity. Returns null when
/// no user is authenticated. LipiTable falls back gracefully when null is returned.
/// </summary>
public interface ICurrentUserAccessor
{
    /// <summary>
    /// Resolve the current user's GUID. Returns null when no user is authenticated.
    /// Implementations should be idempotent and cheap on repeat calls (the persistence
    /// service may call this several times per circuit).
    /// </summary>
    ValueTask<Guid?> GetUserIdAsync(CancellationToken ct = default);
}
