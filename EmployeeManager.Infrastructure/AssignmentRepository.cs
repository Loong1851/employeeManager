
using EmployeeManager.Application.Repositories;
using EmployeeManager.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManager.Infrastructure;

public class AssignmentRepository : IAssignmentRepository
{
    protected readonly AppDbContext _context;

    public AssignmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<EmployeeDepartmentAssignment> CreateAssignment(EmployeeDepartmentAssignment assignment, CancellationToken cancellationToken = default)
    {
        _context.Assignments.Add(assignment);
        await _context.SaveChangesAsync(cancellationToken);
        return assignment;
    }

    public async Task<bool> DeleteAssignmentIfExist(int id, CancellationToken cancellationToken = default)
    {
        var requestedAssignment = await _context.Assignments
            .FirstOrDefaultAsync(e => e.AssignmentId == id, cancellationToken);

        if (requestedAssignment is null) return false;

        _context.Assignments.Remove(requestedAssignment);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<EmployeeDepartmentAssignment?> GetAssignmentById(int id, CancellationToken cancellationToken = default)
    {
        var requestedAssignment = await _context.Assignments
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.AssignmentId == id, cancellationToken);

        return requestedAssignment;
    }

    public async Task<EmployeeDepartmentAssignment?> UpdateAssignment(int id, EmployeeDepartmentAssignment assignment, CancellationToken cancellationToken = default)
    {
        var existing = await _context.Assignments
            .FirstOrDefaultAsync(e => e.AssignmentId == id, cancellationToken);

        if (existing is null) return null;

        existing.AssignmentId = assignment.AssignmentId;
        existing.EmployeeId = assignment.EmployeeId;
        existing.DepartmentId = assignment.DepartmentId;
        existing.AssignmentDate = assignment.AssignmentDate;
        existing.Status = assignment.Status;

        await _context.SaveChangesAsync(cancellationToken);

        return existing;
    }

    public async Task<bool> EmployeeExists(int Id, CancellationToken cancellationToken = default)
    {
        return await _context.Employees
            .AsNoTracking()
            .AnyAsync(d => d.Id == Id, cancellationToken);
    }

    public async Task<bool> DepartmentExists(int departmentId, CancellationToken cancellationToken = default)
    {
        return await _context.Departments
            .AsNoTracking()
            .AnyAsync(d => d.Id == departmentId, cancellationToken);
    }

    public async Task<bool> CheckActiveAssignmentByEmployeeId(int EmployeeId, CancellationToken cancellationToken = default)
    {
        return await _context.Assignments
            .AsNoTracking()
            .AnyAsync(e => e.EmployeeId == EmployeeId && e.Status == AssignmentStatus.Active);
    }

    public async Task<EmployeeDepartmentAssignment?> CheckPermanentDepartmentId(int EmployeeId, CancellationToken cancellationToken = default)
    {
        return await _context.Assignments
            .Include(e => e.Employee)
            .FirstOrDefaultAsync(e => e.EmployeeId == EmployeeId, cancellationToken);
    }
}
