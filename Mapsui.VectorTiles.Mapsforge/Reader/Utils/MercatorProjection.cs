/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2013 Stephan Brandt <stephan@contagt.com>
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

namespace Mapsui.VectorTiles.Mapsforge.Reader.Utils
{
    using Geometries;
    using System;

    /// <summary>
    /// An implementation of the spherical Mercator projection.
    /// 
    /// There are generally two methods for each operation: one taking a byte zoomlevel and
    /// a parallel one taking a double scaleFactor. The scaleFactor is Math.pow(2, zoomLevel)
    /// for a given zoomlevel, but it the operations take intermediate values as well.
    /// The zoomLevel operation is a little faster as it can make use of shift operations,
    /// the scaleFactor operation offers greater flexibility in computing the values for
    /// intermediate zoomlevels.
    /// 
    /// </summary>
    public sealed class MercatorProjection
	{
		/// <summary>
		/// The circumference of the earth at the equator in meters.
		/// </summary>
		public const double EARTH_CIRCUMFERENCE = 40075016.686;
		/// <summary>
		/// Maximum possible latitude coordinate of the map.
		/// </summary>
		public const double LATITUDE_MAX = 85.05112877980659;

		/// <summary>
		/// Minimum possible latitude coordinate of the map.
		/// </summary>
		public static readonly double LATITUDE_MIN = -LATITUDE_MAX;

		// TODO some operations actually do not rely on the tile size, but are composited
		// from operations that require a tileSize parameter (which is effectively cancelled
		// out). A shortcut version of those operations should be implemented and then this
		// variable be removed.
		private const int DUMMY_TILE_SIZE = 256;

		/// <summary>
		/// Calculates the distance on the ground that is represented by a single pixel on the map.
		/// </summary>
		/// <param name="latitude">
		///            the latitude coordinate at which the resolution should be calculated. </param>
		/// <param name="scaleFactor">
		///            the zoom level at which the resolution should be calculated. </param>
		/// <returns> the ground resolution at the given latitude and zoom level. </returns>
		public static double CalculateGroundResolutionWithScaleFactor(double latitude, double scaleFactor, int tileSize)
		{
			long mapSize = GetMapSizeWithScaleFactor(scaleFactor, tileSize);
			return Math.Cos(latitude * (Math.PI / 180)) * EARTH_CIRCUMFERENCE / mapSize;
		}

		/// <summary>
		/// Calculates the distance on the ground that is represented by a single pixel on the map.
		/// </summary>
		/// <param name="latitude">
		///            the latitude coordinate at which the resolution should be calculated. </param>
		/// <param name="mapSize">
		///            precomputed size of map. </param>
		/// <returns> the ground resolution at the given latitude and zoom level. </returns>
		public static double CalculateGroundResolution(double latitude, long mapSize)
		{
			return Math.Cos(latitude * (Math.PI / 180)) * EARTH_CIRCUMFERENCE / mapSize;
		}


		/// <summary>
		/// Get Point from Pixels.
		/// </summary>
		public static Point FromPixelsWithScaleFactor(double pixelX, double pixelY, double scaleFactor, int tileSize)
		{
			return new Point(PixelYToLatitudeWithScaleFactor(pixelY, scaleFactor, tileSize), PixelXToLongitudeWithScaleFactor(pixelX, scaleFactor, tileSize));
		}

		/// <summary>
		/// Get Point from Pixels.
		/// </summary>
		public static Point FromPixels(double pixelX, double pixelY, long mapSize)
		{
			return new Point(PixelYToLatitude(pixelY, mapSize), PixelXToLongitude(pixelX, mapSize));
		}

		/// <param name="scaleFactor">
		///            the scale factor for which the size of the world map should be returned. </param>
		/// <returns> the horizontal and vertical size of the map in pixel at the given zoom level. </returns>
		/// <exception cref="IllegalArgumentException">
		///             if the given scale factor is < 1 </exception>
		public static long GetMapSizeWithScaleFactor(double scaleFactor, int tileSize)
		{
			if (scaleFactor < 1)
			{
				throw new System.ArgumentException("scale factor must not < 1 " + scaleFactor);
			}
			return (long)(tileSize * (Math.Pow(2, ScaleFactorToZoomLevel(scaleFactor))));
		}

		/// <param name="zoomLevel">
		///            the zoom level for which the size of the world map should be returned. </param>
		/// <returns> the horizontal and vertical size of the map in pixel at the given zoom level. </returns>
		/// <exception cref="IllegalArgumentException">
		///             if the given zoom level is negative. </exception>
		public static long GetMapSize(sbyte zoomLevel, int tileSize)
		{
			if (zoomLevel < 0)
			{
				throw new System.ArgumentException("zoom level must not be negative: " + zoomLevel);
			}
			return (long) tileSize << zoomLevel;
		}

		public static Point GetPixelWithScaleFactor(Point Point, double scaleFactor, int tileSize)
		{
			double pixelX = MercatorProjection.LongitudeToPixelXWithScaleFactor(Point.X, scaleFactor, tileSize);
			double pixelY = MercatorProjection.LatitudeToPixelYWithScaleFactor(Point.Y, scaleFactor, tileSize);
			return new Point(pixelX, pixelY);
		}

		public static Point GetPixel(Point Point, long mapSize)
		{
			double pixelX = MercatorProjection.LongitudeToPixelX(Point.X, mapSize);
			double pixelY = MercatorProjection.LatitudeToPixelY(Point.Y, mapSize);
			return new Point(pixelX, pixelY);
		}

		/// <summary>
		/// Calculates the absolute pixel position for a zoom level and tile size
		/// </summary>
		/// <param name="Point"> the geographic position. </param>
		/// <param name="mapSize"> precomputed size of map. </param>
		/// <returns> the absolute pixel coordinates (for world) </returns>

		public static Point GetPixelAbsolute(Point Point, long mapSize)
		{
			return GetPixelRelative(Point, mapSize, 0, 0);
		}

		/// <summary>
		/// Calculates the absolute pixel position for a zoom level and tile size relative to origin
		/// </summary>
		/// <param name="Point"> </param>
		/// <param name="mapSize"> precomputed size of map. </param>
		/// <returns> the relative pixel position to the origin values (e.g. for a tile) </returns>
		public static Point GetPixelRelative(Point Point, long mapSize, double x, double y)
		{
			double pixelX = MercatorProjection.LongitudeToPixelX(Point.X, mapSize) - x;
			double pixelY = MercatorProjection.LatitudeToPixelY(Point.Y, mapSize) - y;
			return new Point(pixelX, pixelY);
		}

		/// <summary>
		/// Calculates the absolute pixel position for a zoom level and tile size relative to origin
		/// </summary>
		/// <param name="Point"> </param>
		/// <param name="mapSize"> precomputed size of map. </param>
		/// <returns> the relative pixel position to the origin values (e.g. for a tile) </returns>
		public static Point GetPixelRelative(Point Point, long mapSize, Point origin)
		{
			return GetPixelRelative(Point, mapSize, origin.X, origin.Y);
		}

		/// <summary>
		/// Calculates the absolute pixel position for a zoom level and tile size relative to origin
		/// </summary>
		/// <param name="Point"> </param>
		/// <param name="tile"> tile </param>
		/// <returns> the relative pixel position to the origin values (e.g. for a tile) </returns>
		//public static Point GetPixelRelativeToTile(Point Point, TileInfo tile)
		//{
		//	return GetPixelRelative(Point, tile.Index.MapSize, tile.Origin);
		//}

		/// <summary>
		/// Converts a latitude coordinate (in degrees) to a pixel Y coordinate at a certain zoom level.
		/// </summary>
		/// <param name="latitude">
		///            the latitude coordinate that should be converted. </param>
		/// <param name="scaleFactor">
		///            the scale factor at which the coordinate should be converted. </param>
		/// <returns> the pixel Y coordinate of the latitude value. </returns>
		public static double LatitudeToPixelYWithScaleFactor(double latitude, double scaleFactor, int tileSize)
		{
			double sinLatitude = Math.Sin(latitude * (Math.PI / 180));
			long mapSize = GetMapSizeWithScaleFactor(scaleFactor, tileSize);
			// FIXME improve this formula so that it works correctly without the clipping
			double pixelY = (0.5 - Math.Log((1 + sinLatitude) / (1 - sinLatitude)) / (4 * Math.PI)) * mapSize;
			return Math.Min(Math.Max(0, pixelY), mapSize);
		}

		/// <summary>
		/// Converts a latitude coordinate (in degrees) to a pixel Y coordinate at a certain zoom level.
		/// </summary>
		/// <param name="latitude">
		///            the latitude coordinate that should be converted. </param>
		/// <param name="zoomLevel">
		///            the zoom level at which the coordinate should be converted. </param>
		/// <returns> the pixel Y coordinate of the latitude value. </returns>
		public static double LatitudeToPixelY(double latitude, sbyte zoomLevel, int tileSize)
		{
			double sinLatitude = Math.Sin(latitude * (Math.PI / 180));
			long mapSize = GetMapSize(zoomLevel, tileSize);
			// FIXME improve this formula so that it works correctly without the clipping
			double pixelY = (0.5 - Math.Log((1 + sinLatitude) / (1 - sinLatitude)) / (4 * Math.PI)) * mapSize;
			return Math.Min(Math.Max(0, pixelY), mapSize);
		}

		/// <summary>
		/// Converts a latitude coordinate (in degrees) to a pixel Y coordinate at a certain zoom level.
		/// </summary>
		/// <param name="latitude">
		///            the latitude coordinate that should be converted. </param>
		/// <param name="mapSize">
		///            precomputed size of map. </param>
		/// <returns> the pixel Y coordinate of the latitude value. </returns>
		public static double LatitudeToPixelY(double latitude, long mapSize)
		{
			double sinLatitude = Math.Sin(latitude * (Math.PI / 180));
			// FIXME improve this formula so that it works correctly without the clipping
			double pixelY = (0.5 - Math.Log((1 + sinLatitude) / (1 - sinLatitude)) / (4 * Math.PI)) * mapSize;
			return Math.Min(Math.Max(0, pixelY), mapSize);
		}

		/// <summary>
		/// Converts a latitude coordinate (in degrees) to a tile Y number at a certain zoom level.
		/// </summary>
		/// <param name="latitude">
		///            the latitude coordinate that should be converted. </param>
		/// <param name="scaleFactor">
		///            the scale factor at which the coordinate should be converted. </param>
		/// <returns> the tile Y number of the latitude value. </returns>
		public static int LatitudeToTileY(double latitude, double scaleFactor)
		{
			return PixelYToTileY(LatitudeToPixelYWithScaleFactor(latitude, scaleFactor, DUMMY_TILE_SIZE), scaleFactor, DUMMY_TILE_SIZE);
		}

		/// <summary>
		/// Converts a latitude coordinate (in degrees) to a tile Y number at a certain zoom level.
		/// </summary>
		/// <param name="latitude">
		///            the latitude coordinate that should be converted. </param>
		/// <param name="zoomLevel">
		///            the zoom level at which the coordinate should be converted. </param>
		/// <returns> the tile Y number of the latitude value. </returns>
		public static int LatitudeToTileY(double latitude, sbyte zoomLevel)
		{
			return PixelYToTileY(LatitudeToPixelY(latitude, zoomLevel, DUMMY_TILE_SIZE), zoomLevel, DUMMY_TILE_SIZE);
		}

		/// <summary>
		/// Converts a longitude coordinate (in degrees) to a pixel X coordinate at a certain scale factor.
		/// </summary>
		/// <param name="longitude">
		///            the longitude coordinate that should be converted. </param>
		/// <param name="scaleFactor">
		///            the scale factor at which the coordinate should be converted. </param>
		/// <returns> the pixel X coordinate of the longitude value. </returns>
		public static double LongitudeToPixelXWithScaleFactor(double longitude, double scaleFactor, int tileSize)
		{
			long mapSize = GetMapSizeWithScaleFactor(scaleFactor, tileSize);
			return (longitude + 180) / 360 * mapSize;
		}

		/// <summary>
		/// Converts a longitude coordinate (in degrees) to a pixel X coordinate at a certain zoom level.
		/// </summary>
		/// <param name="longitude">
		///            the longitude coordinate that should be converted. </param>
		/// <param name="mapSize">
		///            precomputed size of map. </param>
		/// <returns> the pixel X coordinate of the longitude value. </returns>
		public static double LongitudeToPixelX(double longitude, long mapSize)
		{
			return (longitude + 180) / 360 * mapSize;
		}

		/// <summary>
		/// Converts a longitude coordinate (in degrees) to the tile X number at a certain scale factor.
		/// </summary>
		/// <param name="longitude">
		///            the longitude coordinate that should be converted. </param>
		/// <param name="scaleFactor">
		///            the scale factor at which the coordinate should be converted. </param>
		/// <returns> the tile X number of the longitude value. </returns>
		public static int LongitudeToTileX(double longitude, double scaleFactor)
		{
			return PixelXToTileX(LongitudeToPixelXWithScaleFactor(longitude, scaleFactor, DUMMY_TILE_SIZE), scaleFactor, DUMMY_TILE_SIZE);
		}

		/// <summary>
		/// Converts a longitude coordinate (in degrees) to the tile X number at a certain zoom level.
		/// </summary>
		/// <param name="longitude">
		///            the longitude coordinate that should be converted. </param>
		/// <param name="zoomLevel">
		///            the zoom level at which the coordinate should be converted. </param>
		/// <returns> the tile X number of the longitude value. </returns>
		public static int LongitudeToTileX(double longitude, sbyte zoomLevel)
		{
			return PixelXToTileX(LongitudeToPixelX(longitude, GetMapSize(zoomLevel, DUMMY_TILE_SIZE)), zoomLevel, DUMMY_TILE_SIZE);
		}

		/// <summary>
		/// Converts meters to pixels at latitude for zoom-level.
		/// </summary>
		/// <param name="meters">
		///            the meters to convert </param>
		/// <param name="latitude">
		///            the latitude for the conversion. </param>
		/// <param name="scaleFactor">
		///            the scale factor for the conversion. </param>
		/// <returns> pixels that represent the meters at the given zoom-level and latitude. </returns>
		public static double MetersToPixelsWithScaleFactor(float meters, double latitude, double scaleFactor, int tileSize)
		{
			return meters / MercatorProjection.CalculateGroundResolutionWithScaleFactor(latitude, scaleFactor, tileSize);
		}

		/// <summary>
		/// Converts meters to pixels at latitude for zoom-level.
		/// </summary>
		/// <param name="meters">
		///            the meters to convert </param>
		/// <param name="latitude">
		///            the latitude for the conversion. </param>
		/// <param name="mapSize">
		///            precomputed size of map. </param>
		/// <returns> pixels that represent the meters at the given zoom-level and latitude. </returns>
		public static double MetersToPixels(float meters, double latitude, long mapSize)
		{
			return meters / MercatorProjection.CalculateGroundResolution(latitude, mapSize);
		}

		/// <summary>
		/// Converts a pixel X coordinate at a certain zoom level to a longitude coordinate.
		/// </summary>
		/// <param name="pixelX">
		///            the pixel X coordinate that should be converted. </param>
		/// <param name="scaleFactor">
		///            the scale factor at which the coordinate should be converted. </param>
		/// <returns> the longitude value of the pixel X coordinate. </returns>
		/// <exception cref="IllegalArgumentException">
		///             if the given pixelX coordinate is invalid. </exception>
		public static double PixelXToLongitudeWithScaleFactor(double pixelX, double scaleFactor, int tileSize)
		{
			long mapSize = GetMapSizeWithScaleFactor(scaleFactor, tileSize);
			if (pixelX < 0 || pixelX > mapSize)
			{
				throw new System.ArgumentException("invalid pixelX coordinate at zoom level " + scaleFactor + ": " + pixelX);
			}
			return 360 * ((pixelX / mapSize) - 0.5);
		}

		/// <summary>
		/// Converts a pixel X coordinate at a certain zoom level to a longitude coordinate.
		/// </summary>
		/// <param name="pixelX">
		///            the pixel X coordinate that should be converted. </param>
		/// <param name="mapSize">
		///            precomputed size of map. </param>
		/// <returns> the longitude value of the pixel X coordinate. </returns>
		/// <exception cref="IllegalArgumentException">
		///             if the given pixelX coordinate is invalid. </exception>
		public static double PixelXToLongitude(double pixelX, long mapSize)
		{
			if (pixelX < 0 || pixelX > mapSize)
			{
				throw new System.ArgumentException("invalid pixelX coordinate " + mapSize + ": " + pixelX);
			}
			return 360 * ((pixelX / mapSize) - 0.5);
		}

		/// <summary>
		/// Converts a pixel X coordinate to the tile X number.
		/// </summary>
		/// <param name="pixelX">
		///            the pixel X coordinate that should be converted. </param>
		/// <param name="scaleFactor">
		///            the scale factor at which the coordinate should be converted. </param>
		/// <returns> the tile X number. </returns>
		public static int PixelXToTileX(double pixelX, double scaleFactor, int tileSize)
		{
			return (int) Math.Min(Math.Max(pixelX / tileSize, 0), scaleFactor - 1);
		}

		/// <summary>
		/// Converts a pixel X coordinate to the tile X number.
		/// </summary>
		/// <param name="pixelX">
		///            the pixel X coordinate that should be converted. </param>
		/// <param name="zoomLevel">
		///            the zoom level at which the coordinate should be converted. </param>
		/// <returns> the tile X number. </returns>
		public static int PixelXToTileX(double pixelX, sbyte zoomLevel, int tileSize)
		{
			return (int) Math.Min(Math.Max(pixelX / tileSize, 0), Math.Pow(2, zoomLevel) - 1);
		}

		/// <summary>
		/// Converts a pixel Y coordinate at a certain zoom level to a latitude coordinate.
		/// </summary>
		/// <param name="pixelY">
		///            the pixel Y coordinate that should be converted. </param>
		/// <param name="scaleFactor">
		///            the scale factor at which the coordinate should be converted. </param>
		/// <returns> the latitude value of the pixel Y coordinate. </returns>
		/// <exception cref="IllegalArgumentException">
		///             if the given pixelY coordinate is invalid. </exception>
		public static double PixelYToLatitudeWithScaleFactor(double pixelY, double scaleFactor, int tileSize)
		{
			long mapSize = GetMapSizeWithScaleFactor(scaleFactor, tileSize);
			if (pixelY < 0 || pixelY > mapSize)
			{
				throw new System.ArgumentException("invalid pixelY coordinate at zoom level " + scaleFactor + ": " + pixelY);
			}
			double y = 0.5 - (pixelY / mapSize);
			return 90 - 360 * Math.Atan(Math.Exp(-y * (2 * Math.PI))) / Math.PI;
		}

		/// <summary>
		/// Converts a pixel Y coordinate at a certain zoom level to a latitude coordinate.
		/// </summary>
		/// <param name="pixelY">
		///            the pixel Y coordinate that should be converted. </param>
		/// <param name="mapSize">
		///            precomputed size of map. </param>
		/// <returns> the latitude value of the pixel Y coordinate. </returns>
		/// <exception cref="IllegalArgumentException">
		///             if the given pixelY coordinate is invalid. </exception>
		public static double PixelYToLatitude(double pixelY, long mapSize)
		{
			if (pixelY < 0 || pixelY > mapSize)
			{
				throw new System.ArgumentException("invalid pixelY coordinate " + mapSize + ": " + pixelY);
			}
			double y = 0.5 - (pixelY / mapSize);
			return 90 - 360 * Math.Atan(Math.Exp(-y * (2 * Math.PI))) / Math.PI;
		}

		/// <summary>
		/// Converts a pixel Y coordinate to the tile Y number.
		/// </summary>
		/// <param name="pixelY">
		///            the pixel Y coordinate that should be converted. </param>
		/// <param name="scaleFactor">
		///            the scale factor at which the coordinate should be converted. </param>
		/// <returns> the tile Y number. </returns>
		public static int PixelYToTileY(double pixelY, double scaleFactor, int tileSize)
		{
			return (int) Math.Min(Math.Max(pixelY / tileSize, 0), scaleFactor - 1);
		}

		/// <summary>
		/// Converts a pixel Y coordinate to the tile Y number.
		/// </summary>
		/// <param name="pixelY">
		///            the pixel Y coordinate that should be converted. </param>
		/// <param name="zoomLevel">
		///            the zoom level at which the coordinate should be converted. </param>
		/// <returns> the tile Y number. </returns>
		public static int PixelYToTileY(double pixelY, sbyte zoomLevel, int tileSize)
		{
			return (int) Math.Min(Math.Max(pixelY / tileSize, 0), Math.Pow(2, zoomLevel) - 1);
		}

		/// <summary>
		/// Converts a scaleFactor to a zoomLevel.
		/// Note that this will return a double, as the scale factors cover the
		/// intermediate zoom levels as well.
		/// </summary>
		/// <param name="scaleFactor">
		///            the scale factor to convert to a zoom level. </param>
		/// <returns> the zoom level. </returns>
		public static double ScaleFactorToZoomLevel(double scaleFactor)
		{
			return Math.Log(scaleFactor) / Math.Log(2);
		}

		/// <param name="tileNumber">
		///            the tile number that should be converted. </param>
		/// <returns> the pixel coordinate for the given tile number. </returns>
		public static long TileToPixel(long tileNumber, int tileSize)
		{
			return tileNumber * tileSize;
		}

		/// <summary>
		/// Converts a tile X number at a certain zoom level to a longitude coordinate.
		/// </summary>
		/// <param name="tileX">
		///            the tile X number that should be converted. </param>
		/// <param name="scaleFactor">
		///            the scale factor at which the number should be converted. </param>
		/// <returns> the longitude value of the tile X number. </returns>
		public static double TileXToLongitude(long tileX, double scaleFactor)
		{
			return PixelXToLongitudeWithScaleFactor(tileX * DUMMY_TILE_SIZE, scaleFactor, DUMMY_TILE_SIZE);
		}

		/// <summary>
		/// Converts a tile X number at a certain zoom level to a longitude coordinate.
		/// </summary>
		/// <param name="tileX">
		///            the tile X number that should be converted. </param>
		/// <param name="zoomLevel">
		///            the zoom level at which the number should be converted. </param>
		/// <returns> the longitude value of the tile X number. </returns>
		public static double TileXToLongitude(long tileX, sbyte zoomLevel)
		{
			return PixelXToLongitude(tileX * DUMMY_TILE_SIZE, GetMapSize(zoomLevel, DUMMY_TILE_SIZE));
		}

		/// <summary>
		/// Converts a tile Y number at a certain zoom level to a latitude coordinate.
		/// </summary>
		/// <param name="tileY">
		///            the tile Y number that should be converted. </param>
		/// <param name="scaleFactor">
		///            the scale factor at which the number should be converted. </param>
		/// <returns> the latitude value of the tile Y number. </returns>
		public static double TileYToLatitude(long tileY, double scaleFactor)
		{
			return PixelYToLatitudeWithScaleFactor(tileY * DUMMY_TILE_SIZE, scaleFactor, DUMMY_TILE_SIZE);
		}

		/// <summary>
		/// Converts a tile Y number at a certain zoom level to a latitude coordinate.
		/// </summary>
		/// <param name="tileY">
		///            the tile Y number that should be converted. </param>
		/// <param name="zoomLevel">
		///            the zoom level at which the number should be converted. </param>
		/// <returns> the latitude value of the tile Y number. </returns>
		public static double TileYToLatitude(long tileY, sbyte zoomLevel)
		{
			return PixelYToLatitude(tileY * DUMMY_TILE_SIZE, GetMapSize(zoomLevel, DUMMY_TILE_SIZE));
		}

		/// <summary>
		/// Converts a zoom level to a scale factor.
		/// </summary>
		/// <param name="zoomLevel">
		///            the zoom level to convert. </param>
		/// <returns> the corresponding scale factor. </returns>
		public static double ZoomLevelToScaleFactor(sbyte zoomLevel)
		{
			return Math.Pow(2, zoomLevel);
		}

		private MercatorProjection()
		{
			throw new System.InvalidOperationException();
		}
	}
}