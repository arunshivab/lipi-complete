using System.ComponentModel.DataAnnotations.Schema;

namespace LiPi.Clinic.Core.Entities;

[Table("icd10_codes", Schema = "core")]
public class Icd10Code
{
    public string Code { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string? Chapter { get; set; }
    public string? Category { get; set; }
    public bool IsBillable { get; set; } = true;
}

[Table("snomed_codes", Schema = "core")]
public class SnomedCode
{
    public string Code { get; set; } = default!;
    public string FullySpecifiedName { get; set; } = default!;
    public string PreferredTerm { get; set; } = default!;
    public string? SemanticTag { get; set; }
    public bool IsActive { get; set; } = true;
}

[Table("loinc_codes", Schema = "core")]
public class LoincCode
{
    public string Code { get; set; } = default!;
    public string LongName { get; set; } = default!;
    public string? ShortName { get; set; }
    public string? Component { get; set; }
    public string? Property { get; set; }
    public string? ScaleType { get; set; }
    public string? MethodType { get; set; }
}

[Table("rxnorm_codes", Schema = "core")]
public class RxNormCode
{
    public string Rxcui { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Tty { get; set; } = default!;              // term type (SCD, SBD, IN, BN, ...)
    public bool IsActive { get; set; } = true;
}

[Table("cdsco_device_classes", Schema = "core")]
public class CdscoDeviceClass
{
    public string Code { get; set; } = default!;             // A | B | C | D
    public string Description { get; set; } = default!;
    public string RiskLevel { get; set; } = default!;
}
