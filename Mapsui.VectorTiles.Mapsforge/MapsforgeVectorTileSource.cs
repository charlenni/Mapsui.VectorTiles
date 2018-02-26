namespace Mapsui.VectorTiles.Mapsforge
{
    using Geometries;
    using Reader;
    using System.Collections.Generic;
    using System.IO;
    using System;
    using Datastore;

    public class MapsforgeVectorTileSource : IVectorTileSource
    {
        private Stream mapStream;
        private MapFile mapFile;

        private double epsilon = 0.0000000000001;

        public MapsforgeVectorTileSource(Stream stream)
        {
            mapStream = stream;

            if (mapStream == null)
            {
                return;
            }

            mapFile = new Reader.MapFile(mapStream);
        }

        public string CRS { get => "EPSG:3857"; }

        public int ZoomLevelMin
        {
            get
            {
                if (mapFile == null)
                    return 0;
                return mapFile.MapFileInfo.ZoomLevelMin;
            }
        }

        public int ZoomLevelMax
        {
            get
            {
                if (mapFile == null)
                    return 127;
                return mapFile.MapFileInfo.ZoomLevelMax;
            }
        }

        public BoundingBox GetExtents()
        {
            if (mapFile == null)
            {
                return null;
            }

            return mapFile.BoundingBox;
        }

        public IEnumerable<VectorTileLayer> GetTile(Tile tile)
        {
            // Check MapFile
            if (mapFile == null)
            {
                throw new NullReferenceException("mapFile shouldn't be null");
            }

            // Read tile data
            MapReadResult mapReadResult = mapFile.ReadMapData(tile);

            List<VectorTileLayer> result = new List<VectorTileLayer>();
            Dictionary<int, VectorTileLayer> layers = new Dictionary<int, VectorTileLayer>();

            if (mapReadResult == null)
            {
                return result;
            }
            
            if (mapReadResult.PointOfInterests.Count == 0 && mapReadResult.Ways.Count == 0)
            {
                return result;
            }

            // Convert PointOfInterests to Features
            for (int i = 0; i < mapReadResult.PointOfInterests.Count; i++)
            {
                PointOfInterest poi = mapReadResult.PointOfInterests[i];

                VectorTileFeature feature = new VectorTileFeature();
                feature.GeometryType = GeometryType.Point;
                feature.Geometry.Add(new VectorTileGeometry(poi.Position));
                feature.Tags.AddRange(poi.Tags);

                VectorTileLayer layer;
                if (!layers.ContainsKey(poi.Layer))
                {
                    layers.Add(poi.Layer, new VectorTileLayer { Name = poi.Layer.ToString() });
                }
                layers.TryGetValue(poi.Layer, out layer);

                layer.VectorTileFeatures.Add(feature);
            }   

            // Convert Ways to Features
            for (int i = 0; i < mapReadResult.Ways.Count; i++)
            {
                Way way = mapReadResult.Ways[i];
                List<VectorTileFeature> features = new List<VectorTileFeature>();

                foreach (List<Point> points in way.Points)
                {
                    VectorTileFeature feature = new VectorTileFeature();

                    if (Math.Abs(points[0].X - points[points.Count-1].X) < epsilon && Math.Abs(points[0].Y - points[points.Count - 1].Y) < epsilon)
                        feature.GeometryType = GeometryType.Polygon;
                    else
                        feature.GeometryType = GeometryType.LineString;

                    feature.Geometry.Add(new VectorTileGeometry(points));
                    feature.Tags.AddRange(way.Tags);

                    features.Add(feature);
                }

                VectorTileLayer layer;
                if (!layers.ContainsKey(way.Layer))
                {
                    layers.Add(way.Layer, new VectorTileLayer { Name = way.Layer.ToString() });
                }
                layers.TryGetValue(way.Layer, out layer);

                layer.VectorTileFeatures.AddRange(features);
            }

            foreach (var layer in layers)
            {
                result.Add(layer.Value);
            }

            return result;

            // Get zoom level for resolution
            // Get tiles for bounding box
            // For each tile
            // Get POIs and Ways for tile
            // Convert POIs and Ways to Feature
            // Add to Feature list
            // Return Feature list
        }
    }
}