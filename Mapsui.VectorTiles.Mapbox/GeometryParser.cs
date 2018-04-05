using Mapsui.Geometries;
using System.Collections.Generic;

namespace Mapsui.VectorTiles.Mapbox
{
    public static class GeometryParser
    {
        /// <summary>
        /// Convert Mapbox tile format (see https://www.mapbox.com/vector-tiles/specification/)
        /// </summary>
        /// <param name="geom">Geometry information in Mapbox format</param>
        /// <param name="geomType">GeometryType of this geometry</param>
        /// <param name="offsetX">World coordinates of left top edge of tile</param>
        /// <param name="offsetY">World coordinates of left top edge of tile</param>
        /// <param name="factor">Factor for converting Mapbox coordinates to world coordinates</param>
        /// <returns>List of list of points in world coordinates</returns>
        public static List<List<Point>> ParseGeometry(List<uint> geom, Tile.GeomType geomType, double offsetX, double offsetY, double factor)
        {
            const uint cmdMoveTo = 1;
            //const uint cmdLineTo = 2;
            const uint cmdSegEnd = 7;
            //const uint cmdBits = 3;

            long x = 0;
            long y = 0;
            var coordsList = new List<List<Point>>();
            List<Point> coords = null;
            var geometryCount = geom.Count;
            uint length = 0;
            uint command = 0;
            var i = 0;
            while (i < geometryCount)
            {
                if (length <= 0)
                {
                    length = geom[i++];
                    command = length & ((1 << 3) - 1);
                    length = length >> 3;
                }

                if (length > 0)
                {
                    if (command == cmdMoveTo)
                    {
                        coords = new List<Point>();
                        coordsList.Add(coords);
                    }
                }

                if (command == cmdSegEnd)
                {
                    if (geomType != Tile.GeomType.Point && coords?.Count != 0)
                    {
                        coords?.Add(coords[0]);
                    }
                    length--;
                    continue;
                }

                var dx = geom[i++];
                var dy = geom[i++];

                length--;

                var ldx = ZigZag.Decode(dx);
                var ldy = ZigZag.Decode(dy);

                x = x + ldx;
                y = y + ldy;

                // Calc coordinates in EPSG:3857 format
                var  coord = new Point(offsetX + x * factor, offsetY - y * factor);
                coords?.Add(coord);
            }
            return coordsList;
        }
    }
}