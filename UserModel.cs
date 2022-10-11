using System.Text.Json.Serialization;

namespace art_gallery_api
{
  public class UserModel {
  public int Id {get; set;}
  public string Email {get; set;}
  public string FirstName {get; set;}
  public string LastName {get; set;}
  public string PasswordHash {get; set;}
  public string? Description {get; set;}
  public string? Role {get; set;}
  public DateTime CreatedDate {get; set;}
  public DateTime ModifiedDate {get; set;}

    public UserModel() {}

    [JsonConstructor]
    public UserModel(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = "";
        PasswordHash = "";
    }
}
}
