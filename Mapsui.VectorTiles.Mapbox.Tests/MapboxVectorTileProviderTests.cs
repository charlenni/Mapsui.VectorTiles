using BruTile;

namespace Mapsui.VectorTiles.Mapbox.Tests
{
    using NUnit.Framework;
    using SQLite;
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Reflection;

    class Map
    {
        public int zoom_level { get; set; }
        public int tile_column { get; set; }
        public int tile_row { get; set; }
        public string tile_id { get; set; }
        public string grid_id { get; set; }
    }

    class Images
    {
        public string tile_id { get; set; }
        public byte[] tile_data { get; set; }
    }

    public class MapboxVectorTileProviderTests
    {
        private object temp = new object();

        [Test]
        public void TestMapboxVectorTileProvider()
        {
            string path = Path.GetTempPath();
            string fileName = "monaco.mbtiles";
            string fullPath = Path.Combine(new string[] { path, fileName});
            if (!File.Exists(fullPath))
            {
                using (Stream inputStream = EmbeddedResourceLoader.Load("Resources." + fileName, this.GetType()))
                {
                    using (Stream outputStream = File.Create(fullPath))
                    {
                        inputStream.CopyTo(outputStream);
                    }
                }
            }

            var tileCol = 8529;
            var tileRow = 10410;
            var tileLevel = 14;

            var sql = new SQLiteConnection(fullPath);

            var tile = sql.Query<Map>("select * from map where zoom_level=? and tile_column=? and tile_row=?", new object[] { tileLevel, tileCol, tileRow });

            Assert.AreEqual(tile.Count, 1);

            var pbf = sql.Query<Images>("select * from images where tile_id=?", tile[0].tile_id);

            Assert.AreEqual(pbf.Count, 1);

            // Data is zipped
            var zippedData = new MemoryStream(pbf[0].tile_data);
            var unzippedData = new GZipStream(zippedData, CompressionMode.Decompress);

            var tileInfo = new TileInfo();

            tileInfo.Index = new TileIndex(tileCol, tileRow, tileLevel.ToString());

            var provider = new MapboxVectorTileSource(unzippedData);
            var layers = provider.GetTile(tileInfo);
        }
    }
}