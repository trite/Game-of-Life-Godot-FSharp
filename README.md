# game_of_life_godot_f-sharp

Attempting to implement [Conway's Game of Life](https://en.wikipedia.org/wiki/Conway%27s_Game_of_Life) in Godot with F#.

Starting by using [this repository](https://github.com/andrew-wilkes/godot-game-of-life/tree/main) from [this tutorial](https://gdscript.com/projects/game-of-life/) on GDScript.com for inspiration.

Given that this changes from using GDScript to F# as well as from Godot 3 to 4 these will end up being fairly different.

# Continuous Spatial Automata

WIP - after wanting to try things with a larger kernel and continuous values instead of discrete, I learned that this is already a thing with its own sub-community within the greater cellular automata community. After learning more I'm ready to try my hand at figuring out good ways to apply it in a shader in Godot.

Kernel to start looks like this:
```
[
  [0, 0, w, w, w, 0, 0],
  [0, w, w, w, w, w, 0],
  [w, w, w, w, w, w, w],
  [w, w, w, 0, w, w, w],
  [w, w, w, w, w, w, w],
  [0, w, w, w, w, w, 0],
  [0, 0, w, w, w, 0, 0]
]
```

Where w is a uniform weight.

Use own cell after first pass of calculation only.

Planned steps as of now:
0) convert nearby neighbors from RGB to HSL
1) multiplying nearby neighboring cells lightness values by their weights from the kernel
2) calculate the sum of the weights of the kernel
3) divide the sum of the weighted neighboring cells by the sum of the weights of the kernel
4) apply smoothstep to the result of the division
  REMINDER: Need to be able to easily change the smoothstep inputs
5) that gives a new lightness value to use
6) get current hue (changes over time)
7) get saturation (const to start)
8) HSL to RGB conversion
9) set new RGB value to cell


# Ideas for things to do

## Drag-to-select and friends

Wonder what it would take to get some drag-to-select functionality going.

Could then try adding:
* Move selection
* Copy/paste selection

Would need to make it so that when dragging the shape would try to stick/snap to the grid while moving around.

## Save/load

Being able to save/load layouts would be nice for playing around with things.

## Testing
Once the ability to save/load exists it shouldn't be too bad to make a few example setups of varying complexities. Could then automate testing of how long it takes to execute X frames and use that for benchmarking.

