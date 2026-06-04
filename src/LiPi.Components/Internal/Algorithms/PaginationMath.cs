// SPEC: docs/00-COMPONENTS/2.8/03-LipiPagination-Spec.md §3.5 (page-number algorithm, REVISED per LP-14)
// PHASE: 2.8 Data Display — Stage 7 (LipiPagination)
// AMEND: docs/CHANGE-LOG.md A50; FLAGS LP-14 (AntDesign/Material rule + Siblings/BoundaryCount API)
//
// PURE, UNIT-TESTED algorithm — extracted from the component per the "test pure before Blazor"
// pattern. 15 test cases (the full LP-14 verification table) pass; see CHANGE-LOG A50.
// No Blazor, no state — a function of its inputs, so test cycles are sub-second.
//
// Rule (LP-14, AntDesign/Material): always show the first/last BoundaryCount pages; show a
// window of (2*Siblings+1) pages centered on current, clamped; ellipsis for any gap > 1
// between consecutive shown pages; if a gap is exactly 1, render that single hidden page
// instead of an ellipsis (the "no ellipsis for one page" reclaim). Small page counts that
// fit within a full ellipsis layout render 1..N contiguously (no ellipses).

namespace LiPi.Components.Internal.Algorithms;

/// <summary>Pure pagination math: total-page count and page-slot generation.</summary>
public static class PaginationMath
{
    /// <summary>The "All" sentinel (int.MaxValue) collapses to a single page when there is data.</summary>
    public const int AllValue = int.MaxValue;

    /// <summary>ceil(total/size); 1 when size is "All" (or non-positive) and data exists; 0 when no data.</summary>
    public static int ComputeTotalPages(int totalCount, int pageSize)
    {
        if (totalCount <= 0) return 0;
        if (pageSize == AllValue || pageSize <= 0) return 1;
        return (totalCount + pageSize - 1) / pageSize;
    }

    /// <summary>
    /// The page-button "slots" to render, in order. A <c>null</c> entry is an ellipsis gap.
    /// See file header for the rule (LP-14). Returns empty when <paramref name="totalPages"/> is 0.
    /// </summary>
    public static IReadOnlyList<int?> ComputePageSlots(int currentPage, int totalPages, int siblings, int boundaryCount)
    {
        if (totalPages <= 0) return Array.Empty<int?>();
        if (totalPages == 1) return new int?[] { 1 };
        if (siblings < 0) siblings = 0;
        if (boundaryCount < 1) boundaryCount = 1;
        currentPage = Math.Clamp(currentPage, 1, totalPages);

        // "All fit" guard: if every page fits within the slots a full ellipsis layout would
        // occupy (2*boundary + 2*siblings + 3), render 1..N contiguously — no ellipses.
        int fitThreshold = boundaryCount * 2 + siblings * 2 + 3;
        if (totalPages <= fitThreshold)
            return Enumerable.Range(1, totalPages).Select(p => (int?)p).ToList();

        var pages = new SortedSet<int>();
        for (int i = 1; i <= Math.Min(boundaryCount, totalPages); i++) pages.Add(i);
        for (int i = Math.Max(1, totalPages - boundaryCount + 1); i <= totalPages; i++) pages.Add(i);
        int from = Math.Max(1, currentPage - siblings);
        int to = Math.Min(totalPages, currentPage + siblings);
        for (int i = from; i <= to; i++) pages.Add(i);

        var ordered = pages.ToList();
        var slots = new List<int?>(ordered.Count + 2);
        for (int idx = 0; idx < ordered.Count; idx++)
        {
            if (idx > 0)
            {
                int gap = ordered[idx] - ordered[idx - 1];
                if (gap == 2) slots.Add(ordered[idx - 1] + 1);  // reclaim single hidden page
                else if (gap > 2) slots.Add(null);              // ellipsis
            }
            slots.Add(ordered[idx]);
        }
        return slots;
    }
}
