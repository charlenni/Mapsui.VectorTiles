/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
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

using Mapsui.VectorTiles.Mapsforge.Reader.Utils;

namespace Mapsui.VectorTiles.Mapsforge.Reader.Header
{
	/// <summary>
	/// Holds all parameters of a sub-file.
	/// </summary>
	public class SubFileParameter
	{
		/// <summary>
		/// Number of bytes a single index entry consists of.
		/// </summary>
		public const sbyte BYTES_PER_INDEX_ENTRY = 5;

		/// <summary>
		/// Base zoom level of the sub-file, which equals to one block.
		/// </summary>
		public readonly sbyte BaseZoomLevel;

		/// <summary>
		/// Vertical amount of blocks in the grid.
		/// </summary>
		public readonly long BlocksHeight;

		/// <summary>
		/// Horizontal amount of blocks in the grid.
		/// </summary>
		public readonly long BlocksWidth;

		/// <summary>
		/// Y number of the tile at the bottom boundary in the grid.
		/// </summary>
		public readonly long BoundaryTileBottom;

		/// <summary>
		/// X number of the tile at the left boundary in the grid.
		/// </summary>
		public readonly long BoundaryTileLeft;

		/// <summary>
		/// X number of the tile at the right boundary in the grid.
		/// </summary>
		public readonly long BoundaryTileRight;

		/// <summary>
		/// Y number of the tile at the top boundary in the grid.
		/// </summary>
		public readonly long BoundaryTileTop;

		/// <summary>
		/// Absolute end address of the index in the enclosing file.
		/// </summary>
		public readonly long IndexEndAddress;

		/// <summary>
		/// Absolute start address of the index in the enclosing file.
		/// </summary>
		public readonly long IndexStartAddress;

		/// <summary>
		/// Total number of blocks in the grid.
		/// </summary>
		public readonly long NumberOfBlocks;

		/// <summary>
		/// Absolute start address of the sub-file in the enclosing file.
		/// </summary>
		public readonly long StartAddress;

		/// <summary>
		/// Size of the sub-file in bytes.
		/// </summary>
		public readonly long SubFileSize;

		/// <summary>
		/// Maximum zoom level for which the block entries tables are made.
		/// </summary>
		public readonly sbyte ZoomLevelMax;

		/// <summary>
		/// Minimum zoom level for which the block entries tables are made.
		/// </summary>
		public readonly sbyte ZoomLevelMin;

		/// <summary>
		/// Stores the hash code of this object.
		/// </summary>
		private readonly int HashCodeValue;

		internal SubFileParameter(SubFileParameterBuilder subFileParameterBuilder)
		{
			this.StartAddress = subFileParameterBuilder.StartAddress;
			this.IndexStartAddress = subFileParameterBuilder.IndexStartAddress;
			this.SubFileSize = subFileParameterBuilder.SubFileSize;
			this.BaseZoomLevel = subFileParameterBuilder.BaseZoomLevel;
			this.ZoomLevelMin = subFileParameterBuilder.ZoomLevelMin;
			this.ZoomLevelMax = subFileParameterBuilder.ZoomLevelMax;
			this.HashCodeValue = CalculateHashCode();

			// calculate the XY numbers of the boundary tiles in this sub-file
			this.BoundaryTileBottom = MercatorProjection.LatitudeToTileY(subFileParameterBuilder.BoundingBox.MinY, this.BaseZoomLevel);
			this.BoundaryTileLeft = MercatorProjection.LongitudeToTileX(subFileParameterBuilder.BoundingBox.MinX, this.BaseZoomLevel);
			this.BoundaryTileTop = MercatorProjection.LatitudeToTileY(subFileParameterBuilder.BoundingBox.MaxY, this.BaseZoomLevel);
			this.BoundaryTileRight = MercatorProjection.LongitudeToTileX(subFileParameterBuilder.BoundingBox.MaxX, this.BaseZoomLevel);

			// calculate the horizontal and vertical amount of blocks in this sub-file
			this.BlocksWidth = this.BoundaryTileRight - this.BoundaryTileLeft + 1;
			this.BlocksHeight = this.BoundaryTileBottom - this.BoundaryTileTop + 1;

			// calculate the total amount of blocks in this sub-file
			this.NumberOfBlocks = this.BlocksWidth * this.BlocksHeight;

			this.IndexEndAddress = this.IndexStartAddress + this.NumberOfBlocks * BYTES_PER_INDEX_ENTRY;
		}

		public override bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}
			else if (!(obj is SubFileParameter))
			{
				return false;
			}
			SubFileParameter other = (SubFileParameter) obj;
			if (this.StartAddress != other.StartAddress)
			{
				return false;
			}
			else if (this.SubFileSize != other.SubFileSize)
			{
				return false;
			}
			else if (this.BaseZoomLevel != other.BaseZoomLevel)
			{
				return false;
			}
			return true;
		}

		public override int GetHashCode()
		{
			return this.HashCodeValue;
		}

		/// <returns> the hash code of this object. </returns>
		private int CalculateHashCode()
		{
			int result = 7;
			result = 31 * result + (int)(this.StartAddress ^ ((long)((ulong)this.StartAddress >> 32)));
			result = 31 * result + (int)(this.SubFileSize ^ ((long)((ulong)this.SubFileSize >> 32)));
			result = 31 * result + this.BaseZoomLevel;
			return result;
		}
	}
}