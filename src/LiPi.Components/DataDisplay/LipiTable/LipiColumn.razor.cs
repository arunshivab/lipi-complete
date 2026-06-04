// SPEC: docs/00-COMPONENTS/2.8/01-LipiTable-Spec.md
//   §3.2.2 (generic signature + parameters), §3.2.3 (Field vs ValueSelector),
//   §3.2.4 (Header resolution), §3.2.5 (ColumnKey), §3.3.3 (alignment defaults),
//   §3.6.2/§3.6.4 (width + type-default track)
// PHASE: 2.8 Data Display — Stage 2 core shell
// COMPONENT: LipiColumn<TItem, TValue>
//
// Captures the declarative column parameters and registers a type-erased
// ColumnDefinition<TItem> with the parent LipiTable on initialization. Renders no DOM.
//
// SCOPE NOTE (Stage 2 bare chassis): sort/filter/group/edit/pin/resize/aggregate
// parameters are declared here so the public API shape is correct and pages compile
// against the final surface, but they are INERT this stage — the table does not yet act
// on them. They wire up in their proper stages (sort/filter/selection = Stage 4,
// edit/group/tree = Stage 5, pin/resize/density/aggregate = Stage 6). Declaring them now
// avoids breaking-change churn later.

using System;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using LiPi.Components.Shared.Internal;

namespace LiPi.Components.DataDisplay;

public partial class LipiColumn<TItem, TValue> : ComponentBase, IDisposable
{
    [CascadingParameter] private LipiTable<TItem>? Parent { get; set; }

    // ── Identity / value source (§3.2.3) ─────────────────────────────────
    [Parameter] public Expression<Func<TItem, TValue>>? Field { get; set; }
    [Parameter] public Func<TItem, TValue>? ValueSelector { get; set; }
    [Parameter] public string? ColumnKey { get; set; }

    // ── Display (§3.2.4, §3.3) ───────────────────────────────────────────
    [Parameter] public string? Header { get; set; }
    [Parameter] public ColumnType Type { get; set; } = ColumnType.Text;
    [Parameter] public RenderFragment<TItem>? CellTemplate { get; set; }
    [Parameter] public RenderFragment? HeaderTemplate { get; set; }
    [Parameter] public ColumnAlign? Align { get; set; }

    // ── Format (§3.8) ────────────────────────────────────────────────────
    [Parameter] public string? Format { get; set; }
    [Parameter] public Func<TValue, string>? FormatFn { get; set; }

    // ── Width (§3.6) ─────────────────────────────────────────────────────
    [Parameter] public string? Width { get; set; }
    [Parameter] public string? MinWidth { get; set; }
    [Parameter] public string? MaxWidth { get; set; }

    // ── Visibility + copy (active subset this stage) ─────────────────────
    [Parameter] public bool Visible { get; set; } = true;
    [Parameter] public bool Copyable { get; set; }
    [Parameter] public CopyTarget CopyTarget { get; set; } = CopyTarget.Value;

    // ── Avatar (§3.3.2) ──────────────────────────────────────────────────
    [Parameter] public bool IsInitials { get; set; }

    // ── Inert this stage (declared for API stability; wired in later stages) ──
    [Parameter] public bool Sortable { get; set; } = true;
    [Parameter] public IComparer<TValue>? SortComparer { get; set; }
    [Parameter] public bool Filterable { get; set; } = true;
    [Parameter] public bool Searchable { get; set; } = true;
    [Parameter] public bool Groupable { get; set; }
    [Parameter] public bool AllowGroup { get; set; }
    [Parameter] public bool Editable { get; set; }
    [Parameter] public RenderFragment<TItem>? EditTemplate { get; set; }
    [Parameter] public ColumnPin Pinned { get; set; } = ColumnPin.None;
    [Parameter] public bool Resizable { get; set; } = true;
    [Parameter] public bool Reorderable { get; set; } = true;
    [Parameter] public LipiAggregate? AggregateFn { get; set; }

    private bool _registered;

    protected override void OnInitialized()
    {
        if (Parent is null)
            throw new InvalidOperationException(
                "LipiColumn must be a child of LipiTable.");

        Parent.RegisterColumn(BuildDefinition());
        _registered = true;
    }

    private ColumnDefinition<TItem> BuildDefinition()
    {
        var key = ResolveColumnKey();
        var templateOnly = Field is null && ValueSelector is null;

        return new ColumnDefinition<TItem>
        {
            ColumnKey = key,
            Header = ResolveHeader(key),
            HasHeaderTemplate = HeaderTemplate is not null,
            Type = Type,
            Align = ResolveAlign(),
            GridTrack = ResolveGridTrack(),
            Format = Format,
            IsTemplateOnly = templateOnly,
            Copyable = Copyable,
            CopyTarget = CopyTarget,
            IsInitials = IsInitials,
            Visible = Visible,
            Sortable = ResolveSortable(templateOnly),
            SortComparer = BuildErasedComparer(),
            Searchable = Searchable && !templateOnly,
            Filterable = ResolveFilterable(templateOnly),
            GetValue = BuildValueAccessor(),
            CellTemplate = CellTemplate is null
                ? null
                : (row => CellTemplate(row)),
            HeaderTemplate = HeaderTemplate
        };
    }

    // ── Value accessor (§3.2.3): Field wins; FormatFn handled at table render ──
    private Func<TItem, object?> BuildValueAccessor()
    {
        if (Field is not null)
        {
            var compiled = Field.Compile();
            return item => compiled(item);
        }
        if (ValueSelector is not null)
        {
            return item => ValueSelector(item);
        }
        // Template-only column: no value.
        return _ => null;
    }

    // ── Sort resolution (Stage S1) ───────────────────────────────────────
    // Sortable resolves to: caller's Sortable AND a sortable type AND a real value source.
    // Non-sortable types (Actions, Avatar) and template-only columns can't sort — no scalar
    // value to order by. The caller's Sortable=false always wins (explicit opt-out).
    private bool ResolveSortable(bool templateOnly)
    {
        if (!Sortable) return false;            // explicit opt-out
        if (templateOnly) return false;         // no value source
        return Type switch
        {
            ColumnType.Actions => false,
            ColumnType.Avatar  => false,
            _                  => true
        };
    }

    // ── Filter resolution (Stage S3) ─────────────────────────────────────
    // Filterable resolves to: caller's Filterable AND a filterable type AND a value source.
    // Actions/Avatar/template-only can't filter (no scalar value). Caller Filterable=false wins.
    private bool ResolveFilterable(bool templateOnly)
    {
        if (!Filterable) return false;          // explicit opt-out
        if (templateOnly) return false;         // no value source
        return Type switch
        {
            ColumnType.Actions => false,
            ColumnType.Avatar  => false,
            _                  => true
        };
    }

    // Erase IComparer<TValue> to Comparison<object?> so the table can sort boxed values
    // without knowing TValue. Null when no custom comparer — the table falls back to its
    // default IComparable comparison.
    private Comparison<object?>? BuildErasedComparer()
    {
        if (SortComparer is null) return null;
        var cmp = SortComparer;
        return (a, b) =>
        {
            // Box-safe: both null equal; nulls handled by the table's NullSortOrder before
            // reaching here, but guard anyway so a stray null can't throw in the comparer.
            if (a is null && b is null) return 0;
            if (a is null) return -1;
            if (b is null) return 1;
            return cmp.Compare((TValue)a, (TValue)b);
        };
    }

    // ── ColumnKey (§3.2.5): explicit > derived dotted path > fallback ────
    private string ResolveColumnKey()
    {
        if (!string.IsNullOrWhiteSpace(ColumnKey))
            return ColumnKey!;

        if (Field is not null)
        {
            var path = ExpressionPath(Field.Body);
            if (!string.IsNullOrEmpty(path))
                return path;
        }

        // ValueSelector or no field with no explicit key: fall back to header text,
        // else a stable placeholder. (Dev-mode warning belongs in a later stage's
        // validation pass; bare chassis stays non-throwing.)
        return Header ?? $"col-{Guid.NewGuid():N}";
    }

    // ── Header (§3.2.4): HeaderTemplate > Header > humanized field > key ─
    private string ResolveHeader(string key)
    {
        if (HeaderTemplate is not null) return string.Empty; // table renders the template
        if (!string.IsNullOrWhiteSpace(Header)) return Header!;

        if (Field is not null)
        {
            var leaf = LeafMember(Field.Body);
            if (!string.IsNullOrEmpty(leaf))
                return IdentifierHumanizer.Humanize(leaf);
        }
        return IdentifierHumanizer.Humanize(key);
    }

    // ── Alignment (§3.3.3): explicit Align > type default ────────────────
    // First-column-left override is applied by the table (it knows column order),
    // not here. This returns the type-based default; explicit Align wins.
    private ColumnAlign ResolveAlign()
    {
        // Align is nullable: a non-null value is an explicit override. The enum has no
        // Inherit member (ColumnAlign { Left, Center, Right }); "inherit/default" is
        // expressed by leaving Align null, which falls through to the type default below.
        if (Align is { } a) return a;

        return Type switch
        {
            ColumnType.Number or ColumnType.Currency => ColumnAlign.Right,
            ColumnType.Actions => ColumnAlign.Right,
            ColumnType.Date or ColumnType.DateTime or ColumnType.Time
                or ColumnType.Boolean or ColumnType.Status or ColumnType.Avatar
                => ColumnAlign.Center,
            _ => ColumnAlign.Left
        };
    }

    // ── Width (§3.6.2/§3.6.4): explicit Width > type-default track ────────
    private string ResolveGridTrack()
    {
        if (!string.IsNullOrWhiteSpace(Width)) return Width!;

        return Type switch
        {
            ColumnType.Number => "minmax(100px, 140px)",
            ColumnType.Currency => "minmax(120px, 160px)",
            ColumnType.Date => "minmax(110px, 130px)",
            ColumnType.DateTime => "minmax(150px, 180px)",
            ColumnType.Time => "minmax(90px, 110px)",
            ColumnType.Boolean => "minmax(60px, 80px)",
            ColumnType.Status => "minmax(100px, 130px)",
            ColumnType.Avatar => "56px",
            ColumnType.Actions => "auto",
            ColumnType.Mono => "minmax(120px, auto)",
            _ => "minmax(120px, 1fr)" // Text / Link / File / Custom
        };
    }

    // ── Expression helpers ───────────────────────────────────────────────
    // Full dotted path for ColumnKey: x => x.Patient.Name -> "Patient.Name"
    private static string ExpressionPath(Expression expr)
    {
        if (expr is UnaryExpression u) return ExpressionPath(u.Operand);
        if (expr is MemberExpression m)
        {
            var inner = m.Expression is MemberExpression
                ? ExpressionPath(m.Expression) + "."
                : string.Empty;
            return inner + m.Member.Name;
        }
        return string.Empty;
    }

    // Leaf member only, for header humanization: x => x.Patient.Name -> "Name"
    private static string LeafMember(Expression expr)
    {
        if (expr is UnaryExpression u) return LeafMember(u.Operand);
        if (expr is MemberExpression m) return m.Member.Name;
        return string.Empty;
    }

    public void Dispose()
    {
        if (_registered)
            Parent?.UnregisterColumn(ResolveColumnKey());
    }
}
