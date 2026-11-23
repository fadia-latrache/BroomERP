using BroomErp.Domain.Common;

namespace BroomErp.Domain.Entities;

public class Staff : BaseEntity, IAggregateRoot
{
    public string? Number { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Position { get; set; } = "Maid";
    public bool Active { get; set; } = true;
}

public class Team : BaseEntity, IAggregateRoot
{
    public string Name { get; set; } = string.Empty;
    public int? SupervisorStaffId { get; set; }
    public Staff? Supervisor { get; set; }
    public bool Active { get; set; } = true;
    public List<TeamMember> Members { get; set; } = [];

    public string? SupervisorName => Supervisor?.Name;
    public int MemberCount => Members.Count;
}

public class TeamMember : BaseEntity
{
    public int TeamId { get; set; }
    public Team? Team { get; set; }
    public int StaffId { get; set; }
    public Staff? Staff { get; set; }

    public string? Name => Staff?.Name;
}

public class Attendance : BaseEntity, IAggregateRoot
{
    public int StaffId { get; set; }
    public Staff? Staff { get; set; }
    public DateOnly WorkDate { get; set; }
    public int? JobId { get; set; }
    public Job? Job { get; set; }
    public decimal Hours { get; set; }
    public string? ApprovedBy { get; set; }

    public string? StaffName => Staff?.Name;
}

public class RosterEntry : BaseEntity, IAggregateRoot
{
    public int JobId { get; set; }
    public Job? Job { get; set; }
    public DateOnly WorkDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public int StaffId { get; set; }
    public Staff? Staff { get; set; }
    public string Role { get; set; } = "CREW";
    public bool Attended { get; set; } = false;

    public string? StaffName => Staff?.Name;
    public string? JobNumber => Job?.Number;
}
