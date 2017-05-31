namespace Mapsui.VectorTiles.Mapbox.Tests
{
    using Geometries;
    using System.Collections.Generic;

    public class TestData
    {
        public static VectorTileGeometry GetCWPolygon(int factor)
        {
            var firstp = new Point() { X = factor, Y = factor };
            var secondp = new Point() { X = factor, Y = -factor };
            var thirdp = new Point() { X = -factor, Y = -factor };
            var fourthp = new Point() { X = -factor, Y = factor };
            var coords = new VectorTileGeometry(new List<Point> { firstp, secondp, thirdp, fourthp, firstp });
            return coords;
        }

        public static VectorTileGeometry GetCCWPolygon(int factor)
        {
            // arrange
            var firstp = new Point() { X = factor, Y = factor };
            var secondp = new Point() { X = -factor, Y = factor };
            var thirdp = new Point() { X = -factor, Y = -factor };
            var fourthp = new Point() { X = factor, Y = -factor };
            var coords = new VectorTileGeometry(new List<Point> { firstp, secondp, thirdp, fourthp, firstp });
            return coords;
        }
    }
}