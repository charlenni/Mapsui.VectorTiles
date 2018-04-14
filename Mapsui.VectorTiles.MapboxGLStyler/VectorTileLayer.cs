using System.Collections.Generic;
using System.Linq;
using Mapsui.Layers;

namespace Mapsui.VectorTiles.MapboxGLFormat
{
    /// <summary>
    /// VectorTileLayer holding all data for a vector tile like name of layer and features
    /// </summary>
    public class VectorTileLayer : Layer
    {
        public uint Version { get; set; }

        public uint Extent { get; set; }

        public IList<VectorTileFeature> VectorTileFeatures { get; } = new List<VectorTileFeature>();

        public override IReadOnlyList<double> Resolutions => ((VectorTileProvider)DataSource)?.TileSource?.Schema?.Resolutions.Select(r => r.Value.UnitsPerPixel).ToList();
    }
}