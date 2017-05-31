namespace Mapsui.VectorTiles.Mapbox.Tests
{
    using Geometries;
    using NUnit.Framework;
    using System.Collections.Generic;

    public class SignedAreaTests
    {
        [Test]
        public void SignedAreaTest()
        {
            // arrange
            // create a closed polygon (first point is the same as the last)
            var points = new VectorTileGeometry(new List<Point>
            {
                new Point() { X = 1, Y = 1 },
                new Point() { X = 2, Y = 2 },
                new Point() { X = 3, Y = 1 },
                new Point() { X = 1, Y = 1 },
            });

            var polygon = new VectorTilePolygon(points);

            // act
            var area = polygon.SignedArea();

            // assert
            // polygon is defined clock-wise so area should be negative
            Assert.IsTrue(area == -1);
        }
    }
}