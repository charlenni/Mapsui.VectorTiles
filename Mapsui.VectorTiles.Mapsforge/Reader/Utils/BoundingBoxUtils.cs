namespace Mapsui.VectorTiles.Mapsforge.Reader.Utils
{
    using Geometries;
    using System;

    public static class BoundingBoxUtils
    {
        public static BoundingBox ToBoundingBox(this Tile tile)
        {
            sbyte level = (sbyte)tile.ZoomLevel;

            double minY = Math.Max(MercatorProjection.LATITUDE_MIN, MercatorProjection.TileYToLatitude(tile.Row + 1, level));
            double minX = Math.Max(-180, MercatorProjection.TileXToLongitude(tile.Col, level));
            double maxY = Math.Min(MercatorProjection.LATITUDE_MAX, MercatorProjection.TileYToLatitude(tile.Row, level));
            double maxX = Math.Min(180, MercatorProjection.TileXToLongitude(tile.Col + 1, level));

            if (maxX == -180)
            {
                // fix for dateline crossing, where the right tile starts at -180 and causes an invalid bbox
                maxX = 180;
            }

            return new BoundingBox(minX, minY, maxX, maxY);
        }
    }
}