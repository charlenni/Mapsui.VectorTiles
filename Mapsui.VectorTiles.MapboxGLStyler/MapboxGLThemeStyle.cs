using System;
using System.Collections.Generic;
using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using Mapsui.VectorTiles.MapboxGLStyler.Converter;
using Mapsui.VectorTiles.MapboxGLStyler.Json;

namespace Mapsui.VectorTiles.MapboxGLStyler
{
    public class MapboxGLThemeStyle : IThemeStyle
    {
        private readonly StyleLayerConverter _converter;
        private readonly StyleLayer _styleLayer;
        private readonly Dictionary<string, Styles.Sprite> _sprites;

        public double MinVisible { get; set; }
        public double MaxVisible { get; set; }
        public bool Enabled { get; set; }
        public float Opacity { get; set; }

        public MapboxGLThemeStyle(StyleLayerConverter converter, StyleLayer styleLayer, Dictionary<string, Styles.Sprite> sprites)
        {
            _converter = converter;
            _styleLayer = styleLayer;
            _sprites = sprites;
        }

        public IStyle GetStyle(IFeature f, double resolution)
        {
            if (f.Geometry is IRaster)
            {
                return _converter.ConvertRasterLayer((float)resolution, _styleLayer);
            }

            if (!(f is VectorTileFeature))
                return null;

            var feature = (VectorTileFeature) f;
            var styles = new StyleCollection { Enabled = true, MinVisible = double.NegativeInfinity, MaxVisible = double.PositiveInfinity };
            var context = new EvaluationContext((float)resolution, feature);

            if (_styleLayer.Filter == null || _styleLayer.Filter.Evaluate(context))
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
                    if (_styleLayer.Paint != null || _styleLayer.Layout != null)
                    {
                        foreach (var style in _converter.Convert(context, _styleLayer, _sprites))
                        {
                            styles.Add(style);
                            if (styles.MinVisible < style.MinVisible)
                                styles.MinVisible = style.MinVisible;
                            if (styles.MaxVisible > style.MaxVisible)
                                styles.MaxVisible = style.MaxVisible;
                        }
                    }

                    // TODO: Cache it
                }
            }

            if (styles.Count == 0)
                return null;

            if (styles.Count == 1)
                return styles[0];

            return styles;
        }

        private double FromResolution(double resolution)
        {
            var zoom = Math.Log(78271.51696401953125 / resolution, 2);

            return zoom < 0 ? 0 : zoom;
        }

    }
}
