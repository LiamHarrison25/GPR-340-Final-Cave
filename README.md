# GPR-340-Final-Cave

## The Project
This was a group project for GPR-340 where we wrote a program that would generate 3D caves using Perlin noise and grid quantization. 


## Tips on how to use:
1. Chunk render distance should be at least one less than world radius
2. HalfChunk size should be divisible by halfCellSize
3. Press G to reload the chunks and cells. Allows you to change the noise values during runtime.
4. Press F to toggle the Gizmo grid.
5. WASD and mouse to move.
6. Space to jump. (can infinite jump)

## Default value guide:
world chunk radius: 20
<br />
Chunk Render Distance Radius: 10
<br />
Half chunk size: 1
<br />
Half cell size: 1
<br />
Noise Scale: 0.07
<br />
Threshold: 0.45
<br />

## Suggestions when exploring the project:
Play around with the noise scale and threshold, and press g to see changes while in the game. 
<br />


## Known Issues 
1. Player Camere will move slightly towards the right occasionally.
2. Chunks with more than one cell will appear incorrectly with the noise, but will still work
3. Can be very slow and lag when scaling up the values
