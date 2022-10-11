using Microsoft.AspNetCore.Mvc;
using art_gallery_api.Persistence;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;


namespace art_gallery_api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserDataAccess _userRepo;
    private readonly IConfiguration Configuration;
    public UsersController(IUserDataAccess userRepo, IConfiguration configuration)
    {
        _userRepo = userRepo;
        Configuration = configuration;
    }

    /// <summary>
    /// Gets all user.
    /// </summary>
    /// <returns>All user</returns>
    /// <remarks>
    /// Sample request:
    /// GET /api/user
    /// </remarks>
    /// <response code="200">Successful request</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles ="admin, Admin")]
    [HttpGet()]
    public IEnumerable<UserModel> GetAllUsers() => _userRepo.GetUsers();

    /// <summary>
    /// Gets all admin users
    /// </summary>
    /// <returns>All users whose role is an admin</returns>
    /// <remarks>
    /// Sample request:
    /// GET /api/user/admin
    /// </remarks>
    /// <returns>Status Code 204</returns>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles ="admin, Admin")]
    [HttpGet("admin")]
    public IEnumerable<UserModel> GetAllAdminUser() => _userRepo.GetAdminUsers();

    /// <summary>
    /// Gets user by ID
    /// </summary>
    /// <remarks>
    /// Sample request:
    /// GET /api/user/id
    /// </remarks>
    /// <returns>Status Code 204</returns>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles ="admin, Admin")]
    [HttpGet("{id}", Name = "GetUser")]
    public IActionResult GetUserById(int id) => !_userRepo.CheckEntry(id) ? NotFound() : Ok(_userRepo.GetUsers().Where(i => i.Id == id));

    /// <summary>
    /// Creates a user.
    /// </summary>
    /// <param name="newUser">A new user from the HTTP request.</param>
    /// <returns>A newly created user</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/user
    ///     {
    ///        "firstname": "Betty",
    ///        "lastname": "Yuliani
    ///     }
    ///
    /// </remarks>
    /// <response code="201">Returns the newly created user</response>
    /// <response code="400">If the user is null</response>
    /// <response code="409">If a user with the same name already exists.</response>
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [HttpPost()]
    public IActionResult AddUser(UserModel newUser)
    {
        if (newUser == null) return BadRequest();
        else if (_userRepo.CheckEmail(newUser.Email)) return Conflict();
        _userRepo.InsertUser(newUser);
        return CreatedAtRoute("GetUser", new { id = newUser.Id }, newUser);
    }

    /// <summary>
    /// Creates token for authorization.
    /// </summary>
    /// <param name="user">User login details.</param>
    /// <returns>Login Token</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/user/token
    ///     {
    ///        "Email": "betty2@gmail.com",
    ///        "Password": "12345
    ///     }
    ///
    /// </remarks>
    /// <response code="200">Request is successful</response>
    /// <response code="400">If the user is null</response>
    /// <response code="409">If a user with the same name already exists.</response>
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [HttpPost("token")]
    public IResult CreateToken(LoginModel _user)
    {
        UserModel user = null;
        if (!string.IsNullOrEmpty(_user.Email) && !string.IsNullOrEmpty(_user.Password))
        {
            user = _userRepo.GetUsers().Where(i => i.Email == _user.Email).FirstOrDefault();
            if (user is null) return Results.NotFound("User not found");
        }

        var password = _user.Password;
        var pwVerificationResult = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        if (!pwVerificationResult) return Results.BadRequest("Invalid login details!!");

        var claims = new[]
            {
                new Claim("name", $"{user.FirstName}{user.FirstName}"),
                new Claim(ClaimTypes.Role, user.Role ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email)
                // any other claims that you think might be useful
            };

            var token = new JwtSecurityToken
            (
              issuer: Configuration["Jwt:Issuer"],
              audience: Configuration["Jwt:Audience"],
              claims: claims,
              expires: DateTime.UtcNow.AddDays(60),
              notBefore: DateTime.UtcNow,
              signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"])),
                SecurityAlgorithms.HmacSha256)
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return Results.Ok(tokenString);
    }


    /// <summary>
    /// Updates a user.
    /// </summary>
    /// <param name="id">ID of user to be updated</param>
    /// <param name="newUser">A new user to replaced the old user.</param>
    /// <returns>Status Code 204</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     PUT /api/user/1
    ///     {
    ///        "firstname": "updated",
    ///        "lasttname": "user",
    ///        "description": "Updated user"
    ///     }
    ///
    /// </remarks>
    /// <response code="204">Request has succeeded</response>
    /// <response code="404">If no user correspond to the provided ID</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles ="admin, Admin")]
    [HttpPut("{id}")]
    public IActionResult UpdateUser(int id, UserModel updatedUser)
    {
        if (!_userRepo.CheckEntry(id)) return NotFound();

        _userRepo.UpdateUser(id, updatedUser);
        return NoContent();
    }

    /// <summary>
    /// Delete a user.
    /// </summary>
    /// <param name="id">ID of user to be deleted</param>
    /// <returns>Status Code 204</returns>
    /// <remarks>
    /// Sample request:
    /// DELETE /api/users/1
    /// </remarks>
    /// <response code="204">Request has succeeded</response>
    /// <response code="404">If no user correspond to the provided ID</response>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles ="admin, Admin")]
    [HttpDelete("{id}")]
    public IActionResult DeleteUser(int id)
    {
        if (!_userRepo.CheckEntry(id)) return NotFound();

        _userRepo.DeleteUser(id);
        return NoContent();
    }
}
