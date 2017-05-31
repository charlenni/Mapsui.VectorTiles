namespace Mapsui.VectorTiles.Mapbox
{
    using Geometries;
    using System.Collections.Generic;

    public class ClassifyRings
    {
        // docs for inner/outer rings https://www.mapbox.com/vector-tiles/specification/
        public static List<List<VectorTileGeometry>> Classify(List<VectorTileGeometry> rings)
        {
            var polygons = new List<List<VectorTileGeometry>>();
            List<VectorTileGeometry> newpoly = null;
            foreach (var ring in rings)
            {
                var poly = new VectorTilePolygon(ring);

                if (poly.IsOuterRing())
                {
                    newpoly = new List<VectorTileGeometry>() { ring };
                    polygons.Add(newpoly);
                }
                else
                {
                    newpoly?.Add(ring);
                }
            }

            return polygons;
        }
    }
}