namespace art_gallery_api;

public class Artefact
{
    public int Id {get; set;}
    public string Name {get; set;}
    public SourceLocation? SourceLocation {get; set;}
    public string PublishedYear {get; set;}
    public string? Description {get; set;}
    public DateTime CreatedDate {get; set;}
    public DateTime ModifiedDate {get; set;}
    public ArtTypeOnArtefact? ArtType {get; set;}
    public List<ArtistOnArtefact>? Artists {get; set;}

}
