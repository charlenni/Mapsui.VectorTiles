namespace Mapsui.VectorTiles.MapsforgeStyler.Rules
{
    public sealed class Closed
    {
        private readonly Rule outerInstance;

        public Closed(Rule outerInstance)
        {
            this.outerInstance = outerInstance;
        }

        public static readonly int NO = 1 << 0;
        public static readonly int YES = 1 << 1;
        public static readonly int ANY = NO | YES;
    }
}
