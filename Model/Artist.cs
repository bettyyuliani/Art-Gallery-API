namespace art_gallery_api;

public class Artist
{
    public int Id {get; set;}
    public string Name {get; set;}
    public string DisplayStartYear {get; set;}
    public string? DisplayEndYear {get; set;}
    public List<ArtefactOnArtist>? Artefacts {get; set;}
    public DateTime CreatedDate {get; set;}
    public DateTime ModifiedDate {get;set;}
}
