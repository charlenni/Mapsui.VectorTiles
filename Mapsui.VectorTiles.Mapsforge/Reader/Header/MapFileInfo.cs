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
    using VectorTiles;

    /// <summary>
    /// Contains the immutable metadata of a map file.
    /// </summary>
    /// <seealso cref= MapsforgeSharp.Reader.MapFile#getMapFileInfo() </seealso>
    public class MapFileInfo
	{
		/// <summary>
		/// The bounding box of the map file.
		/// </summary>
		public readonly BoundingBox BoundingBox;

		/// <summary>
		/// The comment field of the map file (may be null).
		/// </summary>
		public readonly string Comment;

		/// <summary>
		/// The created by field of the map file (may be null).
		/// </summary>
		public readonly string CreatedBy;

		/// <summary>
		/// True if the map file includes debug information, false otherwise.
		/// </summary>
		public readonly bool DebugFile;

		/// <summary>
		/// The size of the map file, measured in bytes.
		/// </summary>
		public readonly long FileSize;

		/// <summary>
		/// The file version number of the map file.
		/// </summary>
		public readonly int FileVersion;

		/// <summary>
		/// The preferred language(s) separated with ',' for names as defined in ISO 639-1 or ISO 639-2 (may be null).
		/// </summary>
		public readonly string LanguagesPreference;

		/// <summary>
		/// The date of the map data in milliseconds since January 1, 1970.
		/// </summary>
		public readonly long MapDate;

		/// <summary>
		/// The number of sub-files in the map file.
		/// </summary>
		public readonly sbyte NumberOfSubFiles;

		/// <summary>
		/// The POI tags.
		/// </summary>
		public readonly Tag[] PoiTags;

		/// <summary>
		/// The name of the projection used in the map file.
		/// </summary>
		public readonly string ProjectionName;

		/// <summary>
		/// The map start position from the file header (may be null).
		/// </summary>
		public readonly Point StartPosition;

		/// <summary>
		/// The map start zoom level from the file header (may be null).
		/// </summary>
		public readonly sbyte? StartZoomLevel;

		/// <summary>
		/// The size of the tiles in pixels.
		/// </summary>
		public readonly int TilePixelSize;

		/// <summary>
		/// The way tags.
		/// </summary>
		public readonly Tag[] WayTags;

		public readonly sbyte ZoomLevelMin;
		public readonly sbyte ZoomLevelMax;

		internal MapFileInfo(MapFileInfoBuilder mapFileInfoBuilder)
		{
			this.Comment = mapFileInfoBuilder.optionalFields.Comment;
			this.CreatedBy = mapFileInfoBuilder.optionalFields.CreatedBy;
			this.DebugFile = mapFileInfoBuilder.optionalFields.IsDebugFile;
			this.FileSize = mapFileInfoBuilder.fileSize;
			this.FileVersion = mapFileInfoBuilder.fileVersion;
			this.LanguagesPreference = mapFileInfoBuilder.optionalFields.LanguagesPreference;
			this.BoundingBox = mapFileInfoBuilder.boundingBox;
			this.MapDate = mapFileInfoBuilder.mapDate;
			this.NumberOfSubFiles = mapFileInfoBuilder.numberOfSubFiles;
			this.PoiTags = mapFileInfoBuilder.poiTags;
			this.ProjectionName = mapFileInfoBuilder.projectionName;
			this.StartPosition = mapFileInfoBuilder.optionalFields.StartPosition;
			this.StartZoomLevel = mapFileInfoBuilder.optionalFields.StartZoomLevel;
			this.TilePixelSize = mapFileInfoBuilder.tilePixelSize;
			this.WayTags = mapFileInfoBuilder.wayTags;
			this.ZoomLevelMax = mapFileInfoBuilder.zoomLevelMax;
			this.ZoomLevelMin = mapFileInfoBuilder.zoomLevelMin;
		}
	}
}