using System;
using System.Collections.Generic;

namespace MazeGameVisual
{
    // Class representing a maze cell
    class MazeCell
    {
        // All walls are present by default
        public bool NorthWall = true;
        public bool SouthWall = true;
        public bool EastWall = true;
        public bool WestWall = true;
        public bool Visited = false;
    }

    // Maze class generated using Depth-First Search (DFS)
    class Maze
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public MazeCell[,] Cells;
        private Random rand = new Random();

        public Maze(int width, int height)
        {
            Width = width;
            Height = height;
            Cells = new MazeCell[height, width];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    Cells[y, x] = new MazeCell();

            // Generate the maze starting at (0,0)
            GenerateMaze(0, 0);
        }

        // Generate maze using DFS
        private void GenerateMaze(int startX, int startY)
        {
            Stack<(int x, int y)> stack = new Stack<(int, int)>();
            Cells[startY, startX].Visited = true;
            stack.Push((startX, startY));

            while (stack.Count > 0)
            {
                var current = stack.Peek();
                int cx = current.x;
                int cy = current.y;
                var neighbors = new List<(int nx, int ny, char direction)>();

                // North (up)
                if (cy > 0 && !Cells[cy - 1, cx].Visited)
                    neighbors.Add((cx, cy - 1, 'N'));
                // South (down)
                if (cy < Height - 1 && !Cells[cy + 1, cx].Visited)
                    neighbors.Add((cx, cy + 1, 'S'));
                // West (left)
                if (cx > 0 && !Cells[cy, cx - 1].Visited)
                    neighbors.Add((cx - 1, cy, 'W'));
                // East (right)
                if (cx < Width - 1 && !Cells[cy, cx + 1].Visited)
                    neighbors.Add((cx + 1, cy, 'E'));

                if (neighbors.Count > 0)
                {
                    var next = neighbors[rand.Next(neighbors.Count)];
                    // Remove the wall between current cell and neighbor
                    if (next.direction == 'N')
                    {
                        Cells[cy, cx].NorthWall = false;
                        Cells[next.ny, next.nx].SouthWall = false;
                    }
                    else if (next.direction == 'S')
                    {
                        Cells[cy, cx].SouthWall = false;
                        Cells[next.ny, next.nx].NorthWall = false;
                    }
                    else if (next.direction == 'W')
                    {
                        Cells[cy, cx].WestWall = false;
                        Cells[next.ny, next.nx].EastWall = false;
                    }
                    else if (next.direction == 'E')
                    {
                        Cells[cy, cx].EastWall = false;
                        Cells[next.ny, next.nx].WestWall = false;
                    }
                    Cells[next.ny, next.nx].Visited = true;
                    stack.Push((next.nx, next.ny));
                }
                else
                {
                    stack.Pop();
                }
            }
        }
    }

    class Program
    {
        // Maze dimensions (in cells)
        static int mazeWidth = 10;
        static int mazeHeight = 10;
        // View radius (Manhattan distance)
        static int viewRadius = 3;

        static Maze maze;
        // Discovered (visited or within view) cells
        static bool[,] discovered;
        // Player coordinates (cell indices)
        static int playerX = 0, playerY = 0;

        static void Main(string[] args)
        {
            // Ensure correct display of Unicode characters
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.CursorVisible = false;

            maze = new Maze(mazeWidth, mazeHeight);
            discovered = new bool[mazeHeight, mazeWidth];
            UpdateDiscovered();

            while (true)
            {
                DrawMaze();

                // Exit on Escape key
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.Escape)
                    break;

                int newX = playerX, newY = playerY;

                // Use ConsoleKey to handle WASD input
                switch (keyInfo.Key)
                {
                    case ConsoleKey.W:
                        if (!maze.Cells[playerY, playerX].NorthWall)
                            newY = playerY - 1;
                        break;
                    case ConsoleKey.S:
                        if (!maze.Cells[playerY, playerX].SouthWall)
                            newY = playerY + 1;
                        break;
                    case ConsoleKey.A:
                        if (!maze.Cells[playerY, playerX].WestWall)
                            newX = playerX - 1;
                        break;
                    case ConsoleKey.D:
                        if (!maze.Cells[playerY, playerX].EastWall)
                            newX = playerX + 1;
                        break;
                }

                // Check boundaries
                if (newX >= 0 && newX < mazeWidth && newY >= 0 && newY < mazeHeight)
                {
                    playerX = newX;
                    playerY = newY;
                }

                UpdateDiscovered();
            }
            Console.CursorVisible = true;
        }

        // Mark cells as discovered if they are within viewRadius of the player
        static void UpdateDiscovered()
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                for (int x = 0; x < mazeWidth; x++)
                {
                    if (Math.Abs(x - playerX) + Math.Abs(y - playerY) <= viewRadius)
                        discovered[y, x] = true;
                }
            }
        }

        // Draw the maze using the specified Unicode characters:
        // - Horizontal walls: "═══" (for the top/bottom borders)
        // - Vertical walls: "║"
        // For the upper left cell, if it has both a north and a west wall, we replace the left part with "╔".
        static void DrawMaze()
        {
            Console.Clear();

            // Draw the top border (first row of cells)
            string topBorder = "";
            for (int x = 0; x < mazeWidth; x++)
            {
                if (discovered[0, x])
                {
                    // If the cell has a north wall, draw the horizontal segment; otherwise, draw spaces.
                    string segment = maze.Cells[0, x].NorthWall ? "═══" : "   ";
                    // For the very first cell, if it has a north and west wall, replace the left symbol with "╔"
                    if (x == 0 && maze.Cells[0, 0].NorthWall && maze.Cells[0, 0].WestWall)
                    {
                        segment = "╔" + segment.Substring(1);
                    }
                    topBorder += segment;
                }
                else
                {
                    topBorder += "   ";
                }
                topBorder += " "; // cell separator
            }
            Console.WriteLine(topBorder);

            // Draw the maze rows
            for (int y = 0; y < mazeHeight; y++)
            {
                // Build the cell content line (with vertical walls)
                string cellLine = "";
                for (int x = 0; x < mazeWidth; x++)
                {
                    if (discovered[y, x])
                    {
                        // For the first cell in the row, draw its west wall (if any)
                        if (x == 0)
                            cellLine += maze.Cells[y, x].WestWall ? "║" : " ";

                        // The cell's content: show " P " if the player is here, otherwise blank
                        cellLine += (x == playerX && y == playerY) ? " P " : "   ";

                        // Draw the cell's east wall
                        cellLine += maze.Cells[y, x].EastWall ? "║" : " ";
                    }
                    else
                    {
                        // Undiscovered cells are drawn as spaces
                        cellLine += "    ";
                    }
                }
                Console.WriteLine(cellLine);

                // Build the bottom border of the cells
                string bottomLine = "";
                for (int x = 0; x < mazeWidth; x++)
                {
                    if (discovered[y, x])
                        bottomLine += maze.Cells[y, x].SouthWall ? "═══" : "   ";
                    else
                        bottomLine += "   ";
                    bottomLine += " ";
                }
                Console.WriteLine(bottomLine);
            }
        }
    }
}