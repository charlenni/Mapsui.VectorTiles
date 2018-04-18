using Mapsui.Geometries;
using Mapsui.Providers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mapsui.VectorTiles
{
    public class SymbolProvider : IProvider
    {
        private readonly Map _map;

        public IList<Symbol> Symbols = new List<Symbol>();

        public string CRS { get; set; }

        public BoundingBox Bounds { get; } = new BoundingBox(0, 0, 0, 0);

        public SymbolProvider(Map map)
        {
            _map = map;

            map.Viewport.ViewportChanged += ViewportChanged;
        }

        private void ViewportChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Viewport.Resolution))
            {
                Symbols.Clear();
            }
        }

        public BoundingBox GetExtents()
        {
            return Bounds;
        }

        public IEnumerable<IFeature> GetFeaturesInView(BoundingBox box, double resolution)
        {
            return new List<IFeature>();
        }
    }
}
