/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014 Ludwig M Brinckmann
 * Copyright 2016, 2017 Dirk Weltz
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

namespace Mapsui.VectorTiles.Mapsforge.Reader.Header
{
	/// <summary>
	/// Reads and validates the header data from a binary map file.
	/// </summary>
	public class MapFileHeader
	{
		/// <summary>
		/// Maximum valid base zoom level of a sub-file.
		/// </summary>
		private const int BASE_ZOOM_LEVEL_MAX = 20;

		/// <summary>
		/// Minimum size of the file header in bytes.
		/// </summary>
		private const int HEADER_SIZE_MIN = 70;

		/// <summary>
		/// Length of the debug signature at the beginning of the index.
		/// </summary>
		private const sbyte SIGNATURE_LENGTH_INDEX = 16;

		/// <summary>
		/// A single whitespace character.
		/// </summary>
		private const char SPACE = ' ';

		private MapFileInfo mapFileInfo;
		private SubFileParameter[] subFileParameters;
		private sbyte zoomLevelMaximum;
		private sbyte zoomLevelMinimum;

		/// <returns> a MapFileInfo containing the header data. </returns>
		public virtual MapFileInfo MapFileInfo
		{
			get
			{
				return this.mapFileInfo;
			}
		}

		/// <param name="zoomLevel">
		///            the originally requested zoom level. </param>
		/// <returns> the closest possible zoom level which is covered by a sub-file. </returns>
		public virtual sbyte GetQueryZoomLevel(sbyte zoomLevel)
		{
			if (zoomLevel > this.zoomLevelMaximum)
			{
				return this.zoomLevelMaximum;
			}
			else if (zoomLevel < this.zoomLevelMinimum)
			{
				return this.zoomLevelMinimum;
			}
			return zoomLevel;
		}

		/// <param name="queryZoomLevel">
		///            the zoom level for which the sub-file parameters are needed. </param>
		/// <returns> the sub-file parameters for the given zoom level. </returns>
		public virtual SubFileParameter GetSubFileParameter(int queryZoomLevel)
		{
			return this.subFileParameters[queryZoomLevel];
		}

		/// <summary>
		/// Reads and validates the header block from the map file.
		/// </summary>
		/// <param name="readBuffer">
		///            the ReadBuffer for the file data. </param>
		/// <param name="fileSize">
		///            the size of the map file in bytes. </param>
		/// <exception cref="IOException">
		///             if an error occurs while reading the file. </exception>
		public virtual void ReadHeader(ReadBuffer readBuffer, long fileSize)
		{
			RequiredFields.ReadMagicByte(readBuffer);

			RequiredFields.ReadRemainingHeader(readBuffer);

			MapFileInfoBuilder mapFileInfoBuilder = new MapFileInfoBuilder();

			RequiredFields.ReadFileVersion(readBuffer, mapFileInfoBuilder);

			RequiredFields.ReadFileSize(readBuffer, fileSize, mapFileInfoBuilder);

			RequiredFields.ReadMapDate(readBuffer, mapFileInfoBuilder);

			RequiredFields.ReadBoundingBox(readBuffer, mapFileInfoBuilder);

			RequiredFields.ReadTilePixelSize(readBuffer, mapFileInfoBuilder);

			RequiredFields.ReadProjectionName(readBuffer, mapFileInfoBuilder);

			OptionalFields.ReadOptionalFields(readBuffer, mapFileInfoBuilder);

			RequiredFields.ReadPoiTags(readBuffer, mapFileInfoBuilder);

			RequiredFields.ReadWayTags(readBuffer, mapFileInfoBuilder);

			ReadSubFileParameters(readBuffer, fileSize, mapFileInfoBuilder);

			this.mapFileInfo = mapFileInfoBuilder.Build();
		}

		private void ReadSubFileParameters(ReadBuffer readBuffer, long fileSize, MapFileInfoBuilder mapFileInfoBuilder)
		{
			// get and check the number of sub-files (1 byte)
			sbyte numberOfSubFiles = readBuffer.ReadByte();
			if (numberOfSubFiles < 1)
			{
				throw new MapFileException("invalid number of sub-files: " + numberOfSubFiles);
			}
			mapFileInfoBuilder.numberOfSubFiles = numberOfSubFiles;

			SubFileParameter[] tempSubFileParameters = new SubFileParameter[numberOfSubFiles];
			this.zoomLevelMinimum = sbyte.MaxValue;
			this.zoomLevelMaximum = sbyte.MinValue;

			// get and check the information for each sub-file
			for (sbyte currentSubFile = 0; currentSubFile < numberOfSubFiles; ++currentSubFile)
			{
				SubFileParameterBuilder subFileParameterBuilder = new SubFileParameterBuilder();

				// get and check the base zoom level (1 byte)
				sbyte baseZoomLevel = readBuffer.ReadByte();
				if (baseZoomLevel < 0 || baseZoomLevel > BASE_ZOOM_LEVEL_MAX)
				{
					throw new MapFileException("invalid base zoom level: " + baseZoomLevel);
				}
				subFileParameterBuilder.BaseZoomLevel = baseZoomLevel;

				// get and check the minimum zoom level (1 byte)
				sbyte zoomLevelMin = readBuffer.ReadByte();
				if (zoomLevelMin < 0 || zoomLevelMin > 22)
				{
					throw new MapFileException("invalid minimum zoom level: " + zoomLevelMin);
				}
				subFileParameterBuilder.ZoomLevelMin = zoomLevelMin;

				// get and check the maximum zoom level (1 byte)
				sbyte zoomLevelMax = readBuffer.ReadByte();
				if (zoomLevelMax < 0 || zoomLevelMax > 22)
				{
					throw new MapFileException("invalid maximum zoom level: " + zoomLevelMax);
				}
				subFileParameterBuilder.ZoomLevelMax = zoomLevelMax;

				// check for valid zoom level range
				if (zoomLevelMin > zoomLevelMax)
				{
					throw new MapFileException("invalid zoom level range: " + zoomLevelMin + SPACE + zoomLevelMax);
				}

				// get and check the start address of the sub-file (8 bytes)
				long startAddress = readBuffer.ReadLong();
				if (startAddress < HEADER_SIZE_MIN || startAddress >= fileSize)
				{
					throw new MapFileException("invalid start address: " + startAddress);
				}
				subFileParameterBuilder.StartAddress = startAddress;

				long indexStartAddress = startAddress;
				if (mapFileInfoBuilder.optionalFields.IsDebugFile)
				{
					// the sub-file has an index signature before the index
					indexStartAddress += SIGNATURE_LENGTH_INDEX;
				}
				subFileParameterBuilder.IndexStartAddress = indexStartAddress;

				// get and check the size of the sub-file (8 bytes)
				long subFileSize = readBuffer.ReadLong();
				if (subFileSize < 1)
				{
					throw new MapFileException("invalid sub-file size: " + subFileSize);
				}
				subFileParameterBuilder.SubFileSize = subFileSize;

				subFileParameterBuilder.BoundingBox = mapFileInfoBuilder.boundingBox;

				// add the current sub-file to the list of sub-files
				tempSubFileParameters[currentSubFile] = subFileParameterBuilder.Build();


				// update the global minimum and maximum zoom level information
				if (this.zoomLevelMinimum > tempSubFileParameters[currentSubFile].ZoomLevelMin)
				{
					this.zoomLevelMinimum = tempSubFileParameters[currentSubFile].ZoomLevelMin;
					mapFileInfoBuilder.zoomLevelMin = this.zoomLevelMinimum;
				}
				if (this.zoomLevelMaximum < tempSubFileParameters[currentSubFile].ZoomLevelMax)
				{
					this.zoomLevelMaximum = tempSubFileParameters[currentSubFile].ZoomLevelMax;
					mapFileInfoBuilder.zoomLevelMax = this.zoomLevelMaximum;
				}
			}

			// create and fill the lookup table for the sub-files
			this.subFileParameters = new SubFileParameter[this.zoomLevelMaximum + 1];
			for (int currentMapFile = 0; currentMapFile < numberOfSubFiles; ++currentMapFile)
			{
				SubFileParameter subFileParameter = tempSubFileParameters[currentMapFile];
				for (sbyte zoomLevel = subFileParameter.ZoomLevelMin; zoomLevel <= subFileParameter.ZoomLevelMax; ++zoomLevel)
				{
					this.subFileParameters[zoomLevel] = subFileParameter;
				}
			}
		}
	}
}