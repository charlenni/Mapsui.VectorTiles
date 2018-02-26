using BruTile.Predefined;
using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.Utilities;
using Mapsui.VectorTiles.Sources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

            var assembly = this.GetType().Assembly;
            var stream = assembly.GetManifestResourceStream("Mapsui.VectorTiles.Sample.Wpf.andorra.map");

            //ZoomHelper.ZoomToBoudingbox(MapControl.Map.Viewport, -180, -90, 180, 90, 1000, 800);
            MapControl.Map.Layers.Add(new TileLayer(KnownTileSources.Create(KnownTileSource.OpenStreetMap)) { Name = "OSM" });
            MapControl.Map.Layers.Add(new Layer
            {
                Name = "Mapsforge",
                DataSource = new DummyMapsforgeVectorTileProvider(stream),
                Opacity = 1,
                Style = null,
                //Style = new VectorStyle { Fill = new Styles.Brush(Styles.Color.Red), Outline = new Styles.Pen { Color = Styles.Color.Black, PenStyle = PenStyle.Dash }, Line = new Styles.Pen { Color = Styles.Color.Black, PenStyle = PenStyle.Solid } }
            });
        }
    }
}
