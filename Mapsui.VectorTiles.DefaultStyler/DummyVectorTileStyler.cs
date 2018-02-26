namespace Mapsui.VectorTiles.DummyStyler
{
    using System;
    using System.Collections.Generic;
    using Styles;

    public class DummyVectorTileStyler : IVectorTileStyler
    {
        public IStyle GetStyle(List<Tag> tags)
        {
            return null;
        }
    }
}