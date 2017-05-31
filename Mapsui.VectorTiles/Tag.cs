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

namespace Mapsui.VectorTiles
{
    using System.Text;
    using System.Runtime.Serialization;

    /// <summary>
    /// A tag represents an immutable key-value pair.
    /// </summary>
    [DataContract]
	public class Tag
	{
		private const char KeyValueSeparator = '=';

        /// <summary>
        /// The key of this tag.
        /// </summary>
        [DataMember]
        public readonly string Key;

        /// <summary>
        /// The value of this tag.
        /// </summary>
        [DataMember]
        public readonly object Value;

		/// <param name="tag">
		///            the textual representation of the tag. </param>
		public Tag(string tag) : this(tag, tag.IndexOf(KeyValueSeparator))
		{
		}

		/// <param name="key">
		///            the key of the tag. </param>
		/// <param name="value">
		///            the value of the tag. </param>
		public Tag(string key, object value)
		{
			this.Key = key;
			this.Value = value;
		}

		private Tag(string tag, int splitPosition) : this(tag.Substring(0, splitPosition), tag.Substring(splitPosition + 1))
		{
		}

		public override bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}
			else if (!(obj is Tag))
			{
				return false;
			}
			Tag other = (Tag) obj;
			if (string.ReferenceEquals(this.Key, null))
			{
				if (!string.ReferenceEquals(other.Key, null))
				{
					return false;
				}
			}
			else if (!this.Key.Equals(other.Key))
			{
				return false;
			}
			else if (string.ReferenceEquals(this.Value, null))
			{
				if (!string.ReferenceEquals(other.Value, null))
				{
					return false;
				}
			}
			else if (!this.Value.Equals(other.Value))
			{
				return false;
			}
			return true;
		}

		public override int GetHashCode()
		{
			const int prime = 31;
			int result = 1;
			result = prime * result + ((string.ReferenceEquals(this.Key, null)) ? 0 : this.Key.GetHashCode());
			result = prime * result + ((string.ReferenceEquals(this.Value, null)) ? 0 : this.Value.GetHashCode());
			return result;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("key=");
			stringBuilder.Append(this.Key);
			stringBuilder.Append(", value=");
			stringBuilder.Append(this.Value.ToString());
			return stringBuilder.ToString();
		}
	}
}