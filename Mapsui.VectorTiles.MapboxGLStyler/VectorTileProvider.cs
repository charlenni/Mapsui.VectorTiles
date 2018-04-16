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
        private ITileCache<IList<IFeature>> _cache;

        public string CRS { get; set; }

        public ITileSource TileSource { get; }

        public BoundingBox Bounds { get; }

        public VectorTileProvider(ITileSource tileSource, BoundingBox bounds, ITileCache<IList<IFeature>> cache = null)
        {
            TileSource = tileSource;
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

                    // Draw only features, that are symbols/text or in bounding box
                    foreach (var feature in features)
                    {
                        if (feature.Geometry is Point || feature.Geometry is MultiPoint || box.Intersects(feature.Geometry.GetBoundingBox()))
                            result.Add(feature);
                    }
                    //result.AddRange(features);
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

            var features = _cache?.Find(tileInfo.Index);

            // Is tile in cache?
            if (features == null)
            {
                // No, tile not in cache, so get tile data new

                // Calc tile offset relative to upper left corner
                double factor = (tileInfo.Extent.MaxX - tileInfo.Extent.MinX) / 4096.0;

                var tileData = TileSource.GetTile(tileInfo);

                if (tileData == null)
                {
                    // Tile isn't in this source, so construct one from lower zoom level
                    var maxZoom = int.Parse(BruTile.Utilities.GetNearestLevel(TileSource.Schema.Resolutions, TileSource.Schema.Resolutions[(TileSource.Schema.Resolutions.Count-1).ToString()].UnitsPerPixel));
                    // Get bounds of this tile
                    var bounds = new BoundingBox(new Point(tileInfo.Extent.MinX, tileInfo.Extent.MinY), new Point(tileInfo.Extent.MaxX, tileInfo.Extent.MaxY));
                    // Calc new TileInfo
                    var zoom = int.Parse(tileInfo.Index.Level);

                    // If zoom is ok, than there are no tile informations for this tile
                    if (zoom <= maxZoom)
                        return null;

                    var tileX = tileInfo.Index.Col;
                    var tileY = tileInfo.Index.Row;

                    while (zoom > maxZoom)
                    {
                        zoom--;
                        tileX = tileX >> 1;
                        tileY = tileY >> 1;
                    }
                    var newTileInfo = new TileInfo { Index = new TileIndex(tileX, tileY, zoom.ToString()) };
                    // Now get features for this overview tile
                    var newFeatures = GetFeaturesInTile(newTileInfo);
                    // Extract all features, which belong to small tile
                    features = new List<IFeature>();
                    foreach (var feature in newFeatures)
                    {
                        if (feature is VectorTileFeature vft)
                        {
                            if (vft.Bounds.Intersects(bounds))
                                features.Add(vft);
                        }
                    }

                    // Save for later use
                    if (_cache != null)
                        _cache.Add(tileInfo.Index, features);
                }

                if (features == null && tileData != null)
                {
                    // Parse tile and convert it to a feature list
                    Stream stream = new MemoryStream(tileData);

                    if (IsGZipped(stream))
                        stream = new GZipStream(stream, CompressionMode.Decompress);

                    var layer = VectorTileParser.Parse(tileInfo, stream);

                    features = new List<IFeature>();
                    foreach (var feature in layer.VectorTileFeatures)
                    {
                        features.Add(feature);
                    }

                    // Save for later use
                    if (_cache != null && layer.VectorTileFeatures.Count > 0)
                        _cache.Add(tileInfo.Index, features);

                    stream = null;
                }
            }

            System.Diagnostics.Debug.WriteLine($"Cached Tile Level={tileInfo.Index.Level}, Col={tileInfo.Index.Col}, Row={tileInfo.Index.Row}");

            return features;
        }

        /// <summary>
        /// Check, if stream contains gzipped data 
        /// </summary>
        /// <param name="stream">Stream to check</param>
        /// <returns>True, if the stream is gzipped</returns>
        bool IsGZipped(Stream stream)
        {
            return IsZipped(stream, 3, "1F-8B-08");
        }

        /// <summary>
        /// Check, if stream contains zipped data
        /// </summary>
        /// <param name="stream">Stream to check</param>
        /// <param name="signatureSize">Length of bytes to check for signature</param>
        /// <param name="expectedSignature">Signature to check</param>
        /// <returns>True, if the stream is zipped</returns>
        bool IsZipped(Stream stream, int signatureSize = 4, string expectedSignature = "50-4B-03-04")
        {
            if (stream.Length < signatureSize)
                return false;
            byte[] signature = new byte[signatureSize];
            int bytesRequired = signatureSize;
            int index = 0;
            while (bytesRequired > 0)
            {
                int bytesRead = stream.Read(signature, index, bytesRequired);
                bytesRequired -= bytesRead;
                index += bytesRead;
            }
            stream.Seek(0, SeekOrigin.Begin);
            string actualSignature = BitConverter.ToString(signature);
            if (actualSignature == expectedSignature) return true;
            return false;
        }
    }
}
