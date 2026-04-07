using CourseManagementApi.DTOs.Course;
using CourseManagementApi.DTOs.Enrollment;
using CourseManagementApi.Exceptions;
using CourseManagementApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CourseReadDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var items = await _courseService.GetAllAsync(cancellationToken);
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CourseReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var item = await _courseService.GetByIdAsync(id, cancellationToken);
        if (item == null)
            return NotFound(new { message = "Course not found." });

        return Ok(item);
    }

    [HttpGet("{id:guid}/students")]
    [ProducesResponseType(typeof(IReadOnlyList<EnrollmentReadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStudents(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var rows = await _courseService.GetStudentsAsync(id, cancellationToken);
            return Ok(rows);
        }
        catch (NotFoundException)
        {
            return NotFound(new { message = "Course not found." });
        }
    }

    [Authorize(Roles = "Admin,Instructor")]
    [HttpPost]
    [ProducesResponseType(typeof(CourseReadDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateCourseDto dto, CancellationToken cancellationToken)
    {
        var created = await _courseService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [Authorize(Roles = "Admin,Instructor")]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CourseReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCourseDto dto, CancellationToken cancellationToken)
    {
        var updated = await _courseService.UpdateAsync(id, dto, cancellationToken);
        if (updated == null)
            return NotFound(new { message = "Course not found." });

        return Ok(updated);
    }

    [Authorize(Roles = "Admin,Instructor")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _courseService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
