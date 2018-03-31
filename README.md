# Mapsui.VectorTiles
Library for handling vector tiles in Mapsui

## Introduction
Mapsui.VectorTile is a add on to [Mapsui](https://github.com/pauldendulk/Mapsui). 
It has no platform dependencies and come up to now as a PCL with Profile111.

Vector tile formats use no ready rendered bitmaps, but vectors (points, lines and polygons) with tags,
which describe, what features they have. This vectors are than rendered on-the-fly with settings 
from a style. In this style is described, which type of vector with special given tags should rendered 
in which color and which line width.

It is work in progress and far from ready.

## Supported vector formats
Mapsui.VectorTiles support the following formats.

### Mapbox Vector Tile Format
Mapbox Vector Tile format. Because it uses [BruTile](https://github.com/BruTile/BruTile) 
to fetch the data, it could handle online vector tiles, which come normally with the ending 
"MVT", and offline vetor tiles, which come normally as MBTiles. For vector tile format descriptions
see the [Mapbox Vector Tile Specification](https://www.mapbox.com/vector-tiles/specification/) and 
for MBTiles file format see [MBTiles Specification](https://github.com/mapbox/mbtiles-spec).

## Supported styling formats
A style is a document that defines the visual appearance of a map: what data to draw, the order to 
draw it in, and how to style the data when drawing it. Normally each vector tile format has its own 
styling format, because each format brings its own tag names. So it is difficult to use one styling 
format for all vector tile formats.

### Mapbox GL Style
The [Mapbox GL Style](https://www.mapbox.com/mapbox-gl-js/style-spec) is well defined and has many 
different settings. If you use vector tiles from Mapbox, you could use styles, which are created with
[Mapbox Studio](https://www.mapbox.com/mapbox-studio/). An alternative is [Maputnik](https://github.com/maputnik/editor). 
If you use this, you could get vector tiles from [OpenMapTiles](https://openmaptiles.org/). For
a ready made style see [Osm-Liberty](https://github.com/lukasmartinelli/osm-liberty).

A Mapbox GL Style file comes as Json file.

## Projects used by Mapsui.VectorTiles
Mapsui.VectorTiles use many other open source projects, without this wouldn't be possible. Thanks for 
all the work of this authors.
* [Mapsui](https://github.com/pauldendulk/Mapsui)
* [BruTile](https://github.com/BruTile/BruTile)
* [Mapbox Vector Tile CS](https://github.com/bertt/mapbox-vector-tile-cs)
* [Osm-Liberty](https://github.com/lukasmartinelli/osm-liberty)

There are many others, that inspired the project with their work on StackOverflow, GitHub and other
great websites out there in the internet. Thank you!
