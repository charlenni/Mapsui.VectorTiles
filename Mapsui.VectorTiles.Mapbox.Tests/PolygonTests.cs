namespace Mapsui.VectorTiles.Mapbox.Tests
{
    using NUnit.Framework;

    public class PolygonTests
    {
        [Test]
        public void TestCWPolygon()
        {
            // arrange
            var coords = TestData.GetCWPolygon(1);
            var poly = new VectorTilePolygon(coords);

            // act
            var ccw = poly.IsCW();

            // assert
            Assert.IsTrue(poly.SignedArea() < 0);
            Assert.IsTrue(ccw);
        }

        [Test]
        public void TestCCWPolygon()
        {
            var coords = TestData.GetCCWPolygon(1);
            var poly = new VectorTilePolygon(coords);

            // act
            var ccw = poly.IsCCW();

            // assert
            Assert.IsTrue(poly.SignedArea() > 0);
            Assert.IsTrue(ccw);
        }
    }
}