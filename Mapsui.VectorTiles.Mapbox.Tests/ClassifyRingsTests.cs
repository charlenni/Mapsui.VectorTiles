namespace Mapsui.VectorTiles.Mapbox.Tests
{
    using Geometries;
    using NUnit.Framework;
    using System.Collections.Generic;

    public class ClassifyRingsTests
    {
        [Test]
        public void TestReversePolygon()
        {
            var poly1 = TestData.GetCWPolygon(2);
            Assert.IsTrue(new VectorTilePolygon(poly1).IsCW());
            poly1.Reverse();
            Assert.IsTrue(new VectorTilePolygon(poly1).IsCCW());
        }

        [Test]
        public void TestClassifyRingsWithTwoOuterrings()
        {
            // arrange
            var coords = new List<VectorTileGeometry>();
            var poly1 = TestData.GetCCWPolygon(2);
            var poly2 = TestData.GetCCWPolygon(1);
            coords.Add(poly1);
            coords.Add(poly2);

            // act
            var classify = ClassifyRings.Classify(coords);

            // assert
            Assert.IsTrue(classify.Count == 2);
            Assert.IsTrue(classify[0].Count == 1);
            Assert.IsTrue(classify[1].Count == 1);
        }

        [Test]
        public void TestClassifyRingsWithOuterAndInnerRing()
        {
            // arrange
            var coords = new List<VectorTileGeometry>();
            var poly1 = TestData.GetCCWPolygon(2);
            var poly2 = TestData.GetCWPolygon(1);
            coords.Add(poly1);
            coords.Add(poly2);

            // act
            var classify = ClassifyRings.Classify(coords);

            // assert
            Assert.IsTrue(classify.Count == 1);
            Assert.IsTrue(classify[0].Count == 2);
        }

        [Test]
        public void TestClassifyRingsWithOuterAndInnerRingAndOuterring()
        {
            // arrange
            var coords = new List<VectorTileGeometry>();
            var poly1 = TestData.GetCCWPolygon(3);
            var poly2 = TestData.GetCWPolygon(2);
            var poly3 = TestData.GetCCWPolygon(1);

            coords.Add(poly1);
            coords.Add(poly2);
            coords.Add(poly3);

            // act
            var classify = ClassifyRings.Classify(coords);

            // assert
            Assert.IsTrue(classify.Count == 2);
            Assert.IsTrue(classify[0].Count == 2);
            Assert.IsTrue(classify[1].Count == 1);
        }
    }
}