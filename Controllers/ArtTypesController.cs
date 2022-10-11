using Microsoft.AspNetCore.Mvc;
using art_gallery_api.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace art_gallery_api.Controllers;

[ApiController]
[Route("api/art-types")]
public class ArtTypesController : ControllerBase
{
    private readonly IArtTypeDataAccess _artTypesRepo;
    public ArtTypesController(IArtTypeDataAccess artTypesRepo)
    {
        _artTypesRepo = artTypesRepo;
    }

    /// <summary>
    /// Gets all art types.
    /// </summary>
    /// <returns>List of all art types</returns>
    /// <remarks>
    /// Sample request:
    /// GET /api/art-types
    /// </remarks>
    /// <response code="200">Successful request</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles ="user, admin, User, Admin")]
    [HttpGet]
    public IEnumerable<ArtType> GetAllArtTypes() => _artTypesRepo.GetArtTypes();


    /// <summary>
    /// Gets an art type based on the specified id.
    /// </summary>
    /// <param name="id">ID of art type.</param>
    /// <returns>Art type based on the specified id</returns>
    /// <remarks>
    /// Sample request:
    /// GET /api/art-types/1
    /// </remarks>
    /// <response code="200">Successful request</response>
    [HttpGet("{id}", Name = "GetArtType")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles ="user, admin, User, Admin")]
    public IActionResult GetArtTypeById(int id) => !_artTypesRepo.CheckEntry(id) ? NotFound() : Ok(_artTypesRepo.GetArtTypes().Where(i => i.Id == id));

    /// <summary>
    /// Creates a new art type.
    /// </summary>
    /// <param name="newArtType">A new art type from the HTTP request.</param>
    /// <returns>A newly created art type</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/art-types
    ///     {
    ///        {
    ///             "Name":"Poetry",
    ///             "Description": "Aboriginal Poetry"
    ///         }
    ///     }
    ///
    /// </remarks>
    /// <response code="201">Returns the newly created art type</response>
    /// <response code="400">If the art type in the parameter is null</response>
    /// <response code="409">If the exact same art type already exists.</response>
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles ="admin, Admin")]
    [HttpPost()]
    public IActionResult AddArtType(ArtType newArtType)
    {
        if (newArtType == null) return BadRequest();
        else if (_artTypesRepo.CheckName(newArtType.Name)) return Conflict();

        int id = _artTypesRepo.InsertArtType(newArtType);
        newArtType.Id = id;
        return CreatedAtRoute("GetArtType", new { id = newArtType.Id }, newArtType);
    }

    /// <summary>
    /// Updates an existing art type.
    /// </summary>
    /// <param name="updatedArtType">A new art type from the HTTP request to replace the old art type.</param>
    /// <returns>Status Code 204</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     PUT /api/art-types/1
    ///     {
    ///        {
    ///             "Name":"Poetry",
    ///             "Description": "Aboriginal Poetry"
    ///         }
    ///     }
    ///
    /// </remarks>
    /// <response code="204">Request has succeeded</response>
    /// <response code="404">If no artefact correspond to the provided ID</response>
    /// <response code="409">If the updated art type has a name similar to the one existing</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles ="admin, Admin")]
    [HttpPut("{id}")]
    public IActionResult UpdateArtType(int id, ArtType updatedArtType)
    {
        if (!_artTypesRepo.CheckEntry(id)) return NotFound();
        else if (_artTypesRepo.CheckName(updatedArtType.Name)) return Conflict();

        _artTypesRepo.UpdateArtType(id, updatedArtType);
        return NoContent();
    }

    /// <summary>
    /// Deletes an art type.
    /// </summary>
    /// <param name="id">ID of art type to be deleted</param>
    /// <returns>Status Code 204</returns>
    /// <remarks>
    /// Sample request:
    /// DELETE /api/art-types/1
    /// </remarks>
    /// <response code="204">Request has succeeded</response>
    /// <response code="404">If no art type correspond to the provided ID</response>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles ="admin, Admin")]
    [HttpDelete("{id}")]
    public IActionResult DeleteArtType(int id)
    {
        if (!_artTypesRepo.CheckEntry(id)) return NotFound();

        _artTypesRepo.DeleteArtType(id);
        return NoContent();
    }

}
