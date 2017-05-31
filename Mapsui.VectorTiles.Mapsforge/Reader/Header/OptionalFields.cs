/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014 Ludwig M Brinckmann
 * Copyright 2015 devemux86
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
    using System;
    using Utils;

    internal sealed class OptionalFields
	{
		/// <summary>
		/// Bitmask for the comment field in the file header.
		/// </summary>
		private const int HEADER_BITMASK_COMMENT = 0x08;

		/// <summary>
		/// Bitmask for the created by field in the file header.
		/// </summary>
		private const int HEADER_BITMASK_CREATED_BY = 0x04;

		/// <summary>
		/// Bitmask for the debug flag in the file header.
		/// </summary>
		private const int HEADER_BITMASK_DEBUG = 0x80;

		/// <summary>
		/// Bitmask for the language(s) preference field in the file header.
		/// </summary>
		private const int HEADER_BITMASK_LANGUAGES_PREFERENCE = 0x10;

		/// <summary>
		/// Bitmask for the start position field in the file header.
		/// </summary>
		private const int HEADER_BITMASK_START_POSITION = 0x40;

		/// <summary>
		/// Bitmask for the start zoom level field in the file header.
		/// </summary>
		private const int HEADER_BITMASK_START_ZOOM_LEVEL = 0x20;

		/// <summary>
		/// Maximum valid start zoom level.
		/// </summary>
		private const int START_ZOOM_LEVEL_MAX = 22;

		internal static void ReadOptionalFields(ReadBuffer readBuffer, MapFileInfoBuilder mapFileInfoBuilder)
		{
			OptionalFields optionalFields = new OptionalFields(readBuffer.ReadByte());
			mapFileInfoBuilder.optionalFields = optionalFields;

			optionalFields.ReadOptionalFields(readBuffer);
		}

		internal string Comment;
		internal string CreatedBy;
		internal readonly bool HasComment;
		internal readonly bool HasCreatedBy;
		internal readonly bool HasLanguagesPreference;
		internal readonly bool HasStartPosition;
		internal readonly bool HasStartZoomLevel;
		internal readonly bool IsDebugFile;
		internal string LanguagesPreference;
		internal Point StartPosition;
		internal sbyte? StartZoomLevel;

		private OptionalFields(sbyte flags)
		{
			this.IsDebugFile = (flags & HEADER_BITMASK_DEBUG) != 0;
			this.HasStartPosition = (flags & HEADER_BITMASK_START_POSITION) != 0;
			this.HasStartZoomLevel = (flags & HEADER_BITMASK_START_ZOOM_LEVEL) != 0;
			this.HasLanguagesPreference = (flags & HEADER_BITMASK_LANGUAGES_PREFERENCE) != 0;
			this.HasComment = (flags & HEADER_BITMASK_COMMENT) != 0;
			this.HasCreatedBy = (flags & HEADER_BITMASK_CREATED_BY) != 0;
		}

		private void ReadLanguagesPreference(ReadBuffer readBuffer)
		{
			if (this.HasLanguagesPreference)
			{
				this.LanguagesPreference = readBuffer.ReadUTF8EncodedString();
			}
		}

		private void ReadMapStartPosition(ReadBuffer readBuffer)
		{
			if (this.HasStartPosition)
			{
				double mapStartLatitude = PointUtils.MicrodegreesToDegrees(readBuffer.ReadInt());
				double mapStartLongitude = PointUtils.MicrodegreesToDegrees(readBuffer.ReadInt());
				try
				{
                    PointUtils.ValidateLatitude(mapStartLatitude);
                    PointUtils.ValidateLongitude(mapStartLongitude);
                    this.StartPosition = new Point(mapStartLongitude, mapStartLatitude);
				}
				catch (System.ArgumentException e)
				{
					throw new MapFileException(e.Message);
				}
			}
		}

		private void ReadMapStartZoomLevel(ReadBuffer readBuffer)
		{
			if (this.HasStartZoomLevel)
			{
				// get and check the start zoom level (1 byte)
				sbyte mapStartZoomLevel = readBuffer.ReadByte();
				if (mapStartZoomLevel < 0 || mapStartZoomLevel > START_ZOOM_LEVEL_MAX)
				{
					throw new MapFileException("invalid map start zoom level: " + mapStartZoomLevel);
				}

				this.StartZoomLevel = Convert.ToSByte(mapStartZoomLevel);
			}
		}

		private void ReadOptionalFields(ReadBuffer readBuffer)
		{
			ReadMapStartPosition(readBuffer);

			ReadMapStartZoomLevel(readBuffer);

			ReadLanguagesPreference(readBuffer);

			if (this.HasComment)
			{
				this.Comment = readBuffer.ReadUTF8EncodedString();
			}

			if (this.HasCreatedBy)
			{
				this.CreatedBy = readBuffer.ReadUTF8EncodedString();
			}
		}
	}
}