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

namespace Mapsui.VectorTiles.Mapsforge.Reader.Tests
{
    using Reader;
    using NUnit.Framework;

    public class DeserializerTests
	{
        [Test()]
		public virtual void GetIntTest()
		{
			sbyte[] buffer = new sbyte[] {0, 0, 0, 0};
			Assert.AreEqual(0, Deserializer.GetInt(buffer, 0));

			buffer = new sbyte[] {0, 0, 0, 1};
			Assert.AreEqual(1, Deserializer.GetInt(buffer, 0));

			buffer = new sbyte[] {0, 0, 0, 127};
			Assert.AreEqual(127, Deserializer.GetInt(buffer, 0));

			buffer = new sbyte[] {0, 0, 0, (sbyte)-128};
			Assert.AreEqual(128, Deserializer.GetInt(buffer, 0));

			buffer = new sbyte[] {0, 0, 0, (sbyte)-127};
			Assert.AreEqual(129, Deserializer.GetInt(buffer, 0));

			buffer = new sbyte[] {0, 0, 0, (sbyte)-1};
			Assert.AreEqual(255, Deserializer.GetInt(buffer, 0));

			buffer = new sbyte[] {0, 0, 1, 0};
			Assert.AreEqual(256, Deserializer.GetInt(buffer, 0));

			buffer = new sbyte[] {0, 0, 1, 1};
			Assert.AreEqual(257, Deserializer.GetInt(buffer, 0));

			buffer = new sbyte[] {0, 1, 0, 0};
			Assert.AreEqual(65536, Deserializer.GetInt(buffer, 0));

			buffer = new sbyte[] {0, 1, 0, 1};
			Assert.AreEqual(65537, Deserializer.GetInt(buffer, 0));

			buffer = new sbyte[] {1, 0, 0, 0};
			Assert.AreEqual(16777216, Deserializer.GetInt(buffer, 0));

			buffer = new sbyte[] {1, 0, 0, 1};
			Assert.AreEqual(16777217, Deserializer.GetInt(buffer, 0));
		}

        [Test()]
		public virtual void GetShortTest()
		{
			sbyte[] buffer = new sbyte[] {0, 0};
			Assert.AreEqual(0, Deserializer.GetShort(buffer, 0));

			buffer = new sbyte[] {0, 1};
			Assert.AreEqual(1, Deserializer.GetShort(buffer, 0));

			buffer = new sbyte[] {0, 127};
			Assert.AreEqual(127, Deserializer.GetShort(buffer, 0));

			buffer = new sbyte[] {0, (sbyte)-128};
			Assert.AreEqual(128, Deserializer.GetShort(buffer, 0));

			buffer = new sbyte[] {0, (sbyte)-127};
			Assert.AreEqual(129, Deserializer.GetShort(buffer, 0));

			buffer = new sbyte[] {0, (sbyte)-1};
			Assert.AreEqual(255, Deserializer.GetShort(buffer, 0));

			buffer = new sbyte[] {1, 0};
			Assert.AreEqual(256, Deserializer.GetShort(buffer, 0));

			buffer = new sbyte[] {1, 1};
			Assert.AreEqual(257, Deserializer.GetShort(buffer, 0));
		}
	}
}