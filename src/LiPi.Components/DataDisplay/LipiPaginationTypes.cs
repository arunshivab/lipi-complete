// SPEC: docs/00-COMPONENTS/2.8/03-LipiPagination-Spec.md §2.1 (PaginationVariant),
//       §3.3 (PageSizeOption + DefaultPageSizeOptions), §3.5 (page-number algorithm)
// PHASE: 2.8 Data Display — Stage 7 (LipiPagination standalone component)
// AMEND: docs/CHANGE-LOG.md A50, A50.1 (visual-style axes: CellStyle/ActiveStyle/ChevronStyle);
//        Stage 7: +PaginationOrientation, +PaginationRange, +PageSizeOptions helper.
//        PageChangeReason is NOT here — it lives in LipiTable Contexts.cs (LP-23, redistributable isolation).
//        FLAGS LP-0 (Full/Compact/Minimal names), LP-11 (file count),
//        LP-12-REVERSED (composes LipiSelect, not native <select>)
//
// Types for LipiPagination. Lives in LiPi.Components.DataDisplay — redistributable,
// no HIS coupling. PageSizeOption.ToString() returns DisplayLabel so a
// LipiSelect<PageSizeOption> renders "All" for the int.MaxValue entry (LipiSelect's
// ItemLabel uses ToString()), decoupling display from value without LipiCombobox.

using System.Linq;

namespace LiPi.Components.DataDisplay;

/// <summary>
/// Visual layout of the pager. Locked names per FLAGS LP-0 (reconciled from
/// "Standard/Compact/Mini" across the source specs).
/// </summary>
public enum PaginationVariant
{
    /// <summary>Page-size + row count + page-number buttons + first/last + jump-to-page. Default.</summary>
    Full,

    /// <summary>Page-size dropdown + prev/next + "Page X of M".</summary>
    Compact,

    /// <summary>Prev/next chevrons only. No counts, no size selector.</summary>
    Minimal
}

/// <summary>
/// Page-number cell affordance (visual style axis 1). Independent of <see cref="PaginationActiveStyle"/>
/// and <see cref="PaginationChevronStyle"/>, so a consumer composes any combination.
/// </summary>
public enum PaginationCellStyle
{
    /// <summary>Each page number sits in a 1px-bordered cell — a button grid (AntDesign/Material). Default.</summary>
    Bordered,

    /// <summary>Page numbers are borderless text; only the active page carries any fill/tint.</summary>
    Borderless
}

/// <summary>
/// How the active (current) page is signalled (visual style axis 2). Independent of
/// <see cref="PaginationCellStyle"/>.
/// </summary>
public enum PaginationActiveStyle
{
    /// <summary>Active page = solid primary fill + inverse text. Strongest signal. Default.</summary>
    Solid,

    /// <summary>Active page = soft primary tint + primary-colored text. Quieter, clinical feel.</summary>
    Tint
}

/// <summary>
/// Nav chevron (First/Prev/Next/Last) border treatment (visual style axis 3).
/// <see cref="Auto"/> follows <see cref="PaginationCellStyle"/> so chevrons match the grid
/// without the consumer setting it explicitly — override only when you want them to differ.
/// </summary>
public enum PaginationChevronStyle
{
    /// <summary>Follow CellStyle: bordered chevrons when cells are bordered, ghost when borderless. Default.</summary>
    Auto,

    /// <summary>Force bordered (Secondary) chevrons regardless of CellStyle.</summary>
    Bordered,

    /// <summary>Force borderless (Ghost) chevrons regardless of CellStyle.</summary>
    Ghost
}

/// <summary>
/// Orientation of the pager (Stage 7 / LP-15 Q1 amendment). Horizontal is the default;
/// Vertical is used by LipiTable Left/Right placement, where the pager is a narrow side column.
/// Vertical is compatible with Compact / Minimal variants only (Full auto-downgrades — owned by LipiTable).
/// </summary>
public enum PaginationOrientation
{
    /// <summary>Standard left-to-right bar. Default.</summary>
    Horizontal,

    /// <summary>Stacked top-to-bottom for side (Left/Right) placement.</summary>
    Vertical
}

/// <summary>
/// Horizontal alignment of the pager's zones (Stage 7). SpaceBetween (default) pins the
/// left zone (page-size + row count) to the start and pushes the nav to the end — the data-grid
/// standard. Start packs everything to the start (useful in narrow contexts); Center centers all
/// zones; End packs everything to the end. Applies to the horizontal pager; vertical (side) pagers
/// stack regardless.
/// </summary>
public enum PaginationAlign
{
    /// <summary>Left zone start, nav end (default — the standard data-grid layout).</summary>
    SpaceBetween,
    /// <summary>All zones packed to the start.</summary>
    Start,
    /// <summary>All zones centered.</summary>
    Center,
    /// <summary>All zones packed to the end.</summary>
    End
}

/// <summary>
/// The current visible row range + page context (Stage 7 / LP-21). Passed to a consumer's
/// <c>RowCountTemplate</c> so they can render any phrasing ("51–75 of 487", "Page 3/20",
/// "Page 3 (items 51-75 of 487)"). All five members are included now: cheap to add, breaking
/// to add later once consumer code depends on the record shape.
/// </summary>
public sealed record PaginationRange(
    int Start,        // 1-based, inclusive — first item index on the current page
    int End,          // 1-based, inclusive — last item index on the current page
    int Total,        // total item count across all pages
    int CurrentPage,  // 1-based page number
    int PageSize      // current page size
);

/// <summary>
/// A page-size choice: the integer <paramref name="Value"/> sent on selection, and a
/// <paramref name="DisplayLabel"/> shown in the dropdown. <c>int.MaxValue</c> conventionally
/// means "All". <see cref="ToString"/> returns the label so a <c>LipiSelect&lt;PageSizeOption&gt;</c>
/// (whose ItemLabel uses ToString) shows "All" rather than 2147483647.
/// </summary>
public sealed record PageSizeOption(int Value, string DisplayLabel)
{
    /// <summary>Label-as-text, so LipiSelect's ToString-based ItemLabel renders DisplayLabel.</summary>
    public override string ToString() => DisplayLabel;
}

/// <summary>
/// Default page-size options: 10 / 25 / 50 / 100 / 200 / All (All = int.MaxValue).
/// Per §3.3. Callers may supply their own list.
/// </summary>
public static class PaginationDefaults
{
    /// <summary>The "All" sentinel value — renders all rows on one page (parent caps in server mode).</summary>
    public const int AllValue = int.MaxValue;

    /// <summary>
    /// Default options shown in the page-size dropdown.
    /// NOTE: deviates from spec §3.3 (which starts at 10). Per project owner, the default
    /// starts at 5 with a 15 tier added — clinical lists are often short, so 5/10/15 give
    /// finer low-end control before the 25/50/100/200 jumps. Callers may still supply their own.
    /// </summary>
    public static readonly IReadOnlyList<PageSizeOption> PageSizeOptions = new[]
    {
        new PageSizeOption(5,   "5"),
        new PageSizeOption(10,  "10"),
        new PageSizeOption(15,  "15"),
        new PageSizeOption(25,  "25"),
        new PageSizeOption(50,  "50"),
        new PageSizeOption(100, "100"),
        new PageSizeOption(200, "200"),
        new PageSizeOption(AllValue, "All")
    };
}


/// <summary>
/// Construction helpers for <see cref="PageSizeOption"/> lists (Stage 7 / LP-20). Lets a consumer
/// build page-size lists from plain ints without weakening the public type — the option type stays
/// <c>IReadOnlyList&lt;PageSizeOption&gt;</c> everywhere, so the "All" sentinel keeps its label.
/// (Distinct container from <see cref="PaginationDefaults.PageSizeOptions"/>, which is a member, not a type.)
/// </summary>
public static class PageSizeOptions
{
    /// <summary>The "All" option — value <c>int.MaxValue</c>, label "All".</summary>
    public static readonly PageSizeOption All = new(int.MaxValue, "All");

    /// <summary>A numeric option whose label is the number itself.</summary>
    public static PageSizeOption Of(int value) => new(value, value.ToString());

    /// <summary>A numeric option with a custom label (e.g. "25 per page").</summary>
    public static PageSizeOption Of(int value, string label) => new(value, label);

    /// <summary>Build a list of numeric options from plain ints (labels = the numbers).</summary>
    public static IReadOnlyList<PageSizeOption> FromInts(params int[] values)
        => values.Select(Of).ToList();

    /// <summary>A convenience default mirroring the library's own page-size ladder + All.</summary>
    public static readonly IReadOnlyList<PageSizeOption> Default = new[]
    {
        Of(5), Of(10), Of(25), Of(50), Of(100), Of(200), All
    };
}
