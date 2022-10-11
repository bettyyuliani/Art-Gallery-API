namespace art_gallery_api.Persistence;

public interface IArtefactDataAccess
{
  List<Artefact> GetArtefacts();
  List<ArtistOnArtefact> GetArtists(int artefactId);
  int InsertArtefact(Artefact artefact);
  int UpdateArtefact(int id, Artefact artefact);
  int DeleteArtefact(int id);
  int DeleteArtist(int artefactId, int artistId);
  Boolean CheckDuplicate(Artefact artefact);
  Boolean CheckEntry(int id);
  Boolean CheckArtistEntry(Artefact artefact);
  Boolean CheckArtistEntry(int artefactId, int artistId);
}
