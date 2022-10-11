using Dapper;
using Npgsql;

namespace art_gallery_api.Persistence;

public class ArtTypeDataAccess : IArtTypeDataAccess
{
  private const string CONNECTION_STRING = "Host=localhost;Username=postgres;Password=12345;Database=sit331";

  public Boolean CheckEntry(int id)
  {
    using (var connection = new NpgsqlConnection(CONNECTION_STRING))
    {
      var entries = connection.Query<ArtType>(@"select * from arttype WHERE id=" + id);

      if (entries.Count() > 0) return true;
    }
    return false;
  }

  public Boolean CheckName(string name)
  {
    using (var connection = new NpgsqlConnection(CONNECTION_STRING))
    {
      var entries = connection.Query<ArtType>(@"select * from arttype WHERE name='" + name + "'") ;
      if (entries.Count() > 0) return true;
    }
    return false;
  }
  public List<ArtType> GetArtTypes()
  {
    var sql = @"select * from arttype";
    List<ArtType> list = new List<ArtType>();
    using (var connection = new NpgsqlConnection(CONNECTION_STRING))
    {
      var entries = connection.Query<ArtType>(sql);

      foreach (var entry in entries)
      {
        list.Add(entry);
      }
    }
    return list;
  }

  public int InsertArtType(ArtType artType)
  {
    var Sqlparams = new DynamicParameters();
    Sqlparams.Add("@Name", artType.Name);
    Sqlparams.Add("@Description", artType.Description == null ? null : artType.Description);
    Sqlparams.Add("@CreatedDate", System.DateTime.Now);
    Sqlparams.Add("@ModifiedDate", System.DateTime.Now);

    string sql = @"INSERT INTO PUBLIC.ARTTYPE(name, description, createddate, modifieddate) VALUES(@Name, @Description, @CreatedDate, @ModifiedDate) returning id;";
    int id = -1;
    using (var connection = new NpgsqlConnection(CONNECTION_STRING))
    {
      id = (int)connection.ExecuteScalar(sql, Sqlparams);
    }
    return id;
  }

  public int UpdateArtType(int id, ArtType artType)
  {
    var Sqlparams = new DynamicParameters();
    Sqlparams.Add("@Id", id);
    Sqlparams.Add("@Name", artType.Name);
    Sqlparams.Add("@Description", artType.Description == null ? null : artType.Description);
    Sqlparams.Add("@ModifiedDate", System.DateTime.Now);

    string sql = @"update arttype set Name=@Name, description=@Description, modifieddate=@ModifiedDate where id=@Id returning id;";
    int _id = -1;
    using (var connection = new NpgsqlConnection(CONNECTION_STRING))
    {
      _id = (int)connection.ExecuteScalar(sql, Sqlparams);
    }
    return _id;

  }

  public int DeleteArtType(int id)
  {
    var Sqlparams = new DynamicParameters();
    Sqlparams.Add("@Id", id);
    string sql = @"UPDATE ARTEFACT set arttypeid=null where arttypeid=@Id; DELETE FROM arttype WHERE id=@Id returning id;";
    int _id = -1;
    using (var connection = new NpgsqlConnection(CONNECTION_STRING))
    {
      _id = (int)connection.ExecuteScalar(sql, Sqlparams);
    }
    return _id;

  }
}
