using System.Linq;
using BruTile;
using Mapsui.Geometries;
using System.Collections.Generic;

namespace Mapsui.VectorTiles.Mapbox
{
    public static class FeatureParser
    {
        /// <summary>
        /// Converts a Mapbox feature in Mapbox coordinates into a VectorTileFeature in Mapsui coordinates
        /// </summary>
        /// <param name="tileInfo">TileInfo for tile informations like left top coordinates</param>
        /// <param name="feature">Mapbox feature to convert</param>
        /// <param name="keys">List of known keys for this tile</param>
        /// <param name="values">List of known values for this tile</param>
        /// <param name="extent">Extent/width of this Mapbox formated tile (normally 4096)</param>
        /// <returns></returns>
        public static VectorTileFeature Parse(TileInfo tileInfo, Tile.Feature feature, List<string> keys, List<Tile.Value> values, uint extent)
        {
            var vtf = new VectorTileFeature(feature.Id.ToString());

            // Calc tile offset relative to upper left corner and resolution
            var offsetX = tileInfo.Extent.MinX;
            var offsetY = tileInfo.Extent.MaxY;
            var factor = (tileInfo.Extent.MaxX - tileInfo.Extent.MinX) / extent;

            var geometries =  GeometryParser.ParseGeometry(feature.Geometry, feature.Type, offsetX, offsetY, factor);
            vtf.GeometryType = (GeometryType)feature.Type;

            // Add the geometry
            switch (vtf.GeometryType)
            {
                case GeometryType.Point:
                    // Convert all Points from Mapbox to Mapsui format
                    if (geometries.Count == 1)
                    {
                        // Single point
                        vtf.Geometry = geometries[0][0];
                    }
                    else
                    {
                        // Multi point
                        var multiPoints = new MultiPoint();
                        foreach (var points in geometries)
                        {
                            foreach (var point in points)
                            {
                                multiPoints.Points.Add(point);
                            }
                        }
                        vtf.Geometry = multiPoints;
                    }
                    break;
                case GeometryType.LineString:
                    // Convert all LineStrings from Mapbox to Mapsui format
                    if (geometries.Count == 1)
                    {
                        // Single line
                        vtf.Geometry = new LineString(geometries[0]);
                    }
                    else
                    {
                        // Multi line
                        var multiLines = new MultiLineString();
                        foreach (var line in geometries)
                        {
                            multiLines.LineStrings.Add(new LineString(line));
                        }
                        vtf.Geometry = multiLines;
                    }
                    break;
                case GeometryType.Polygon:
                    // Convert all Polygons from Mapbox to Mapsui format
                    MultiPolygon polygons = new MultiPolygon();
                    Polygon polygon = null;
                    var i = 0;
                    do
                    {
                        var ring = new LinearRing();

                        // Check, if first and last are the same points
                        if (!geometries[i].First().Equals(geometries[i].Last()))
                            geometries[i].Add(geometries[i].First());

                        // Convert all points of this ring
                        foreach (var point in geometries[i])
                        {
                            ring.Vertices.Add(point);
                        }

                        if (ring.IsCCW() && polygon != null)
                        {
                            polygon.InteriorRings.Add(ring);
                        }
                        else
                        {
                            if (polygon != null)
                            {
                                polygons.Polygons.Add(polygon);
                            }
                            polygon = new Polygon(ring);
                        }

                        i++;
                    } while (i < geometries.Count);
                    // Save last one
                    polygons.Polygons.Add(polygon);
                    // Now save correct geometry
                    if (polygons.Polygons.Count > 1)
                    {
                        vtf.Geometry = polygons;
                    }
                    else
                    {
                        vtf.Geometry = polygons.Polygons.First();
                    }
                    break;
            }

            // now add the tags
            vtf.Tags.Add(TagsParser.Parse(keys, values, feature.Tags));

            return vtf;
        }
    }
}