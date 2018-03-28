using BruTile;

namespace Mapsui.VectorTiles
{
    using Providers;
    using System.Collections.Generic;

    public interface IVectorTileProvider : IProvider
    {
        /// <summary>
        /// Get all Features for the given tile. 
        /// Each Feature has the styling, which is correct for the given Tags.
        /// </summary>
        /// <param name="tileInfo">TileInfo, for which Feature are requested</param>
        /// <param name="zoomFactor">Zoom factor, for which Feature should be styled</param>
        /// <returns>List of features to display</returns>
        List<IFeature> GetTile(TileInfo tileInfo, float zoomFactor);
    }
}