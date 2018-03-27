namespace Mapsui.VectorTiles.Mapbox.Tests
{
    using System.Reflection;
    using NUnit.Framework;
    using ProtoBuf;

    public class AttributesParserTests
    {
        [Test]
        public void TestAttributeParser()
        {
            // arrange
            const string mapboxfile = "Mapsui.VectorTiles.Mapbox.Tests.Resources.14-8801-5371.vector.pbf";
            var pbfStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(mapboxfile);
            var tile = Serializer.Deserialize<Tile>(pbfStream);
            var keys = tile.Layers[0].Keys;
            var values = tile.Layers[0].Values;
            var tagsf1 = tile.Layers[0].Features[0].Tags;

            // act
            var attributes = TagsParser.Parse(keys, values, tagsf1);

            // assert
            Assert.IsTrue(attributes.Count == 2);
            Assert.IsTrue(attributes.ContainsKey("class"));
            Assert.IsTrue(attributes["class"].ToString() == "park");
            Assert.IsTrue(attributes.ContainsKey("osm_id"));
            Assert.IsTrue(attributes["osm_id"].ToString() == "3000000224480");
        }
    }
}
