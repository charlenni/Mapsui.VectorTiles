using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mapsui.VectorTiles.Mapbox
{
    public class map
    {
        public int zoom_level { get; set; }
        public int tile_row { get; set; }
        public int tile_column { get; set; }
        public int tile_id { get; set; }
    }
}
