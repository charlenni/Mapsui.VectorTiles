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

namespace Mapsui.VectorTiles.Mapsforge.Reader.Header
{
    using Geometries;
    using Utils;

    internal sealed class RequiredFields
	{
		/// <summary>
		/// Magic byte at the beginning of a valid binary map file.
		/// </summary>
		private const string BINARY_OSM_MAGIC_BYTE = "mapsforge binary OSM";

		/// <summary>
		/// Maximum size of the file header in bytes.
		/// </summary>
		private const int HEADER_SIZE_MAX = 1000000;

		/// <summary>
		/// Minimum size of the file header in bytes.
		/// </summary>
		private const int HEADER_SIZE_MIN = 70;

		/// <summary>
		/// The name of the Mercator projection as stored in the file header.
		/// </summary>
		private const string MERCATOR = "Mercator";

		/// <summary>
		/// Lowest version of the map file format supported by this implementation.
		/// </summary>
		private const int SUPPORTED_FILE_VERSION_MIN = 3;

		/// <summary>
		/// Highest version of the map file format supported by this implementation.
		/// </summary>
		private const int SUPPORTED_FILE_VERSION_MAX = 4;

		internal static void ReadBoundingBox(ReadBuffer readBuffer, MapFileInfoBuilder mapFileInfoBuilder)
		{
			double minLatitude = PointUtils.MicrodegreesToDegrees(readBuffer.ReadInt());
			double minLongitude = PointUtils.MicrodegreesToDegrees(readBuffer.ReadInt());
			double maxLatitude = PointUtils.MicrodegreesToDegrees(readBuffer.ReadInt());
			double maxLongitude = PointUtils.MicrodegreesToDegrees(readBuffer.ReadInt());

			try
			{
				mapFileInfoBuilder.boundingBox = new BoundingBox(minLongitude, minLatitude, maxLongitude, maxLatitude);
			}
			catch (System.ArgumentException e)
			{
				throw new MapFileException(e.Message);
			}
		}

		internal static void ReadFileSize(ReadBuffer readBuffer, long fileSize, MapFileInfoBuilder mapFileInfoBuilder)
		{
			// get and check the file size (8 bytes)
			long headerFileSize = readBuffer.ReadLong();
			if (headerFileSize != fileSize)
			{
				throw new MapFileException("invalid file size: " + headerFileSize);
			}
			mapFileInfoBuilder.fileSize = fileSize;
		}

		internal static void ReadFileVersion(ReadBuffer readBuffer, MapFileInfoBuilder mapFileInfoBuilder)
		{
			// get and check the file version (4 bytes)
			int fileVersion = readBuffer.ReadInt();
			if (fileVersion < SUPPORTED_FILE_VERSION_MIN || fileVersion > SUPPORTED_FILE_VERSION_MAX)
			{
				throw new MapFileException("unsupported file version: " + fileVersion);
			}
			mapFileInfoBuilder.fileVersion = fileVersion;
		}

		internal static void ReadMagicByte(ReadBuffer readBuffer)
		{
			// read the the magic byte and the file header size into the buffer
			int magicByteLength = BINARY_OSM_MAGIC_BYTE.Length;
			if (!readBuffer.ReadFromStream(magicByteLength + 4))
			{
				throw new MapFileException("reading magic byte has failed");
			}

			// get and check the magic byte
			string magicByte = readBuffer.ReadUTF8EncodedString(magicByteLength);
			if (!BINARY_OSM_MAGIC_BYTE.Equals(magicByte))
			{
				throw new MapFileException("invalid magic byte: " + magicByte);
			}
		}

		internal static void ReadMapDate(ReadBuffer readBuffer, MapFileInfoBuilder mapFileInfoBuilder)
		{
			// get and check the the map date (8 bytes)
			long mapDate = readBuffer.ReadLong();
			// is the map date before 2010-01-10 ?
			if (mapDate < 1200000000000L)
			{
				throw new MapFileException("invalid map date: " + mapDate);
			}
			mapFileInfoBuilder.mapDate = mapDate;
		}

		internal static void ReadPoiTags(ReadBuffer readBuffer, MapFileInfoBuilder mapFileInfoBuilder)
		{
			// get and check the number of POI tags (2 bytes)
			int numberOfPoiTags = readBuffer.ReadShort();
			if (numberOfPoiTags < 0)
			{
				throw new MapFileException("invalid number of POI tags: " + numberOfPoiTags);
			}

			Tag[] poiTags = new Tag[numberOfPoiTags];
			for (int currentTagId = 0; currentTagId < numberOfPoiTags; ++currentTagId)
			{
				// get and check the POI tag
				string tag = readBuffer.ReadUTF8EncodedString();
				if (string.ReferenceEquals(tag, null))
				{
					throw new MapFileException("POI tag must not be null: " + currentTagId);
				}
				poiTags[currentTagId] = new Tag(tag);
			}
			mapFileInfoBuilder.poiTags = poiTags;
		}

		internal static void ReadProjectionName(ReadBuffer readBuffer, MapFileInfoBuilder mapFileInfoBuilder)
		{
			// get and check the projection name
			string projectionName = readBuffer.ReadUTF8EncodedString();
			if (!MERCATOR.Equals(projectionName))
			{
				throw new MapFileException("unsupported projection: " + projectionName);
			}
			mapFileInfoBuilder.projectionName = projectionName;
		}

		internal static void ReadRemainingHeader(ReadBuffer readBuffer)
		{
			// get and check the size of the remaining file header (4 bytes)
			int remainingHeaderSize = readBuffer.ReadInt();
			if (remainingHeaderSize < HEADER_SIZE_MIN || remainingHeaderSize > HEADER_SIZE_MAX)
			{
				throw new MapFileException("invalid remaining header size: " + remainingHeaderSize);
			}

			// read the header data into the buffer
			if (!readBuffer.ReadFromStream(remainingHeaderSize))
			{
				throw new MapFileException("reading header data has failed: " + remainingHeaderSize);
			}
		}

		internal static void ReadTilePixelSize(ReadBuffer readBuffer, MapFileInfoBuilder mapFileInfoBuilder)
		{
			// get and check the tile pixel size (2 bytes)
			int tilePixelSize = readBuffer.ReadShort();
			// if (tilePixelSize != Tile.TILE_SIZE) {
			// return new FileOpenResult("unsupported tile pixel size: " + tilePixelSize);
			// }
			mapFileInfoBuilder.tilePixelSize = tilePixelSize;
		}

		internal static void ReadWayTags(ReadBuffer readBuffer, MapFileInfoBuilder mapFileInfoBuilder)
		{
			// get and check the number of way tags (2 bytes)
			int numberOfWayTags = readBuffer.ReadShort();
			if (numberOfWayTags < 0)
			{
				throw new MapFileException("invalid number of way tags: " + numberOfWayTags);
			}

			Tag[] wayTags = new Tag[numberOfWayTags];

			for (int currentTagId = 0; currentTagId < numberOfWayTags; ++currentTagId)
			{
				// get and check the way tag
				string tag = readBuffer.ReadUTF8EncodedString();
				if (string.ReferenceEquals(tag, null))
				{
					throw new MapFileException("way tag must not be null: " + currentTagId);
				}
				wayTags[currentTagId] = new Tag(tag);
			}
			mapFileInfoBuilder.wayTags = wayTags;
		}

		private RequiredFields()
		{
			throw new System.InvalidOperationException();
		}
	}
}