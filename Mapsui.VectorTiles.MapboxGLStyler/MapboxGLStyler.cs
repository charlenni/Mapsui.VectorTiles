using System.Collections.Generic;
using System.IO;
using Mapsui.Geometries;
using Mapsui.Styles;
using Mapsui.VectorTiles.MapboxGLStyler.Converter;
using Newtonsoft.Json;

namespace Mapsui.VectorTiles.MapboxGLStyler
{
    public class MapboxGLStyler : IVectorTileStyler
    {
        MapboxJson styleJson;
        private PaintConverter paintConverter;

        private int SpriteBitmap;
        private Dictionary<string, Atlas> SpriteAtlas = new Dictionary<string, Atlas>();

        public MapboxGLStyler(Stream input)
        {
            using (var reader = new StreamReader(input))
                styleJson = JsonConvert.DeserializeObject<MapboxJson>(reader.ReadToEnd());

            if (styleJson.Center != null)
                Center = Projection.SphericalMercator.FromLonLat(styleJson.Center[0], styleJson.Center[1]);
            Zoom = styleJson.Zoom ?? 12;

            // Save urls for sprite and glyphs
            SpriteUrl = styleJson.Sprite;
            GlyphsUrl = styleJson.Glyphs;

            var filterConverter = new FilterConverter();
            paintConverter = new PaintConverter();

            foreach (var layer in styleJson.Layers)
            {
                if (layer.Type.Equals("background") && layer.Paint.BackgroundColor != null)
                    Background = paintConverter.ConvertColor(layer.Paint.BackgroundColor);

                // Create filters for each layer
                if (layer.NativeFilter != null)
                    layer.Filter = filterConverter.ConvertFilter(layer.NativeFilter);
            }
        }

        public Color Background { get; }

        public Point Center { get; }

        public float Zoom { get; }

        public string SpriteUrl { get; }

        public string GlyphsUrl { get; }

        /// <summary>
        /// Create sprite atlas from a stream
        /// </summary>
        /// <param name="jsonStream">Stream with Json sprite file information</param>
        /// <param name="jsonAtlasBitmap">Stream with containing bitmap with sprite atlas bitmap</param>
        public void CreateSprites(Stream jsonStream, Stream jsonAtlasBitmap)
        {
            string json;

            using (var reader = new StreamReader(jsonStream))
            {
                json = reader.ReadToEnd();
            }

            var atlasBitmapId = Styles.BitmapRegistry.Instance.Register(jsonAtlasBitmap);

            CreateSprites(json, atlasBitmapId);
        }

        /// <summary>
        /// Create sprite atlas from a string
        /// </summary>
        /// <param name="json">String with Json sprite file information</param>
        /// <param name="atlasBitmapId">Id of Mapsui bitmap with sprite atlas bitmap</param>
        public void CreateSprites(string json, int atlasBitmapId)
        {
            SpriteBitmap = atlasBitmapId;

            var sprites = JsonConvert.DeserializeObject<Dictionary<string, Atlas>>(json);

            foreach (var sprite in sprites)
            {
                sprite.Value.BitmapId = BitmapRegistry.Instance.Register(sprite.Value.ToMapsui());
                if (sprite.Value.BitmapId >= 0)
                    SpriteAtlas.Add(sprite.Key, sprite.Value);
            }
        }

        public IList<IStyle> GetStyle(VectorTileLayer layer, EvaluationContext context)
        {
            List<IStyle> styles = new List<IStyle>();

            if (layer == null)
                return styles;

            foreach (var styleLayer in styleJson.Layers)
            {
                if (styleLayer.SourceLayer == null || !styleLayer.SourceLayer.Equals(layer.Name))
                    continue;

                if (styleLayer.ZoomMin != null && styleLayer.ZoomMin > context.Zoom)
                    continue;

                if (styleLayer.ZoomMax != null && styleLayer.ZoomMax < context.Zoom)
                    continue;

                if (styleLayer.Filter == null || styleLayer.Filter.Evaluate(context))
                {
                    // We found a style for this feature
                    // So create the new style

                    // TODO: Caching
                    // Check with hash, if we have for this type of feature and zoom allready 
                    // a style created?
                    if (false)
                    { }
                    else
                    {
                        // Create style for this feature
                        if (styleLayer.Paint != null)
                            styles.AddRange(paintConverter.ConvertPaint(context, styleLayer, SpriteAtlas));
                        // TODO: Cache it
                    }
                }
            }

            return styles;
        }
    }
}