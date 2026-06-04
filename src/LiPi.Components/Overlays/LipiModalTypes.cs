// SPEC:  docs/01-LipiModal-Spec.md §4
// PHASE: 2.6.2 — Overlay Surfaces
// AMEND: docs/CHANGE-LOG.md A30 — file rebuilt from spec after loss-of-source.
//        Spec §4 is authoritative; this file mirrors it verbatim.
//
// NAMING: The spec calls this LipiModalTypes.cs (not LipiOverlayTypes.cs)
//         — same pattern as LipiDrawerTypes.cs / LipiDynamicTabsTypes.cs.
//
// USAGE:
//   ModalSize        — <LipiModal Size="..."> + LipiModalService.ShowAsync(size:)
//   ModalIconColor   — <LipiModal IconColor="...">
//   ModalIntent      — <LipiModal Intent="..."> — cascades auto-defaults (see spec §10)
//   ModalAnimation   — <LipiModal Animation="...">. Critical Intent forces None.
//   ModalFooterAlign — <LipiModal FooterAlign="...">; matches CSS classes
//                      .lipi-modal-footer-start / -space-between in lipi-overlays.css
//   ConfirmIntent    — LipiModalService.ConfirmAsync(intent:)
//   AlertIntent      — LipiModalService.AlertAsync(intent:)

namespace LiPi.Components.Overlays;

public enum ModalSize
{
    Compact,    // 400px — confirmations, simple inputs
    Standard,   // 520px — forms, most use cases (default)
    Wide,       // 680px — complex content, pickers, duplicate detection
    Fullscreen  // 95vw — wizards, previews, large datasets
}

public enum ModalIconColor
{
    None,
    Info,       // blue tint
    Success,    // green tint
    Warning,    // amber tint
    Danger,     // red tint
    Critical    // dark red tint (text color reversed — see spec §7 IconColor table)
}

public enum ModalIntent
{
    Default,       // form, info, generic
    Confirmation,  // auto: Size=Compact, FooterAlign=End
    Alert,         // auto: Size=Compact, FooterAlign=End
    Wizard,        // auto: CloseOnEscape=false, FooterAlign=SpaceBetween
    Preview,       // auto: Size=Fullscreen, body padding=0
    Progress       // auto: ShowCloseButton=false, CloseOnEscape=false,
                   //       CloseOnBackdrop=false, Animate=false
}

public enum ModalAnimation
{
    None,         // instant — forced when Intent=Critical
    Fade,         // 150ms opacity
    FadeSlide,    // 200ms opacity + 20px translateY (default)
    FadeScale     // 200ms opacity + scale 0.95 → 1.0
}

public enum ModalFooterAlign
{
    Start,        // left-aligned (back/cancel only)
    End,          // right-aligned — default
    SpaceBetween  // wizards — Back left, Next right
}

public enum ConfirmIntent
{
    Default,    // generic yes/no
    Danger,     // delete/destructive — red primary button
    Warning,    // overwrite/replace — amber primary button
    Critical    // mandatory — no Cancel, primary acts as "I understand"
}

public enum AlertIntent
{
    Info,
    Success,
    Warning,
    Danger,
    Critical
}
