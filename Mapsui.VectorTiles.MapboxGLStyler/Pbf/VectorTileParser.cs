using BruTile;
using Mapsui.Providers;
using ProtoBuf;
using System.Collections.Generic;
using System.IO;

namespace Mapsui.VectorTiles.MapboxGLFormat
{
    public static class VectorTileParser
    {
        /// <summary>
        /// Parses a unzipped tile in Mapbox format
        /// </summary>
        /// <param name="tileInfo">TileInfo of this tile</param>
        /// <param name="stream">Stream containing tile data in Pbf format</param>
        /// <returns>List of VectorTileLayers, which contain Name and VectorTilesFeatures of each layer, this tile containing</returns>
        public static IList<IFeature> Parse(TileInfo tileInfo, Stream stream)
        {
            // Get tile information from Pbf format
            var tile = Serializer.Deserialize<Tile>(stream);

            // Create new vector tile layer
            var features = new List<IFeature>();

            foreach (var layer in tile.Layers)
            {
                // Convert all features from Mapbox format into Mapsui format
                foreach (var feature in layer.Features)
                {
                    var vectorTileFeature = FeatureParser.Parse(tileInfo, layer.Name, feature, layer.Keys, layer.Values, layer.Extent);

                    // Add to layer
                    features.Add(vectorTileFeature);
                }
            }

            return features;
        }
    }
}