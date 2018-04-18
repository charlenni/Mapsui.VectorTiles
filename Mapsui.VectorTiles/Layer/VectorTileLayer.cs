using System;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Layers;

namespace Mapsui.VectorTiles
{
    /// <summary>
    /// VectorTileLayer holding all data for a vector tile like name of layer and features
    /// </summary>
    public class VectorTileLayer : Layer
    {
        public uint Version { get; set; }

        public uint Extent { get; set; }

        /// <summary>
        /// List holding all layer names, that belong to this vector tile layer
        /// E.g. in Mapbox this are the source-layers
        /// </summary>
        public IList<string> InternalLayers { get; } = new List<string>();

        /// <summary>
        /// List holding all vector tile feature, that belong to this 
        /// </summary>
        public IList<VectorTileFeature> VectorTileFeatures { get; } = new List<VectorTileFeature>();

        /// <summary>
        /// All resolutions that this vector tile layer holds natively
        /// </summary>
        public override IReadOnlyList<double> Resolutions => ((IVectorTileProvider)DataSource)?.TileSource?.Schema?.Resolutions.Select(r => r.Value.UnitsPerPixel).ToList();

        /// <summary>
        /// Provider that gets all symbols of a vector tile layer
        /// </summary>
        public SymbolProvider SymbolProvider { get; }

        public VectorTileLayer(SymbolProvider symbolProvider) : base()
        {
            SymbolProvider = symbolProvider ?? throw new ArgumentException("SymbolProvider must not be null");
        }

        public VectorTileLayer(string layername, SymbolProvider symbolProvider) : base (layername)
        {
            SymbolProvider = symbolProvider ?? throw new ArgumentException("SymbolProvider must not be null");
        }
    }
}