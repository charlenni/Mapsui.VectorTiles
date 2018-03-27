namespace Mapsui.VectorTiles
{
    using System.Collections.Generic;

    public class VectorTileLayer
	{
        public VectorTileLayer()
        {
            VectorTileFeatures = new List<Feature>();
        }

		public List<Feature> VectorTileFeatures { get; set; }
		public string Name { get; set; }
        public uint Version { get; set; }
		public uint Extent { get; set; }
	}
}