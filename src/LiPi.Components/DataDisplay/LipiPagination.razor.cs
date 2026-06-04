// SPEC: docs/00-COMPONENTS/2.8/03-LipiPagination-Spec.md §3 (API), §3.5 (page-number algorithm),
//       §3.6 (programmatic API), §4 (states), §5 (a11y)
// PHASE: 2.8 Data Display — Stage 7 (LipiPagination)
// AMEND: docs/CHANGE-LOG.md A50
//
// Stateless pager: render is a pure function of parameters (§11.4). The only local state is
// the jump-to-page input's typed-but-unsubmitted text. Page-size dropdown composes
// LipiSelect<PageSizeOption>; nav buttons compose LipiButton (both package-clean, A43/A49).

using Microsoft.AspNetCore.Components;
using LiPi.Components.Internal.Algorithms;

namespace LiPi.Components.DataDisplay;

public partial class LipiPagination : ComponentBase
{
    // ── Required (§3.1) ───────────────────────────────────────────────────
    [Parameter, EditorRequired] public int TotalCount { get; set; }
    [Parameter, EditorRequired] public int PageSize { get; set; }
    [Parameter, EditorRequired] public int CurrentPage { get; set; }
    [Parameter] public EventCallback<int> CurrentPageChanged { get; set; }
    [Parameter] public EventCallback<int> PageSizeChanged { get; set; }

    // ── Optional (§3.2) ───────────────────────────────────────────────────
    [Parameter] public PaginationVariant Variant { get; set; } = PaginationVariant.Full;
    [Parameter] public IReadOnlyList<PageSizeOption> PageSizeOptions { get; set; } = PaginationDefaults.PageSizeOptions;

    /// <summary>Pages shown on each side of the current page in the number window. Default 1 (LP-14).</summary>
    [Parameter] public int Siblings { get; set; } = 1;

    /// <summary>Pages shown at each end (start + end). Default 1 (LP-14).</summary>
    [Parameter] public int BoundaryCount { get; set; } = 1;

    [Parameter] public bool ShowPageSize { get; set; } = true;
    [Parameter] public bool ShowRowCount { get; set; } = true;
    [Parameter] public bool ShowPageNav { get; set; } = true;
    [Parameter] public bool ShowFirstLast { get; set; } = true;
    [Parameter] public bool ShowJumpToPage { get; set; } = false;

    // ── Visual style (A50.1) — three independent axes, all token-overridable ──
    /// <summary>Page-number cell affordance. Default <see cref="PaginationCellStyle.Bordered"/>.</summary>
    [Parameter] public PaginationCellStyle CellStyle { get; set; } = PaginationCellStyle.Bordered;

    /// <summary>Active-page signal. Default <see cref="PaginationActiveStyle.Solid"/>.</summary>
    [Parameter] public PaginationActiveStyle ActiveStyle { get; set; } = PaginationActiveStyle.Solid;

    /// <summary>Nav chevron border treatment. Default <see cref="PaginationChevronStyle.Auto"/> (follows CellStyle).</summary>
    [Parameter] public PaginationChevronStyle ChevronStyle { get; set; } = PaginationChevronStyle.Auto;

    // ── Orientation (Stage 7 / LP-15 Q1) ──────────────────────────────────
    /// <summary>Bar orientation. Horizontal default; Vertical for LipiTable Left/Right side placement.</summary>
    [Parameter] public PaginationOrientation Orientation { get; set; } = PaginationOrientation.Horizontal;

    /// <summary>Horizontal alignment of the pager zones. Default <see cref="PaginationAlign.SpaceBetween"/>.</summary>
    [Parameter] public PaginationAlign Align { get; set; } = PaginationAlign.SpaceBetween;

    [Parameter] public int ServerSideAllCap { get; set; } = 1000;
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string? AriaLabel { get; set; }
    [Parameter] public string? Class { get; set; }
    [Parameter] public string? Style { get; set; }

    // ── Local UI state (jump-to-page typed value) ─────────────────────────
    private string _jumpText = string.Empty;

    // ── Derived ───────────────────────────────────────────────────────────
    private int TotalPages => PaginationMath.ComputeTotalPages(TotalCount, PageSize);

    private bool IsFirstPage => CurrentPage <= 1;
    private bool IsLastPage => CurrentPage >= TotalPages;

    // ── Lifecycle: snap an out-of-range page (§3.5 edge, §8.2.4 parent parallels) ──
    protected override async Task OnParametersSetAsync()
    {
        if (TotalCount > 0 && CurrentPage > TotalPages)
        {
            await GoToPageAsync(TotalPages);   // silently snap + fire
        }
        else if (CurrentPage < 1 && TotalCount > 0)
        {
            await GoToPageAsync(1);
        }
    }

    // ── §3.5 — page-number rendering: delegated to the pure PaginationMath helper (LP-14) ──
    private IReadOnlyList<int?> PageSlots =>
        PaginationMath.ComputePageSlots(CurrentPage, TotalPages, Siblings, BoundaryCount);

    // ── Page change handlers ──────────────────────────────────────────────
    private async Task SetPageAsync(int page)
    {
        if (Disabled) return;
        int clamped = Math.Clamp(page, 1, Math.Max(1, TotalPages));
        if (clamped == CurrentPage) return;
        CurrentPage = clamped;
        if (CurrentPageChanged.HasDelegate) await CurrentPageChanged.InvokeAsync(clamped);
    }

    private Task OnFirst()    => SetPageAsync(1);
    private Task OnPrev()     => SetPageAsync(CurrentPage - 1);
    private Task OnNext()     => SetPageAsync(CurrentPage + 1);
    private Task OnLast()     => SetPageAsync(TotalPages);
    private Task OnPageNum(int p) => SetPageAsync(p);

    private async Task OnSizeValueChanged(int newSize)
    {
        if (Disabled || newSize == PageSize) return;
        PageSize = newSize;
        if (PageSizeChanged.HasDelegate) await PageSizeChanged.InvokeAsync(newSize);
    }

    private async Task OnJumpSubmit()
    {
        if (int.TryParse(_jumpText, out var p))
        {
            await SetPageAsync(p);
        }
        _jumpText = string.Empty;
    }

    private async Task OnJumpKeyDown(Microsoft.AspNetCore.Components.Web.KeyboardEventArgs e)
    {
        if (e.Key == "Enter") await OnJumpSubmit();
    }

    // ── Programmatic API (§3.6) ───────────────────────────────────────────
    public ValueTask GoToPageAsync(int page)     { _ = SetPageAsync(page); return ValueTask.CompletedTask; }
    public ValueTask GoToFirstAsync()            { _ = SetPageAsync(1); return ValueTask.CompletedTask; }
    public ValueTask GoToLastAsync()             { _ = SetPageAsync(TotalPages); return ValueTask.CompletedTask; }
    public ValueTask GoToNextAsync()             { _ = SetPageAsync(CurrentPage + 1); return ValueTask.CompletedTask; }
    public ValueTask GoToPreviousAsync()         { _ = SetPageAsync(CurrentPage - 1); return ValueTask.CompletedTask; }
    public async ValueTask SetPageSizeAsync(int size) => await OnSizeValueChanged(size);

    /// <summary>Force a re-render (§3.7) — rarely needed; render is a pure function of params.</summary>
    public ValueTask RefreshAsync() { StateHasChanged(); return ValueTask.CompletedTask; }

    // ── A11y label ────────────────────────────────────────────────────────
    private string NavAriaLabel => AriaLabel ?? "Pagination";

    // ── Visual-style derivation (A50.1) ───────────────────────────────────
    /// <summary>Resolved: should nav chevrons render as bordered (Secondary) buttons?
    /// Auto follows CellStyle (bordered grid → bordered chevrons).</summary>
    private bool ChevronsBordered => ChevronStyle switch
    {
        PaginationChevronStyle.Bordered => true,
        PaginationChevronStyle.Ghost    => false,
        _ => CellStyle == PaginationCellStyle.Bordered   // Auto
    };

    /// <summary>The space-separated style classes appended to .lipi-pagination.</summary>
    private string StyleClasses =>
        $"lipi-pagination--cell-{CellStyle.ToString().ToLowerInvariant()} "
      + $"lipi-pagination--active-{ActiveStyle.ToString().ToLowerInvariant()}";

    // ── Orientation (Stage 7 side-pager) ──────────────────────────────────
    private bool IsVertical => Orientation == PaginationOrientation.Vertical;

    /// <summary>Orientation modifier appended to the root nav class.</summary>
    private string OrientationClass =>
        IsVertical ? "lipi-pagination--vertical" : "lipi-pagination--horizontal";

    /// <summary>Alignment modifier appended to the root nav class (horizontal pager only).</summary>
    private string AlignClass => Align switch
    {
        PaginationAlign.Start   => "lipi-pagination--align-start",
        PaginationAlign.Center  => "lipi-pagination--align-center",
        PaginationAlign.End     => "lipi-pagination--align-end",
        _                       => "lipi-pagination--align-between"
    };

    /// <summary>Prev chevron icon — points "back": left when horizontal, up when vertical.</summary>
    private string PrevIcon => IsVertical ? "chevron-up" : "chevron-left";
    /// <summary>Next chevron icon — points "forward": right when horizontal, down when vertical.</summary>
    private string NextIcon => IsVertical ? "chevron-down" : "chevron-right";
    // First/Last icons (Full variant only). Full NEVER renders vertically (side placement
    // downgrades Full -> Compact), so these stay horizontal double-chevrons. NOTE: LiPicons
    // has no vertical double-chevron (chevrons-up/down absent in v1.0.4), so a vertical Full
    // pager is intentionally unsupported — Compact (single chevrons) is the vertical path.
    private string FirstIcon => "chevrons-left";
    private string LastIcon  => "chevrons-right";

    /// <summary>Compact/Minimal chevron variant. Vertical side-pagers get bordered (Secondary)
    /// buttons so the narrow column reads as a control (matches the Option-A mockup); horizontal
    /// Compact keeps the quieter Ghost look. Full uses ChevronsBordered (its own A50.1 axis).</summary>
    private LiPi.Components.ButtonVariant CompactChevronVariant =>
        IsVertical ? LiPi.Components.ButtonVariant.Secondary : LiPi.Components.ButtonVariant.Ghost;
}
