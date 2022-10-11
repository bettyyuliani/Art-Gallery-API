using System;
using Microsoft.AspNetCore.Mvc;
using art_gallery_api.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace art_gallery_api.Controllers;

[ApiController]
[Route("api/artists")]
public class ArtistsController : ControllerBase
{
    private readonly IArtistDataAccess _artistsRepo;
    public ArtistsController(IArtistDataAccess artistsRepo)
    {
        _artistsRepo = artistsRepo;
    }

    /// <summary>
    /// Gets all artists.
    /// </summary>
    /// <returns>List of all artists</returns>
    /// <remarks>
    /// Sample request:
    /// GET /api/artists
    /// </remarks>
    /// <response code="200">Successful request</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles ="user, User, admin, Admin")]
    [HttpGet()]
    public IEnumerable<Artist> GetAllArtists() => _artistsRepo.GetArtists();

    /// <summary>
    /// Gets an artist based on the specified id.
    /// </summary>
    /// <param name="id">ID of artist.</param>
    /// <returns>Artist based on the specified id</returns>
    /// <remarks>
    /// Sample request:
    /// GET /api/artists/1
    /// </remarks>
    /// <response code="200">Successful request</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles ="user, User, admin, Admin")]
    [HttpGet("{id}", Name = "GetArtistById")]
    public IActionResult GetArtistById(int id) => ! _artistsRepo.CheckEntry(id) ? NotFound() : Ok( _artistsRepo.GetArtists().Where(i => i.Id == id));

    /// <summary>
    /// Gets list artefacts of an artist based on the specified id.
    /// </summary>
    /// <param name="artistId">ID of artist.</param>
    /// <returns>List of artefacts corresponding to the artist based on the specified id</returns>
    /// <remarks>
    /// Sample request:
    /// GET /api/artists/artefacts/1
    /// </remarks>
    /// <response code="200">Successful request</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles ="user, User, admin, Admin")]
    [HttpGet("artefacts/{artistId}")]
    public IEnumerable<ArtefactOnArtist> GetArtefacts(int artistId) =>_artistsRepo.GetArtefacts(artistId);

    /// <summary>
    /// Creates a new artist.
    /// </summary>
    /// <param name="newArtist">A new artist from the HTTP request.</param>
    /// <returns>A newly created artist</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/artists
    ///     {
    ///        {
    ///             "Name":"Betty Yuliani",
    ///             "DisplayStartYear": "2012",
    ///             "Artefacts": [
    ///             {
    ///                 "Id":44
    ///             },
    ///             {
    ///                 "Id":30
    ///             }
    ///             ]
    ///         }
    ///     }
    ///
    /// </remarks>
    /// <response code="201">Returns the newly created artist</response>
    /// <response code="400">If one or more of the provided artefact does not exist, or if the updated artist is null</response>
    /// <response code="409">If the exact same artist already exists.</response>
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles ="admin, Admin")]
    [HttpPost()]
    public IActionResult AddArtist(Artist newArtist)
    {
        if (newArtist == null || !_artistsRepo.CheckArtefactEntry(newArtist)) return BadRequest();
        else if (_artistsRepo.CheckDuplicate(newArtist)) return Conflict();

        int _id = _artistsRepo.InsertArtist(newArtist);
        newArtist.Id = _id;

        return CreatedAtRoute("GetArtistById", new {id = newArtist.Id}, newArtist);
    }

    /// <summary>
    /// Updates an existing artist.
    /// </summary>
    /// <param name="id">ID of artist to be updated</param>
    /// <param name="updatedArtist">A new artist from the HTTP request to replace the old artist.</param>
    /// <returns>Status code 204</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     PUT /api/artists/1
    ///     {
    ///        {
    ///             "Name":"Betty Yuliani",
    ///             "DisplayStartYear": "2012",
    ///             "Artefacts": [
    ///             {
    ///                 "Id":44
    ///             },
    ///             {
    ///                 "Id":30
    ///             }
    ///             ]
    ///         }
    ///     }
    ///
    /// </remarks>
    /// <response code="204">Request has succeeded</response>
    /// <response code="400">If one or more of the provided artefact does not exist, or if the updated artist is null</response>
    /// <response code="404">If no artist correspond to the provided ID</response>
    /// <response code="409">If the exact same artist already exist.</response>
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles ="admin, Admin")]
    [HttpPut("{id}")]
    public IActionResult UpdateArtist(int id, Artist updatedArtist)
    {
        if (!_artistsRepo.CheckEntry(id)) return NotFound();
        else if (updatedArtist == null || !_artistsRepo.CheckArtefactEntry(updatedArtist)) return BadRequest();
        else if (_artistsRepo.CheckDuplicate(updatedArtist)) return Conflict();
        _artistsRepo.UpdateArtist(id, updatedArtist);
        return NoContent();
    }

    /// <summary>
    /// Deletes an artist.
    /// </summary>
    /// <param name="id">ID of artist to be deleted</param>
    /// <returns>Status Code 204</returns>
    /// <remarks>
    /// Sample request:
    /// DELETE /api/artefact/1
    /// </remarks>
    /// <response code="204">Request has succeeded</response>
    /// <response code="404">If no artist correspond to the provided ID</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles ="admin, Admin")]
    [HttpDelete("{id}")]
    public IActionResult DeleteArtist(int id)
    {
        if (!_artistsRepo.CheckEntry(id)) return NotFound();
        _artistsRepo.DeleteArtist(id);
        return NoContent();
    }

    /// <summary>
    /// Deletes an artefact within an artist.
    /// </summary>
    /// <param name="artefactId">ID of the artefact to be deleted</param>
    /// <param name="artistId">ID of the artist whose artefact is to be deleted</param>
    /// <returns>Status Code 204</returns>
    /// <remarks>
    /// Sample request:
    /// DELETE api/artists/artefacts/?artefactid=49&amp;artistid=3
    /// </remarks>
    /// <response code="204">Request has succeeded</response>
    /// <response code="404">If no artist or artefact within the artist correspond to the provided IDs</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles ="admin, Admin")]
    [HttpDelete("artefacts", Name = "DeleteArtefactFromArtist")]
    public IActionResult DeleteArtefact(int artistId, int artefactId)
    {
        if (!_artistsRepo.CheckEntry(artistId) || !_artistsRepo.CheckArtefactEntry(artistId, artefactId)) return NotFound();
        int id = _artistsRepo.DeleteArtefact(artistId, artefactId);
        return NoContent();
    }
}
