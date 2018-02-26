namespace Mapsui.VectorTiles.Mapbox
{
    using System.Collections.Generic;
    using System.IO;

    public class MapboxVectorTileSource : IVectorTileSource
    {
        private Stream mapStream;

        public MapboxVectorTileSource(Stream stream)
        {
            mapStream = stream;
        }

        public int ZoomLevelMin
        {
            get
            {
                return 0;
            }
        }

        public int ZoomLevelMax
        {
            get
            {
                return 127;
            }
        }

        /// <summary>
        /// A Mapbox stream consists only of one tile, so the 
        /// </summary>
        /// <param name="tile"></param>
        /// <returns></returns>
        public IEnumerable<VectorTileLayer> GetTile(VectorTiles.Tile tile)
        {
            return VectorTileParser.Parse(mapStream);
        }
    }
}