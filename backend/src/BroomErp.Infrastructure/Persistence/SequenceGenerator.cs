using BroomErp.Application.Common.Interfaces;
using BroomErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BroomErp.Infrastructure.Persistence;

public class SequenceGenerator : ISequenceGenerator
{
    private readonly AppDbContext _context;

    public SequenceGenerator(AppDbContext context)
    {
        _context = context;
    }

    public async Task<string> NextAsync(string prefix, CancellationToken cancellationToken = default)
    {
        var seq = await _context.Sequences.FirstOrDefaultAsync(s => s.Prefix == prefix, cancellationToken);
        if (seq == null)
        {
            seq = new Sequence { Prefix = prefix, Value = 1 };
            _context.Sequences.Add(seq);
        }
        else
        {
            seq.Value++;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return $"{prefix}{seq.Value:D6}";
    }
}
