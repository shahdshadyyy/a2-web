using CourseManagementApi.DTOs.Enrollment;
using CourseManagementApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentsController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EnrollmentReadDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var items = await _enrollmentService.GetAllAsync(cancellationToken);
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EnrollmentReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var item = await _enrollmentService.GetByIdAsync(id, cancellationToken);
        if (item == null)
            return NotFound(new { message = "Enrollment not found." });

        return Ok(item);
    }

    [Authorize(Roles = "Admin,Student")]
    [HttpPost]
    [ProducesResponseType(typeof(EnrollmentReadDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateEnrollmentDto dto, CancellationToken cancellationToken)
    {
        var created = await _enrollmentService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [Authorize(Roles = "Admin,Instructor")]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(EnrollmentReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEnrollmentDto dto, CancellationToken cancellationToken)
    {
        var updated = await _enrollmentService.UpdateAsync(id, dto, cancellationToken);
        if (updated == null)
            return NotFound(new { message = "Enrollment not found." });

        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _enrollmentService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
