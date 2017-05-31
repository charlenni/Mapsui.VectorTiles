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
    using System.Collections.Generic;

    /// <summary>
    /// An immutable container for the data returned from a MapDataStore.
    /// </summary>
    public class MapReadResult
	{
		/// <summary>
		/// True if the read area is completely covered by water, false otherwise.
		/// </summary>
		public bool IsWater;

		/// <summary>
		/// The read POIs.
		/// </summary>
		public IList<PointOfInterest> PointOfInterests;

		/// <summary>
		/// The read ways.
		/// </summary>
		public IList<Way> Ways;

		public MapReadResult()
		{
			this.PointOfInterests = new List<PointOfInterest>();
			this.Ways = new List<Way>();
		}

		public virtual void Add(PoiWayBundle poiWayBundle)
		{
			((List<PointOfInterest>)this.PointOfInterests).AddRange(poiWayBundle.Pois);
			((List<Way>)this.Ways).AddRange(poiWayBundle.Ways);
		}
	}
}