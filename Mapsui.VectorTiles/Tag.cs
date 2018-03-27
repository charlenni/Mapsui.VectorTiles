using Newtonsoft.Json.Linq;

namespace Mapsui.VectorTiles
{
    /// <summary>
    /// A tag represents a key-value pair.
    /// </summary>
	public class Tag
	{
		private const char KeyValueSeparator = '=';

        /// <summary>
        /// The key of this tag.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The value of this tag.
        /// </summary>
        public JValue Value { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public Tag()
        { }

        /// <summary>
        /// Constructor for string representation of a key-value pair like "key=value"
        /// </summary>
		/// <param name="tag">
		/// Textual representation of the tag. 
        /// </param>
		public Tag(string tag) : this(tag, tag.IndexOf(KeyValueSeparator))
		{
		}

	    /// <summary>
	    /// Constructor for a key-value-pair
	    /// </summary>
	    /// <param name="key">
	    /// Key of the tag
	    /// </param>
	    /// <param name="value">
	    /// Value of the tag
	    /// </param>
	    public Tag(string key, JValue value)
	    {
	        Key = key;
	        Value = value;
	    }

        /// <summary>
        /// Constructor for a key-value-pair
        /// </summary>
		/// <param name="key">
		/// Key of the tag
        /// </param>
		/// <param name="value">
		/// Value of the tag as string
        /// </param>
		public Tag(string key, string value)
		{
			Key = key;
			Value = new JValue(value);
		}

        /// <summary>
        /// Constructor for string representation of a key-value pair like "key=value" and given position of "="
        /// </summary>
		/// <param name="tag">
		/// String with key-value-pair
        /// </param>
		/// <param name="splitPosition">
		/// Position of "="
        /// </param>
		private Tag(string tag, int splitPosition) : this(tag.Substring(0, splitPosition), tag.Substring(splitPosition + 1))
		{
		}

		public override bool Equals(object o)
		{
			if (this == o)
			{
				return true;
			}

			if (!(o is Tag other))
				return false;

            if (!Key.Equals(other.Key))
				return false;

		    return Value.Equals(other.Value);
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
            return $"{Key}={Value}";
		}
	}
}