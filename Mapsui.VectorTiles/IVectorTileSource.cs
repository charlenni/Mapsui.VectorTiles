namespace Mapsui.VectorTiles
{
    using System.Collections.Generic;

    public interface IVectorTileSource
    {
        /// <summary>
        /// May return null
        /// </summary>
        /// <param name="tile">Tile data</param>
        /// <returns></returns>
        IEnumerable<VectorTileLayer> GetTile(Tile tile);

        /// <summary>
        /// Minimal zoom level for this tile source
        /// </summary>
        int ZoomLevelMin { get; }

        /// <summary>
        /// Maximal zoom level for this tile source
        /// </summary>
        int ZoomLevelMax { get; }
    }
}