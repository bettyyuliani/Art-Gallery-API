using Microsoft.AspNetCore.Mvc;
using art_gallery_api.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace art_gallery_api.Controllers;

[ApiController]
[Route("api/source-locations")]
public class SourceLocationController : ControllerBase
{
    private readonly ISourceLocationDataAccess _sourceLocationRepo;
    public SourceLocationController(ISourceLocationDataAccess sourceLocationRepo)
    {
        _sourceLocationRepo = sourceLocationRepo;
    }

    /// <summary>
    /// Gets all source locations.
    /// </summary>
    /// <returns>List of all source locations</returns>
    /// <remarks>
    /// Sample request:
    /// GET /api/source-locations
    /// </remarks>
    /// <response code="200">Successful request</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles ="user, User, admin, Admin")]
    [HttpGet]
    public IEnumerable<SourceLocation> GetAllSourceLocations() => _sourceLocationRepo.GetSourceLocations();


    /// <summary>
    /// Gets a source location based on the specified id.
    /// </summary>
    /// <param name="id">ID of source location.</param>
    /// <returns>Source location based on the specified id</returns>
    /// <remarks>
    /// Sample request:
    /// GET /api/source-locations/1
    /// </remarks>
    /// <response code="200">Successful request</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles ="user, User, admin, Admin")]
    [HttpGet("{id}", Name = "GetSourceLocation")]
    public IActionResult GetSourceLocationById(int id) => !_sourceLocationRepo.CheckEntry(id) ? NotFound() : Ok(_sourceLocationRepo.GetSourceLocations().Where(i => i.Id == id));

    /// <summary>
    /// Creates a new source location.
    /// </summary>
    /// <param name="newSourceLocation">A new source location from the HTTP request.</param>
    /// <returns>A newly created source location</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/source-locations
    ///     {
    ///        {
    ///             "Suburb":"Doncaster",
    ///             "Postcode":"3108",
    ///             "State":"Victoria,
    ///             "Longitude":145.1,
    ///             "Latitude":37.8
    ///         }
    ///     }
    ///
    /// </remarks>
    /// <response code="201">Returns the newly created source location</response>
    /// <response code="400">If the source location in the parameter is null</response>
    /// <response code="409">If the source location of the same postcode already exists.</response>
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles ="admin, Admin")]
    [HttpPost()]
    public IActionResult AddSourceLocation(SourceLocation newSourceLocation)
    {
        if (newSourceLocation == null) return BadRequest();
        else if (_sourceLocationRepo.CheckPostCode(newSourceLocation.Postcode)) return Conflict();

        int id = _sourceLocationRepo.InsertSourceLocation(newSourceLocation);
        newSourceLocation.Id = id;
        return CreatedAtRoute("GetSourceLocation", new { id = newSourceLocation.Id }, newSourceLocation);
    }

    /// <summary>
    /// Gets distance between two postcodes.
    /// </summary>
    /// <returns>Distance between two postcodes in kilometers</returns>
    /// <remarks>
    /// Sample request:
    /// GET /api/source-locations/distance/?postCode1=3020&amp;postCode2=3012
    /// </remarks>
    /// <response code="200">Successful request</response>
    /// <response code="404">If one of the postcode entered is not in the database</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles ="admin, Admin")]
    [HttpGet("distance", Name = "GetDistance")]
    public IActionResult GetDistance(string postCode1, string postCode2)
    {
      if (!_sourceLocationRepo.CheckPostCode(postCode1) || !_sourceLocationRepo.CheckPostCode(postCode2)) return NotFound();
      return Ok(_sourceLocationRepo.GetDistance(postCode1, postCode2));
    }

    /// <summary>
    /// Updates an existing source location.
    /// </summary>
    /// <param name="updatedSourceLocation">A new source location from the HTTP request to replace the old source location.</param>
    /// <returns>Status Code 204</returns>
    /// <remarks>
    /// Sample request:
    ///
    ///     PUT /api/source-locations
    ///     {
    ///        {
    ///             "Suburb":"Doncaster",
    ///             "Postcode":"3108",
    ///             "State":"Victoria,
    ///             "Longitude":145.1,
    ///             "Latitude":37.8
    ///         }
    ///     }
    ///
    /// </remarks>
    /// <response code="204">Request has succeeded</response>
    /// <response code="404">If no source location correspond to the provided ID</response>
    /// <response code="409">If the updated source location has a postcode similar to the one existing</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles ="admin, Admin")]
    [HttpPut("{id}")]
    public IActionResult UpdateSourceLocation(int id, SourceLocation updatedSourceLocation)
    {
        if (!_sourceLocationRepo.CheckEntry(id)) return NotFound();
        else if (_sourceLocationRepo.CheckPostCode(updatedSourceLocation.Postcode)) return Conflict();

        _sourceLocationRepo.UpdateSourceLocation(id, updatedSourceLocation);
        return NoContent();
    }

    /// <summary>
    /// Deletes an source location.
    /// </summary>
    /// <param name="id">ID of source location to be deleted</param>
    /// <returns>Status Code 204</returns>
    /// <remarks>
    /// Sample request:
    /// DELETE /api/source-locations/1
    /// </remarks>
    /// <response code="204">Request has succeeded</response>
    /// <response code="404">If no source location correspond to the provided ID</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles ="admin, Admin")]
    [HttpDelete("{id}")]
    public IActionResult DeleteSourceLocation(int id)
    {
        if (!_sourceLocationRepo.CheckEntry(id)) return NotFound();

        _sourceLocationRepo.DeleteSourceLocation(id);
        return NoContent();
    }

}
