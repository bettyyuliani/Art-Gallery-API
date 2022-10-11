using Dapper;
using Npgsql;

namespace art_gallery_api.Persistence;

public class SourceLocationDataAccess : ISourceLocationDataAccess
{
  private const string CONNECTION_STRING = "Host=localhost;Username=postgres;Password=12345;Database=sit331";

  public Boolean CheckEntry(int id)
  {
    using (var connection = new NpgsqlConnection(CONNECTION_STRING))
    {
      var entries = connection.Query<SourceLocation>(@"select sl.id, sl.suburb, sl.postcode, sl.state, sl.longitude, sl.latitude, sl.geolocation::text, sl.createddate, sl.modifieddate from sourcelocation as sl WHERE id=" + id);

      if (entries.Count() > 0) return true;
    }
    return false;
  }

  public Boolean CheckPostCode(string postCode)
  {
    using (var connection = new NpgsqlConnection(CONNECTION_STRING))
    {
      var entries = connection.Query<SourceLocation>(@"select sl.id, sl.suburb, sl.postcode, sl.state, sl.longitude, sl.latitude, sl.geolocation::text, sl.createddate, sl.modifieddate from sourcelocation as sl  WHERE postcode='" + postCode + "'") ;
      if (entries.Count() > 0) return true;
    }
    return false;
  }
  public List<SourceLocation> GetSourceLocations()
  {
    var sql = @"select sl.id, sl.suburb, sl.postcode, sl.state, sl.longitude, sl.latitude, sl.geolocation::text, sl.createddate, sl.modifieddate from sourcelocation as sl";
    List<SourceLocation> list = new List<SourceLocation>();
    using (var connection = new NpgsqlConnection(CONNECTION_STRING))
    {
      var entries = connection.Query<SourceLocation>(sql);

      foreach (var entry in entries)
      {
        list.Add(entry);
      }
    }
    return list;
  }

  public double GetDistance(string postCode1, string postCode2)
  {
        var Sqlparams = new DynamicParameters();
        Sqlparams.Add("@PostCode1", postCode1);
        Sqlparams.Add("@PostCode2", postCode2);
        var sql = @"SELECT ST_Distance(a.geolocation, b.geolocation)/1000 as km from sourcelocation as a, sourcelocation as b where a.postcode=@PostCode1 and b.postcode=@PostCode2;";
    double distance = 0;
    using (var connection = new NpgsqlConnection(CONNECTION_STRING))
    {
      distance = (double) connection.ExecuteScalar(sql, Sqlparams);
    }
    return distance;
  }
  public int InsertSourceLocation(SourceLocation sourceLocation)
  {
    var lala = "point(" + sourceLocation.Longitude + " " + sourceLocation.Latitude + ")";
    var Sqlparams = new DynamicParameters();
    Sqlparams.Add("@Suburb", sourceLocation.Suburb == null ? null : sourceLocation.Suburb);
    Sqlparams.Add("@Postcode", sourceLocation.Postcode == null ? null : sourceLocation.Postcode);
    Sqlparams.Add("@Longitude", sourceLocation.Longitude);
    Sqlparams.Add("@Latitude", sourceLocation.Latitude);
    Sqlparams.Add("@Geolocation", "point(" + sourceLocation.Longitude + " " + sourceLocation.Latitude + ")");
    Sqlparams.Add("@CreatedDate", System.DateTime.Now);
    Sqlparams.Add("@ModifiedDate", System.DateTime.Now);

    string sql = @"INSERT INTO public.sourcelocation(suburb, postcode, longitude, latitude, geolocation, createddate, modifieddate) VALUES(@Suburb, @Postcode, @Longitude, @Latitude, 'point(" + sourceLocation.Longitude + " " + sourceLocation.Latitude + ")', @CreatedDate, @ModifiedDate) returning id;";
    int id = -1;
    using (var connection = new NpgsqlConnection(CONNECTION_STRING))
    {
      id = (int)connection.ExecuteScalar(sql, Sqlparams);
    }
    return id;
  }

  public int UpdateSourceLocation(int id, SourceLocation sourceLocation)
  {
    var Sqlparams = new DynamicParameters();
    Sqlparams.Add("@Id", id);
    Sqlparams.Add("@Suburb", sourceLocation.Suburb == null ? null : sourceLocation.Suburb);
    Sqlparams.Add("@Postcode", sourceLocation.Postcode);
    Sqlparams.Add("@Longitude", sourceLocation.Longitude);
    Sqlparams.Add("@Latitude", sourceLocation.Latitude);
    Sqlparams.Add("@Geolocation", "point(" + sourceLocation.Longitude + " " + sourceLocation.Latitude + ")");
    Sqlparams.Add("@ModifiedDate", System.DateTime.Now);

    string sql = @"update public.sourcelocation set suburb=@Suburb, postcode=@Postcode, longitude=@Longitude, latitude=@latitude, geolocation=point(" + sourceLocation.Longitude + " " + sourceLocation.Latitude + "), modifieddate=@ModifiedDate where id=@Id returning id;";
    int _id = -1;
    using (var connection = new NpgsqlConnection(CONNECTION_STRING))
    {
      _id = (int)connection.ExecuteScalar(sql, Sqlparams);
    }
    return _id;

  }

  public int DeleteSourceLocation(int id)
  {
    var Sqlparams = new DynamicParameters();
    Sqlparams.Add("@Id", id);
    string sql = @"UPDATE ARTEFACT set sourcelocationid= null where sourcelocationid=@Id; DELETE FROM sourcelocation WHERE id=@Id returning id;";
    int _id = -1;
    using (var connection = new NpgsqlConnection(CONNECTION_STRING))
    {
      _id = (int)connection.ExecuteScalar(sql, Sqlparams);
    }
    return _id;
  }
}
