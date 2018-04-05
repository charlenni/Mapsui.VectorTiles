using System.Collections.Generic;

namespace Mapsui.VectorTiles
{
    /// <summary>
    /// VectorTileLayer holding all data for a vector tile like name of layer and features
    /// </summary>
    public class VectorTileLayer
	{
        /// <summary>
        /// Name of this layer
        /// </summary>
	    public string Name { get; set; }

	    /// <summary>
        /// List of vector tile features belonging to this layer
        /// </summary>
		public List<VectorTileFeature> VectorTileFeatures { get; } = new List<VectorTileFeature>();

        /// <summary>
        /// Version of this layer
        /// </summary>
        public uint Version { get; set; }

        /// <summary>
        /// Extent (width) of this layer
        /// </summary>
		public uint Extent { get; set; }
	}
}