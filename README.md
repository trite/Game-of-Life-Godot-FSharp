# game_of_life_godot_f-sharp

Attempting to implement [Conway's Game of Life](https://en.wikipedia.org/wiki/Conway%27s_Game_of_Life) in Godot with F#.

Starting by using [this repository](https://github.com/andrew-wilkes/godot-game-of-life/tree/main) from [this tutorial](https://gdscript.com/projects/game-of-life/) on GDScript.com for inspiration.

Given that this changes from using GDScript to F# as well as from Godot 3 to 4 these will end up being fairly different.

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

