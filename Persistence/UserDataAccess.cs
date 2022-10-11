using Npgsql;
using Dapper;

namespace art_gallery_api.Persistence;

public interface IUserDataAccess
{
  Boolean CheckEntry(int id);
  Boolean CheckEmail(string email);
  int DeleteUser(int id);
  List<UserModel> GetUsers();
  List<UserModel> GetAdminUsers();
  int InsertUser(UserModel user);
  int UpdateUser(int userId, UserModel newUser);
}

public class UserDataAccess: IUserDataAccess
{
  private const string CONNECTION_STRING = "Host=localhost;Username=postgres;Password=12345;Database=sit331";

  public List<UserModel> GetUsers()
  {
    var sql = @"select * from public.user";
    List<UserModel> list = new List<UserModel>();
    using (var connection = new NpgsqlConnection(CONNECTION_STRING))
    {
      var entries = connection.Query<UserModel>(sql);

      foreach (var entry in entries)
      {
        list.Add(entry);
      }
    }
    return list;
  }

  public List<UserModel> GetAdminUsers()
  {
    var sql = @"SELECT * FROM public.user WHERE role=\'admin\' or role=\'Admin\' or role=\'ADMIN\'";
    List<UserModel> list = new List<UserModel>();
    using (var connection = new NpgsqlConnection(CONNECTION_STRING))
    {
      var entries = connection.Query<UserModel>(sql);

      foreach (var entry in entries)
      {
        list.Add(entry);
      }
    }
    return list;
  }
  public int UpdateUser(int id, UserModel newUser)
  {
    var Sqlparams = new DynamicParameters();
    Sqlparams.Add("@Id", id);
    Sqlparams.Add("@FirstName", newUser.FirstName);
    Sqlparams.Add("@LastName", newUser.LastName);
    Sqlparams.Add("@Description", newUser.Description == null ? null : newUser.Description);
    Sqlparams.Add("@Role", newUser.Role);

    string sql = @"UPDATE public.user SET firstname=@FirstName, lastname=@LastName, description=@Description, role=@Role, modifieddate=current_timestamp WHERE id=@Id;";
    int _id = -1;
    using (var connection = new NpgsqlConnection(CONNECTION_STRING))
    {
      _id = (int)connection.ExecuteScalar(sql, Sqlparams);
    }
    return _id;
  }


  public int InsertUser(UserModel newUser)
  {
    var password = newUser.PasswordHash;
    var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
    newUser.PasswordHash = hashedPassword;
    var Sqlparams = new DynamicParameters();
    Sqlparams.Add("@FirstName", newUser.FirstName);
    Sqlparams.Add("@LastName", newUser.LastName);
    Sqlparams.Add("@Description", newUser.Description == null ? null : newUser.Description);
    Sqlparams.Add("@PasswordHash", newUser.PasswordHash);
    Sqlparams.Add("@Role", newUser.Role);

    var sql = @"INSERT INTO public.user(email, firstname, lastname, passwordhash, description, role, createddate, modifieddate) VALUES(@Email, @FirstName, @LastName, @PasswordHash, @Description, @Role, current_timestamp, current_timestamp);";
    int _id = -1;
    using (var connection = new NpgsqlConnection(CONNECTION_STRING))
    {
      _id = (int)connection.ExecuteScalar(sql, Sqlparams);
    }
    return _id;
  }

  public int DeleteUser(int id)
  {
    string sql = @"DELETE FROM public.user WHERE id=" + id;
    int _id = -1;
    using (var connection = new NpgsqlConnection(CONNECTION_STRING))
    {
      _id = (int)connection.ExecuteScalar(sql);
    }
    return _id;
  }

  public Boolean CheckEntry(int id)
  {
    string sql = @"SELECT * FROM public.user WHERE id=" + id;
    using (var connection = new NpgsqlConnection(CONNECTION_STRING))
    {
      var entries = connection.Query<UserModel>(sql);
      if (entries.Count() > 0) return true;
    }
    return false;
  }

  public Boolean CheckEmail(string email)
  {
    string sql = @"SELECT * FROM public.user WHERE email=" + email;
    using (var connection = new NpgsqlConnection(CONNECTION_STRING))
    {
      var entries = connection.Query<UserModel>(sql);
      if (entries.Count() > 0) return true;
    }
    return false;
  }
}
