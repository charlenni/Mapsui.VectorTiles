using BruTile;
using ProtoBuf;
using System.Collections.Generic;
using System.IO;

namespace Mapsui.VectorTiles.Mapbox
{
    public static class VectorTileParser
    {
        /// <summary>
        /// Parses a unzipped tile in Mapbox format
        /// </summary>
        /// <param name="tileInfo">TileInfo of this tile</param>
        /// <param name="stream">Stream containing tile data in Pbf format</param>
        /// <returns>List of VectorTileLayers, which contain Name and VectorTilesFeatures of each layer, this tile containing</returns>
        public static List<VectorTileLayer> Parse(TileInfo tileInfo, Stream stream)
        {
            // Get tile information from Pbf format
            var tile = Serializer.Deserialize<Tile>(stream);

            // Create list for vector tile layers
            var list = new List<VectorTileLayer>();

            foreach (var layer in tile.Layers)
            {
                // Save width in pixel of tile for later use
                var extent = layer.Extent;
                
                // Create new vector tile layer
                var vectorTileLayer = new VectorTileLayer
                {
                    Name = layer.Name,
                    Version = layer.Version,
                    Extent = layer.Extent
                };

                // Convert all features from Mapbox format into Mapsui format
                foreach (var feature in layer.Features)
                {
                    var vectorTileFeature = FeatureParser.Parse(tileInfo, feature, layer.Keys, layer.Values, extent);
                    vectorTileLayer.VectorTileFeatures.Add(vectorTileFeature);
                }

                list.Add(vectorTileLayer);
            }
            return list;
        }
    }
}