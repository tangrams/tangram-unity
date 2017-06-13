namespace Mapzen.VectorData.Filters
{
	public interface IFeatureMatcher
	{
		bool MatchesFeature(Feature feature);
	}
}
