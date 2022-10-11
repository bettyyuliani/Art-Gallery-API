using Dapper;
using Npgsql;

namespace art_gallery_api.Persistence;

public class ArtistDataAccess : IArtistDataAccess
{
  private const string CONNECTION_STRING = "Host=localhost;Username=postgres;Password=12345;Database=sit331";
  public List<Artist> GetArtists()
  {
    var sql = @"select artist.*, artefact.*, arttype.*,
    sl.id, sl.suburb, sl.postcode, sl.state, sl.longitude, sl.latitude, sl.geolocation::text, sl.createddate, sl.modifieddate
    from artist
    inner join artistartefact on artist.id = artistartefact.artistid
    inner join artefact on artefact.id = artistartefact.artefactid
    inner join arttype on artefact.arttypeid = arttype.id
    inner join sourcelocation as sl on sl.id = artefact.sourcelocationid;";

    using (var connection = new NpgsqlConnection(CONNECTION_STRING))
    {
      var artistDictionary = new Dictionary<int, Artist>();

      List<Artist> result = connection.Query<Artist, ArtefactOnArtist, ArtTypeOnArtefact, SourceLocation, Artist>(sql, (artist, artefact, artType, sourceLocation) =>
      {
        Artist artistEntry;
        if (!artistDictionary.TryGetValue(artist.Id, out artistEntry))
        {
          artistEntry = artist;
          artistEntry.Artefacts = new List<ArtefactOnArtist>();
          artistDictionary.Add(artistEntry.Id, artistEntry);
        }
        artefact.ArtType = artType;
        artefact.SourceLocation = sourceLocation;
        artistEntry.Artefacts.Add(artefact);
        return artistEntry;
      }, splitOn: "id").Distinct().ToList();

      return result;
    }
  }

  public List<ArtefactOnArtist> GetArtefacts(int artistId)
  {
    var sql = @"select artefact.*, arttype.*,
    sl.id, sl.suburb, sl.postcode, sl.state, sl.longitude, sl.latitude, sl.geolocation::text, sl.createddate, sl.modifieddate
    from artist
    inner join artistartefact on artist.id = artistartefact.artistid
    inner join artefact on artefact.id = artistartefact.artefactid
    inner join arttype on artefact.arttypeid = arttype.id
    inner join sourcelocation as sl on sl.id = artefact.sourcelocationid where artist.id=" + artistId;
    List<ArtefactOnArtist> list = new List<ArtefactOnArtist>();
    using (var connection = new NpgsqlConnection(CONNECTION_STRING))
    {
      var entries = connection.Query<ArtefactOnArtist, ArtTypeOnArtefact, SourceLocation, ArtefactOnArtist>(sql, (artefact, arttype, sourceLocation) =>
      {
        artefact.ArtType = arttype;
        artefact.SourceLocation = sourceLocation;
        list.Add(artefact);
        return artefact;
      }).ToList();
    }
    return list;
  }

  public Boolean CheckArtefactEntry(Artist artist)
  {
    using (var connection = new NpgsqlConnection(CONNECTION_STRING))
    {
      for (int i = 0; i < artist.Artefacts.Count(); i++)
      {
        var entries = connection.Query<Artist>(@"select * from artefact where id=" + artist.Artefacts[i].Id);
        if (entries.Count() <= 0) return false;
      }
    }
    return true;
  }


  public Boolean CheckArtefactEntry(int artistId, int artefactId)
  {
    var Sqlparams = new DynamicParameters();
    Sqlparams.Add("@ArtefactId", artefactId);
    Sqlparams.Add("@ArtistId", artistId);
    using (var connection = new NpgsqlConnection(CONNECTION_STRING))
    {
      var entries = connection.Query<Artefact>(@"select * from artistartefact inner join artist on artist.id = artistartefact.artistid inner join artefact on artefact.id = artistartefact.artefactid where artist.id = @ArtistId and artefact.id = @ArtefactId", Sqlparams);

      if (entries.Count() > 0) return true;
    }
    return false;
  }

  public Boolean CheckEntry(int id)
  {
    using (var connection = new NpgsqlConnection(CONNECTION_STRING))
    {
      var entries = connection.Query<Artist>(@"select * from artist WHERE id=" + id);

      if (entries.Count() > 0) return true;
    }
    return false;
  }

  public Boolean CheckDuplicate(Artist artist)
  {
    var Sqlparams = new DynamicParameters();
    Sqlparams.Add("@Name", artist.Name);
    Sqlparams.Add("@DisplayStartYear", artist.DisplayStartYear);
    Sqlparams.Add("@DisplayEndYear", artist.DisplayEndYear);

    using (var connection = new NpgsqlConnection(CONNECTION_STRING))
    {
      var entries = connection.Query<Artefact>(@"select * from artist WHERE Name=@Name and DisplayStartYear=@DisplayStartYear and DisplayEndYear=@DisplayEndYear", Sqlparams);
      if (entries.Count() > 0) return true;
    }
    return false;
  }

  public int InsertArtist(Artist artist)
  {
    var Sqlparams = new DynamicParameters();
    Sqlparams.Add("@Name", artist.Name);
    Sqlparams.Add("@DisplayStartYear", artist.DisplayStartYear);
    Sqlparams.Add("@DisplayEndYear", artist.DisplayEndYear);
    Sqlparams.Add("@CreatedDate", System.DateTime.Now);
    Sqlparams.Add("@ModifiedDate", System.DateTime.Now);

    List<int> ArtefactIds = new List<int>();
    for (int i = 0; i < artist.Artefacts.Count; i++)
    {
      ArtefactIds.Add(artist.Artefacts[i].Id);
    }

    string sql = @"INSERT INTO PUBLIC.ARTIST(name, displaystartyear, displayendyear, createddate, modifieddate)
    VALUES(@Name, @DisplayStartYear, @DisplayEndYear, @CreatedDate, @ModifiedDate) returning id;";
    string sql2 = @"INSERT INTO PUBLIC.ARTISTARTEFACT(artistid, artefactid) VALUES(@ArtistId, @ArtefactId)";
    int id = -1;
    using (var connection = new NpgsqlConnection(CONNECTION_STRING))
    {
      id = (int)connection.ExecuteScalar(sql, Sqlparams);

      for (int i = 0; i < ArtefactIds.Count; i++)
      {
        var objectParams = new DynamicParameters();
        objectParams.Add("@ArtefactId", ArtefactIds[i]);
        objectParams.Add("@ArtistId", id);
        if (!CheckArtefactEntry(id, ArtefactIds[i]))
        {
          connection.Execute(sql2, objectParams);
        }
      }
    }
    return id;
  }

  public int UpdateArtist(int id, Artist artist)
  {
    var Sqlparams = new DynamicParameters();
    Sqlparams.Add("@Id", id);
    Sqlparams.Add("@Name", artist.Name);
    Sqlparams.Add("@DisplayStartYear", artist.DisplayStartYear);
    Sqlparams.Add("@DisplayEndYear", artist.DisplayEndYear);
    Sqlparams.Add("@ModifiedDate", System.DateTime.Now);

    List<int> ArtefactIds = new List<int>();
    for (int i = 0; i < artist.Artefacts.Count; i++)
    {
      ArtefactIds.Add(artist.Artefacts[i].Id);
    }

    string sql = @"update artist set Name=@Name, displaystartyear=@DisplayStartYear, displayendyear=@DisplayEndYear, modifieddate=@ModifiedDate where id=@Id returning id;";
    string sql2 = @"DELETE FROM artistartefact WHERE artistid=@Id";
    string sql3 = @"INSERT INTO PUBLIC.ARTISTARTEFACT(artistid, artefactid) VALUES(@ArtistId, @ArtefactId)";
    int _id = -1;
    using (var connection = new NpgsqlConnection(CONNECTION_STRING))
    {
      _id = (int)connection.ExecuteScalar(sql, Sqlparams);

      connection.Execute(sql2, Sqlparams);

      for (int i = 0; i < ArtefactIds.Count; i++)
      {
        var objectParams = new DynamicParameters();
        objectParams.Add("@ArtefactId", ArtefactIds[i]);
        objectParams.Add("@ArtistId", id);
        if (!CheckArtefactEntry(id, ArtefactIds[i]))
        {
          connection.Execute(sql3, objectParams);
        }
      }
    }
    return _id;
  }

  public int DeleteArtist(int id)
  {
    var Sqlparams = new DynamicParameters();
    Sqlparams.Add("@Id", id);
    string sql = @"DELETE FROM artistartefact WHERE artistid=@Id; DELETE FROM artist WHERE id=@Id returning id;";
    int _id = -1;
    using (var connection = new NpgsqlConnection(CONNECTION_STRING))
    {
      _id = (int)connection.ExecuteScalar(sql, Sqlparams);
    }
    return _id;
  }

  public int DeleteArtefact(int artistId, int artefactId)
  {
    var Sqlparams = new DynamicParameters();
    Sqlparams.Add("@ArtefactId", artefactId);
    Sqlparams.Add("@ArtistId", artistId);
    string sql = @"delete from artistartefact where artefactid=@ArtefactId and artistid=@ArtistId returning artefactid;";
    int _id = -1;
    using (var connection = new NpgsqlConnection(CONNECTION_STRING))
    {
      _id = (int)connection.ExecuteScalar(sql, Sqlparams);
    }
    return _id;
  }
}
