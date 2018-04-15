namespace Mapsui.VectorTiles.Mapbox.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Mapsui.VectorTiles.MapboxGLFormat;
    using NUnit.Framework;

    public class GeometryParserTests
    {
        [Test]
        public void AnotherGeometryParserTest()
        {
            var input = new List<uint> {9, 7796, 3462};
            var output = GeometryParser.ParseGeometry(input, Tile.GeomType.Point, 0, 0, 256/4096);
            Assert.IsTrue(output.ToList().Count == 1);
			Assert.IsTrue(output.ToList()[0].Points[0].X == 3898);
			Assert.IsTrue(output.ToList()[0].Points[0].Y == 1731);
        }
    }
}