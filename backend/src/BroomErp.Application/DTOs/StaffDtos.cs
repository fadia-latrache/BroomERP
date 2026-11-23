namespace BroomErp.Application.DTOs;

public record StaffDto(int Id, string? Number, string Name, string Phone, string Position, bool Active, DateTime CreatedAt);
public record CreateStaffRequest(string Name, string Phone = "", string Position = "Maid");
public record UpdateStaffRequest(string? Name = null, string? Phone = null, string? Position = null, bool? Active = null);

public record TeamDto(int Id, string Name, int? SupervisorStaffId, string? SupervisorName, int MemberCount, bool Active, DateTime CreatedAt);
public record CreateTeamRequest(string Name, int? SupervisorStaffId = null);
public record TeamMemberDto(int Id, int StaffId, string Name);
public record AddMemberRequest(int StaffId);

public record AttendanceDto(int Id, int StaffId, string? StaffName, DateOnly WorkDate, int? JobId, decimal Hours, string? ApprovedBy, DateTime CreatedAt);
public record CreateAttendanceRequest(int StaffId, DateOnly WorkDate, decimal Hours, int? JobId = null);

public record RosterEntryDto(int Id, int JobId, string? JobNumber, DateOnly WorkDate, string Location, int StaffId, string? StaffName, string Role, bool Attended);
public record CreateRosterRequest(DateOnly WorkDate, int StaffId, string Location = "", string Role = "CREW");
