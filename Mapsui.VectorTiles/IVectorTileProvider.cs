namespace Mapsui.VectorTiles
{
    using System.Collections.Generic;

    public interface IVectorTileProvider
    {
        /// <summary>
        /// May return null
        /// </summary>
        /// <param name="tile">Tile data</param>
        /// <returns></returns>
        IEnumerable<VectorTileLayer> GetTile(Tile tile);
    }
}