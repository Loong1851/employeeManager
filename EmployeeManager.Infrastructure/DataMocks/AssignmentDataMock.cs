using EmployeeManager.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace EmployeeManager.Infrastructure.DataMocks;

public class AssignmentDataMock
{
    private const string AssignmentData = """
    [
        { "AssignmentId": 1, "EmployeeId": 1, "DepartmentId": 5, "AssignmentData": "2026-08-24 17:25:30"},
        { "AssignmentId": 2, "EmployeeId": 2, "DepartmentId": 4, "AssignmentData": "2026-08-24 17:25:30"},
        { "AssignmentId": 3, "EmployeeId": 3, "DepartmentId": 3, "AssignmentData": "2026-08-24 17:25:30"},
        { "AssignmentId": 4, "EmployeeId": 4, "DepartmentId": 2, "AssignmentData": "2026-08-24 17:25:30"},
        { "AssignmentId": 5, "EmployeeId": 5, "DepartmentId": 1, "AssignmentData": "2026-08-24 17:25:30", "Status" : 1},
        { "AssignmentId": 6, "EmployeeId": 5, "DepartmentId": 1, "AssignmentData": "2026-08-24 17:25:30", "Status" : 0}
    ]
    """;

    public static List<EmployeeDepartmentAssignment> GetAllAssignment() =>
        JsonSerializer.Deserialize<List<EmployeeDepartmentAssignment>>(AssignmentData) ?? [];

    public static EmployeeDepartmentAssignment? GetAssignmentById(int id) =>
        GetAllAssignment().FirstOrDefault(d => d.AssignmentId == id);
}
