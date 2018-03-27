/*
 * Copyright 2010, 2011, 2012 mapsforge.org
 * Copyright 2013 Hannes Janetzek
 * Copyright 2017 devemux86
 *
 * This file is part of the OpenScienceMap project (http://www.opensciencemap.org).
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
namespace org.oscim.theme
{

	using GeometryType = org.oscim.core.GeometryBuffer.GeometryType;
	using TagSet = org.oscim.core.TagSet;
	using RenderStyle = org.oscim.theme.styles.RenderStyle;

	public interface IRenderTheme
	{

		/// <summary>
		/// Matches a MapElement with the given parameters against this RenderTheme.
		/// </summary>
		/// <param name="zoomLevel"> the zoom level at which the way should be matched. </param>
		/// <returns> matching render instructions </returns>
		RenderStyle[] matchElement(GeometryType type, TagSet tags, int zoomLevel);

		/// <summary>
		/// Must be called when this RenderTheme gets destroyed to clean up and free
		/// resources.
		/// </summary>
		void dispose();

		/// <returns> the number of distinct drawing levels required by this
		/// RenderTheme. </returns>
		int Levels {get;}

		/// <returns> the map background color of this RenderTheme. </returns>
		int MapBackground {get;}

		/// <summary>
		/// Is Mapsforge or VTM theme.
		/// </summary>
		bool MapsforgeTheme {get;}

		void updateStyles();

		/// <summary>
		/// Scales the text size of this RenderTheme by the given factor.
		/// </summary>
		/// <param name="scaleFactor"> the factor by which the text size should be scaled. </param>
		void scaleTextSize(float scaleFactor);
	}

	public class IRenderTheme_ThemeException : System.ArgumentException
	{
		public IRenderTheme_ThemeException(string @string) : base(@string)
		{
		}

		internal const long serialVersionUID = 1L;
	}

}