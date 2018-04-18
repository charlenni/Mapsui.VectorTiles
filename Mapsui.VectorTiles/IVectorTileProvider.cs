using BruTile;
using Mapsui.Providers;

namespace Mapsui.VectorTiles
{
    public interface IVectorTileProvider : IProvider
    {
        ITileSource TileSource { get; }
    }
}