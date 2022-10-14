namespace art_gallery_api;

public class ArtefactOnArtist
{
    public int Id {get; set;}
    public string? Name {get; set;}
    public SourceLocation? SourceLocation {get; set;}
    public string? PublishedYear {get; set;}
    public string? Description {get; set;}
    public ArtTypeOnArtefact? ArtType {get; set;}
}
