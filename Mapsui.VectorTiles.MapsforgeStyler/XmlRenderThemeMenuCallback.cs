﻿using System.Collections.Generic;

/*
 * Copyright 2014 Ludwig M Brinckmann
 * Copyright 2016 devemux86
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
	/// Interface callback in render theme to create a settings menu on the fly.
	/// </summary>
	public interface XmlRenderThemeMenuCallback
	{
		/// <summary>
		/// Called when the style menu section of the xml file has been parsed.
		/// </summary>
		ISet<string> getCategories(XmlRenderThemeStyleMenu style);
	}

}