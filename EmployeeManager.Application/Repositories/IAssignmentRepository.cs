using EmployeeManager.Core.Models;

namespace EmployeeManager.Application.Repositories;

public interface IAssignmentRepository
{
    Task<EmployeeDepartmentAssignment> CreateAssignment(EmployeeDepartmentAssignment assignment, CancellationToken cancellationToken = default);
    Task<EmployeeDepartmentAssignment?> GetAssignmentById(int id, CancellationToken cancellationToken = default);

    Task<EmployeeDepartmentAssignment?> UpdateAssignment(int id, EmployeeDepartmentAssignment assignment, CancellationToken cancellationToken = default);

    Task<bool> DeleteAssignmentIfExist(int id, CancellationToken cancellationToken = default);
    Task<bool> EmployeeExists(int Id, CancellationToken cancellationToken = default);
    Task<bool> DepartmentExists(int departmentId, CancellationToken cancellationToken = default);
    Task<bool> CheckActiveAssignmentByEmployeeId(int EmployeeId, CancellationToken cancellationToken = default);
    Task<EmployeeDepartmentAssignment?> CheckPermanentDepartmentId(int EmployeeId, CancellationToken cancellationToken = default);
}
