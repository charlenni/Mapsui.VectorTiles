using System.Collections.Generic;
using System.IO;
using Mapsui.Geometries;
using Mapsui.Styles;
using Mapsui.VectorTiles.MapboxGLStyler.Converter;
using Mapsui.VectorTiles.MapboxGLStyler.Filter;
using Newtonsoft.Json;

namespace Mapsui.VectorTiles.MapboxGLStyler
{
    public class MapboxGLStyler : IVectorTileStyler
    {
        MapboxJson styleJson;
        private PaintConverter paintConverter;

        public MapboxGLStyler(Stream input)
        {
            using (var reader = new StreamReader(input))
                styleJson = JsonConvert.DeserializeObject<MapboxJson>(reader.ReadToEnd());

            Center = Projection.SphericalMercator.FromLonLat(styleJson.Center[0], styleJson.Center[1]);
            Zoom = styleJson.Zoom ?? 12;

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

        public List<IStyle> GetStyle(VectorTileLayer layer, EvaluationContext context)
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
                            styles.AddRange(paintConverter.ConvertPaint(context, styleLayer));
                        // TODO: Cache it
                    }
                }
            }

            return styles;
        }
    }
}