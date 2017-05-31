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

namespace Mapsui.VectorTiles.Mapsforge.Reader.Tests
{
    using Geometries;
    using Header;
    using NUnit.Framework;
    using System;

    public class MapFileFileHeaderTests
	{
		private static readonly BoundingBox BOUNDING_BOX = new BoundingBox(0.2, 0.1, 0.4, 0.3);
		private const string COMMENT = "testcomment";
		private const string CREATED_BY = "mapsforge-map-writer-0.3.1-SNAPSHOT";
		private const int FILE_SIZE = 709;
		private const int FILE_VERSION = 3;
		private const string LANGUAGES_PREFERENCE = "en";
		private const long MAP_DATE = 1335871456973L;
		private const int NUMBER_OF_SUBFILES = 3;
		private const string PROJECTION_NAME = "Mercator";
		private static readonly Point START_POSITION = new Point(0.25, 0.15);
		private static readonly sbyte? START_ZOOM_LEVEL = Convert.ToSByte((sbyte) 16);
		private const int TILE_PIXEL_SIZE = 256;

        [Test()]
		public virtual void GetMapFileInfoTest()
		{
            MapFile mapFile = new MapFile(EmbeddedResourceLoader.Load("Resources.FileHeader.output.map", this.GetType()));

            MapFileInfo mapFileInfo = mapFile.MapFileInfo;
			mapFile.Close();

			Assert.AreEqual(BOUNDING_BOX, mapFileInfo.BoundingBox);
			Assert.AreEqual(FILE_SIZE, mapFileInfo.FileSize);
			Assert.AreEqual(FILE_VERSION, mapFileInfo.FileVersion);
			Assert.AreEqual(MAP_DATE, mapFileInfo.MapDate);
			Assert.AreEqual(NUMBER_OF_SUBFILES, mapFileInfo.NumberOfSubFiles);
			Assert.AreEqual(PROJECTION_NAME, mapFileInfo.ProjectionName);
			Assert.AreEqual(TILE_PIXEL_SIZE, mapFileInfo.TilePixelSize);

			Assert.AreEqual(0, mapFileInfo.PoiTags.Length);
			Assert.AreEqual(0, mapFileInfo.WayTags.Length);

			Assert.False(mapFileInfo.DebugFile);
			Assert.AreEqual(START_POSITION, mapFileInfo.StartPosition);
			Assert.AreEqual(START_ZOOM_LEVEL, mapFileInfo.StartZoomLevel);
			Assert.AreEqual(LANGUAGES_PREFERENCE, mapFileInfo.LanguagesPreference);
			Assert.AreEqual(COMMENT, mapFileInfo.Comment);
			Assert.AreEqual(CREATED_BY, mapFileInfo.CreatedBy);
		}
	}
}