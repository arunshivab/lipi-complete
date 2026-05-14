// SPEC:  docs/00-Phase2.6.2-Overview.md — Shared infrastructure
// PHASE: 2.6.2 — Overlay Surfaces

namespace LiPi.Web.Services;

/// <summary>
/// Reference-counted body scroll lock.
/// First <see cref="LockAsync"/> call adds <c>overflow: hidden</c> to
/// <c>&lt;body&gt;</c>. Each subsequent call increments the count.
/// <see cref="UnlockAsync"/> decrements; when the count reaches zero
/// the overflow restriction is removed.
/// <para>
/// This prevents the modal+drawer combination from double-locking:
/// opening a modal while a drawer is already open still locks only once
/// and requires two Unlock calls to release.
/// </para>
/// </summary>
public interface IScrollLockService
{
    ValueTask LockAsync();
    ValueTask UnlockAsync();
}
