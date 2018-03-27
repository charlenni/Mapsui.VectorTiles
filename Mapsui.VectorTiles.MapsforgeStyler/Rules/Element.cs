namespace Mapsui.VectorTiles.MapsforgeStyler.Rules
{
    public sealed class Element
    {
        private readonly Rule outerInstance;

        public Element(Rule outerInstance)
        {
            this.outerInstance = outerInstance;
        }

        public static readonly int NODE = 1 << 0;
        public static readonly int LINE = 1 << 1;
        public static readonly int POLY = 1 << 2;
        public static readonly int WAY = LINE | POLY;
        public static readonly int ANY = NODE | WAY;
    }
}
