# Snake#

**Snake#** is a basic TUI implementation of the game *Snake*, written in C# for
.net Core 10.

I wrote this mostly on a Saturday just for fun to try out some C#. As this was
my first time writing any C#, the code is probably lacking some idiomaticity and
could be improved in many places. Feedback is of course welcome. I may or may
not come back to tweak this further in the future.

## Gameplay

Use the arrow keys or WASD to move the Snake. If you collide with yourself or
the boundary the snake dies and the game ends. Eat the coloured fruits that 
appear at random positions to grow your snake and gain points. As the snake
grows it will also move faster on its own. You can hasten the snake by just
pressing a button in a direction you're going repeatedly (or holding it).
Scores are incremental in that for each fruit you eat, the score grows by the
current size of the snake itself.

To pause press P and to resume press Enter. Press Escape to quit the game.

## Building etc.

Run with `dotnet run` or build a debug build with `dotnet build`. Build a
deployment build with `dotnet publish SnakeSharp.sln`.
