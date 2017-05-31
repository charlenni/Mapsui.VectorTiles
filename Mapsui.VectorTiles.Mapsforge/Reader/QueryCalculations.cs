/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014 Ludwig M Brinckmann
 * Copyright 2016, 2017 Dirk Weltz
 * Copyright 2016 Michael Oed
 *
 * This program is free software: you can redistribute it and/or modify it under the
 * terms of the GNU Lesser General Public License as published by the Free Software
 * Foundation, either version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY
 * WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A
 * PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License along with
 * this program. If not, see <http://www.gnu.org/licenses/>.
 */

namespace Mapsui.VectorTiles.Mapsforge.Reader
{
    internal sealed class QueryCalculations
	{
		internal static int CalculateTileBitmask(Tile tile, int zoomLevelDifference)
		{
			if (zoomLevelDifference == 1)
			{
				return GetFirstLevelTileBitmask(tile);
			}

			// calculate the XY numbers of the second level sub-tile
			long subtileX = (int)((uint)tile.Col >> (zoomLevelDifference - 2));
			long subtileY = (int)((uint)tile.Row >> (zoomLevelDifference - 2));

			// calculate the XY numbers of the parent tile
			long parentTileX = (long)((ulong)subtileX >> 1);
			long parentTileY = (long)((ulong)subtileY >> 1);

			// determine the correct bitmask for all 16 sub-tiles
			if (parentTileX % 2 == 0 && parentTileY % 2 == 0)
			{
				return GetSecondLevelTileBitmaskUpperLeft(subtileX, subtileY);
			}
			else if (parentTileX % 2 == 1 && parentTileY % 2 == 0)
			{
				return GetSecondLevelTileBitmaskUpperRight(subtileX, subtileY);
			}
			else if (parentTileX % 2 == 0 && parentTileY % 2 == 1)
			{
				return GetSecondLevelTileBitmaskLowerLeft(subtileX, subtileY);
			}
			else
			{
				return GetSecondLevelTileBitmaskLowerRight(subtileX, subtileY);
			}
		}

		private static int GetFirstLevelTileBitmask(Tile tile)
		{
			if (tile.Col % 2 == 0 && tile.Row % 2 == 0)
			{
				// upper left quadrant
				return 0xcc00;
			}
			else if (tile.Col % 2 == 1 && tile.Row % 2 == 0)
			{ //NOSONAR tiles are always positiv
				// upper right quadrant
				return 0x3300;
			}
			else if (tile.Col % 2 == 0 && tile.Row % 2 == 1)
			{
				// lower left quadrant
				return 0xcc;
			}
			else
			{
				// lower right quadrant
				return 0x33;
			}
		}

		private static int GetSecondLevelTileBitmaskLowerLeft(long subtileX, long subtileY)
		{
			if (subtileX % 2 == 0 && subtileY % 2 == 0)
			{
				// upper left sub-tile
				return 0x80;
			}
			else if (subtileX % 2 == 1 && subtileY % 2 == 0)
			{
				// upper right sub-tile
				return 0x40;
			}
			else if (subtileX % 2 == 0 && subtileY % 2 == 1)
			{
				// lower left sub-tile
				return 0x8;
			}
			else
			{
				// lower right sub-tile
				return 0x4;
			}
		}

		private static int GetSecondLevelTileBitmaskLowerRight(long subtileX, long subtileY)
		{
			if (subtileX % 2 == 0 && subtileY % 2 == 0)
			{
				// upper left sub-tile
				return 0x20;
			}
			else if (subtileX % 2 == 1 && subtileY % 2 == 0)
			{
				// upper right sub-tile
				return 0x10;
			}
			else if (subtileX % 2 == 0 && subtileY % 2 == 1)
			{
				// lower left sub-tile
				return 0x2;
			}
			else
			{
				// lower right sub-tile
				return 0x1;
			}
		}

		private static int GetSecondLevelTileBitmaskUpperLeft(long subtileX, long subtileY)
		{
			if (subtileX % 2 == 0 && subtileY % 2 == 0)
			{
				// upper left sub-tile
				return 0x8000;
			}
			else if (subtileX % 2 == 1 && subtileY % 2 == 0)
			{
				// upper right sub-tile
				return 0x4000;
			}
			else if (subtileX % 2 == 0 && subtileY % 2 == 1)
			{
				// lower left sub-tile
				return 0x800;
			}
			else
			{
				// lower right sub-tile
				return 0x400;
			}
		}

		private static int GetSecondLevelTileBitmaskUpperRight(long subtileX, long subtileY)
		{
			if (subtileX % 2 == 0 && subtileY % 2 == 0)
			{
				// upper left sub-tile
				return 0x2000;
			}
			else if (subtileX % 2 == 1 && subtileY % 2 == 0)
			{
				// upper right sub-tile
				return 0x1000;
			}
			else if (subtileX % 2 == 0 && subtileY % 2 == 1)
			{
				// lower left sub-tile
				return 0x200;
			}
			else
			{
				// lower right sub-tile
				return 0x100;
			}
		}

		private QueryCalculations()
		{
			throw new System.InvalidOperationException();
		}
	}
}