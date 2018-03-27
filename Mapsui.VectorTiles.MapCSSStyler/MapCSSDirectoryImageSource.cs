// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2013 Abelshausen Ben
// 
// This file is part of OsmSharp.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

namespace Mapsui.VectorTiles.MapCSSStyler
{
    /// <summary>
    /// An image source that gets it's image from a file system directory.
    /// </summary>
    public class MapCSSDirectoryImageSource : IMapCSSImageSource
    {
        /// <summary>
        /// Holds the directory info.
        /// </summary>
        private readonly string _path;

        /// <summary>
        /// Creates a new MapCSS image source.
        /// </summary>
        public MapCSSDirectoryImageSource(string path)
        {
            _path = path;
        }

        /// <summary>
        /// Returns true if the image with the given name exists and sets the output parameter with the image data.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="imageData"></param>
        /// <returns></returns>
        public bool TryGet(string name, out byte[] imageData)
        {
            imageData = null;
            return false;
        }
    }
}