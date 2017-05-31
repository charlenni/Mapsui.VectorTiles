﻿namespace Mapsui.VectorTiles.Mapbox
{
    using Geometries;
    using System.Collections.Generic;

    public static class GeometryParser
    {
        public static List<VectorTileGeometry> ParseGeometry(List<uint> geom, Tile.GeomType geomType)
        {
            const uint cmdMoveTo = 1;
            //const uint cmdLineTo = 2;
            const uint cmdSegEnd = 7;
            //const uint cmdBits = 3;

            long x = 0;
            long y = 0;
            var coordsList = new List<VectorTileGeometry>();
            VectorTileGeometry coords = null;
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
                        coords = new VectorTileGeometry();
                        coordsList.Add(coords);
                    }
                }

                if (command == cmdSegEnd)
                {
                    if (geomType != Tile.GeomType.Point && !(coords.Points.Count == 0))
                    {
                        coords.Points.Add(coords.Points[0]);
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

                // use scale? var  coord = new Coordinate(x / scale, y / scale);
                var  coord = new Point(x, y);
                coords.Points.Add(coord);
            }
            return coordsList;
        }
    }
}