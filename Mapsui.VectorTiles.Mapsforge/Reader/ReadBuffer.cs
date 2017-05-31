/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2015 devemux86
 * Copyright 2016 
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
    using Logging;
    using System;
    using System.IO;

    /// <summary>
    /// Reads from a <seealso cref="RandomAccessFile"/> into a buffer and decodes the data.
    /// </summary>
    public class ReadBuffer
	{
        private static object sync = new object();

		private const string CHARSET_UTF8 = "UTF-8";

		/// <summary>
		/// Default maximum buffer size which is supported by this implementation.
		/// </summary>
		private const int DEFAULT_MAXIMUM_BUFFER_SIZE = 2500000;

        private static int maximumBufferSize = DEFAULT_MAXIMUM_BUFFER_SIZE;

		/// <summary>
		/// Returns the maximum buffer size.
		/// </summary>
		/// <returns> the maximum buffer size. </returns>
		public static int MaximumBufferSize
		{
			get
			{
				lock (sync)
				{
					return maximumBufferSize;
				}
			}
			set
			{
				lock (sync)
				{
					ReadBuffer.maximumBufferSize = value;
				}
			}
		}

		private sbyte[] bufferData;
		private int bufferPosition;
		private readonly Stream inputFile;

		internal ReadBuffer(Stream inputFile)
		{
			this.inputFile = inputFile;
		}

		/// <summary>
		/// Returns one signed byte from the read buffer.
		/// </summary>
		/// <returns> the byte value. </returns>
		public virtual sbyte ReadByte()
		{
			return Convert.ToSByte(this.bufferData[this.bufferPosition++]);
		}

		/// <summary>
		/// Reads the given amount of bytes from the stream into the read buffer and resets the internal buffer position. If
		/// the capacity of the read buffer is too small, a larger one is created automatically.
		/// </summary>
		/// <param name="length">
		///            the amount of bytes to read from the stream. </param>
		/// <returns> true if the whole data was read successfully, false otherwise. </returns>
		/// <exception cref="IOException">
		///             if an error occurs while reading from stream. </exception>
		public virtual bool ReadFromStream(int length)
		{
			// ensure that the read buffer is large enough
			if (this.bufferData == null || this.bufferData.Length < length)
			{
				// ensure that the read buffer is not too large
				if (length > maximumBufferSize)
				{
					Logger.Log(LogLevel.Warning, "invalid read length: " + length);
					return false;
				}
				this.bufferData = new sbyte[length];
			}

			// reset the buffer position and read the data into the buffer
			this.bufferPosition = 0;
			return this.inputFile.Read((byte[])(Array)this.bufferData, 0, length) == length;
        }

        /// <summary>
        /// Reads the given amount of bytes from the stream into the read buffer and resets the internal buffer position. If
        /// the capacity of the read buffer is too small, a larger one is created automatically.
        /// </summary>
        /// <param name="offset">
        ///            the offset position, measured in bytes from the beginning of the file, at which to set the file pointer. </param>
        /// <param name="length">
        ///            the amount of bytes to read from the stream. </param>
        /// <returns> true if the whole data was read successfully, false otherwise. </returns>
        /// <exception cref="IOException">
        ///             if an error occurs while reading from stream. </exception>
        public virtual bool ReadFromStream(long offset, int length)
        {
            // ensure that the read buffer is large enough
            if (this.bufferData == null || this.bufferData.Length < length)
            {
                // ensure that the read buffer is not too large
                if (length > maximumBufferSize)
                {
                    Logger.Log(LogLevel.Warning, "invalid read length: " + length);
                    return false;
                }
                this.bufferData = new sbyte[length];
            }

            // reset the buffer position and read the data into the buffer
            this.bufferPosition = 0;

            lock(sync)
            {
                this.inputFile.Seek(offset, SeekOrigin.Begin);
                return this.inputFile.Read((byte[])(Array)this.bufferData, 0, length) == length;
            }
        }

        /// <summary>
        /// Converts four bytes from the read buffer to a signed int.
        /// <para>
        /// The byte order is big-endian.
        /// 
        /// </para>
        /// </summary>
        /// <returns> the int value. </returns>
        public virtual int ReadInt()
		{
			this.bufferPosition += 4;
			return Deserializer.GetInt(this.bufferData, this.bufferPosition - 4);
		}

		/// <summary>
		/// Converts eight bytes from the read buffer to a signed long.
		/// <para>
		/// The byte order is big-endian.
		/// 
		/// </para>
		/// </summary>
		/// <returns> the long value. </returns>
		public virtual long ReadLong()
		{
			this.bufferPosition += 8;
			return Deserializer.GetLong(this.bufferData, this.bufferPosition - 8);
		}

		/// <summary>
		/// Converts two bytes from the read buffer to a signed int.
		/// <para>
		/// The byte order is big-endian.
		/// 
		/// </para>
		/// </summary>
		/// <returns> the int value. </returns>
		public virtual int ReadShort()
		{
			this.bufferPosition += 2;
			return Deserializer.GetShort(this.bufferData, this.bufferPosition - 2);
		}

		/// <summary>
		/// Converts a variable amount of bytes from the read buffer to a signed int.
		/// <para>
		/// The first bit is for continuation info, the other six (last byte) or seven (all other bytes) bits are for data.
		/// The second bit in the last byte indicates the sign of the number.
		/// 
		/// </para>
		/// </summary>
		/// <returns> the int value. </returns>
		public virtual int ReadSignedInt()
		{
			int variableByteDecode = 0;
			sbyte variableByteShift = 0;

			// check if the continuation bit is set
			while ((this.bufferData[this.bufferPosition] & 0x80) != 0)
			{
				variableByteDecode |= (this.bufferData[this.bufferPosition++] & 0x7f) << variableByteShift;
				variableByteShift += 7;
			}

			// read the six data bits from the last byte
			if ((this.bufferData[this.bufferPosition] & 0x40) != 0)
			{
				// negative
				return -(variableByteDecode | ((this.bufferData[this.bufferPosition++] & 0x3f) << variableByteShift));
			}
			// positive
			return variableByteDecode | ((this.bufferData[this.bufferPosition++] & 0x3f) << variableByteShift);
		}

		/// <summary>
		/// Converts a variable amount of bytes from the read buffer to an unsigned int.
		/// <para>
		/// The first bit is for continuation info, the other seven bits are for data.
		/// 
		/// </para>
		/// </summary>
		/// <returns> the int value. </returns>
		public virtual int ReadUnsignedInt()
		{
			int variableByteDecode = 0;
			sbyte variableByteShift = 0;

			// check if the continuation bit is set
			while ((this.bufferData[this.bufferPosition] & 0x80) != 0)
			{
				variableByteDecode |= (this.bufferData[this.bufferPosition++] & 0x7f) << variableByteShift;
				variableByteShift += 7;
			}

			// read the seven data bits from the last byte
			return variableByteDecode | (this.bufferData[this.bufferPosition++] << variableByteShift);
		}

		/// <summary>
		/// Decodes a variable amount of bytes from the read buffer to a string.
		/// </summary>
		/// <returns> the UTF-8 decoded string (may be null). </returns>
		public virtual string ReadUTF8EncodedString()
		{
			return ReadUTF8EncodedString(ReadUnsignedInt());
		}

		/// <summary>
		/// Decodes the given amount of bytes from the read buffer to a string.
		/// </summary>
		/// <param name="stringLength">
		///            the length of the string in bytes. </param>
		/// <returns> the UTF-8 decoded string (may be null). </returns>
		public virtual string ReadUTF8EncodedString(int stringLength)
		{
			if (stringLength > 0 && this.bufferPosition + stringLength <= this.bufferData.Length)
			{
				this.bufferPosition += stringLength;
				try
				{
					return System.Text.Encoding.GetEncoding(CHARSET_UTF8).GetString((byte[])(Array)this.bufferData, this.bufferPosition - stringLength, stringLength);
				}
				catch (ArgumentException e)
				{
					throw new System.InvalidOperationException(e.Message);
				}
			}
			Logger.Log(LogLevel.Warning, "invalid string length: " + stringLength);
			return null;
		}

		/// <returns> the current buffer position. </returns>
		internal virtual int BufferPosition
		{
			get
			{
				return this.bufferPosition;
			}
			set
			{
				this.bufferPosition = value;
			}
		}

		/// <returns> the current size of the read buffer. </returns>
		internal virtual int BufferSize
		{
			get
			{
				return this.bufferData.Length;
			}
		}

		/// <summary>
		/// Skips the given number of bytes in the read buffer.
		/// </summary>
		/// <param name="bytes">
		///            the number of bytes to skip. </param>
		internal virtual void SkipBytes(int bytes)
		{
			this.bufferPosition += bytes;
		}
	}
}