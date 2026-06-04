namespace LiPicons.Blazor;

/// <summary>
/// The rendered weight / treatment of a <see cref="LipiIcon"/>.
/// Members and order are part of the locked public contract.
/// </summary>
public enum LipiconVariant
{
    /// <summary>1.75 stroke — the everyday weight (HIS default for data-display).</summary>
    Regular,

    /// <summary>2.25 stroke — emphasis.</summary>
    Bold,

    /// <summary>1.25 stroke — dense layouts.</summary>
    Light,

    /// <summary>1.00 stroke — marketing / hero.</summary>
    Thin,

    /// <summary>Solid silhouette.</summary>
    Fill,

    /// <summary>1.5 stroke + 22% currentColor fill on the primary shape.</summary>
    Duotone
}
