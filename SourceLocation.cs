namespace art_gallery_api;

public class SourceLocation
{
  public int Id {get; set;}
  public string? Suburb {get; set;}
  public string? Postcode {get; set;}
  public double? Longitude {get; set;}
  public double? Latitude {get; set;}
  public string? Geolocation {get; set;}
  public DateTime CreatedDate {get; set;}
  public DateTime ModifiedDate {get; set;}
}
