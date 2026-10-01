using EmployeeManager.Application.Dtos;
using EmployeeManager.Core.Models;
using EmployeeManagerApi.IntegrationTests.Urls;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using EmployeeManagerApi.IntegrationTests.Urls;

namespace EmployeeManagerApi.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public class EmployeeDepartmentAssignmentControllerTests
{
    private readonly HttpClient _client;

    public EmployeeDepartmentAssignmentControllerTests(ApiTestFixture fixture)
    {
        _client = fixture.Client;
    }
    [Fact]
    public async Task Have_atMostOne_assignment_withStatus_Active()
    {
        const int activeAssignmentId = 6;
        var request = new UpdateAssignmentReequest
        {
            EmployeeId = 5,
            DepartmentId = 1,
            AssignmentDate = DateTime.Now,
            Status = AssignmentStatus.Active,
        };
        //Act
        var response = await _client.PutAsJsonAsync(ApiRoutes.Assignments.ById(activeAssignmentId), request);
        var body = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Status: {response.StatusCode}");
        Console.WriteLine($"Body: {body}");
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task AssignmentDate_greaterThan_31Days_OnPut()
    {
        const int activeEmployeeId = 2;
        var request = new UpdateAssignmentReequest
        {
            AssignmentDate = DateTime.UtcNow.Date.AddDays(40),
            Status = AssignmentStatus.Scheduled,
        };
        //Act
        var response = await _client.PutAsJsonAsync(ApiRoutes.Assignments.ById(activeEmployeeId), request);

        
        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task EveryNewly_Assignment_is_Scheduled()
    {
        var request = new UpdateAssignmentReequest
        {
            EmployeeId = 6,
            DepartmentId = 1,
            AssignmentDate = DateTime.UtcNow.Date.AddDays(10),
            
        };
        //Act
        var response = await _client.PostAsJsonAsync(ApiRoutes.Assignments.Base, request);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task UpdateAssignment_WithStatus_ActivetoSchedule_ShouldReturnBadRequest()
    {
        //Arrange
        const int activeEmployeeId = 1;
        var request = new UpdateAssignmentReequest
        {
            EmployeeId = 2,
            DepartmentId = 1,
            AssignmentDate = DateTime.UtcNow.Date.AddDays(40),
            Status = AssignmentStatus.Scheduled
        };
        //Act
        var response = await _client.PutAsJsonAsync(ApiRoutes.Assignments.ById(activeEmployeeId), request);

        //Assert - a foreign key that does not resolve is a client error, not a 500.
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Employee_beTemporarilyAssignedto_TheirOwnPermanent_Department__ShouldReturnBadReques()
    {
        var request = new UpdateAssignmentReequest
        {
            EmployeeId = 2,
            DepartmentId = 1,
            AssignmentDate = DateTime.UtcNow.Date.AddDays(40),

        };
        //Act
        var response = await _client.PostAsJsonAsync(ApiRoutes.Assignments.Base, request);

        //Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }



}
