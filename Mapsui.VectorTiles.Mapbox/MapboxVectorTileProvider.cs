namespace Mapsui.VectorTiles.Mapbox
{
    using System.Collections.Generic;
    using System.IO;

    public class MapboxVectorTileProvider : IVectorTileProvider
    {
        private Stream mapStream;

        public MapboxVectorTileProvider(Stream stream)
        {
            mapStream = stream;
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