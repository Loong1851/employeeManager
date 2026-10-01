using EmployeeManager.Application.Dtos;
using EmployeeManager.Application.Repositories;
using EmployeeManager.Core.Models;
using EmployeeManager.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManager.API.Controllers
{
    [Route("api/assignment")]
    [ApiController]

    public class EmployeeDepartmentAssignmentController : ControllerBase
    {
        private readonly ILogger<EmployeeController> _logger;
        private readonly IAssignmentRepository _assignmentRepository;

        public EmployeeDepartmentAssignmentController(ILogger<EmployeeController> logger, IAssignmentRepository assignmentRepository) {
            _logger = logger;
            _assignmentRepository = assignmentRepository;
        }

        [HttpPost]
        [ProducesResponseType(typeof(AssignmentResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> CreateAssignment(
            [FromBody] CreateAssignmentRequest request,  
            CancellationToken cancellationToken)
        {
            if (!await _assignmentRepository.DepartmentExists(request.DepartmentId, cancellationToken))
            {
                ModelState.AddModelError(
                    nameof(request.DepartmentId),
                    $"Department {request.DepartmentId} does not exist.");

                //Returns 400 with the same RFC 7807 ValidationProblemDetails body that
                //[ApiController] produces for annotation failures, so clients see one consistent error shape.
                return BadRequest(new ValidationProblemDetails(ModelState)
                {
                    Status = StatusCodes.Status400BadRequest
                });
            }

            if (!await _assignmentRepository.EmployeeExists(request.EmployeeId, cancellationToken))
            {
                ModelState.AddModelError(
                    nameof(request.EmployeeId),
                    $"Employee {request.EmployeeId} does not exist.");

                return BadRequest(new ValidationProblemDetails(ModelState)
                {
                    Status = StatusCodes.Status400BadRequest
                });
            }

            if (request.AssignmentDate > DateTime.UtcNow.Date.AddDays(31)) {
                ModelState.AddModelError(
                    nameof(request.AssignmentDate),
                    $"AssignmentDate {request.AssignmentDate} is more than 31 days.");

               return BadRequest(new ValidationProblemDetails(ModelState)
               {
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var assignmentCheckResult = await _assignmentRepository.CheckPermanentDepartmentId(request.EmployeeId);
            if (request.DepartmentId == assignmentCheckResult?.Employee.DepartmentId)
            {
                ModelState.AddModelError(
                    nameof(request.DepartmentId),
                    $"DepartmentId {request.DepartmentId} can not be same.");

                return BadRequest(new ValidationProblemDetails(ModelState)
                {
                    Status = StatusCodes.Status400BadRequest
                });

            }

            var created = await _assignmentRepository.CreateAssignment(
                new EmployeeDepartmentAssignment
                {   
                    EmployeeId = request.EmployeeId,
                    DepartmentId = request.DepartmentId,
                    AssignmentDate = request.AssignmentDate,
                    Status = AssignmentStatus.Scheduled
                },
                cancellationToken);

            _logger.LogInformation("Created assignment with id {id}", created.AssignmentId);

            //201 Created, with a Location header pointing at the new resource.
            return CreatedAtAction(nameof(GetAssignmentById), new { id = created.AssignmentId }, ToResponse(created));
        }

        [HttpGet]
        [Route("{id:int}")]
        [ProducesResponseType(typeof(AssignmentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetAssignmentById(int id, CancellationToken cancellationToken) {
            _logger.LogInformation("Fetching Assignment with id {id}", id);
            var employee = await _assignmentRepository.GetAssignmentById(id, cancellationToken);

            if (employee is null) return NotFound();

            return Ok(ToResponse(employee));
        }

        [HttpPut]
        [Route("{id:int}")]
        [ProducesResponseType(typeof(AssignmentResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult> UpdateAssignment(
            int id,
            [FromBody] UpdateAssignmentReequest request,
            CancellationToken cancellationToken)
        {
            //Precedence step 1: does the resource exist? A 404 outranks a 400.
            //Validating the body first would answer "your department id is wrong" for a
            //URL that identifies nothing, which tells the client to fix the wrong thing.
            var existing = await _assignmentRepository.GetAssignmentById(id, cancellationToken);

            if (existing is null) return NotFound();

            //Precedence step 2: is the body valid?
            if (!await _assignmentRepository.DepartmentExists(request.DepartmentId, cancellationToken))
            {
                ModelState.AddModelError(
                    nameof(request.DepartmentId),
                    $"Department {request.DepartmentId} does not exist.");

                //Returns 400 with the same RFC 7807 ValidationProblemDetails body that
                //[ApiController] produces for annotation failures, so clients see one
                //consistent error shape.
                return BadRequest(new ValidationProblemDetails(ModelState)
                {
                    Status = StatusCodes.Status400BadRequest
                });
            }

            if (!await _assignmentRepository.EmployeeExists(request.EmployeeId, cancellationToken))
            {
                ModelState.AddModelError(
                    nameof(request.EmployeeId),
                    $"EmployeeId {request.EmployeeId} does not exist.");

                return BadRequest(new ValidationProblemDetails(ModelState)
                {
                    Status = StatusCodes.Status400BadRequest
                });
            }
            // BR-04
            //Scheduled → Active or Cancelled
            //Active → Completed or Cancelled
            if (existing.Status != request.Status)
            {
                if(existing.Status > request.Status 
                    || (existing.Status == AssignmentStatus.Scheduled && request.Status == AssignmentStatus.Completed)
                    || (existing.Status == AssignmentStatus.Completed && request.Status == AssignmentStatus.Cancelled))
                {
                    ModelState.AddModelError(
                    nameof(request.Status),
                    $"Assignment {request.Status} does not follow the rules.");

                    return BadRequest(new ValidationProblemDetails(ModelState)
                    {
                        Status = StatusCodes.Status400BadRequest
                    });
                }
                
            }
            // check BR-01 An employee may have at most one assignment with Status = Active at any time.
            bool checkResult = await _assignmentRepository.CheckActiveAssignmentByEmployeeId(request.EmployeeId);
            if (request.Status == AssignmentStatus.Active && checkResult) {

                ModelState.AddModelError(
                    nameof(request.Status),
                    $"Assignment status {request.Status} can not assign. As the employee is already active .");

                return Conflict(new ValidationProblemDetails(ModelState)
                {
                    Status = StatusCodes.Status409Conflict
                });

            }
            // check BR-02 AssignmentDate must not be more than 31 days in the future.
            if (request.AssignmentDate > DateTime.UtcNow.Date.AddDays(31))
            {
                ModelState.AddModelError(
                    nameof(request.AssignmentDate),
                    $"AssignmentDate {request.AssignmentDate} is more than 31 days.");

                return BadRequest(new ValidationProblemDetails(ModelState)
                {
                    Status = StatusCodes.Status400BadRequest
                });
            }

            if (existing.Status == AssignmentStatus.Completed || existing.Status == AssignmentStatus.Cancelled)
            {
                ModelState.AddModelError(
                    nameof(request.EmployeeId),
                    $"Employee {request.EmployeeId} does not exist.");

                return BadRequest(new ValidationProblemDetails(ModelState)
                {
                    Status = StatusCodes.Status400BadRequest
                });

            }
            
            var updated = await _assignmentRepository.UpdateAssignment(
                id,
                new EmployeeDepartmentAssignment
                {
                    AssignmentDate = request.AssignmentDate,
                    Status = request.Status,
                },
                cancellationToken);

            //Still possible if the row was deleted between the two calls above.
            if (updated is null) return NotFound();

            _logger.LogInformation("Updated Assignment with id {id}", id);

            return Ok(ToResponse(updated));
        }

        [HttpDelete]
        [Route("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteAssignmentById(int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting Assignment with id {id}", id);

            var isDeleted = await _assignmentRepository.DeleteAssignmentIfExist(id, cancellationToken);

            if (!isDeleted) return NotFound();

            //204 No Content: the delete succeeded and there is no body to return.
            return NoContent();
        }

        private static AssignmentResponse ToResponse(EmployeeDepartmentAssignment assignment) =>
            new(assignment.AssignmentId, assignment.EmployeeId, assignment.DepartmentId, assignment.AssignmentDate, assignment.Status);


    }
}