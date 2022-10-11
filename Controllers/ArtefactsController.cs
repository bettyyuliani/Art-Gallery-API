using Microsoft.AspNetCore.Mvc;
using art_gallery_api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace art_gallery_api.Controllers;

[ApiController]
[Route("api/artefacts")]
public class ArtefactsController : ControllerBase
{
    private readonly IArtefactDataAccess _artefactsRepo;
    public ArtefactsController(IArtefactDataAccess artefactsRepo)
    {
        _artefactsRepo = artefactsRepo;
    }

    /// <summary>
    /// Gets all artefacts.
    /// </summary>
    /// <returns>List of all artefacts</returns>
    /// <remarks>
    /// Sample request:
    /// GET /api/artefacts
    /// </remarks>
    /// <response code="200">Successful request</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles ="user, User, admin, Admin")]
    [HttpGet()]
    public IEnumerable<Artefact> GetAllArtefacts() => _artefactsRepo.GetArtefacts();

    /// <summary>
    /// Gets an artefact based on the specified id.
    /// </summary>
    /// <param name="id">ID of artefact.</param>
    /// <returns>Artefact based on the specified id</returns>
    /// <remarks>
    /// Sample request:
    /// GET /api/artefacts/1
    /// </remarks>
    /// <response code="200">Successful request</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles ="user, User, admin, Admin")]
    [HttpGet("{id}", Name = "GetArtefactById")]
    public IActionResult GetArtefactById(int id) => !_artefactsRepo.CheckEntry(id) ? NotFound() : Ok(_artefactsRepo.GetArtefacts().Where(i => i.Id == id));

    /// <summary>
    /// Creates a new artefact.
    /// </summary>
    /// <param name="newArtefact">A new artefact from the HTTP request.</param>
    /// <returns>A newly created artefact</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/artefacts
    ///     {
    ///        {
    ///             "Name":"Tasmania_Art",
    ///             "State": "Tasmania",
    ///             "PublishedYear": "2019",
    ///             "ArtType":
    ///             {
    ///                 "Id": 1
    ///             },
    ///             "Artists": [
    ///             {
    ///                 "Id":1
    ///             },
    ///             {
    ///                 "Id":3
    ///             },
    ///
    ///                 "Id":7
    ///             }
    ///             ]
    ///         }
    ///     }
    ///
    /// </remarks>
    /// <response code="201">Returns the newly created artefact</response>
    /// <response code="400">If the artefact in the parameter is null or one of the provided artist is null</response>
    /// <response code="409">If the exact same artefact already exists.</response>
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles ="admin, Admin")]
    [HttpPost()]
    public IActionResult AddArtefact(Artefact newArtefact)
    {
        if (newArtefact == null || !_artefactsRepo.CheckArtistEntry(newArtefact)) return BadRequest();
        else if (_artefactsRepo.CheckDuplicate(newArtefact)) return Conflict();

        int _id = _artefactsRepo.InsertArtefact(newArtefact);
        newArtefact.Id = _id;

        return CreatedAtRoute("GetArtefactById", new {id = newArtefact.Id}, newArtefact);
    }

    /// <summary>
    /// Updates an existing artefact.
    /// </summary>
    /// <param name="id">ID of artefact to be updated</param>
    /// <param name="updatedArtefact">A new artefact from the HTTP request to replace the old artefact.</param>
    /// <returns>Status code 204</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     PUT /api/artefact/1
    ///     {
    ///        {
    ///             "Name":"Tasmania_Art",
    ///             "State": "Tasmania",
    ///             "PublishedYear": "2019",
    ///             "ArtType":
    ///             {
    ///                 "Id": 1
    ///             },
    ///             "Artists": [
    ///             {
    ///                 "Id":1
    ///             },
    ///             {
    ///                 "Id":3
    ///             },
    ///
    ///                 "Id":7
    ///             }
    ///             ]
    ///         }
    ///     }
    ///
    /// </remarks>
    /// <response code="204">Request has succeeded</response>
    /// <response code="400">If one or more of the provided artist does not exist, or if the updatedArtefact is null</response>
    /// <response code="404">If no artefact correspond to the provided ID</response>
    /// <response code="409">If the exact same artefact already exist.</response>
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles ="admin, Admin")]
    [HttpPut("{id}")]
    public IActionResult UpdateArtefact(int id, Artefact updatedArtefact)
    {
        if (!_artefactsRepo.CheckEntry(id)) return NotFound();
        else if (updatedArtefact == null || !_artefactsRepo.CheckArtistEntry(updatedArtefact)) return BadRequest();
        else if (_artefactsRepo.CheckDuplicate(updatedArtefact)) return Conflict();
        _artefactsRepo.UpdateArtefact(id, updatedArtefact);
        return NoContent();
    }

    /// <summary>
    /// Deletes an artefact.
    /// </summary>
    /// <param name="id">ID of artefact to be deleted</param>
    /// <returns>Status Code 204</returns>
    /// <remarks>
    /// Sample request:
    /// DELETE /api/artefacts/1
    /// </remarks>
    /// <response code="204">Request has succeeded</response>
    /// <response code="404">If no artefact correspond to the provided ID</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles ="admin, Admin")]
    [HttpDelete("{id}")]
    public IActionResult DeleteArtefact(int id)
    {
        if (!_artefactsRepo.CheckEntry(id)) return NotFound();
        _artefactsRepo.DeleteArtefact(id);
        return NoContent();
    }

    /// <summary>
    /// Deletes an artist within an artefact.
    /// </summary>
    /// <param name="artefactId">ID of the artefact whose artist is to be deleted</param>
    /// <param name="artistId">ID of the artist corresponding to be deleted</param>
    /// <returns>Status Code 204</returns>
    /// <remarks>
    /// Sample request:
    /// DELETE api/artefacts/artists/?artefactid=49&amp;artistid=3
    /// </remarks>
    /// <response code="204">Request has succeeded</response>
    /// <response code="404">If no artefact correspond to the provided ID</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles ="admin, Admin")]
    [HttpDelete("artists", Name = "DeleteArtistFromArtefact")]
    public IActionResult DeleteArtist(int artefactId, int artistId)
    {
        if (!_artefactsRepo.CheckEntry(artefactId) || !_artefactsRepo.CheckArtistEntry(artefactId, artistId)) return NotFound();
        int id = _artefactsRepo.DeleteArtist(artefactId, artistId);
        return NoContent();
    }

    /// <summary>
    /// Gets list artists of an artefact based on the specified id.
    /// </summary>
    /// <param name="artefactId">ID of artefact.</param>
    /// <returns>List of artists corresponding to the artefact based on the specified id</returns>
    /// <remarks>
    /// Sample request:
    /// GET /api/artefacts/artists/45
    /// </remarks>
    /// <response code="200">Successful request</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles ="user, User, admin, Admin")]
    [HttpGet("artists/{artefactId}", Name = "GetArtist")]
    public IEnumerable<ArtistOnArtefact> GetArtists(int artefactId) => _artefactsRepo.GetArtists(artefactId);
}
