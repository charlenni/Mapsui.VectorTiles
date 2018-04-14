using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BruTile;
using BruTile.Cache;
using BruTile.Predefined;
using BruTile.Web;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.UI;
using Mapsui.VectorTiles.MapboxGLFormat;
using Mapsui.VectorTiles.MapboxGLStyler.Converter;
using Mapsui.VectorTiles.MapboxGLStyler.Extensions;
using Mapsui.VectorTiles.MapboxGLStyler.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;

namespace Mapsui.VectorTiles.MapboxGLStyler
{
    public class MapboxGLStyler
    {
        MapboxGL styleJson;

        private int SpriteBitmap;
        private Dictionary<string, Styles.Sprite> SpriteAtlas = new Dictionary<string, Styles.Sprite>();

        public MapboxGLStyler(Stream input, Map map)
        {
            using (var reader = new StreamReader(input))
                styleJson = JsonConvert.DeserializeObject<MapboxGL>(reader.ReadToEnd());

            Zoom = styleJson.Zoom ?? 12;

            // Save urls for sprite and glyphs
            SpriteUrl = styleJson.Sprite;
            GlyphsUrl = styleJson.Glyphs;

            var filterConverter = new FilterConverter();
            var styleLayerConverter = new StyleLayerConverter();

            foreach (var styleLayer in styleJson.StyleLayers)
            {
                if (styleLayer.Type.Equals("background") && styleLayer.Paint.BackgroundColor != null)
                {
                    Background = styleLayer.Paint.BackgroundColor;
                    map.BackColor = Background;
                }

                // Create filters for each style layer
                if (styleLayer.NativeFilter != null)
                    styleLayer.Filter = filterConverter.ConvertFilter(styleLayer.NativeFilter);

                // Create ThemeStyles for each style layer
                styleLayer.Style = new MapboxGLThemeStyle(styleLayerConverter, styleLayer, SpriteAtlas)
                {
                    MinVisible = styleLayer.MinVisible,
                    MaxVisible = styleLayer.MaxVisible
                };
            }

            // Read all tile sources
            foreach (var source in styleJson.Sources)
            {
                var sourceJson = source.Value;

                switch (source.Value.Type)
                {
                    case "raster":
                        if (!string.IsNullOrEmpty(source.Value.Url))
                        {
                            // Get TileJSON from http/https url
                            sourceJson = new Source();
                        }
                        // Create new raster layer
                        var rasterLayer = CreateRasterLayer(sourceJson);

                        if (rasterLayer != null)
                        {
                            rasterLayer.Name = source.Key;

                            rasterLayer.Style = new StyleCollection();

                            // Add all ThemeStyles for this layer
                            foreach (var styleLayer in styleJson.StyleLayers)
                            {
                                if (!rasterLayer.Name.Equals(styleLayer.Source, StringComparison.CurrentCultureIgnoreCase))
                                    continue;

                                ((StyleCollection)rasterLayer.Style).Add(styleLayer.Style);
                            }

                            map.Layers.Add(rasterLayer);
                        }

                        break;
                    case "vector":
                        if (!string.IsNullOrEmpty(source.Value.Url))
                        {
                            if (source.Value.Url.StartsWith("http"))
                            {
                                // TODO: Get TileJSON from http/https url
                                sourceJson = JsonConvert.DeserializeObject<Source>(@"{'tiles':['https://free.tilehosting.com/data/v3/{z}/{x}/{y}.pbf.pict?key=tXiQqN3lIgskyDErJCeY'],'name':'OpenMapTiles','format':'pbf','basename':'v3.7.mbtiles','id':'openmaptiles','attribution':'<a href=\'http://www.openmaptiles.org/\' target=\'_blank\'>&copy; OpenMapTiles</a> <a href=\'http://www.openstreetmap.org/about/\' target=\'_blank\'>&copy; OpenStreetMap contributors</a>','description':'A tileset showcasing all layers in OpenMapTiles. http://openmaptiles.org','maxzoom':14,'minzoom':0,'pixel_scale':'256','vector_layers':[{'maxzoom':14,'fields':{'class':'String'},'minzoom':0,'id':'water','description':''},{'maxzoom':14,'fields':{'name:mt':'String','name:pt':'String','name:az':'String','name:cy':'String','name:rm':'String','name:ko':'String','name:kn':'String','name:ar':'String','name:cs':'String','name_de':'String','name:ro':'String','name:it':'String','name_int':'String','name:ru':'String','name:pl':'String','name:ca':'String','name:hu':'String','name:ka':'String','name:fi':'String','name:da':'String','name:de':'String','name:tr':'String','name:fr':'String','name:mk':'String','name:nonlatin':'String','name:fy':'String','name:zh':'String','name:sl':'String','name:lv':'String','name:ja':'String','name:lt':'String','name:no':'String','name:kk':'String','name:sv':'String','name:he':'String','name:ja_rm':'String','name:ga':'String','name:br':'String','name:bs':'String','name:lb':'String','class':'String','name:ko_rm':'String','name:la':'String','name:sk':'String','name:uk':'String','name:hy':'String','name:be':'String','name_en':'String','name:bg':'String','name:hr':'String','name:sr':'String','name:sq':'String','name:el':'String','name:eo':'String','name:en':'String','name':'String','name:gd':'String','name:ja_kana':'String','name:is':'String','name:th':'String','name:latin':'String','name:sr-Latn':'String','name:et':'String','name:nl':'String','name:es':'String'},'minzoom':0,'id':'waterway','description':''},{'maxzoom':14,'fields':{'class':'String','subclass':'String'},'minzoom':0,'id':'landcover','description':''},{'maxzoom':14,'fields':{'class':'String'},'minzoom':0,'id':'landuse','description':''},{'maxzoom':14,'fields':{'name:mt':'String','name:pt':'String','name:az':'String','name:cy':'String','name:rm':'String','name:ko':'String','name:kn':'String','name:ar':'String','name:cs':'String','rank':'Number','name_de':'String','name:ro':'String','name:it':'String','name_int':'String','name:nl':'String','name:pl':'String','ele':'Number','name:ca':'String','name:hu':'String','name:ka':'String','name:fi':'String','name:da':'String','name:de':'String','name:tr':'String','name:fr':'String','name:mk':'String','name:nonlatin':'String','name:fy':'String','name:zh':'String','name:sl':'String','name:lv':'String','name:ja':'String','name:lt':'String','name:no':'String','name:kk':'String','name:sv':'String','name:he':'String','name:ja_rm':'String','name:ga':'String','name:br':'String','name:bs':'String','name:lb':'String','name:ko_rm':'String','name:la':'String','name:sk':'String','name:uk':'String','name:hy':'String','name:ru':'String','name:be':'String','name_en':'String','name:bg':'String','name:hr':'String','name:sr':'String','name:sq':'String','name:el':'String','name:eo':'String','name:en':'String','name':'String','name:gd':'String','ele_ft':'Number','name:ja_kana':'String','name:is':'String','osm_id':'Number','name:th':'String','name:latin':'String','name:sr-Latn':'String','name:et':'String','name:es':'String'},'minzoom':0,'id':'mountain_peak','description':''},{'maxzoom':14,'fields':{'class':'String'},'minzoom':0,'id':'park','description':''},{'maxzoom':14,'fields':{'admin_level':'Number','disputed':'Number','maritime':'Number'},'minzoom':0,'id':'boundary','description':''},{'maxzoom':14,'fields':{'ref':'String','class':'String'},'minzoom':0,'id':'aeroway','description':''},{'maxzoom':14,'fields':{'layer':'Number','service':'String','level':'Number','brunnel':'String','indoor':'Number','ramp':'Number','subclass':'String','oneway':'Number','class':'String'},'minzoom':0,'id':'transportation','description':''},{'maxzoom':14,'fields':{'render_min_height':'Number','render_height':'Number'},'minzoom':0,'id':'building','description':''},{'maxzoom':14,'fields':{'name:mt':'String','name:pt':'String','name:az':'String','name:cy':'String','name:rm':'String','name:ko':'String','name:kn':'String','name:ar':'String','name:cs':'String','name_de':'String','name:ro':'String','name:it':'String','name_int':'String','name:ru':'String','name:pl':'String','name:ca':'String','name:hu':'String','name:ka':'String','name:fi':'String','name:da':'String','name:de':'String','name:tr':'String','name:fr':'String','name:mk':'String','name:nonlatin':'String','name:fy':'String','name:zh':'String','name:sl':'String','name:lv':'String','name:ja':'String','name:lt':'String','name:no':'String','name:kk':'String','name:sv':'String','name:he':'String','name:ja_rm':'String','name:ga':'String','name:br':'String','name:bs':'String','name:lb':'String','class':'String','name:ko_rm':'String','name:la':'String','name:sk':'String','name:uk':'String','name:hy':'String','name:be':'String','name_en':'String','name:bg':'String','name:hr':'String','name:sr':'String','name:sq':'String','name:el':'String','name:eo':'String','name:en':'String','name':'String','name:gd':'String','name:ja_kana':'String','name:is':'String','name:th':'String','name:latin':'String','name:sr-Latn':'String','name:et':'String','name:nl':'String','name:es':'String'},'minzoom':0,'id':'water_name','description':''},{'maxzoom':14,'fields':{'name:mt':'String','name:pt':'String','name:az':'String','name:cy':'String','name:rm':'String','name:ko':'String','name:kn':'String','name:ar':'String','name:cs':'String','layer':'Number','name_de':'String','name:ro':'String','name:it':'String','name_int':'String','name:ru':'String','name:pl':'String','name:ca':'String','name:hu':'String','name:ka':'String','name:fi':'String','name:da':'String','subclass':'String','name:de':'String','indoor':'Number','name:tr':'String','name:fr':'String','name:mk':'String','name:nonlatin':'String','name:fy':'String','name:zh':'String','name:sl':'String','name:lv':'String','name:ja':'String','name:lt':'String','name:no':'String','name:kk':'String','name:sv':'String','name:he':'String','name:ja_rm':'String','name:ga':'String','name:br':'String','name:bs':'String','name:lb':'String','class':'String','name:ko_rm':'String','name:la':'String','name:sk':'String','name:uk':'String','name:hy':'String','name:be':'String','name_en':'String','name:bg':'String','name:hr':'String','name:sr':'String','name:sq':'String','network':'String','name:el':'String','name:eo':'String','name:en':'String','name':'String','name:gd':'String','ref':'String','name:ja_kana':'String','level':'Number','ref_length':'Number','name:is':'String','name:th':'String','name:latin':'String','name:sr-Latn':'String','name:et':'String','name:nl':'String','name:es':'String'},'minzoom':0,'id':'transportation_name','description':''},{'maxzoom':14,'fields':{'name:mt':'String','name:pt':'String','name:az':'String','name:cy':'String','name:rm':'String','name:ko':'String','name:kn':'String','name:ar':'String','name:cs':'String','rank':'Number','name_de':'String','name:ro':'String','name:it':'String','name_int':'String','name:ru':'String','name:pl':'String','name:ca':'String','name:hu':'String','name:ka':'String','name:fi':'String','name:da':'String','name:de':'String','name:tr':'String','name:fr':'String','name:mk':'String','name:nonlatin':'String','name:fy':'String','name:zh':'String','capital':'Number','name:sl':'String','name:lv':'String','name:ja':'String','name:ko_rm':'String','name:no':'String','name:kk':'String','name:sv':'String','name:he':'String','name:ja_rm':'String','name:ga':'String','name:br':'String','name:bs':'String','name:lb':'String','class':'String','name:la':'String','name:sk':'String','name:uk':'String','name:hy':'String','name:be':'String','name_en':'String','name:bg':'String','name:hr':'String','name:sr':'String','name:sq':'String','name:th':'String','name:el':'String','name:eo':'String','name:en':'String','name':'String','name:gd':'String','iso_a2':'String','name:ja_kana':'String','name:is':'String','name:lt':'String','name:latin':'String','name:sr-Latn':'String','name:et':'String','name:nl':'String','name:es':'String'},'minzoom':0,'id':'place','description':''},{'maxzoom':14,'fields':{'housenumber':'String'},'minzoom':0,'id':'housenumber','description':''},{'maxzoom':14,'fields':{'name:mt':'String','name:pt':'String','name:az':'String','name:cy':'String','name:rm':'String','name:ko':'String','name:kn':'String','name:ar':'String','name:cs':'String','rank':'Number','name_de':'String','name:ro':'String','name:it':'String','name_int':'String','name:ru':'String','name:pl':'String','name:ca':'String','name:hu':'String','name:ka':'String','name:fi':'String','name:da':'String','subclass':'String','name:de':'String','name:tr':'String','name:fr':'String','name:mk':'String','name:nonlatin':'String','name:fy':'String','name:zh':'String','name:sl':'String','name:lv':'String','name:ja':'String','name:lt':'String','name:no':'String','name:kk':'String','name:sv':'String','name:he':'String','name:ja_rm':'String','name:ga':'String','name:br':'String','name:bs':'String','name:lb':'String','class':'String','name:ko_rm':'String','name:la':'String','name:sk':'String','name:uk':'String','name:hy':'String','name:be':'String','name_en':'String','name:bg':'String','name:hr':'String','name:sr':'String','name:sq':'String','name:el':'String','name:eo':'String','name:en':'String','name':'String','name:gd':'String','name:ja_kana':'String','name:is':'String','name:th':'String','agg_stop':'Number','name:latin':'String','name:sr-Latn':'String','name:et':'String','name:nl':'String','name:es':'String'},'minzoom':0,'id':'poi','description':''},{'maxzoom':14,'fields':{'name:mt':'String','name:pt':'String','name:az':'String','name:cy':'String','name:rm':'String','name:ko':'String','name:kn':'String','name:ar':'String','name:cs':'String','name_de':'String','name:ro':'String','name:it':'String','name_int':'String','name:nl':'String','name:pl':'String','ele':'Number','name:lt':'String','name:ca':'String','name:hu':'String','name:ka':'String','name:fi':'String','name:da':'String','name:de':'String','name:tr':'String','name:fr':'String','name:mk':'String','name:nonlatin':'String','name:fy':'String','name:zh':'String','name:latin':'String','name:sl':'String','name:lv':'String','name:ja':'String','name:ko_rm':'String','name:no':'String','name:kk':'String','name:sv':'String','name:he':'String','name:ja_rm':'String','name:ga':'String','name:br':'String','name:bs':'String','name:lb':'String','class':'String','name:la':'String','name:sk':'String','name:uk':'String','name:hy':'String','name:ru':'String','name:be':'String','name_en':'String','name:bg':'String','name:hr':'String','name:sr':'String','name:sq':'String','name:th':'String','name:el':'String','name:eo':'String','name:en':'String','name':'String','name:gd':'String','ele_ft':'Number','name:ja_kana':'String','name:is':'String','osm_id':'Number','iata':'String','icao':'String','name:sr-Latn':'String','name:et':'String','name:es':'String'},'minzoom':0,'id':'aerodrome_label','description':''}],'center':[-12.2168,28.6135,4],'bounds':[-180,-85.0511,180,85.0511],'maskLevel':'8','planettime':'1523232000000','version':'3.7','tilejson':'2.0.0'}");
                                sourceJson.Name = source.Key;
                            }
                            else if (source.Value.Url.StartsWith("mbtiles"))
                            {
                                var dataSource = new MbTilesTileSource(new SQLiteConnectionString(source.Value.Url.Substring(10), false));
                                sourceJson = JsonConvert.DeserializeObject<Source>(@"{'vector_layers':[{'maxzoom':14,'fields':{'class':'String'},'minzoom':0,'id':'water','description':''},{'maxzoom':14,'fields':{'name:mt':'String','name:pt':'String','name:az':'String','name:ka':'String','name:rm':'String','name:ko':'String','name:kn':'String','name:ar':'String','name:cs':'String','name_de':'String','name:ro':'String','name:it':'String','name_int':'String','name:ru':'String','name:pl':'String','name:ca':'String','name:lv':'String','name:bg':'String','name:cy':'String','name:fi':'String','name:he':'String','name:da':'String','name:de':'String','name:tr':'String','name:fr':'String','name:mk':'String','name:nonlatin':'String','name:fy':'String','name:be':'String','name:zh':'String','name:sr':'String','name:sl':'String','name:nl':'String','name:ja':'String','name:lt':'String','name:no':'String','name:kk':'String','name:ko_rm':'String','name:ja_rm':'String','name:br':'String','name:bs':'String','name:lb':'String','name:la':'String','name:sk':'String','name:uk':'String','name:hy':'String','name:sv':'String','name_en':'String','name:hu':'String','name:hr':'String','class':'String','name:sq':'String','name:el':'String','name:ga':'String','name:en':'String','name':'String','name:gd':'String','name:ja_kana':'String','name:is':'String','name:th':'String','name:latin':'String','name:sr-Latn':'String','name:et':'String','name:es':'String'},'minzoom':0,'id':'waterway','description':''},{'maxzoom':14,'fields':{'class':'String','subclass':'String'},'minzoom':0,'id':'landcover','description':''},{'maxzoom':14,'fields':{'class':'String'},'minzoom':0,'id':'landuse','description':''},{'maxzoom':14,'fields':{'name:mt':'String','name:pt':'String','name:az':'String','name:ka':'String','name:rm':'String','name:ko':'String','name:kn':'String','name:ar':'String','name:cs':'String','rank':'Number','name_de':'String','name:ro':'String','name:it':'String','name_int':'String','name:lv':'String','name:pl':'String','name:de':'String','name:ca':'String','name:bg':'String','name:cy':'String','name:fi':'String','name:he':'String','name:da':'String','ele':'Number','name:tr':'String','name:fr':'String','name:mk':'String','name:nonlatin':'String','name:fy':'String','name:be':'String','name:zh':'String','name:sl':'String','name:nl':'String','name:ja':'String','name:lt':'String','name:no':'String','name:kk':'String','name:ko_rm':'String','name:ja_rm':'String','name:br':'String','name:bs':'String','name:lb':'String','name:la':'String','name:sk':'String','name:uk':'String','name:hy':'String','name:ru':'String','name:sv':'String','name_en':'String','name:hu':'String','name:hr':'String','name:sr':'String','name:sq':'String','name:el':'String','name:ga':'String','name:en':'String','name':'String','name:gd':'String','ele_ft':'Number','name:ja_kana':'String','name:is':'String','osm_id':'Number','name:th':'String','name:latin':'String','name:sr-Latn':'String','name:et':'String','name:es':'String'},'minzoom':0,'id':'mountain_peak','description':''},{'maxzoom':14,'fields':{'class':'String'},'minzoom':0,'id':'park','description':''},{'maxzoom':14,'fields':{'admin_level':'Number','disputed':'Number','maritime':'Number'},'minzoom':0,'id':'boundary','description':''},{'maxzoom':14,'fields':{'class':'String'},'minzoom':0,'id':'aeroway','description':''},{'maxzoom':14,'fields':{'brunnel':'String','ramp':'Number','class':'String','service':'String','oneway':'Number'},'minzoom':0,'id':'transportation','description':''},{'maxzoom':14,'fields':{'render_min_height':'Number','render_height':'Number'},'minzoom':0,'id':'building','description':''},{'maxzoom':14,'fields':{'name:mt':'String','name:pt':'String','name:az':'String','name:ka':'String','name:rm':'String','name:ko':'String','name:kn':'String','name:ar':'String','name:cs':'String','name_de':'String','name:ro':'String','name:it':'String','name_int':'String','name:ru':'String','name:pl':'String','name:ca':'String','name:lv':'String','name:bg':'String','name:cy':'String','name:fi':'String','name:he':'String','name:da':'String','name:de':'String','name:tr':'String','name:fr':'String','name:mk':'String','name:nonlatin':'String','name:fy':'String','name:be':'String','name:zh':'String','name:sr':'String','name:sl':'String','name:nl':'String','name:ja':'String','name:lt':'String','name:no':'String','name:kk':'String','name:ko_rm':'String','name:ja_rm':'String','name:br':'String','name:bs':'String','name:lb':'String','name:la':'String','name:sk':'String','name:uk':'String','name:hy':'String','name:sv':'String','name_en':'String','name:hu':'String','name:hr':'String','class':'String','name:sq':'String','name:el':'String','name:ga':'String','name:en':'String','name':'String','name:gd':'String','name:ja_kana':'String','name:is':'String','name:th':'String','name:latin':'String','name:sr-Latn':'String','name:et':'String','name:es':'String'},'minzoom':0,'id':'water_name','description':''},{'maxzoom':14,'fields':{'name:mt':'String','name:pt':'String','name:az':'String','name:ka':'String','name:rm':'String','name:ko':'String','name:kn':'String','name:ar':'String','name:cs':'String','name_de':'String','name:ro':'String','name:it':'String','name_int':'String','name:ru':'String','name:pl':'String','name:ca':'String','name:lv':'String','name:bg':'String','name:cy':'String','name:fi':'String','name:he':'String','name:da':'String','name:de':'String','name:tr':'String','name:fr':'String','name:mk':'String','name:nonlatin':'String','name:fy':'String','name:be':'String','name:zh':'String','name:sr':'String','name:sl':'String','name:nl':'String','name:ja':'String','name:lt':'String','name:no':'String','name:kk':'String','name:ko_rm':'String','name:ja_rm':'String','name:br':'String','name:bs':'String','name:lb':'String','name:la':'String','name:sk':'String','name:uk':'String','name:hy':'String','name:sv':'String','name_en':'String','name:hu':'String','name:hr':'String','class':'String','name:sq':'String','network':'String','name:el':'String','name:ga':'String','name:en':'String','name':'String','name:gd':'String','ref':'String','name:ja_kana':'String','ref_length':'Number','name:is':'String','name:th':'String','name:latin':'String','name:sr-Latn':'String','name:et':'String','name:es':'String'},'minzoom':0,'id':'transportation_name','description':''},{'maxzoom':14,'fields':{'name:mt':'String','name:pt':'String','name:az':'String','name:ka':'String','name:rm':'String','name:ko':'String','name:kn':'String','name:ar':'String','name:cs':'String','rank':'Number','name_de':'String','name:ro':'String','name:it':'String','name_int':'String','name:ru':'String','name:pl':'String','name:ca':'String','name:lv':'String','name:bg':'String','name:cy':'String','name:hr':'String','name:fi':'String','name:he':'String','name:da':'String','name:de':'String','name:tr':'String','name:fr':'String','name:mk':'String','name:nonlatin':'String','name:fy':'String','name:be':'String','name:zh':'String','name:sr':'String','name:sl':'String','name:nl':'String','name:ja':'String','name:ko_rm':'String','name:no':'String','name:kk':'String','capital':'Number','name:ja_rm':'String','name:br':'String','name:bs':'String','name:lb':'String','name:la':'String','name:sk':'String','name:uk':'String','name:hy':'String','name:sv':'String','name_en':'String','name:hu':'String','name:lt':'String','class':'String','name:sq':'String','name:el':'String','name:ga':'String','name:en':'String','name':'String','name:gd':'String','name:ja_kana':'String','name:is':'String','name:th':'String','name:latin':'String','name:sr-Latn':'String','name:et':'String','name:es':'String'},'minzoom':0,'id':'place','description':''},{'maxzoom':14,'fields':{'housenumber':'String'},'minzoom':0,'id':'housenumber','description':''},{'maxzoom':14,'fields':{'name:mt':'String','name:pt':'String','name:az':'String','name:ka':'String','name:rm':'String','name:ko':'String','name:kn':'String','name:ar':'String','name:cs':'String','rank':'Number','name_de':'String','name:ro':'String','name:it':'String','name_int':'String','name:ru':'String','name:pl':'String','name:ca':'String','name:lv':'String','name:bg':'String','name:cy':'String','name:fi':'String','name:he':'String','name:da':'String','subclass':'String','name:de':'String','name:tr':'String','name:fr':'String','name:mk':'String','name:nonlatin':'String','name:fy':'String','name:be':'String','name:zh':'String','name:sr':'String','name:sl':'String','name:nl':'String','name:ja':'String','name:lt':'String','name:no':'String','name:kk':'String','name:ko_rm':'String','name:ja_rm':'String','name:br':'String','name:bs':'String','name:lb':'String','name:la':'String','name:sk':'String','name:uk':'String','name:hy':'String','name:sv':'String','name_en':'String','name:hu':'String','name:hr':'String','class':'String','name:sq':'String','name:el':'String','name:ga':'String','name:en':'String','name':'String','name:gd':'String','name:ja_kana':'String','name:is':'String','name:th':'String','name:latin':'String','name:sr-Latn':'String','name:et':'String','name:es':'String'},'minzoom':0,'id':'poi','description':''}]}");
                                //sourceJson = JsonConvert.DeserializeObject<Source>(dataSource.Json);
                                sourceJson.Name = source.Key;
                                sourceJson.Tiles = new List<string> {source.Value.Url};
                                sourceJson.Scheme = dataSource.Schema.YAxis == YAxis.TMS ? "tms" : "osm";
                                sourceJson.Bounds = new JValue[]
                                {
                                    new JValue(dataSource.Schema.Extent.MinX),
                                    new JValue(dataSource.Schema.Extent.MinY),
                                    new JValue(dataSource.Schema.Extent.MaxX),
                                    new JValue(dataSource.Schema.Extent.MinX)
                                };
                            }
                        }
                        // Create new vector layers
                        var layers = CreateVectorLayers(sourceJson);
                        // Add all styles to each layer
                        foreach (var layer in layers)
                        {
                            layer.Style = new StyleCollection();

                            // Add all ThemeStyles for this layer
                            foreach (var styleLayer in styleJson.StyleLayers)
                            {
                                if (!layer.Name.Equals(styleLayer.Source + "." + styleLayer.SourceLayer, StringComparison.CurrentCultureIgnoreCase))
                                    continue;

                                ((StyleCollection)layer.Style).Add(styleLayer.Style);
                            }

                            // TODO: Only when Style avalible
                            layer.Enabled = true;

                            // Add layer to map
                            map.Layers.Add(layer);
                        }
                        break;
                    case "geoJson":
                        break;
                    case "image":
                        break;
                    case "video":
                        break;
                    default:
                        throw new ArgumentException($"{source.Value.Type} isn't a valid source");
                }
            }

            if (styleJson.Center != null)
            {
                Center = Projection.SphericalMercator.FromLonLat(styleJson.Center[0], styleJson.Center[1]);
                map.NavigateTo(Center);
            }

            map.ZoomMode = ZoomMode.None;
        }

        public Color Background { get; }

        public Point Center { get; }

        public float Zoom { get; }

        public string SpriteUrl { get; }

        public string GlyphsUrl { get; }

        private ILayer CreateRasterLayer(Source source)
        {
            return new TileLayer(CreateTileSource(source))
            {
                MinVisible = ((float?)source?.ZoomMax)?.ToResolution() ?? 0,
                MaxVisible = ((float?)source?.ZoomMin)?.ToResolution() ?? double.PositiveInfinity,
            };
        }

        private IList<ILayer> CreateVectorLayers(Source source)
        {
            var vectorLayers = new List<ILayer>();
            var tileSource = CreateTileSource(source);
            var cache = new MemoryCache<IList<VectorTileLayer>>();  // All DataSources should use the same cache
            var left = source.Bounds[0].Type == JTokenType.Float ? (float) source.Bounds[0] : -180.0f;
            var bottom = source.Bounds[1].Type == JTokenType.Float ? (float)source.Bounds[1] : -85.0511f;
            var right = source.Bounds[2].Type == JTokenType.Float ? (float)source.Bounds[2] : 180.0f;
            var top = source.Bounds[3].Type == JTokenType.Float ? (float)source.Bounds[3] : 85.0511f;
            var bounds = new BoundingBox(new Point(left, bottom), new Point(right, top));

            foreach (var layer in source.VectorLayers)
            {
                var l = new Layers.Layer(source.Name+"."+layer.Id)
                {
                    DataSource = new VectorTileProvider(tileSource, layer.Id, bounds, cache),
                    CRS = "EPSG:3857",
                    MinVisible = (source.ZoomMax ?? 30).ToResolution(),
                    MaxVisible = (source.ZoomMin ?? 0).ToResolution(),
                };

                vectorLayers.Add(l);
            }

            return vectorLayers;
        }

        private ITileSource CreateTileSource(Source source)
        {
            ITileSource tileSource = null;

            if (source.Tiles == null || source.Tiles.Count == 0)
                return null;

            if (source.Tiles[0].StartsWith("http"))
            {
                tileSource = new HttpTileSource(new GlobalSphericalMercator(
                        source.Scheme == "tms" ? YAxis.TMS : YAxis.OSM,
                        minZoomLevel: source.ZoomMin ?? 0,
                        maxZoomLevel: source.ZoomMax ?? 30
                    ),
                    source.Tiles[0], //"{s}",
                    source.Tiles,
                    name: source.Name,
                    attribution: new Attribution(source.Attribution)
                );
            }
            else if (source.Tiles[0].StartsWith("mbtiles://"))
            {
                // We should get the tile source from someone else
                tileSource = new MbTilesTileSource(new SQLiteConnectionString(source.Tiles[0].Substring(10), false),
                    new GlobalSphericalMercator(
                        source.Scheme == "tms" ? YAxis.TMS : YAxis.OSM,
                        minZoomLevel: source.ZoomMin ?? 0,
                        maxZoomLevel: source.ZoomMax ?? 30
                    ));
            }

            return tileSource;
        }

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
        private void CreateSprites(string json, int atlasBitmapId)
        {
            var sprites = JsonConvert.DeserializeObject<Dictionary<string, Json.Sprite>>(json);

            foreach (var sprite in sprites)
            {
                // Set BitmapId for Mapsui.Atlas to atlasBitmapId
                sprite.Value.BitmapId = atlasBitmapId;
                // Replace BitmapId with correct BitmapId of new sprite
                sprite.Value.BitmapId = BitmapRegistry.Instance.Register(sprite.Value.ToMapsui());
                // Add new sprite to atlas
                if (sprite.Value.BitmapId >= 0)
                    SpriteAtlas.Add(sprite.Key, sprite.Value.ToMapsui());
            }
        }
    }
}