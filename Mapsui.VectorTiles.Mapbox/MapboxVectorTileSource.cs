using System.Globalization;
using System.Linq;
using BruTile;
using SQLite;
using System.IO.Compression;
using Mapsui.Geometries;

namespace Mapsui.VectorTiles.Mapbox
{
    using System.Collections.Generic;
    using System.IO;

    public class MapboxVectorTileSource : IVectorTileSource
    {
        private Stream mapStream;
        private SQLiteConnection connection;

        public MapboxVectorTileSource(Stream stream)
        {
            mapStream = stream;

            connection = new SQLiteConnection("S:\\Entwicklung\\Mapsui.VectorTiles\\Samples\\Mapsui.VectorTiles.Sample.Wpf\\monaco.mbtiles");

            var metadata = connection.Query<Mapbox.metadata>("SELECT * FROM metadata");

            ZoomLevelMin = int.Parse(connection.Query<Mapbox.metadata>("SELECT * FROM metadata WHERE name=?", "minzoom").First().value);
            ZoomLevelMax = int.Parse(connection.Query<Mapbox.metadata>("SELECT * FROM metadata WHERE name=?", "maxzoom").First().value);

            var result = connection.Query<Mapbox.metadata>("SELECT * FROM metadata WHERE name=?", "name");
            if (result != null && result.Count > 0)
                Name = result.First().value;

           result = connection.Query<Mapbox.metadata>("SELECT * FROM metadata WHERE name=?", "attribution");
            if (result != null && result.Count > 0)
                Attribution = result.First().value;

            result = connection.Query<Mapbox.metadata>("SELECT * FROM metadata WHERE name=?", "json");
            if (result != null && result.Count > 0)
            {
                string temp = result.First().value;
            }

            result = connection.Query<Mapbox.metadata>("SELECT * FROM metadata WHERE name=?", "bounds");
            string bounds = string.Empty;
            if (result != null && result.Count > 0)
                bounds = result.First().value;

            if (!string.IsNullOrWhiteSpace(bounds))
            {
                var list = bounds.Split(',');
                var numberFormat = new CultureInfo("en-US").NumberFormat;
                var point1 = new Point(double.Parse(list[0], numberFormat), double.Parse(list[1], numberFormat));
                var point2 = new Point(double.Parse(list[2], numberFormat), double.Parse(list[3], numberFormat));
                var bb = new BoundingBox(new Point(813637.25, 5375558), new Point(849720.25, 5442556.5));
            }
        }

        public int ZoomLevelMin { get; } = 0;

        public int ZoomLevelMax { get; } = 127;

        public string Name { get; } = "Unknown";

        public string Attribution { get; }

        /// <summary>
        /// A Mapbox stream consists only of one tile, so the 
        /// </summary>
        /// <param name="tile"></param>
        /// <returns></returns>
        public IEnumerable<VectorTileLayer> GetTile(TileInfo tileInfo)
        {
            System.Diagnostics.Debug.WriteLine($"Tile Row={tileInfo.Index.Row} Col={tileInfo.Index.Col} Zoom={tileInfo.Index.Level}");

            var imageData = connection.Query<images>("SELECT images.tile_id,images.tile_data FROM map,images WHERE map.tile_id=images.tile_id AND map.zoom_level=? AND map.tile_row=? AND map.tile_column=?", new object[] { tileInfo.Index.Level, tileInfo.Index.Row, tileInfo.Index.Col });

            if (imageData == null || imageData.Count == 0)
                return new List<VectorTileLayer>();

            return VectorTileParser.Parse(new GZipStream(new MemoryStream(imageData.First().tile_data), CompressionMode.Decompress));
        }
    }
}