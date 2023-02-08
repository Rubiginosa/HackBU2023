# HackBU2023
24 hour hackathon, with friends!

---

An ASCII-stl renderer that runs in the terminal.

## Useage:
`TerminalRenderer <filename> <scale> [-r]`

Scale scales the model appropriately, which is useful for certain files. `-r` rotates the model in place.

Use WASD to move, arrow keys to look, Q/E to roll, R/F for height, and O/P to change FOV.


Currently quite slow. For better performance use a small window size.

---

## Build:
Everything is contained in Program.cs. Compile and run with `dotnet` (works better when compiled to an executable first, then run).

## Samples:
Some models are available here https://people.sc.fsu.edu/~jburkardt/data/stla/stla.html
One can also convert the more common binary stl format to ASCII for use with this program.
