using System;
using System.Collections.Generic;

namespace MultiLevelMazeGame
{
    // Class representing a maze cell
    class MazeCell
    {
        // Walls (all present by default)
        public bool NorthWall = true;
        public bool SouthWall = true;
        public bool EastWall = true;
        public bool WestWall = true;
        public bool Visited = false;

        // Staircases for floor transitions
        public bool IsStairsUp = false;
        public bool IsStairsDown = false;

        // Secret doors (hidden passages) on each side
        public bool HasSecretDoorNorth = false;
        public bool HasSecretDoorSouth = false;
        public bool HasSecretDoorEast = false;
        public bool HasSecretDoorWest = false;

        // Whether the secret doors have been revealed (if revealed, consider the door open)
        public bool SecretDoorRevealedNorth = false;
        public bool SecretDoorRevealedSouth = false;
        public bool SecretDoorRevealedEast = false;
        public bool SecretDoorRevealedWest = false;

        // Helper methods to check if a side is "open" considering secret doors
        public bool IsOpenNorth() => !NorthWall || (HasSecretDoorNorth && SecretDoorRevealedNorth);
        public bool IsOpenSouth() => !SouthWall || (HasSecretDoorSouth && SecretDoorRevealedSouth);
        public bool IsOpenEast() => !EastWall || (HasSecretDoorEast && SecretDoorRevealedEast);
        public bool IsOpenWest() => !WestWall || (HasSecretDoorWest && SecretDoorRevealedWest);
    }

    // Maze class (a floor) generated using depth-first search (DFS)
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
            Cells = new MazeCell[Height, Width];
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    Cells[y, x] = new MazeCell();

            GenerateMaze(0, 0);
            GenerateSecretDoors(0.1); // 10% probability for a hidden passage
        }

        // Generate the maze using DFS
        private void GenerateMaze(int startX, int startY)
        {
            Stack<(int x, int y)> stack = new Stack<(int, int)>();
            Cells[startY, startX].Visited = true;
            stack.Push((startX, startY));

            while (stack.Count > 0)
            {
                var current = stack.Peek();
                int cx = current.x, cy = current.y;
                var neighbors = new List<(int nx, int ny, char direction)>();

                // North
                if (cy > 0 && !Cells[cy - 1, cx].Visited)
                    neighbors.Add((cx, cy - 1, 'N'));
                // South
                if (cy < Height - 1 && !Cells[cy + 1, cx].Visited)
                    neighbors.Add((cx, cy + 1, 'S'));
                // West
                if (cx > 0 && !Cells[cy, cx - 1].Visited)
                    neighbors.Add((cx - 1, cy, 'W'));
                // East
                if (cx < Width - 1 && !Cells[cy, cx + 1].Visited)
                    neighbors.Add((cx + 1, cy, 'E'));

                if (neighbors.Count > 0)
                {
                    var next = neighbors[rand.Next(neighbors.Count)];
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

        // Generate secret doors: for each cell, check if a wall exists and mark it as secret with a given probability
        private void GenerateSecretDoors(double probability)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    // North wall secret door
                    if (y > 0 && Cells[y, x].NorthWall && Cells[y - 1, x].SouthWall)
                    {
                        if (rand.NextDouble() < probability)
                        {
                            Cells[y, x].HasSecretDoorNorth = true;
                            Cells[y - 1, x].HasSecretDoorSouth = true;
                        }
                    }
                    // West wall secret door
                    if (x > 0 && Cells[y, x].WestWall && Cells[y, x - 1].EastWall)
                    {
                        if (rand.NextDouble() < probability)
                        {
                            Cells[y, x].HasSecretDoorWest = true;
                            Cells[y, x - 1].HasSecretDoorEast = true;
                        }
                    }
                    // (Additional sides can be handled via neighboring cells)
                }
            }
        }
    }

    // Dungeon class combines multiple floors (Maze instances) and sets up staircases between them
    class Dungeon
    {
        public List<Maze> Floors { get; private set; }
        public int CurrentFloor { get; set; }
        public int NumFloors { get { return Floors.Count; } }

        public Dungeon(int numFloors, int width, int height)
        {
            Floors = new List<Maze>();
            for (int i = 0; i < numFloors; i++)
                Floors.Add(new Maze(width, height));

            // Set up staircases between floors:
            // On each floor (except the last), a random cell is designated as a staircase down,
            // and on the next floor the corresponding cell is marked as a staircase up.
            Random rand = new Random();
            for (int i = 0; i < numFloors - 1; i++)
            {
                int sx = rand.Next(width);
                int sy = rand.Next(height);
                Floors[i].Cells[sy, sx].IsStairsDown = true;
                Floors[i + 1].Cells[sy, sx].IsStairsUp = true;
            }
            CurrentFloor = 0;
        }
    }

    class Program
    {
        // Maze parameters
        static int mazeWidth = 10;
        static int mazeHeight = 10;
        static int numFloors = 3;
        static int viewRadius = 3;

        static Dungeon dungeon;
        // Player position (assumed to use the same coordinates on all floors)
        static int playerX = 0, playerY = 0;
        // For each floor, store a discovered (visible) matrix for fog-of-war
        static Dictionary<int, bool[,]> discoveredFloors = new Dictionary<int, bool[,]>();

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.CursorVisible = false;

            dungeon = new Dungeon(numFloors, mazeWidth, mazeHeight);
            for (int i = 0; i < numFloors; i++)
                discoveredFloors[i] = new bool[mazeHeight, mazeWidth];

            UpdateDiscovered();

            while (true)
            {
                DrawDungeon();

                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.Escape)
                    break;

                int newX = playerX, newY = playerY;
                Maze currentMaze = dungeon.Floors[dungeon.CurrentFloor];
                bool turnUsed = false;

                // Process input:
                // WASD - movement, E - search for secret doors, T - use stairs
                switch (keyInfo.Key)
                {
                    case ConsoleKey.W:
                        if (currentMaze.Cells[playerY, playerX].IsOpenNorth())
                            newY = playerY - 1;
                        turnUsed = true;
                        break;
                    case ConsoleKey.S:
                        if (currentMaze.Cells[playerY, playerX].IsOpenSouth())
                            newY = playerY + 1;
                        turnUsed = true;
                        break;
                    case ConsoleKey.A:
                        if (currentMaze.Cells[playerY, playerX].IsOpenWest())
                            newX = playerX - 1;
                        turnUsed = true;
                        break;
                    case ConsoleKey.D:
                        if (currentMaze.Cells[playerY, playerX].IsOpenEast())
                            newX = playerX + 1;
                        turnUsed = true;
                        break;
                    case ConsoleKey.E: // search for secret doors
                        SearchForSecretDoors();
                        turnUsed = true;
                        break;
                    case ConsoleKey.T: // use stairs to change floors
                        UseStairs();
                        turnUsed = true;
                        break;
                }

                // Update player's position (with boundary checking)
                if (newX >= 0 && newX < mazeWidth && newY >= 0 && newY < mazeHeight)
                {
                    playerX = newX;
                    playerY = newY;
                }
                if (turnUsed)
                    UpdateDiscovered();
            }
            Console.CursorVisible = true;
        }

        // Update the discovered matrix for the current floor based on the view radius
        static void UpdateDiscovered()
        {
            int floor = dungeon.CurrentFloor;
            bool[,] discovered = discoveredFloors[floor];
            for (int y = 0; y < mazeHeight; y++)
                for (int x = 0; x < mazeWidth; x++)
                    if (Math.Abs(x - playerX) + Math.Abs(y - playerY) <= viewRadius)
                        discovered[y, x] = true;
        }

        // Draw the current floor of the dungeon
        static void DrawDungeon()
        {
            Console.Clear();
            Maze currentMaze = dungeon.Floors[dungeon.CurrentFloor];
            bool[,] discovered = discoveredFloors[dungeon.CurrentFloor];

            // Display the floor number
            Console.WriteLine($"Floor: {dungeon.CurrentFloor + 1} / {dungeon.NumFloors}");

            // Draw the top border (first row of cells)
            string topBorder = "";
            for (int x = 0; x < mazeWidth; x++)
            {
                if (discovered[0, x])
                {
                    string segment = currentMaze.Cells[0, x].NorthWall ? "═══" : "   ";
                    if (x == 0 && currentMaze.Cells[0, 0].NorthWall && currentMaze.Cells[0, 0].WestWall)
                        segment = "╔" + segment.Substring(1);
                    topBorder += segment;
                }
                else
                    topBorder += "   ";
                topBorder += " "; // cell separator
            }
            Console.WriteLine(topBorder);

            // Draw each row of cells
            for (int y = 0; y < mazeHeight; y++)
            {
                string cellLine = "";
                for (int x = 0; x < mazeWidth; x++)
                {
                    if (discovered[y, x])
                    {
                        // For the first cell in a row, draw the left wall (or a space)
                        if (x == 0)
                            cellLine += currentMaze.Cells[y, x].WestWall ? "║" : " ";

                        // Cell content: show " P " if the player is here;
                        // otherwise, if the cell contains stairs, show "↑" (up) or "↓" (down);
                        // if both, show "⇕"; otherwise, show an empty cell.
                        if (x == playerX && y == playerY)
                        {
                            cellLine += " P ";
                        }
                        else
                        {
                            if (currentMaze.Cells[y, x].IsStairsDown && !currentMaze.Cells[y, x].IsStairsUp)
                                cellLine += " ↓ ";
                            else if (currentMaze.Cells[y, x].IsStairsUp && !currentMaze.Cells[y, x].IsStairsDown)
                                cellLine += " ↑ ";
                            else if (currentMaze.Cells[y, x].IsStairsUp && currentMaze.Cells[y, x].IsStairsDown)
                                cellLine += " ⇕ ";
                            else
                                cellLine += "   ";
                        }

                        // Draw the right wall of the cell
                        cellLine += currentMaze.Cells[y, x].EastWall ? "║" : " ";
                    }
                    else
                    {
                        cellLine += "    ";
                    }
                }
                Console.WriteLine(cellLine);

                // Draw the bottom border for the row
                string bottomLine = "";
                for (int x = 0; x < mazeWidth; x++)
                {
                    if (discovered[y, x])
                        bottomLine += currentMaze.Cells[y, x].SouthWall ? "═══" : "   ";
                    else
                        bottomLine += "   ";
                    bottomLine += " ";
                }
                Console.WriteLine(bottomLine);
            }
        }

        // Search for secret doors around the current cell and reveal them
        static void SearchForSecretDoors()
        {
            Maze currentMaze = dungeon.Floors[dungeon.CurrentFloor];
            int x = playerX, y = playerY;
            bool found = false;

            // Check North
            if (y > 0)
            {
                MazeCell current = currentMaze.Cells[y, x];
                MazeCell neighbor = currentMaze.Cells[y - 1, x];
                if (current.HasSecretDoorNorth && !current.SecretDoorRevealedNorth)
                {
                    current.SecretDoorRevealedNorth = true;
                    neighbor.SecretDoorRevealedSouth = true;
                    current.NorthWall = false;
                    neighbor.SouthWall = false;
                    found = true;
                }
            }
            // Check South
            if (y < mazeHeight - 1)
            {
                MazeCell current = currentMaze.Cells[y, x];
                MazeCell neighbor = currentMaze.Cells[y + 1, x];
                if (current.HasSecretDoorSouth && !current.SecretDoorRevealedSouth)
                {
                    current.SecretDoorRevealedSouth = true;
                    neighbor.SecretDoorRevealedNorth = true;
                    current.SouthWall = false;
                    neighbor.NorthWall = false;
                    found = true;
                }
            }
            // Check West
            if (x > 0)
            {
                MazeCell current = currentMaze.Cells[y, x];
                MazeCell neighbor = currentMaze.Cells[y, x - 1];
                if (current.HasSecretDoorWest && !current.SecretDoorRevealedWest)
                {
                    current.SecretDoorRevealedWest = true;
                    neighbor.SecretDoorRevealedEast = true;
                    current.WestWall = false;
                    neighbor.EastWall = false;
                    found = true;
                }
            }
            // Check East
            if (x < mazeWidth - 1)
            {
                MazeCell current = currentMaze.Cells[y, x];
                MazeCell neighbor = currentMaze.Cells[y, x + 1];
                if (current.HasSecretDoorEast && !current.SecretDoorRevealedEast)
                {
                    current.SecretDoorRevealedEast = true;
                    neighbor.SecretDoorRevealedWest = true;
                    current.EastWall = false;
                    neighbor.WestWall = false;
                    found = true;
                }
            }
            if (found)
                Console.Beep(); // play a beep sound when a secret passage is discovered
        }

        // Use the stairs if the current cell has one to change floors
        static void UseStairs()
        {
            Maze currentMaze = dungeon.Floors[dungeon.CurrentFloor];
            MazeCell cell = currentMaze.Cells[playerY, playerX];

            if (cell.IsStairsDown && dungeon.CurrentFloor < dungeon.NumFloors - 1)
            {
                dungeon.CurrentFloor++;
                // When transitioning, we maintain the same coordinates (since the stairs are set in the same cell)
            }
            else if (cell.IsStairsUp && dungeon.CurrentFloor > 0)
            {
                dungeon.CurrentFloor--;
            }
        }
    }
}