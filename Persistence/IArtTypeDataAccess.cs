namespace art_gallery_api.Persistence;

public interface IArtTypeDataAccess
{
  List<ArtType> GetArtTypes();
  int InsertArtType(ArtType artType);
  int UpdateArtType(int id, ArtType artType);
  int DeleteArtType(int id);
  Boolean CheckEntry(int id);
  Boolean CheckName(string name);
}
