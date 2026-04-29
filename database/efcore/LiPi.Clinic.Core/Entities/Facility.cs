using System.ComponentModel.DataAnnotations.Schema;

namespace LiPi.Clinic.Core.Entities;

[Table("facility", Schema = "core")]
public class Facility
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public Guid? AddressId { get; set; }
    public string Contact { get; set; } = "{}";              // JSONB
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public int RowVersion { get; set; }

    public Address? Address { get; set; }
    public ICollection<Department> Departments { get; set; } = new List<Department>();
}

[Table("departments", Schema = "core")]
public class Department
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid? FacilityId { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string DepartmentType { get; set; } = default!;   // clinical | diagnostic | support | admin | emergency | oncology_rt | oncology_med | oncology_surg | nuclear_med
    public Guid? HeadStaffId { get; set; }
    public string? CostCentreCode { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Facility? Facility { get; set; }
    public Staff? Head { get; set; }
    public ICollection<Ward> Wards { get; set; } = new List<Ward>();
}

[Table("specialties", Schema = "core")]
public class Specialty
{
    public Guid Id { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public Guid? ParentId { get; set; }

    public Specialty? Parent { get; set; }
}

[Table("wards", Schema = "core")]
public class Ward
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid DepartmentId { get; set; }
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string WardType { get; set; } = default!;
    public string? Floor { get; set; }
    public int BedCapacity { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Department Department { get; set; } = default!;
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}

[Table("rooms", Schema = "core")]
public class Room
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid WardId { get; set; }
    public string RoomNumber { get; set; } = default!;
    public string RoomType { get; set; } = default!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Ward Ward { get; set; } = default!;
    public ICollection<Bed> Beds { get; set; } = new List<Bed>();
}

[Table("beds", Schema = "core")]
public class Bed
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public Guid RoomId { get; set; }
    public string BedNumber { get; set; } = default!;
    public string BedType { get; set; } = default!;
    public string Status { get; set; } = "available";        // available | occupied | reserved | blocked | cleaning | maintenance
    public Guid? CurrentAdmissionId { get; set; }            // FK wired in clinic/02_ipd.sql
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Room Room { get; set; } = default!;
}
