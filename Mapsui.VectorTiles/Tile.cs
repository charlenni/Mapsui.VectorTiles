namespace Mapsui.VectorTiles
{
    using System;

    public struct Tile : IComparable
    {
        private readonly int col;
        private readonly int row;
        private readonly int zoomLevel;

        public int Col
        {
            get { return col; }
        }

        public int Row
        {
            get { return row; }
        }

        public int ZoomLevel
        {
            get { return zoomLevel; }
        }

        public Tile(int col, int row, int zoomLevel)
        {
            if (col < 0)
            {
                throw new ArgumentException("tile col must not be negative: " + col);
            }
            else if (row < 0)
            {
                throw new ArgumentException("tile row must not be negative: " + row);
            }
            else if (zoomLevel < 0)
            {
                throw new ArgumentException("zoom level must not be negative: " + zoomLevel);
            }

            long maxTileNumber = GetMaxTileNumber(zoomLevel);
            if (col > maxTileNumber)
            {
                throw new ArgumentException("invalid col number on zoom level " + zoomLevel + ": " + col);
            }
            else if (row > maxTileNumber)
            {
                throw new ArgumentException("invalid row number on zoom level " + zoomLevel + ": " + row);
            }

            this.col = col;
            this.row = row;
            this.zoomLevel = zoomLevel;
        }

        public static int GetMaxTileNumber(int zoomLevel)
        {
            if (zoomLevel < 0)
            {
                throw new ArgumentException("zoom level must not be negative: " + zoomLevel);
            }
            else if (zoomLevel == 0)
            {
                return 0;
            }
            return (2 << zoomLevel - 1) - 1;
        }

        public int CompareTo(object obj)
        {
            if (!(obj is Tile))
            {
                throw new ArgumentException("object of type TileIndex was expected");
            }
            return CompareTo((Tile)obj);
        }

        public int CompareTo(Tile index)
        {
            if (col < index.Col) return -1;
            if (col > index.Col) return 1;
            if (row < index.Row) return -1;
            if (row > index.Row) return 1;
            if (zoomLevel > index.ZoomLevel) return 1;
            if (zoomLevel < index.ZoomLevel) return -1;
            return 0;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Tile))
                return false;

            return Equals((Tile)obj);
        }

        public bool Equals(Tile index)
        {
            return col == index.Col && row == index.Row && zoomLevel == index.ZoomLevel;
        }

        public override int GetHashCode()
        {
            return col ^ row ^ zoomLevel;
        }

        public static bool operator ==(Tile key1, Tile key2)
        {
            return Equals(key1, key2);
        }

        public static bool operator !=(Tile key1, Tile key2)
        {
            return !Equals(key1, key2);
        }

        public static bool operator <(Tile key1, Tile key2)
        {
            return (key1.CompareTo(key2) < 0);
        }

        public static bool operator >(Tile key1, Tile key2)
        {
            return (key1.CompareTo(key2) > 0);
        }
    }
}