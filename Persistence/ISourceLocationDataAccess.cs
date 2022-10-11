namespace art_gallery_api.Persistence;

public interface ISourceLocationDataAccess
{
  List<SourceLocation> GetSourceLocations();
  double GetDistance(string postCode1, string postCode2);
  int InsertSourceLocation(SourceLocation sourceLocation);
  int UpdateSourceLocation(int id, SourceLocation sourceLocation);
  int DeleteSourceLocation(int id);
  Boolean CheckEntry(int id);
  Boolean CheckPostCode(string postCode);
}
