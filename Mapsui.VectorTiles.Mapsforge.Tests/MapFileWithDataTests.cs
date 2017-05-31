/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2016 Dirk Weltz
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
    using Datastore;
    using Geometries;
    using Header;
    using NUnit.Framework;
    using System.Collections.Generic;
    using Utils;

    public class MapFileWithDataTests
	{
		private const sbyte ZOOM_LEVEL_MAX = 11;
		private const int ZOOM_LEVEL_MIN = 6;

		private static void AssertLatLongsEquals(List<List<Point>> points1, List<List<Point>> points2)
		{
			Assert.AreEqual(points1.Count, points2.Count);

			for (int i = 0; i < points1.Count; ++i)
			{
				Assert.AreEqual(points1[i].Count, points2[i].Count);

				for (int j = 0; j < points1[i].Count; ++j)
				{
					Point latLong1 = points1[i][j];
                    Point latLong2 = points2[i][j];

					Assert.AreEqual(latLong1.Y, latLong2.Y, 0.000001);
					Assert.AreEqual(latLong1.X, latLong2.X, 0.000001);
				}
			}
		}

		private static void CheckPointOfInterest(PointOfInterest pointOfInterest)
		{
            Point poi = new Point(0.08, 0.04);

            Assert.AreEqual(7, pointOfInterest.Layer);
            Assert.AreEqual(poi.X, pointOfInterest.Position.X, 0);
            Assert.AreEqual(poi.Y, pointOfInterest.Position.Y, 0.000001);
			Assert.AreEqual(4, pointOfInterest.Tags.Count);
			Assert.True(pointOfInterest.Tags.Contains(new Tag("place=country")));
			Assert.True(pointOfInterest.Tags.Contains(new Tag("name=АБВГДЕЖЗ")));
			Assert.True(pointOfInterest.Tags.Contains(new Tag("addr:housenumber=абвгдежз")));
			Assert.True(pointOfInterest.Tags.Contains(new Tag("ele=25")));
		}

		private static void CheckWay(Way way)
		{
			Assert.AreEqual(4, way.Layer);
			Assert.Null(way.LabelPosition);

            Point point1 = new Point(0.00, 0.00);
            Point point2 = new Point(0.08, 0.04);
            Point point3 = new Point(0.00, 0.08);
            List<List<Point>> pointsExpected = new List<List<Point>>();
            pointsExpected.Add(new List<Point>() { point1, point2, point3 });

            AssertLatLongsEquals(pointsExpected, way.Points);
			Assert.AreEqual(3, way.Tags.Count);
			Assert.True(way.Tags.Contains(new Tag("highway=motorway")));
			Assert.True(way.Tags.Contains(new Tag("name=ÄÖÜ")));
			Assert.True(way.Tags.Contains(new Tag("ref=äöü")));
		}

        [Test()]
		public virtual void WithDataTest()
		{
            MapFile mapFile = new MapFile(EmbeddedResourceLoader.Load("Resources.WithData.output.map", this.GetType()));

            MapFileInfo mapFileInfo = mapFile.MapFileInfo;
			Assert.True(mapFileInfo.DebugFile);

			for (sbyte zoomLevel = ZOOM_LEVEL_MIN; zoomLevel <= ZOOM_LEVEL_MAX; ++zoomLevel)
			{
                Point poi = new Point(0.04, 0.04);

				int tileX = MercatorProjection.LongitudeToTileX(0.04, zoomLevel);
				int tileY = MercatorProjection.LatitudeToTileY(0.04, zoomLevel);

                Tile tile = new Tile(tileX, tileY, zoomLevel);

                double lonMin = MercatorProjection.TileXToLongitude(tileX, zoomLevel);
                double lonMax = MercatorProjection.TileXToLongitude(tileX + 1, zoomLevel);
                double latMin = MercatorProjection.TileYToLatitude(tileY + 1, zoomLevel);
                double latMax = MercatorProjection.TileYToLatitude(tileY, zoomLevel);

                //tile.Index = new TileIndex(tileX, tileY, zoomLevel.ToString());
                //tile.Extent = new Extent(lonMin, latMin, lonMax, latMax);

                MapReadResult mapReadResult = mapFile.ReadMapData(tile);

				Assert.AreEqual(1, mapReadResult.PointOfInterests.Count);
				Assert.AreEqual(1, mapReadResult.Ways.Count);

				CheckPointOfInterest(mapReadResult.PointOfInterests[0]);
				CheckWay(mapReadResult.Ways[0]);
			}

			mapFile.Close();
		}
	}
}