

using EmployeeManager.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace EmployeeManager.Application.Dtos;

public record AssignmentResponse(int Id, int EmployeeId, int DepartmentId, DateTime AssignmentDate, AssignmentStatus Status);
public class CreateAssignmentRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "EmployeeId must be a positive integer.")]
    public int EmployeeId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "DepartmentId must be a positive integer.")]
    public int DepartmentId { get; set; }

    [Required]
    public DateTime AssignmentDate { get; set; }

    public AssignmentStatus Status { get; set; }

}
public class UpdateAssignmentReequest
{
    [Range(1, int.MaxValue, ErrorMessage = "EmployeeId must be a positive integer.")]
    public int EmployeeId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "DepartmentId must be a positive integer.")]
    public int DepartmentId { get; set; }

    [Required]
    public DateTime AssignmentDate { get; set; }

    public AssignmentStatus Status { get; set; }

}
