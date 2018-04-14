using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using BruTile;
using BruTile.Cache;
using Mapsui.Geometries;
using Mapsui.Providers;

namespace Mapsui.VectorTiles.MapboxGLFormat
{
    public class VectorTileProvider : IProvider
    {
        private ITileCache<IList<VectorTileLayer>> _cache;

        public string CRS { get; set; }

        public ITileSource TileSource { get; }

        public string LayerName { get; }

        public BoundingBox Bounds { get; }

        public VectorTileProvider(ITileSource tileSource, string layerName, BoundingBox bounds, ITileCache<IList<VectorTileLayer>> cache = null)
        {
            TileSource = tileSource;
            LayerName = layerName;
            Bounds = bounds;
            _cache = cache;
        }

        public BoundingBox GetExtents()
        {
            return Bounds;
        }

        private BoundingBox _lastBox;
        private IEnumerable<IFeature> _lastResult;

        public IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, double resolution)
        {
            //if (box.Equals(_lastBox))
            //    return _lastResult;

            List<IFeature> result = new List<IFeature>();

            // Calc zoom level
            string zoomLevel = BruTile.Utilities.GetNearestLevel(TileSource.Schema.Resolutions, resolution);

            // Calc tiles from box and resolution
            var tileInfos = TileSource.Schema.GetTileInfos(box.ToExtent(), zoomLevel);

            // Add all features in bounding box to results
            foreach (var tileInfo in tileInfos)
            {
                try
                {
                    var features = GetFeaturesInTile(tileInfo);

                    if (features == null)
                        continue;

                    //foreach (var feature in features)
                    //{
                    //    if (box.Intersects(feature.Geometry.GetBoundingBox()))
                    //        result.Add(feature);
                    //}
                    result.AddRange(features);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e);
                }
            }

            _lastBox = box;
            _lastResult = result;

            return result;
        }

        private IList<IFeature> GetFeaturesInTile(TileInfo tileInfo)
        {
            if (TileSource == null)
            {
                return null;
            }

            var layers = _cache?.Find(tileInfo.Index);

            // Is tile in cache?
            if (layers == null)
            {
                // No, tile not in cache, so get tile data new

                // Calc tile offset relative to upper left corner
                double factor = (tileInfo.Extent.MaxX - tileInfo.Extent.MinX) / 4096.0;

                var buffer = TileSource.GetTile(tileInfo);

                // TileSource knows nothing about this tile
                while (buffer == null && int.Parse(tileInfo.Index.Level) >= 0)
                {
                    // Could we use data from a tile above
                    tileInfo.Index = new TileIndex(tileInfo.Index.Col >> 1, tileInfo.Index.Row >> 1, (int.Parse(tileInfo.Index.Level) - 1).ToString());
                    buffer = TileSource.GetTile(tileInfo);
                }

                if (buffer == null)
                    return null;

                layers = _cache?.Find(tileInfo.Index);

                if (layers == null)
                {
                    // Parse tile and convert it to a feature list
                    layers = VectorTileParser.Parse(tileInfo,
                        new GZipStream(new MemoryStream(TileSource.GetTile(tileInfo)), CompressionMode.Decompress));

                    // Save for later use
                    if (_cache != null && layers.Count > 0)
                        _cache.Add(tileInfo.Index, layers);
                }
            }

            var features = new List<IFeature>();

            // Get all features that belong to this layer
            foreach (var layer in layers)
            {
                if (layer.Name == LayerName)
                {
                    foreach (var feature in layer.VectorTileFeatures)
                    {
                        // Add to list of features
                        features.Add(feature);
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine($"Cached Tile Level={tileInfo.Index.Level}, Col={tileInfo.Index.Col}, Row={tileInfo.Index.Row}");
            return features;
        }
    }
}
