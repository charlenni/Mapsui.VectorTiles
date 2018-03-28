using System.Windows;
using Mapsui.Providers;
using Mapsui.VectorTiles;
using Mapsui.VectorTiles.MapboxGLStyler;

namespace Mapsui.VectorTiles.Sample.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            MapControl.RenderMode = UI.Wpf.RenderMode.Skia;

            //var imageSource = new MapCSSDictionaryImageSource();

            //// Get all images
            //var regex = new Regex(Regex.Escape("."));
            var assembly = this.GetType().Assembly;
            //foreach (var name in assembly.GetManifestResourceNames())
            //{
            //    if (name.Contains("Symbols"))
            //    {
            //        var key = name.Substring(name.IndexOf("Symbols")).ToLower();
            //        key = regex.Replace(key, "/", 1);
            //        imageSource.Add(key, assembly.GetManifestResourceStream(name));
            //    }

            //    System.Diagnostics.Debug.WriteLine(name);
            //}

            var styleStream = assembly.GetManifestResourceStream("Mapsui.VectorTiles.Sample.Wpf.Styles.MapCSS.mapnik.mapcss");
            //var mapStream = assembly.GetManifestResourceStream("Mapsui.VectorTiles.Sample.Wpf.andorra.map");
            var mapStream = assembly.GetManifestResourceStream("Mapsui.VectorTiles.Sample.Wpf.trails.mbtiles");
            var jsonStyleStream = assembly.GetManifestResourceStream("Mapsui.VectorTiles.Sample.Wpf.Styles.MapboxGL.osm-liberty.json");
            var jsonStyler = new MapboxGLStyler.MapboxGLStyler(jsonStyleStream);

            MapControl.Map.BackColor = jsonStyler.Background;
            if (jsonStyler.Center != null)
                MapControl.Map.NavigateTo(jsonStyler.Center);
//            MapControl.Map.NavigateTo(MapControl.Map.Resolutions[(int)jsonStyler.Zoom]);

            //var bb = new BoundingBox(Projection.SphericalMercator.FromLonLat(7.3090279, 43.416333),
            //    Projection.SphericalMercator.FromLonLat(7.633167, 43.8519311));
            //var mapcssfile = new MapCSSInterpreter(styleStream, imageSource);

            //var styler = new MapCSSVectorTileStyler(styleStream);

            //ZoomHelper.ZoomToBoudingbox(MapControl.Map.Viewport, -180, -90, 180, 90, 1000, 800);
            //MapControl.Map.Layers.Add(new TileLayer(KnownTileSources.Create(KnownTileSource.OpenStreetMap)) { Name = "OSM" });
            MapControl.Map.Layers.Add(new Layers.Layer
            {
                //Name = "Mapsforge",
                //DataSource = new MapsforgeMapCSSVectorTileProvider(mapStream, mapcssfile),
                Name = "Mapbox",
                CRS = "EPSG:3857",
                DataSource = new MapboxGLVectorTileProvider(mapStream, jsonStyler),
                Opacity = 1,
                Style = null,
            });
        }
    }
}
