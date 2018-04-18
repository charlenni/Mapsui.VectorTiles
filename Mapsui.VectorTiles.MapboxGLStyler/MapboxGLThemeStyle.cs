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
        private readonly Viewport _viewport;
        private readonly SymbolProvider _symbolProvider;

        public double MinVisible { get; set; }
        public double MaxVisible { get; set; }
        public bool Enabled { get; set; }
        public float Opacity { get; set; }
        public float Zoom { get; private set; }
        public int ZoomLevel { get; private set; }

        public MapboxGLThemeStyle(StyleLayerConverter converter, StyleLayer styleLayer, Dictionary<string, Styles.Sprite> sprites, Viewport viewport, SymbolProvider symbolProvider)
        {
            _converter = converter;
            _styleLayer = styleLayer;
            _sprites = sprites;
            _viewport = viewport;
            _symbolProvider = symbolProvider;

            // Do this only once
            if (_viewport != null)
                _viewport.ViewportChanged += (s, e) => { Zoom = FromResolution(_viewport.Resolution); ZoomLevel = (int)Math.Floor(Zoom); };
        }

        public IStyle GetStyle(IFeature f)
        {
            if (_viewport == null)
                return null;

            float resolution = (float)_viewport.Resolution;

            if (f.Geometry is IRaster)
            {
                return _converter.ConvertRasterLayer(Zoom, _styleLayer);
            }

            if (!(f is VectorTileFeature))
                return null;

            var feature = (VectorTileFeature) f;

            // Is this style for given feature?
            if (_styleLayer.SourceLayerHash != feature.VectorTileLayer)
                return null;

            var context = new EvaluationContext(Zoom, feature);

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
                        var styles = _converter.Convert(context, _styleLayer, _sprites, _symbolProvider);

                        if (styles == null || styles.Count == 0)
                            return null;
                        
                        if (styles.Count == 1)
                            return styles[0];

                        var collection = new StyleCollection { Enabled = true, MinVisible = double.NegativeInfinity, MaxVisible = double.PositiveInfinity };

                        foreach (var style in styles)
                        {
                            collection.Add(style);
                            if (collection.MinVisible < style.MinVisible)
                                collection.MinVisible = style.MinVisible;
                            if (collection.MaxVisible > style.MaxVisible)
                                collection.MaxVisible = style.MaxVisible;
                        }

                        return collection;
                    }

                    // TODO: Cache it
                }
            }

            return null;
        }

        private float FromResolution(double resolution)
        {
            var zoom = (float)Math.Log(78271.51696401953125 / resolution, 2);

            return zoom < 0f ? 0f : zoom;
        }
    }
}
