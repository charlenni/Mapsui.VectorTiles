/*
 * Copyright 2010, 2011, 2012 mapsforge.org
 * Copyright 2013 Hannes Janetzek
 * Copyright 2016-2017 devemux86
 * Copyright 2017 Andrey Novikov
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


	/// <summary>
	/// Interface for a render theme which is defined in XML.
	/// </summary>
	public interface ThemeFile
	{
		/// <returns> the interface callback to create a settings menu on the fly. </returns>
		XmlRenderThemeMenuCallback MenuCallback {get;set;}

		/// <returns> the prefix for all relative resource paths. </returns>
		string RelativePathPrefix {get;}

		/// <returns> an InputStream to read the render theme data from. </returns>
		/// <exception cref="ThemeException"> if an error occurs while reading the render theme XML. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: java.io.InputStream getRenderThemeAsStream() throws org.oscim.theme.IRenderTheme_ThemeException;
		System.IO.Stream RenderThemeAsStream {get;}

		/// <summary>
		/// Tells ThemeLoader if theme file is in Mapsforge format
		/// </summary>
		/// <returns> true if theme file is in Mapsforge format </returns>
		bool MapsforgeTheme {get;}

	}

}