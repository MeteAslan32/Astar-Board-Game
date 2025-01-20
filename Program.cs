using System;
using System.Collections.Generic;
using System.Linq;

/*
------------------------------Requirements------------------------------

1. The initial and goal states will be given by user

2. The tiles can be moved up, down, right, or left.

3. The game will begin by the move of Tile #1 (if required) and go on with the
moves of other tiles in order.

4. Distance(cost) between two neighboring states will be measured based on the move costs as given below
right or left move cost =2, up or down move cost =1

5. The A* search will be implemented with Manhattan distance as heuristics.

6. The expansion will go on till 10th expanded node. The program will print out each expanded state and
compare it with given goal state.
 
 */
class Node : IComparable<Node>
{
    public int[][] State { get; set; }
    public Node Parent { get; set; }
    public string Move { get; set; }
    public int Tile { get; set; }
    public int Cost { get; set; }
    public int Heuristic { get; set; }
    public int TotalCost => Cost + Heuristic;
    public string Path { get; set; }

    public Node(int[][] state, Node parent = null, string move = null, int tile = 0, int cost = 0, int heuristic = 0, string path = "")
    {
        State = state;
        Parent = parent;
        Move = move;
        Tile = tile;
        Cost = cost;
        Heuristic = heuristic;
        Path = path;
    }

    public int CompareTo(Node other)
    {
        return TotalCost.CompareTo(other.TotalCost);
    }
}

class Program
{
    //Req 5: A* algorithms manhattan distance comes from this function
    static int ManhattanDistance(int[][] state, int[][] goalState)
    {
        int distance = 0;
        for (int tile = 1; tile <= 3; tile++)
        {
            (int x1, int y1) = FindPosition(state, tile);
            (int x2, int y2) = FindPosition(goalState, tile);
            distance += Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
        }
        return distance;
    }

    static (int, int) FindPosition(int[][] state, int value)
    {
        for (int i = 0; i < state.Length; i++)
        {
            for (int j = 0; j < state[i].Length; j++)
            {
                if (state[i][j] == value)
                    return (i, j);
            }
        }
        return (-1, -1);
    }

    static bool IsTileInGoalPlace(int[][] state, int[][] goalState, int tile)
    {
        var (currentX, currentY) = FindPosition(state, tile);
        var (goalX, goalY) = FindPosition(goalState, tile);
        return currentX == goalX && currentY == goalY;
    }

    //Req 2 and Req 4: this function allows the pieces to move in four directions and horizontal moves costs 2 vertical moves costs 1
    static List<(int[][], string, int, int)> GetNeighbors(int[][] state, int currentTile)
    {
        var neighbors = new List<(int[][], string, int, int)>();
        var directions = new Dictionary<string, (int, int)>
        {
            {"up", (-1, 0)},
            {"down", (1, 0)},
            {"left", (0, -1)},
            {"right", (0, 1)}
        };

        (int x, int y) = FindPosition(state, currentTile);
        foreach (var (move, (dx, dy)) in directions)
        {
            int nx = x + dx, ny = y + dy;

            if (nx >= 0 && nx < 3 && ny >= 0 && ny < 3 && state[nx][ny] == 0)
            {
                var newState = state.Select(row => row.ToArray()).ToArray();
                (newState[x][y], newState[nx][ny]) = (newState[nx][ny], newState[x][y]);

                int moveCost = (move == "up" || move == "down") ? 1 : 2;
                neighbors.Add((newState, move, currentTile, moveCost));
            }
        }
        return neighbors;
    }
    //Req 3, Req 5, Req 6: This function tries making new nodes for the Tile which has the turn, utilizes A* search algorithm and stops after the 10th node expansion
    static Node AStarSearch(int[][] initialState, int[][] goalState)
    {
        var fringe = new SortedSet<Node>();
        fringe.Add(new Node(initialState, heuristic: ManhattanDistance(initialState, goalState)));
        var visited = new HashSet<string>();
        int expandedNodes = 0;
        int currentTile = 1;

        while (fringe.Any())
        {
            var currentNode = fringe.Min;
            fringe.Remove(currentNode);

            for (int i = 0; i <= 2; i++) 
            {
                if (IsTileInGoalPlace(currentNode.State, goalState, currentTile))
                {
                    currentTile = (currentTile % 3) + 1;
                }
            }
            

            if (visited.Contains(StateToString(currentNode.State)))
                continue;

            visited.Add(StateToString(currentNode.State));

            expandedNodes++;
            if (expandedNodes == 1)
            {
                Console.WriteLine("++++++++++++++++++Expanded Node 1:++++++++++++++++++");
                PrintInitialStateSideBySide(currentNode.State, goalState);
            }
            else
            {
                Console.WriteLine($"++++++++++++++++++Expanded Node {expandedNodes}:++++++++++++++++++");
                PrintStateSideBySide(currentNode.State, goalState);
            }
            Console.WriteLine($"Current Cost: {currentNode.Cost}, Heuristic: {currentNode.Heuristic}, Total Cost: {currentNode.TotalCost}\n");

            if (AreStatesEqual(currentNode.State, goalState))
                return currentNode;

            if (expandedNodes >= 10)
            {
                Console.WriteLine("Stopping search the 10 node limit has been reached.");
                return null;
            }

            var newFringe = new List<Node>();
            foreach (var (neighbor, move, tile, moveCost) in GetNeighbors(currentNode.State, currentTile))
            {
                if (!visited.Contains(StateToString(neighbor)))
                {
                    int heuristic = ManhattanDistance(neighbor, goalState);
                    var newNode = new Node(neighbor, currentNode, move, tile, currentNode.Cost + moveCost, heuristic, currentNode.Path + $"{tile}-{move.ToUpper()}-");
                    fringe.Add(newNode);
                    newFringe.Add(newNode);
                }
            }

            Console.WriteLine("Fringe for the Node to be extended:");
            foreach (var node in newFringe)
            {
                var selected = node == fringe.Min ? "SELECTED" : "";
                PrintFringeSideBySide(node, goalState);
                Console.WriteLine($"Current Cost: {node.Cost}, Heuristic: {node.Heuristic}, Total Cost: {node.TotalCost} {selected}\n");
            }
            
            currentTile = (currentTile % 3) + 1;
        }
        return null;
    }

    static string StateToString(int[][] state)
    {
        return string.Join(",", state.Select(row => string.Join("", row)));
    }

    static bool AreStatesEqual(int[][] state1, int[][] state2)
    {
        for (int i = 0; i < state1.Length; i++)
        {
            for (int j = 0; j < state1[i].Length; j++)
            {
                if (state1[i][j] != state2[i][j])
                    return false;
            }
        }
        return true;
    }

    static void PrintInitialStateSideBySide(int[][] state1, int[][] state2)
    {
        Console.WriteLine("Initial State   | Goal State");
        Console.WriteLine(new string('-', 30));
        for (int i = 0; i < state1.Length; i++)
        {
            string currentRow = string.Join(" ", state1[i]);
            string goalRow = string.Join(" ", state2[i]);
            Console.WriteLine($"{currentRow.PadRight(15)} | {goalRow}");
        }
        Console.WriteLine();
    }

    static void PrintStateSideBySide(int[][] state1, int[][] state2)
    {
        Console.WriteLine("Current State   | Goal State");
        Console.WriteLine(new string('-', 30));
        for (int i = 0; i < state1.Length; i++)
        {
            string currentRow = string.Join(" ", state1[i]);
            string goalRow = string.Join(" ", state2[i]);
            Console.WriteLine($"{currentRow.PadRight(15)} | {goalRow}");
        }
        Console.WriteLine();
    }

    static void PrintFringeSideBySide(Node node, int[][] goalState)
    {
        Console.WriteLine("State in Fringe | Goal State");
        Console.WriteLine(new string('-', 30));
        for (int i = 0; i < node.State.Length; i++)
        {
            string fringeRow = string.Join(" ", node.State[i]);
            string goalRow = string.Join(" ", goalState[i]);
            Console.WriteLine($"{fringeRow.PadRight(15)} | {goalRow}");
        }
    }

    static void PrintState(int[][] state)
    {
        foreach (var row in state)
        {
            Console.WriteLine(string.Join(" ", row));
        }
        Console.WriteLine();
    }

    static void Main()
    {
        Console.WriteLine("Enter the initial state (3x3 grid, rows separated by spaces):");
        int[][] initialState = ReadStateFromUser();

        Console.WriteLine("Enter the goal state (3x3 grid, rows separated by spaces):");
        int[][] goalState = ReadStateFromUser();

        var result = AStarSearch(initialState, goalState);
        if (result != null)
        {
            Console.WriteLine("Solution found!");
        }
        else
        {
            Console.WriteLine("No solution found or stopped after 10 expanded nodes.");
        }
    }
    //Req 1: this function gets the inital and goal state by the user
    static int[][] ReadStateFromUser()
    {
        var state = new int[3][];
        for (int i = 0; i < 3; i++)
        {
            var row = Console.ReadLine().Split().Select(int.Parse).ToArray();
            state[i] = row;
        }
        return state;
    }
}
