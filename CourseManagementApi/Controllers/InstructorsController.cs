using CourseManagementApi.DTOs.Course;
using CourseManagementApi.DTOs.Instructor;
using CourseManagementApi.Exceptions;
using CourseManagementApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InstructorsController : ControllerBase
{
    private readonly IInstructorService _instructorService;

    public InstructorsController(IInstructorService instructorService)
    {
        _instructorService = instructorService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<InstructorReadDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var items = await _instructorService.GetAllAsync(cancellationToken);
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(InstructorReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var item = await _instructorService.GetByIdAsync(id, cancellationToken);
        if (item == null)
            return NotFound(new { message = "Instructor not found." });

        return Ok(item);
    }

    [HttpGet("{id:guid}/courses")]
    [ProducesResponseType(typeof(IReadOnlyList<CourseReadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCourses(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var courses = await _instructorService.GetCoursesAsync(id, cancellationToken);
            return Ok(courses);
        }
        catch (NotFoundException)
        {
            return NotFound(new { message = "Instructor not found." });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(InstructorReadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateInstructorDto dto, CancellationToken cancellationToken)
    {
        var created = await _instructorService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(InstructorReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateInstructorDto dto, CancellationToken cancellationToken)
    {
        var updated = await _instructorService.UpdateAsync(id, dto, cancellationToken);
        if (updated == null)
            return NotFound(new { message = "Instructor not found." });

        return Ok(updated);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _instructorService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
