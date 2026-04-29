using System.ComponentModel.DataAnnotations.Schema;

namespace LiPi.Master.Entities;

[Table("subscription_plans", Schema = "master")]
public class SubscriptionPlan
{
    public Guid Id { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Tier { get; set; } = default!;             // starter | professional | enterprise | custom
    public string[] ModulesIncluded { get; set; } = Array.Empty<string>();
    public int? MaxUsers { get; set; }
    public int? MaxBeds { get; set; }
    public int? MaxPatients { get; set; }
    public decimal? PriceInrMonthly { get; set; }
    public decimal? PriceInrAnnual { get; set; }
    public string Features { get; set; } = "{}";             // JSONB
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}

[Table("subscriptions", Schema = "master")]
public class Subscription
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid? ClinicId { get; set; }
    public Guid PlanId { get; set; }
    public DateOnly StartsOn { get; set; }
    public DateOnly EndsOn { get; set; }
    public string BillingCycle { get; set; } = default!;     // monthly | quarterly | annual
    public bool AutoRenew { get; set; } = true;
    public string Status { get; set; } = "active";           // trial | active | past_due | cancelled | expired
    public string[] AddonModules { get; set; } = Array.Empty<string>();
    public decimal? PriceOverrideInr { get; set; }
    public string? PoReference { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public int RowVersion { get; set; }

    public Organization Organization { get; set; } = default!;
    public Clinic? Clinic { get; set; }
    public SubscriptionPlan Plan { get; set; } = default!;
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}

[Table("invoices", Schema = "master")]
public class Invoice
{
    public Guid Id { get; set; }
    public Guid SubscriptionId { get; set; }
    public string InvoiceNumber { get; set; } = default!;
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public decimal AmountInr { get; set; }
    public decimal GstAmountInr { get; set; }
    public decimal TotalInr { get; set; }
    public string Currency { get; set; } = "INR";
    public string Status { get; set; } = "issued";           // draft | issued | paid | overdue | void
    public DateTimeOffset IssuedAt { get; set; }
    public DateTimeOffset? PaidAt { get; set; }
    public string? PaymentReference { get; set; }
    public string LineItems { get; set; } = "[]";            // JSONB

    public Subscription Subscription { get; set; } = default!;
}
