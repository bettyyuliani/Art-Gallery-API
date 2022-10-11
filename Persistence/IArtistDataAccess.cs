namespace art_gallery_api.Persistence;

public interface IArtistDataAccess
{
  List<Artist> GetArtists();
  List<ArtefactOnArtist> GetArtefacts(int artistId);
  int InsertArtist(Artist artist);
  int UpdateArtist(int id, Artist artist);
  int DeleteArtist(int id);
  int DeleteArtefact(int artistId, int artefactId);
  Boolean CheckEntry(int id);
  Boolean CheckDuplicate(Artist artist);
  Boolean CheckArtefactEntry(Artist artist);
  Boolean CheckArtefactEntry(int artistId, int artefactId);
}
