
using System.ComponentModel.DataAnnotations;

namespace EmployeeManager.Core.Models;

public class EmployeeDepartmentAssignment
{
    [Key]
    public int AssignmentId { get; set; }

    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public int DepartmentId { get; set; }
    public Department Department { get; set; }
    public DateTime AssignmentDate { get; set; }
    public AssignmentStatus Status { get; set; }
    

}

public enum AssignmentStatus
{
    Scheduled = 0, 
    Active = 1, 
    Completed = 2,
    Cancelled = 3
}

