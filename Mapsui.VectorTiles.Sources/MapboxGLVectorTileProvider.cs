using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BruTile;
using BruTile.Predefined;
using Mapsui.Geometries;
using Mapsui.Logging;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.VectorTiles.Mapbox;

namespace Mapsui.VectorTiles.Sources
{
    public class MapboxGLVectorTileProvider : IVectorTileProvider
    {
        MapboxVectorTileSource source;
        MapboxGLStyler.MapboxGLStyler styler;

        public MapboxGLVectorTileProvider(Stream mapFile, MapboxGLStyler.MapboxGLStyler s)
        {
            source = new MapboxVectorTileSource(mapFile);
            styler = s;

            Schema = new GlobalSphericalMercator("vec", YAxis.TMS, source.ZoomLevelMin, source.ZoomLevelMax);
        }

        public string CRS { get; set; }

        public ITileSchema Schema { get; }

        public BoundingBox GetExtents()
        {
            var bb = new BoundingBox(int.MinValue, int.MinValue, int.MaxValue, int.MaxValue); //Projection.SphericalMercator.FromLonLat(source..GetExtents().BottomLeft.Y, source.GetExtents().BottomLeft.X), Projection.SphericalMercator.FromLonLat(source.GetExtents().TopRight.Y, source.GetExtents().TopRight.X));
            return new BoundingBox(new Point(813637.25, 5375558), new Point(849720.25, 5442556.5));
        }

        public IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, double resolution)
        {
            List<IFeature> result = new List<IFeature>();

            // Calc zoom level
            string zoomLevel = BruTile.Utilities.GetNearestLevel(Schema.Resolutions, resolution);

            // Calc tiles from box and resolution
            var tileInfos = Schema.GetTileInfos(box.ToExtent(), zoomLevel);

            // Add all features in bounding box to results
            foreach (var tileInfo in tileInfos)
            {
                var list = GetTile(tileInfo);

                result.AddRange(list);
            }

            return result;
        }

        public List<IFeature> GetTile(TileInfo tileInfo)
        {
            if (source == null)
            {
                return null;
            }

            // Get zoom
            var zoom = int.Parse(tileInfo.Index.Level);

            // Calc tile offset relative to upper left corner
            double factor = (tileInfo.Extent.MaxX - tileInfo.Extent.MinX) / 4096.0;

            List<IFeature> features = new List<IFeature>();

            var layers = source.GetTile(tileInfo);

            foreach (var layer in layers)
            {
                foreach (var vtf in layer.VectorTileFeatures)
                {
                    var feature = new Providers.Feature();
                    var styles = styler.GetStyle(layer, new EvaluationContext(zoom, vtf));

                    switch (vtf.GeometryType)
                    {
                        case GeometryType.Point:
                            // Convert all Points from Mapbox to Mapsui format
                            if (vtf.Geometry.Count == 1)
                            {
                                // Single point
                                var point = vtf.Geometry[0].Points[0];
                                point.X = (float)(tileInfo.Extent.MinX + point.X * factor);
                                point.Y = (float)(tileInfo.Extent.MaxY - point.Y * factor);
                                feature.Geometry = point;
                            }
                            else
                            {
                                // Multi point
                                var points = new MultiPoint();
                                foreach (var geom in vtf.Geometry)
                                {
                                    foreach (var point in geom.Points)
                                    {
                                        point.X = (float) (tileInfo.Extent.MinX + point.X * factor);
                                        point.Y = (float) (tileInfo.Extent.MaxY - point.Y * factor);
                                        points.Points.Add(point);
                                    }
                                }
                                feature.Geometry = points;
                            }
                            // Add all styles
                            feature.Styles.Clear();
                            foreach(var style in styles)
                                feature.Styles.Add(style);
                            break;
                        case GeometryType.LineString:
                            // Convert all LineStrings from Mapbox to Mapsui format
                            if (vtf.Geometry.Count == 1)
                            {
                                // Single line
                                var line = new LineString();
                                foreach (var point in vtf.Geometry[0].Points)
                                {
                                    point.X = (float) (tileInfo.Extent.MinX + point.X * factor);
                                    point.Y = (float) (tileInfo.Extent.MaxY - point.Y * factor);
                                    line.Vertices.Add(point);
                                }
                                feature.Geometry = line;
                            }
                            else
                            {
                                // Multi line
                                var lines = new MultiLineString();
                                foreach (var geom in vtf.Geometry)
                                {
                                    var line = new LineString();
                                    foreach (var point in vtf.Geometry[0].Points)
                                    {
                                        point.X = (float)(tileInfo.Extent.MinX + point.X * factor);
                                        point.Y = (float)(tileInfo.Extent.MaxY - point.Y * factor);
                                        line.Vertices.Add(point);
                                    }
                                    lines.LineStrings.Add(line);
                                }
                                feature.Geometry = lines;
                            }
                            feature.Styles.Clear();
                            foreach (var style in styles)
                                feature.Styles.Add(style);
                            //feature.Styles.Add(new VectorStyle { Line = new Pen { Color = Color.Black, Width = 2 } });
                            //foreach (var tag in vecfeature.Tags)
                            //    System.Diagnostics.Debug.WriteLine(string.Format("{0}={1}", tag.Key, tag.Value));
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
                                if (!vtf.Geometry[0].Points[0].Equals(vtf.Geometry[0].Points[vtf.Geometry[0].Points.Count - 1]))
                                    vtf.Geometry[0].Points.Add(vtf.Geometry[0].Points[0]);
                                
                                // Convert all points of this ring
                                foreach (var point in vtf.Geometry[i].Points)
                                {
                                    ring.Vertices.Add(new Point(
                                        (float)(tileInfo.Extent.MinX + point.X * factor),
                                        (float)(tileInfo.Extent.MaxY - point.Y * factor)));
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
                            } while (i < vtf.Geometry.Count);
                            // Save last one
                            polygons.Polygons.Add(polygon);
                            // Now save correct geometry
                            if (polygons.Polygons.Count > 1)
                            {
                                feature.Geometry = polygons;
                            }
                            else
                            {
                                feature.Geometry = polygons.Polygons.First();
                            }
                            break;
                    }

                    // Set style for this feature
                    feature.Styles.Clear();
                    foreach (var style in styles)
                        feature.Styles.Add(style);

                    // Add to list of features
                    features.Add(feature);
                }
            }

            return features;
        }
    }
}
