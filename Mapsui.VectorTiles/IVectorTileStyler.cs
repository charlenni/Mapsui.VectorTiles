namespace Mapsui.VectorTiles
{
    using Styles;
    using System.Collections.Generic;

    public interface IVectorTileStyler
    {
        /// <summary>
        /// Returns an IStyle for the given tags
        /// </summary>
        /// <param name="tags">Tags for getting the style</param>
        /// <returns></returns>
        IStyle GetStyle(List<Tag> tags);
    }
}