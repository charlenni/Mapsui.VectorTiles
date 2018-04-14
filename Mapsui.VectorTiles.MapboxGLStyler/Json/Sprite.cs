using Newtonsoft.Json;

namespace Mapsui.VectorTiles.MapboxGLStyler.Json
{
    /// <summary>
    /// Class holding Sprite data in Json format
    /// </summary>
    public class Sprite
    {
        [JsonProperty("x")]
        public int X { get; set; }

        [JsonProperty("y")]
        public int Y { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("pixelRatio")]
        public float PixelRatio { get; set; }

        public int BitmapId { get; internal set; } = -1;

        public Styles.Sprite ToMapsui()
        {
            return new Styles.Sprite(BitmapId, X, Y, Width, Height, PixelRatio);
        }
    }
}
