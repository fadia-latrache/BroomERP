using BroomErp.Application.Common.Interfaces;
using BroomErp.Application.Common.Models;
using BroomErp.Application.DTOs;
using BroomErp.Application.Interfaces.Services;
using BroomErp.Domain.Entities;
using BroomErp.Domain.Exceptions;

namespace BroomErp.Application.Services;

public class StaffService : IStaffService
{
    private readonly IRepository<Staff> _staffRepo;
    private readonly IRepository<Team> _teamRepo;
    private readonly IRepository<TeamMember> _memberRepo;
    private readonly IRepository<Attendance> _attendanceRepo;
    private readonly IRepository<RosterEntry> _rosterRepo;
    private readonly IUnitOfWork _uow;
    private readonly ISequenceGenerator _sequences;
    private readonly ICurrentUserContext _userContext;

    public StaffService(
        IRepository<Staff> staffRepo,
        IRepository<Team> teamRepo,
        IRepository<TeamMember> memberRepo,
        IRepository<Attendance> attendanceRepo,
        IRepository<RosterEntry> rosterRepo,
        IUnitOfWork uow,
        ISequenceGenerator sequences,
        ICurrentUserContext userContext)
    {
        _staffRepo = staffRepo;
        _teamRepo = teamRepo;
        _memberRepo = memberRepo;
        _attendanceRepo = attendanceRepo;
        _rosterRepo = rosterRepo;
        _uow = uow;
        _sequences = sequences;
        _userContext = userContext;
    }

    public async Task<PagedResult<StaffDto>> GetStaffAsync(QueryParams query, CancellationToken cancellationToken = default)
    {
        var paged = await _staffRepo.GetPagedAsync(query,
            s => (string.IsNullOrEmpty(query.Search) || s.Name.ToLower().Contains(query.Search.ToLower()) || s.Phone.Contains(query.Search)),
            cancellationToken);

        return new PagedResult<StaffDto>
        {
            Items = paged.Items.Select(s => new StaffDto(s.Id, s.Number, s.Name, s.Phone, s.Position, s.Active, s.CreatedAt)).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    public async Task<StaffDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var s = await _staffRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException("Staff not found", 404);
        return new StaffDto(s.Id, s.Number, s.Name, s.Phone, s.Position, s.Active, s.CreatedAt);
    }

    public async Task<StaffDto> CreateAsync(CreateStaffRequest request, CancellationToken cancellationToken = default)
    {
        var staff = new Staff
        {
            Number = await _sequences.NextAsync("STF-", cancellationToken),
            Name = request.Name.Trim(),
            Phone = request.Phone.Trim(),
            Position = request.Position.Trim(),
            Active = true
        };
        await _staffRepo.AddAsync(staff, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return new StaffDto(staff.Id, staff.Number, staff.Name, staff.Phone, staff.Position, staff.Active, staff.CreatedAt);
    }

    public async Task<StaffDto> UpdateAsync(int id, UpdateStaffRequest request, CancellationToken cancellationToken = default)
    {
        var s = await _staffRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException("Staff not found", 404);

        if (request.Name != null) s.Name = request.Name.Trim();
        if (request.Phone != null) s.Phone = request.Phone.Trim();
        if (request.Position != null) s.Position = request.Position.Trim();
        if (request.Active.HasValue) s.Active = request.Active.Value;

        await _uow.SaveChangesAsync(cancellationToken);
        return new StaffDto(s.Id, s.Number, s.Name, s.Phone, s.Position, s.Active, s.CreatedAt);
    }

    public async Task ToggleActiveAsync(int id, bool active, CancellationToken cancellationToken = default)
    {
        var s = await _staffRepo.GetByIdAsync(id, cancellationToken)
            ?? throw new DomainException("Staff not found", 404);
        s.Active = active;
        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<TeamDto>> GetTeamsAsync(CancellationToken cancellationToken = default)
    {
        var teams = await _teamRepo.ListAsync(null, cancellationToken, t => t.Supervisor!, t => t.Members);
        return teams.Select(t => new TeamDto(t.Id, t.Name, t.SupervisorStaffId, t.SupervisorName, t.MemberCount, t.Active, t.CreatedAt)).ToList();
    }

    public async Task<TeamDto> CreateTeamAsync(CreateTeamRequest request, CancellationToken cancellationToken = default)
    {
        var team = new Team
        {
            Name = request.Name.Trim(),
            SupervisorStaffId = request.SupervisorStaffId,
            Active = true
        };
        await _teamRepo.AddAsync(team, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return new TeamDto(team.Id, team.Name, team.SupervisorStaffId, null, 0, team.Active, team.CreatedAt);
    }

    public async Task AddTeamMemberAsync(int teamId, int staffId, CancellationToken cancellationToken = default)
    {
        var exists = await _memberRepo.AnyAsync(m => m.TeamId == teamId && m.StaffId == staffId, cancellationToken);
        if (exists) return;

        var member = new TeamMember { TeamId = teamId, StaffId = staffId };
        await _memberRepo.AddAsync(member, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveTeamMemberAsync(int teamId, int staffId, CancellationToken cancellationToken = default)
    {
        var members = await _memberRepo.ListAsync(m => m.TeamId == teamId && m.StaffId == staffId, cancellationToken);
        var member = members.FirstOrDefault();
        if (member != null)
        {
            await _memberRepo.DeleteAsync(member, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<PagedResult<AttendanceDto>> GetAttendanceAsync(QueryParams query, CancellationToken cancellationToken = default)
    {
        var paged = await _attendanceRepo.GetPagedAsync(query, null, cancellationToken, a => a.Staff!);
        return new PagedResult<AttendanceDto>
        {
            Items = paged.Items.Select(a => new AttendanceDto(a.Id, a.StaffId, a.StaffName, a.WorkDate, a.JobId, a.Hours, a.ApprovedBy, a.CreatedAt)).ToList(),
            TotalCount = paged.TotalCount,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    public async Task<AttendanceDto> RecordAttendanceAsync(CreateAttendanceRequest request, CancellationToken cancellationToken = default)
    {
        var attendance = new Attendance
        {
            StaffId = request.StaffId,
            WorkDate = request.WorkDate,
            Hours = request.Hours,
            JobId = request.JobId,
            ApprovedBy = _userContext.Email
        };
        await _attendanceRepo.AddAsync(attendance, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        var staff = await _staffRepo.GetByIdAsync(request.StaffId, cancellationToken);
        return new AttendanceDto(attendance.Id, attendance.StaffId, staff?.Name, attendance.WorkDate, attendance.JobId, attendance.Hours, attendance.ApprovedBy, attendance.CreatedAt);
    }

    public async Task<List<RosterEntryDto>> GetRosterAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var entries = await _rosterRepo.ListAsync(r => r.WorkDate >= from && r.WorkDate <= to, cancellationToken, r => r.Staff!, r => r.Job!);
        return entries.Select(r => new RosterEntryDto(r.Id, r.JobId, r.JobNumber, r.WorkDate, r.Location, r.StaffId, r.StaffName, r.Role, r.Attended)).ToList();
    }

    public async Task<List<RosterEntryDto>> GetJobRosterAsync(int jobId, CancellationToken cancellationToken = default)
    {
        var entries = await _rosterRepo.ListAsync(r => r.JobId == jobId, cancellationToken, r => r.Staff!, r => r.Job!);
        return entries.Select(r => new RosterEntryDto(r.Id, r.JobId, r.JobNumber, r.WorkDate, r.Location, r.StaffId, r.StaffName, r.Role, r.Attended)).ToList();
    }

    public async Task<RosterEntryDto> AddRosterEntryAsync(int jobId, CreateRosterRequest request, CancellationToken cancellationToken = default)
    {
        var entry = new RosterEntry
        {
            JobId = jobId,
            WorkDate = request.WorkDate,
            Location = request.Location,
            StaffId = request.StaffId,
            Role = request.Role,
            Attended = false
        };
        await _rosterRepo.AddAsync(entry, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        var staff = await _staffRepo.GetByIdAsync(request.StaffId, cancellationToken);
        return new RosterEntryDto(entry.Id, entry.JobId, null, entry.WorkDate, entry.Location, entry.StaffId, staff?.Name, entry.Role, entry.Attended);
    }

    public async Task RemoveRosterEntryAsync(int entryId, CancellationToken cancellationToken = default)
    {
        var entry = await _rosterRepo.GetByIdAsync(entryId, cancellationToken)
            ?? throw new DomainException("Roster entry not found", 404);
        await _rosterRepo.DeleteAsync(entry, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
