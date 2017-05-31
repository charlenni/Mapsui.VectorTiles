/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014 Ludwig M Brinckmann
 * Copyright 2014 Christian Pesch
 * Copyright 2014 devemux86
 * Copyright 2015 Andreas Schildbach
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
    using System;
    using System.Collections.Generic;
    using Geometries;

    /// <summary>
    /// A utility class to convert, parse and validate geographical latitude/longitude coordinates.
    /// </summary>
    public sealed class PointUtils
	{
		/// <summary>
		/// The equatorial radius as defined by the <a href="http://en.wikipedia.org/wiki/World_Geodetic_System">WGS84
		/// ellipsoid</a>. WGS84 is the reference coordinate system used by the Global Positioning System.
		/// </summary>
		public const double EQUATORIAL_RADIUS = 6378137.0;

		/// <summary>
		/// Maximum possible latitude coordinate.
		/// </summary>
		public const double LATITUDE_MAX = 90;

		/// <summary>
		/// Minimum possible latitude coordinate.
		/// </summary>
		public static readonly double LATITUDE_MIN = -LATITUDE_MAX;

		/// <summary>
		/// Maximum possible longitude coordinate.
		/// </summary>
		public const double LONGITUDE_MAX = 180;

		/// <summary>
		/// Minimum possible longitude coordinate.
		/// </summary>
		public static readonly double LONGITUDE_MIN = -LONGITUDE_MAX;

		/// <summary>
		/// Conversion factor from degrees to microdegrees.
		/// </summary>
		private const double CONVERSION_FACTOR = 1000000.0;

		private const string DELIMITER = ",";

		/// <summary>
		/// Converts a coordinate from degrees to microdegrees (degrees * 10^6). No validation is performed.
		/// </summary>
		/// <param name="coordinate">
		///            the coordinate in degrees. </param>
		/// <returns> the coordinate in microdegrees (degrees * 10^6). </returns>
		public static int DegreesToMicrodegrees(double coordinate)
		{
			return (int)(coordinate * CONVERSION_FACTOR);
		}

		/// <summary>
		/// Creates a new LatLong from a comma-separated string of coordinates in the order latitude, longitude. All
		/// coordinate values must be in degrees.
		/// </summary>
		/// <param name="latLongString">
		///            the string that describes the LatLong. </param>
		/// <returns> a new LatLong with the given coordinates. </returns>
		/// <exception cref="IllegalArgumentException">
		///             if the string cannot be parsed or describes an invalid LatLong. </exception>
		public static Point FromString(string latLongString)
		{
			double[] coordinates = ParseCoordinateString(latLongString, 2);
			return new Point(coordinates[1], coordinates[0]);
		}

		/// <summary>
		/// Calculates the amount of degrees of latitude for a given distance in meters.
		/// </summary>
		/// <param name="meters">
		///            distance in meters </param>
		/// <returns> latitude degrees </returns>
		public static double LatitudeDistance(int meters)
		{
			return (meters * 360) / (2 * Math.PI * EQUATORIAL_RADIUS);
		}

		/// <summary>
		/// Calculates the amount of degrees of longitude for a given distance in meters.
		/// </summary>
		/// <param name="meters">
		///            distance in meters </param>
		/// <param name="latitude">
		///            the latitude at which the calculation should be performed </param>
		/// <returns> longitude degrees </returns>
		public static double LongitudeDistance(int meters, double latitude)
		{
			return (meters * 360) / (2 * Math.PI * EQUATORIAL_RADIUS * Math.Cos(latitude * Math.PI / 180.0));
		}

		/// <summary>
		/// Converts a coordinate from microdegrees (degrees * 10^6) to degrees. No validation is performed.
		/// </summary>
		/// <param name="coordinate">
		///            the coordinate in microdegrees (degrees * 10^6). </param>
		/// <returns> the coordinate in degrees. </returns>
		public static double MicrodegreesToDegrees(int coordinate)
		{
			return coordinate / CONVERSION_FACTOR;
		}

		/// <summary>
		/// Parses a given number of comma-separated coordinate values from the supplied string.
		/// </summary>
		/// <param name="coordinatesString">
		///            a comma-separated string of coordinate values. </param>
		/// <param name="numberOfCoordinates">
		///            the expected number of coordinate values in the string. </param>
		/// <returns> the coordinate values in the order they have been parsed from the string. </returns>
		/// <exception cref="IllegalArgumentException">
		///             if the string is invalid or does not contain the given number of coordinate values. </exception>
		public static double[] ParseCoordinateString(string coordinatesString, int numberOfCoordinates)
		{
			string[] stringTokenizer = coordinatesString.Split(DELIMITER[0]);
			IList<string> tokens = new List<string>(numberOfCoordinates);

			foreach(string token in stringTokenizer)
			{
				tokens.Add(token);
			}

        	if (tokens.Count != numberOfCoordinates)
			{
				throw new System.ArgumentException("invalid number of coordinate values: " + coordinatesString);
			}

			double[] coordinates = new double[numberOfCoordinates];
			for (int i = 0; i < numberOfCoordinates; ++i)
			{
                if (!double.TryParse(tokens[i], out coordinates[i]))
                {
                    throw new System.ArgumentException("invalid coordinate value: " + coordinatesString);
                }
            }
			return coordinates;
		}

		/// <param name="latitude">
		///            the latitude coordinate in degrees which should be validated. </param>
		/// <exception cref="IllegalArgumentException">
		///             if the latitude coordinate is invalid or <seealso cref="Double#NaN"/>. </exception>
		public static void ValidateLatitude(double latitude)
		{
			if (double.IsNaN(latitude) || latitude < LATITUDE_MIN || latitude > LATITUDE_MAX)
			{
				throw new System.ArgumentException("invalid latitude: " + latitude);
			}
		}

		/// <param name="longitude">
		///            the longitude coordinate in degrees which should be validated. </param>
		/// <exception cref="IllegalArgumentException">
		///             if the longitude coordinate is invalid or <seealso cref="Double#NaN"/>. </exception>
		public static void ValidateLongitude(double longitude)
		{
			if (double.IsNaN(longitude) || longitude < LONGITUDE_MIN || longitude > LONGITUDE_MAX)
			{
				throw new System.ArgumentException("invalid longitude: " + longitude);
			}
		}

		private PointUtils()
		{
			throw new System.InvalidOperationException();
		}
	}
}