using System;
using System.Collections.Generic;
using System.IO;
using BruTile;
using BruTile.Predefined;
using Mapsui.Geometries;
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

            // Calc tile offset
            double factor = (tileInfo.Extent.MaxX - tileInfo.Extent.MinX) / 4096.0;

            List<IFeature> features = new List<IFeature>();

            var layers = source.GetTile(tileInfo);

            foreach (var layer in layers)
            {
                foreach (var vecfeature in layer.VectorTileFeatures)
                {
                    var feature = new Providers.Feature();
                    var styles = styler.GetStyle(layer, new EvaluationContext(zoom, vecfeature));

                    switch (vecfeature.GeometryType)
                    {
                        case GeometryType.Point:
                            foreach (var point in vecfeature.Geometry[0].Points)
                            {
                                point.X = (float)(tileInfo.Extent.MinX + point.X * factor);
                                point.Y = (float)(tileInfo.Extent.MaxY - point.Y * factor);
                                feature.Geometry = point;
                            }
                            feature.Styles.Clear();
                            foreach(var style in styles)
                                feature.Styles.Add(style);
                            //feature.Styles.Add(new VectorStyle { Outline = new Pen { Color = Color.Red, Width = 1 }, Fill = new Brush { Color = Color.Orange } });
                            break;
                        case GeometryType.LineString:
                            var line = new LineString();
                            foreach (var point in vecfeature.Geometry[0].Points)
                            {
                                point.X = (float)(tileInfo.Extent.MinX + point.X * factor);
                                point.Y = (float)(tileInfo.Extent.MaxY - point.Y * factor);
                                line.Vertices.Add(point);
                            }
                            feature.Geometry = line;
                            feature.Styles.Clear();
                            foreach (var style in styles)
                                feature.Styles.Add(style);
                            //feature.Styles.Add(new VectorStyle { Line = new Pen { Color = Color.Black, Width = 2 } });
                            //foreach (var tag in vecfeature.Tags)
                            //    System.Diagnostics.Debug.WriteLine(string.Format("{0}={1}", tag.Key, tag.Value));
                            break;
                        case GeometryType.Polygon:
                            var poly = new Polygon();
                            // Check if first and last point are the same
                            if (!vecfeature.Geometry[0].Points[0].Equals(vecfeature.Geometry[0].Points[vecfeature.Geometry[0].Points.Count - 1]))
                                vecfeature.Geometry[0].Points.Add(vecfeature.Geometry[0].Points[0]);
                            //poly.ExteriorRing.Vertices.AddRange(vecfeature.Geometry[0].Points.Select((p) => Projection.SphericalMercator.FromLonLat(p.X, p.Y)).ToList<Point>());
                            foreach (var point in vecfeature.Geometry[0].Points)
                            {
                                poly.ExteriorRing.Vertices.Add(new Point((float)(tileInfo.Extent.MinX + point.X * factor), (float)(tileInfo.Extent.MaxY - point.Y * factor)));
                            }
                            feature.Geometry = poly;
                            feature.Styles.Clear();
                            foreach (var style in styles)
                                feature.Styles.Add(style);
                            //if (layer.Name.Equals("0"))
                            //    feature.Styles.Add(new VectorStyle { Outline = new Pen { Color = Color.Green, Width = 1 }, Fill = new Brush { Color = Color.Transparent, Background = Color.Red } });
                            //else
                            //    feature.Styles.Add(new VectorStyle { Outline = new Pen { Color = Color.Green, Width = 1 }, Fill = new Brush { Color = Color.FromArgb(40, 255, 255, 255), Background = Color.Transparent } });
                            break;
                    }

                    features.Add(feature);
                }
            }

            return features;
        }

        private Point WorldToTilePos(double lon, double lat, int zoom)
        {
            Point p = new Point();
            p.X = (float)((lon + 180.0) / 360.0 * (1 << zoom));
            p.Y = (float)((1.0 - Math.Log(Math.Tan(lat * Math.PI / 180.0) +
                1.0 / Math.Cos(lat * Math.PI / 180.0)) / Math.PI) / 2.0 * (1 << zoom));

            return p;
        }

        private Point TileToWorldPos(double tile_x, double tile_y, int zoom)
        {
            Point p = new Point();
            double n = Math.PI - ((2.0 * Math.PI * tile_y) / Math.Pow(2.0, zoom));

            p.X = (float)((tile_x / Math.Pow(2.0, zoom) * 360.0) - 180.0);
            p.Y = (float)(180.0 / Math.PI * Math.Atan(Math.Sinh(n)));

            return p;
        }
    }
}
