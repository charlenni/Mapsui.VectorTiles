﻿namespace Mapsui.VectorTiles.Mapbox
{
    using System.Collections.Generic;
    using System.IO;
    using ProtoBuf;

    public static class VectorTileParser
    {
        public static List<VectorTileLayer> Parse(Stream stream)
        {
            var tile = Serializer.Deserialize<Tile>(stream);
            var list = new List<VectorTileLayer>();
            foreach (var layer in tile.Layers)
            {
                var extent = layer.Extent;
                var vectorTileLayer = new VectorTileLayer();
                vectorTileLayer.Name = layer.Name;
                vectorTileLayer.Version = layer.Version;
                vectorTileLayer.Extent = layer.Extent;

                foreach (var feature in layer.Features)
                {
                    var vectorTileFeature = FeatureParser.Parse(feature, layer.Keys, layer.Values, extent);
                    vectorTileLayer.VectorTileFeatures.Add(vectorTileFeature);
                }
                list.Add(vectorTileLayer);
            }
            return list;
        }
    }
}