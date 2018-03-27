using System.Collections.Generic;
using Mapsui.Styles;

namespace Mapsui.VectorTiles
{
    public interface IVectorTileStyler
    {
        /// <summary>
        /// Returns an IStyle for the given tags
        /// </summary>
        /// <param name="layer">Vector tile layer, for which the style is looked up</param>
        /// <param name="context">Context for lookup</param>
        /// <returns></returns>
        List<IStyle> GetStyle(VectorTileLayer layer, EvaluationContext context);
    }
}