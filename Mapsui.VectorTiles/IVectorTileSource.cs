﻿namespace Mapsui.VectorTiles
{
    using Providers;
    using System.Collections.Generic;

    public interface IVectorTileSource
    {
        /// <summary>
        /// Get all Features for the given tile. 
        /// Each Feature has the styling, which is correct for the given Tags.
        /// </summary>
        /// <param name="tile">Tile, for which Feature are requested</param>
        /// <returns></returns>
        List<IFeature> GetTile(Tile tile);
    }
}