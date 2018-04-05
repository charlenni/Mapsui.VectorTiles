using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BruTile;
using BruTile.Predefined;
using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.VectorTiles.Mapbox;

namespace Mapsui.VectorTiles.MapboxGLStyler
{
    public class MapboxGLVectorTileProvider : IVectorTileProvider
    {
        MapboxVectorTileSource source;
        MapboxGLStyler styler;

        public MapboxGLVectorTileProvider(Stream mapFile, MapboxGLStyler s)
        {
            source = new MapboxVectorTileSource(mapFile);
            styler = s;

            Schema = new GlobalSphericalMercator("pbf", YAxis.TMS, source.ZoomLevelMin, source.ZoomLevelMax);
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
            float zoomFactor = (float)Math.Log(Schema.Resolutions["0"].UnitsPerPixel / resolution, 2);

            // Calc tiles from box and resolution
            var tileInfos = Schema.GetTileInfos(box.ToExtent(), zoomLevel);

            // Add all features in bounding box to results
            foreach (var tileInfo in tileInfos)
            {
                var list = GetTile(tileInfo, zoomFactor);

                result.AddRange(list);
            }

            return result;
        }

        public IList<IFeature> GetTile(TileInfo tileInfo, float zoomFactor)
        {
            if (source == null)
            {
                return null;
            }

            // Calc tile offset relative to upper left corner
            double factor = (tileInfo.Extent.MaxX - tileInfo.Extent.MinX) / 4096.0;

            List<IFeature> features = new List<IFeature>();

            var layers = source.GetTile(tileInfo);

            foreach (var layer in layers)
            {
                foreach (var feature in layer.VectorTileFeatures)
                {
                    var styles = styler.GetStyle(layer, new EvaluationContext(zoomFactor, feature));

                    // If there isn't a style, feature shouldn't be rendered
                    if (styles == null || styles.Count == 0)
                        continue;

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
