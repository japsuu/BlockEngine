# BlockEngine

[![build](https://github.com/japsuu/BlockEngine/actions/workflows/build.yml/badge.svg)](https://github.com/japsuu/BlockEngine/actions/workflows/build.yml)
[![tests](https://github.com/japsuu/BlockEngine/actions/workflows/test.yml/badge.svg)](https://github.com/japsuu/BlockEngine/actions/workflows/test.yml)

![Latest progress screenshot](https://raw.githubusercontent.com/japsuu/BlockEngine/master/BlockEngine/Screenshots/latest.png)

BlockEngine is a (still unnamed) voxel[*](#voxel-engine-vs-block-engine) engine written in C#, that uses OpenGL as the rendering backend.

This project is still searching for its own identity :)

- [Project goals](#project-goals)
- [Personal goals](#personal-goals)
- [Project status](#project-status)
- [Voxel engine vs BlockEngine](#voxel-engine-vs-block-engine)
- [Credits](#credits)

## Project goals

The long-term goal of this project is to eventually develop into its own game, with player-hosted servers and official modding support.

## Personal goals

This is my first-ever custom game-engine. I haven't dabbled with OpenGL before, nor have I worked on a proper game project without an engine or a framework.

My main goal with this project is to get familiar wih OpenGL and the ins-and-outs of engine/tool development.

## Project status

A non-exhaustive list of currently implemented features. Updated every once in a while.

- [Rendering pipeline](#rendering-pipeline)
  - Internal face culling
  - Texturing
  - Ambient occlusion
  - ImGui integration
- [World Chunking](#world-chunking)
  - Cubic chunks
  - Chunk loading/unloading (world streaming)
  - Seamless chunk borders
  - [Palette-based block compression](#palette-compression)
- [Raycasting](#raycasting)
  - Ray-block intersections (with face detection)

### Rendering pipeline

The engine has a fully functional OpenGL voxel rendering pipeline.
A chunk is passed to a chunk mesher, which uses a meshing buffer to create vertices and indices, used to construct a polygonal mesh for the chunk.
The mesh is rendered by a shader which unpacks the bitpacked vertex data, positions the vertices based on the chunk position, assigns textures, applies shading, and renders the fragments.
TODO: More info.

### World chunking

As usual with voxel engines, the actual block data is stored inside so-called "chunks" of blocks. This engine uses **cubic chunks** of 32x32x32 blocks.

Dividing the game world into chunks offers some advantages.
- A chunk-based architecture enables efficient rendering and optimization, allowing the engine to perform chunk culling to selectively update and render only the visible portions of the world. This drastically improves performance by minimizing the computational load and memory requirements.
- Dynamic world modification and streaming are made possible (infinite worlds!), as individual chunks can be loaded or unloaded based on the player's proximity.
- Simple multithreading/parallel processing of chunks. All the currently loaded chunks can be processed in 8 batches, so that each thread in a batch always has a 3x3x3 chunk area to perform operations on. The processing order is [x,y,z] [x±1,y,z] [x,y±1,z] [x,y,z±1] [x±1,y±1,z] [x±1,y,z±1] [x,y±1,z±1] [x±1,y±1,z±1], which can also be visualized in 2D as: TODO

### Raycasting

Raycasting is currently implemented for ray/cube intersections, based on the infamous ["A Fast Voxel Traversal Algorithm for Ray Tracing"](http://www.cse.yorku.ca/~amana/research/grid.pdf) -paper by *John Amanatides & Andrew Woo*. A more in-depth overview can be found [here](https://github.com/cgyurgyik/fast-voxel-traversal-algorithm/blob/master/overview/FastVoxelTraversalOverview.md).

### Palette compression

With the release of Minecraft 1.13, Mojang improved their compression for block data. The method is conceptually similar to color palette compression: the **distinct** items (colors) are compressed into an array, and pixels use variable-length indices to reference these items in said array. This eliminates the need to store the same data multiple times.

At its simplest, it is a combination of a buffer holding variable-bit-length indices, and a palette (array) into which these indices point, whose entries hold the actual block type & state data, including a reference counter, to track how often the block type is being used.

The magic of this technique is, that the fewer types of blocks are in a certain area, the less memory that area needs, both at runtime and when serialized. It is important to remember that a world of discrete voxels (eg. Minecraft), generally consists of large amounts of air and stone, with a few blocks mixed in; these regions thus only need one or two bits for every block stored. They are balanced out by regions of high entropy, like forests of any kind, player-built structures, and overall the 'surface' of the world.

This has a few notable advantages over "normal" block data/state compression methods (example: [VL32](https://eisenwave.github.io/voxel-compression-docs/file_formats/vl32.html)):
- Block data/state is "decoupled" from actual chunk data. This means that we are (in most cases) no longer limited by the memory footprint of a single voxel - a chunk of 32x32x32 voxels will take up about the same amount of memory whether a single voxel takes up 2 bytes or 8 bytes of memory.
- This reduces the memory footprint of blocks: a single block ideally takes up one single bit in memory. The common case is three to four bits, depending on how many distinct elements (block types) are there in a chunk. As a bonus, this allows faster transmission of block groups (chunks) across network connections.
- Practically infinite block types & states:
As long as not every single block in a block group (chunk) is completely different from one another, the game can have as many types of blocks and their possible states as one wants (several tens of thousands).

Notes:
- Palette compression can usually be combined with other compression techniques (like [RLE](https://en.wikipedia.org/wiki/Run-length_encoding)) if required.
  - I'm not planning to implement run-length encoding in this project as I find it unnecessary, and it would have negative effects in high entropy (maximum block randomness) situations.
- Palette compression does not have to be used for just blocks: I'm planning on using it to compress chunk light data, as I will keep lighting data separate from actual block data.

## Input

- Movement: <kbd>W</kbd><kbd>A</kbd><kbd>S</kbd><kbd>D</kbd>
- Up: <kbd>Space</kbd>
- Down: <kbd>LShift</kbd>
- Release cursor: <kbd>Esc</kbd>
- Screenshot: <kbd>F2</kbd>

## Voxel engine vs block engine
Truth be told, this engine is not a "true" voxel engine.
This engine uses [voxel data representation](https://en.wikipedia.org/wiki/Voxel) but does not use voxel volume rendering techniques ([like volume marching](https://en.wikipedia.org/wiki/Volume_ray_casting)), but instead opts for traditional polygonal rendering.
When referring to voxels, I mean blocks ;)

## Credits

- xMrVizzy ([Faithful 32x](https://www.curseforge.com/minecraft/texture-packs/faithful-32x) texture pack, used as programmer-art)
