/*
 * Copyright 2010, 2011, 2012, 2013 mapsforge.org
 * Copyright 2014-2015 Ludwig M Brinckmann
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

namespace Mapsui.VectorTiles.Mapsforge.Datastore
{
    using Geometries;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// An immutable container for all data associated with a single point of interest node (POI).
    /// </summary>
    public class PointOfInterest
	{
		/// <summary>
		/// The layer of this POI + 5 (to avoid negative values).
		/// </summary>
		public readonly sbyte Layer;

		/// <summary>
		/// The position of this POI.
		/// </summary>
		public readonly Point Position;

		/// <summary>
		/// The tags of this POI.
		/// </summary>
		public readonly IList<Tag> Tags;

		public PointOfInterest(sbyte layer, IList<Tag> tags, Point position)
		{
			this.Layer = layer;
			this.Tags = tags;
			this.Position = position;
		}

		public override bool Equals(object obj)
		{
			if (this == obj)
			{
				return true;
			}
			else if (!(obj is PointOfInterest))
			{
				return false;
			}
			PointOfInterest other = (PointOfInterest) obj;
			if (this.Layer != other.Layer)
			{
				return false;
			}
			else if (!this.Tags.SequenceEqual(other.Tags))
			{
				return false;
			}
			else if (!this.Position.Equals(other.Position))
			{
				return false;
			}
			return true;
		}

		public override int GetHashCode()
		{
			const int prime = 31;
			int result = 1;
			result = prime * result + Layer;
			result = prime * result + Tags.GetHashCode();
			result = prime * result + Position.GetHashCode();
			return result;
		}
	}
}