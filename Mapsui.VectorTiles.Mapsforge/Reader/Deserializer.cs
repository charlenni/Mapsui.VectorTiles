/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
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

namespace Mapsui.VectorTiles.Mapsforge.Reader
{
	/// <summary>
	/// An utility class to convert byte arrays to numbers.
	/// </summary>
	public sealed class Deserializer
	{
		/// <summary>
		/// Converts five bytes of a byte array to an unsigned long.
		/// <para>
		/// The byte order is big-endian.
		/// 
		/// </para>
		/// </summary>
		/// <param name="buffer">
		///            the byte array. </param>
		/// <param name="offset">
		///            the offset in the array. </param>
		/// <returns> the long value. </returns>
		public static long GetFiveBytesLong(sbyte[] buffer, int offset)
		{
			return (buffer[offset] & 0xffL) << 32 | (buffer[offset + 1] & 0xffL) << 24 | (buffer[offset + 2] & 0xffL) << 16 | (buffer[offset + 3] & 0xffL) << 8 | (buffer[offset + 4] & 0xffL);
		}

        /// <summary>
        /// Converts four bytes of a byte array to a signed int.
        /// <para>
        /// The byte order is big-endian.
        /// 
        /// </para>
        /// </summary>
        /// <param name="buffer">
        ///            the byte array. </param>
        /// <param name="offset">
        ///            the offset in the array. </param>
        /// <returns> the int value. </returns>
        public static int GetInt(sbyte[] buffer, int offset)
		{
			return buffer[offset] << 24 | (buffer[offset + 1] & 0xff) << 16 | (buffer[offset + 2] & 0xff) << 8 | (buffer[offset + 3] & 0xff);
		}

        /// <summary>
        /// Converts eight bytes of a byte array to a signed long.
        /// <para>
        /// The byte order is big-endian.
        /// 
        /// </para>
        /// </summary>
        /// <param name="buffer">
        ///            the byte array. </param>
        /// <param name="offset">
        ///            the offset in the array. </param>
        /// <returns> the long value. </returns>
        public static long GetLong(sbyte[] buffer, int offset)
		{
			return (buffer[offset] & 0xffL) << 56 | (buffer[offset + 1] & 0xffL) << 48 | (buffer[offset + 2] & 0xffL) << 40 | (buffer[offset + 3] & 0xffL) << 32 | (buffer[offset + 4] & 0xffL) << 24 | (buffer[offset + 5] & 0xffL) << 16 | (buffer[offset + 6] & 0xffL) << 8 | (buffer[offset + 7] & 0xffL);
		}

        /// <summary>
        /// Converts two bytes of a byte array to a signed int.
        /// <para>
        /// The byte order is big-endian.
        /// 
        /// </para>
        /// </summary>
        /// <param name="buffer">
        ///            the byte array. </param>
        /// <param name="offset">
        ///            the offset in the array. </param>
        /// <returns> the int value. </returns>
        public static int GetShort(sbyte[] buffer, int offset)
		{
			return buffer[offset] << 8 | (buffer[offset + 1] & 0xff);
		}

		private Deserializer()
		{
			throw new System.InvalidOperationException();
		}
	}
}