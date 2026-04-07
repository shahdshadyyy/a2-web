using CourseManagementApi.DTOs.Enrollment;
using CourseManagementApi.DTOs.Student;
using CourseManagementApi.Exceptions;
using CourseManagementApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StudentsController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentsController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<StudentReadDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var items = await _studentService.GetAllAsync(cancellationToken);
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(StudentReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var item = await _studentService.GetByIdAsync(id, cancellationToken);
        if (item == null)
            return NotFound(new { message = "Student not found." });

        return Ok(item);
    }

    [HttpGet("{id:guid}/enrollments")]
    [ProducesResponseType(typeof(IReadOnlyList<EnrollmentReadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEnrollments(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var enrollments = await _studentService.GetEnrollmentsAsync(id, cancellationToken);
            return Ok(enrollments);
        }
        catch (NotFoundException)
        {
            return NotFound(new { message = "Student not found." });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(StudentReadDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateStudentDto dto, CancellationToken cancellationToken)
    {
        var created = await _studentService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(StudentReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStudentDto dto, CancellationToken cancellationToken)
    {
        var updated = await _studentService.UpdateAsync(id, dto, cancellationToken);
        if (updated == null)
            return NotFound(new { message = "Student not found." });

        return Ok(updated);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _studentService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
