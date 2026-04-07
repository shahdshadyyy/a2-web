using CourseManagementApi.DTOs.User;
using CourseManagementApi.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CourseManagementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<UserReadDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var users = await _userService.GetAllAsync(cancellationToken);
        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserReadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await _userService.GetByIdAsync(id, cancellationToken);
        if (user == null)
            return NotFound(new { message = "User not found." });

        return Ok(user);
    }
}
