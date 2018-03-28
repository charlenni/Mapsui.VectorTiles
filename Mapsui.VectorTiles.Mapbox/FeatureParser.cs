namespace Mapsui.VectorTiles.Mapbox
{
    using System.Collections.Generic;

    public static class FeatureParser
    {
        public static VectorTileFeature Parse(Tile.Feature feature, List<string> keys, List<Tile.Value> values, uint extent)
        {
            var result = new VectorTileFeature(feature.Id.ToString());

            var geom =  GeometryParser.ParseGeometry(feature.Geometry, feature.Type);
            result.GeometryType = (GeometryType)feature.Type;

            // add the geometry
            result.Geometry.AddRange(geom);
            //result.Extent = extent;

            // now add the tags
            result.Tags.Add(TagsParser.Parse(keys, values, feature.Tags));

            return result;
        }
    }
}