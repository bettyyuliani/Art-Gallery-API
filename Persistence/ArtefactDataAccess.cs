using Dapper;
using Npgsql;

namespace art_gallery_api.Persistence;

public class ArtefactDataAccess : IArtefactDataAccess
{
  private const string CONNECTION_STRING = "Host=localhost;Username=postgres;Password=12345;Database=sit331";
  public List<Artefact> GetArtefacts()
  {
    var sql = @"select artefact.*, artist.*, arttype.*,
    sl.id, sl.suburb, sl.postcode, sl.state, sl.longitude, sl.latitude, sl.geolocation::text, sl.createddate, sl.modifieddate
    from artefact
    inner join artistartefact on artefact.id = artistartefact.artefactid
    inner join artist on artist.id = artistartefact.artistid
    inner join arttype on artefact.arttypeid = arttype.id
    inner join sourcelocation as sl on sl.id = artefact.sourcelocationid;";

    using (var connection = new NpgsqlConnection(CONNECTION_STRING))
    {
      var artefactDictionary = new Dictionary<int, Artefact>();

      List<Artefact> result = connection.Query<Artefact, ArtistOnArtefact, ArtTypeOnArtefact, SourceLocation, Artefact>(sql, (artefact, artist, artType, SourceLocation) =>
      {
        Artefact artefactEntry;
        if (!artefactDictionary.TryGetValue(artefact.Id, out artefactEntry))
        {
          artefactEntry = artefact;
          artefactEntry.Artists = new List<ArtistOnArtefact>();
          artefactEntry.ArtType = artType;
          artefactEntry.SourceLocation = SourceLocation;
          artefactDictionary.Add(artefactEntry.Id, artefactEntry);
        }
        artefactEntry.Artists.Add(artist);
        return artefactEntry;
      }, splitOn: "id").Distinct().ToList();

      return result;
    }
  }

  public List<ArtistOnArtefact> GetArtists(int artefactId)
  {
    var sql = @"select artist.* from artefact inner join artistartefact on artefact.id = artistartefact.artefactid inner join artist on artist.id = artistartefact.artistid inner join arttype on artefact.arttypeid = arttype.id where artefact.id=" + artefactId;
    List<ArtistOnArtefact> list = new List<ArtistOnArtefact>();
    using (var connection = new NpgsqlConnection(CONNECTION_STRING))
    {
      var entries = connection.Query<ArtistOnArtefact>(sql);

      foreach (var entry in entries)
      {
        list.Add(entry);
      }
    }
    return list;
  }

  public Boolean CheckArtistEntry(Artefact artefact)
  {
    using (var connection = new NpgsqlConnection(CONNECTION_STRING))
    {
      for (int i = 0; i < artefact.Artists.Count(); i++)
      {
        var entries = connection.Query<Artist>(@"select * from artist where id=" + artefact.Artists[i].Id);
        if (entries.Count() <= 0) return false;
      }
    }
    return true;
  }

  public Boolean CheckArtistEntry(int artefactId, int artistId)
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
      var entries = connection.Query<Artefact>(@"select * from artefact WHERE id=" + id);

      if (entries.Count() > 0) return true;
    }
    return false;
  }

  public Boolean CheckDuplicate(Artefact artefact)
  {
    var Sqlparams = new DynamicParameters();
    Sqlparams.Add("@Name", artefact.Name);
    Sqlparams.Add("@SourceLocationId", artefact.SourceLocation.Id);
    Sqlparams.Add("@PublishedYear", artefact.PublishedYear);
    Sqlparams.Add("@ArtTypeId", artefact.ArtType.Id);

    using (var connection = new NpgsqlConnection(CONNECTION_STRING))
    {
      var entries = connection.Query<Artefact>(@"select * from artefact WHERE Name=@Name and sourcelocationid=@SourceLocationId and PublishedYear=@PublishedYear and ArtTypeId=@ArtTypeId", Sqlparams);
      if (entries.Count() > 0) return true;
    }
    return false;
  }

  public int InsertArtefact(Artefact artefact)
  {
    var Sqlparams = new DynamicParameters();
    Sqlparams.Add("@Name", artefact.Name);
    Sqlparams.Add("@SourceLocationId", artefact.SourceLocation.Id);
    Sqlparams.Add("@PublishedYear", artefact.PublishedYear);
    Sqlparams.Add("@Description", artefact.Description== null ? null : artefact.Description);
    Sqlparams.Add("@CreatedDate", System.DateTime.Now);
    Sqlparams.Add("@ModifiedDate", System.DateTime.Now);
    Sqlparams.Add("@ArtTypeId", artefact.ArtType.Id);

    List<int> ArtistIds = new List<int>();
    for (int i = 0; i < artefact.Artists.Count; i++)
    {
      ArtistIds.Add(artefact.Artists[i].Id);
    }

    string sql = @"INSERT INTO PUBLIC.ARTEFACT(name, arttypeid, sourcelocationid, publishedyear, description, createddate, modifieddate)
    VALUES(@Name, @ArtTypeId, @SourceLocationId, @PublishedYear, @Description, @CreatedDate, @ModifiedDate) returning id;";
    string sql2 = @"INSERT INTO PUBLIC.ARTISTARTEFACT(artistid, artefactid) VALUES(@ArtistId, @ArtefactId)";
    int id = -1;
    using (var connection = new NpgsqlConnection(CONNECTION_STRING))
    {
      id = (int)connection.ExecuteScalar(sql, Sqlparams);

      for (int i = 0; i < ArtistIds.Count; i++)
      {
        var objectParams = new DynamicParameters();
        objectParams.Add("@ArtefactId", id);
        objectParams.Add("@ArtistId", ArtistIds[i]);
        if (!CheckArtistEntry(id, ArtistIds[i]))
        {
          connection.Execute(sql2, objectParams);
        }
      }
    }
    return id;
  }

  public int UpdateArtefact(int id, Artefact artefact)
  {
    var Sqlparams = new DynamicParameters();
    Sqlparams.Add("@Id", id);
    Sqlparams.Add("@Name", artefact.Name);
    Sqlparams.Add("@SourceLocationId", artefact.SourceLocation.Id);
    Sqlparams.Add("@PublishedYear", artefact.PublishedYear);
    Sqlparams.Add("@Description", artefact.Description == null ? null : artefact.Description);
    Sqlparams.Add("@ModifiedDate", System.DateTime.Now);
    Sqlparams.Add("@ArtTypeId", artefact.ArtType.Id);

    List<int> ArtistIds = new List<int>();
    for (int i = 0; i < artefact.Artists.Count; i++)
    {
      ArtistIds.Add(artefact.Artists[i].Id);
    }

    string sql = @"update artefact set arttypeid=@ArtTypeId, Name=@Name, sourcelocationid=@SourceLocationId, publishedyear=@PublishedYear, description=@Description, modifieddate=@ModifiedDate where id=@Id returning id;";
    string sql2 = @"DELETE FROM artistartefact WHERE artefactid=@Id";
    string sql3 = @"INSERT INTO PUBLIC.ARTISTARTEFACT(artistid, artefactid) VALUES(@ArtistId, @ArtefactId)";
    int _id = -1;
    using (var connection = new NpgsqlConnection(CONNECTION_STRING))
    {
      _id = (int)connection.ExecuteScalar(sql, Sqlparams);

      connection.Execute(sql2, Sqlparams);

      for (int i = 0; i < ArtistIds.Count; i++)
      {
        var objectParams = new DynamicParameters();
        objectParams.Add("@ArtefactId", id);
        objectParams.Add("@ArtistId", ArtistIds[i]);
        if (!CheckArtistEntry(id, ArtistIds[i]))
        {
          connection.Execute(sql3, objectParams);
        }
      }
    }
    return _id;
  }

  public int DeleteArtefact(int id)
  {
    var Sqlparams = new DynamicParameters();
    Sqlparams.Add("@Id", id);
    string sql = @"DELETE FROM artistartefact WHERE artefactid=@Id; DELETE FROM artefact WHERE id=@Id returning id;";
    int _id = -1;
    using (var connection = new NpgsqlConnection(CONNECTION_STRING))
    {
      _id = (int)connection.ExecuteScalar(sql, Sqlparams);
    }
    return _id;
  }

  public int DeleteArtist(int artefactId, int artistId)
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
