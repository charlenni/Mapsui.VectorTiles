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
    using NUnit.Framework;
    using Geometries;
    using Datastore;
    using Utils;

    internal sealed class EncodingTests
	{
		private const sbyte ZOOM_LEVEL = 8;

		internal static void RunTest(MapFile mapFile)
		{
            // Calculate tile X and Y for lat=0 and lon=0
            int tileX = MercatorProjection.LongitudeToTileX(0, ZOOM_LEVEL);
            int tileY = MercatorProjection.LatitudeToTileY(0, ZOOM_LEVEL);

            Tile tile = new Tile(tileX, tileY, ZOOM_LEVEL);

			MapReadResult mapReadResult = mapFile.ReadMapData(tile);
			mapFile.Close();

			Assert.AreEqual(mapReadResult.PointOfInterests.Count, 0);
			Assert.AreEqual(1, mapReadResult.Ways.Count);

			Point point1 = new Point(0.0, 0.0);
			Point point2 = new Point(0.1, 0.0);
			Point point3 = new Point(0.1, -0.1);
            Point point4 = new Point(0.0, -0.1);
            Point[][] latLongsExpected = new Point[][]
			{
				new Point[] {point1, point2, point3, point4, point1}
			};

			Way way = mapReadResult.Ways[0];
            // TODO: Was ArrayEquals()
			Assert.AreEqual(latLongsExpected, way.Points);
		}

		private EncodingTests()
		{
			throw new System.InvalidOperationException();
		}
	}
}