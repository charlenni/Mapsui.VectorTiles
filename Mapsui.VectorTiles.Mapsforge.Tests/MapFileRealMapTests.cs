namespace Mapsui.VectorTiles.Mapsforge.Reader.Tests
{
    using Reader;
    using NUnit.Framework;
    using Header;
    using Geometries;
    using System.Collections.Generic;

    public class MapFileRealMapTests
    {
        private MapFile mapFile;

        private void CheckMapFile()
        {
            if (mapFile == null)
            {
                mapFile = new MapFile(EmbeddedResourceLoader.Load("Resources.RealMap.monaco.map", this.GetType()));
            }
        }

        [Test()]
        public virtual void RealMapTest()
        {
            CheckMapFile();

            MapFileInfo mapFileInfo = mapFile.MapFileInfo;

            Assert.AreEqual(mapFileInfo.FileSize, 156836);
            Assert.AreEqual(mapFileInfo.FileVersion, 3);
            Assert.AreEqual(mapFileInfo.BoundingBox, new BoundingBox(7.309206, 43.623385, 7.548547, 43.851689));
            Assert.AreEqual(mapFileInfo.StartPosition, new Point(7.4391, 43.7372));
            Assert.AreEqual(mapFileInfo.LanguagesPreference, "en");
            Assert.AreEqual(mapFileInfo.ProjectionName, "Mercator");
            Assert.AreEqual(mapFileInfo.NumberOfSubFiles, 3);
            Assert.AreEqual(mapFileInfo.TilePixelSize, 256);
            Assert.AreEqual(mapFileInfo.ZoomLevelMin, 0);
            Assert.AreEqual(mapFileInfo.ZoomLevelMax, 21);
            Assert.AreEqual(mapFileInfo.StartZoomLevel, 14);
            Assert.AreEqual(mapFileInfo.PoiTags.Length, 54);
            Assert.AreEqual(mapFileInfo.WayTags.Length, 76);
        }

        [Test()]
        public virtual void PoiTagsTest()
        {
            CheckMapFile();

            Assert.AreEqual(mapFile.MapFileInfo.PoiTags[0].Key, "highway");
            Assert.AreEqual(mapFile.MapFileInfo.PoiTags[0].Value, "bus_stop");
            Assert.AreEqual(mapFile.MapFileInfo.PoiTags[10].Key, "amenity");
            Assert.AreEqual(mapFile.MapFileInfo.PoiTags[10].Value, "fast_food");
            Assert.AreEqual(mapFile.MapFileInfo.PoiTags[50].Key, "shop");
            Assert.AreEqual(mapFile.MapFileInfo.PoiTags[50].Value, "mall");
        }

        [Test()]
        public virtual void WayTagsTest()
        {
            CheckMapFile();

            Assert.AreEqual(mapFile.MapFileInfo.WayTags[0].Key, "building");
            Assert.AreEqual(mapFile.MapFileInfo.WayTags[0].Value, "yes");
            Assert.AreEqual(mapFile.MapFileInfo.WayTags[10].Key, "highway");
            Assert.AreEqual(mapFile.MapFileInfo.WayTags[10].Value, "steps");
            Assert.AreEqual(mapFile.MapFileInfo.WayTags[70].Key, "building");
            Assert.AreEqual(mapFile.MapFileInfo.WayTags[70].Value, "hangar");
        }

        [Test()]
        public virtual void MapsforgeVectorTileProviderTest()
        {
            CheckMapFile();

            MapsforgeVectorTileSource provider = new MapsforgeVectorTileSource(EmbeddedResourceLoader.Load("Resources.RealMap.monaco.map", this.GetType()));

            IEnumerable<VectorTileLayer> layers = provider.GetTile(new Tile(8529, 5973, 14));
        }
    }
}