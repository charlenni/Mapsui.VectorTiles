namespace Mapsui.VectorTiles.MapsforgeStyler.Rules
{
    public sealed class Selector
    {
        private readonly Rule outerInstance;

        public Selector(Rule outerInstance)
        {
            this.outerInstance = outerInstance;
        }

        public const int ANY = 0;
        public static readonly int FIRST = 1 << 0;
        public static readonly int WHEN_MATCHED = 1 << 1;
    }
}
