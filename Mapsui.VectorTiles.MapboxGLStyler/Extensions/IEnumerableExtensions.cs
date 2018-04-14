namespace Mapsui.VectorTiles.MapboxGLFormat
{
    using System.Collections.Generic;
    using System.Linq;

    public static class EnumerableExtensions
    {
        public static IEnumerable<T> GetOdds<T>(this IEnumerable<T> sequence)
        {
            return sequence.Where((item, index) => index % 2 != 0);
        }

        public static IEnumerable<T> GetEvens<T>(this IEnumerable<T> sequence)
        {
            return sequence.Where((item, index) => index % 2 == 0);
        }
    }
}