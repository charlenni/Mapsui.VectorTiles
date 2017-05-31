namespace Mapsui.VectorTiles.Mapbox.Tests
{
    using NUnit.Framework;
    using System.Reflection;

    public class LotsOfTagsTest
    {
        [Test]
        public void TestLotsOfTags()
        {
            // arrange
            const string mapboxfile = "Mapsui.VectorTiles.Mapbox.Tests.Resources.lots-of-tags.vector.pbf";
            var pbfStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(mapboxfile);

            // act
            var layerInfos = VectorTileParser.Parse(pbfStream);

            // assert
            Assert.IsTrue(layerInfos[0]!=null);
        }
    }
}
