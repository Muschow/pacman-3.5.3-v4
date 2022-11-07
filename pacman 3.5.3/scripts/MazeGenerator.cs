using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class MazeGenerator : TileMap
{
    const int path = 0; //maybe make these constants global variables?
    const int wall = 1;
    const int node = 0; //0 blank 1 green //2 green 3 blank
    [Export] public int width = 31; //originally 31
    [Export] public int height = 38; //was originally 19
    public int mazeOriginY; //maybe make this or mazeStartLoc a global variable
    private int backtrackCount = 0;
    private bool generationComplete = false;

    //public int mazesOnScreen = 0; //have the ghost maze wall decrease this number when passing a maze chunk mazeOnScreen -= 1; //maybe make public

    static Vector2 north = new Vector2(0, -1); //maybe turn these NESW vectors into global variables or just replace with Vector2.Up etc
    static Vector2 east = new Vector2(1, 0);
    static Vector2 south = new Vector2(0, 1);
    static Vector2 west = new Vector2(-1, 0);
    Vector2[] directions = new Vector2[4] { north, east, south, west };

    List<Vector2> visited = new List<Vector2>();
    List<Vector2> wallEdgeList = new List<Vector2>();
    Stack<Vector2> rdfStack = new Stack<Vector2>();

    //-----------------------------------------------------Adjacency Matrix/List properties---------------------------------------------------------------
    public List<Vector2> nodeList = new List<Vector2>(); //for nodes,maybe get rid of this to be honest

    public List<Tuple<Vector2, int>>[] adjList;

    //public int[,] adjMatrix;

    //-------------------------------------------------End of Adjacency Matrix/List properties-----------------------------------------------------------

    //---------------------------------------------------Maze Generator Helper Functions-----------------------------------------------------------------
    private void CorrectMazeSize()
    {
        if (width % 2 != 1)
        {
            width -= 1;
        }
        if (height % 2 != 1)
        {
            height -= 1;
        }
        GD.Print("width " + width);
        GD.Print("height " + height);
    }

    private void CreateStartingGrid()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                //wall tile edges
                if ((i == 0 && j != 0) || (i == width - 1 && j != 0) || (j == height - 1)) //j != 0 stuff removes the entire top layer
                {
                    SetCell(i, j + mazeOriginY, wall);

                    Vector2 wallEdge = new Vector2(i, j + mazeOriginY);
                    wallEdgeList.Add(wallEdge);
                }
                //alternating wall tiles
                else if ((i % 2 == 0 || j % 2 == 0) && (j != 0)) //again, j!=0 removes the top layer so that the next maze can slot into it
                {
                    SetCell(i, j + mazeOriginY, wall);
                }
                //path tiles that go between those alternating wall tiles
                else
                {
                    SetCell(i, j + mazeOriginY, path);
                }
            }
        }
    }

    private void FixDeadEnds(Vector2 currentV)
    {

        bool complete = false;

        for (int i = 0; i < directions.Length; i++)
        {
            if (!complete)
            {
                Vector2 newCell = new Vector2(currentV + directions[i]);
                if ((GetCellv(newCell) == wall) && (!wallEdgeList.Contains(newCell)) && (!visited.Contains(newCell)))
                {
                    SetCellv(newCell, path);

                    if (GetCellv(currentV + (directions[i] * 3)) != path)
                    {
                        AddNode(currentV + (directions[i] * 2));
                    }
                    if (GetCellv(currentV + (directions[i] * -1)) != path)
                    {
                        AddNode(currentV);
                    }
                    complete = true;
                }
            }
        }
    }

    private void PrepMazeForJoin(int numHoles) //dependancy on gameScr.Get(mazesOnTheScreen)
    {
        TileMap nodeTilemap = GetParent().GetNode<Godot.TileMap>("NodeTilemap");

        Random rnd = new Random();
        int numUsedCells = 0;

        for (int i = 1; i < width - 1; i++) //this loop sets the top row of the maze into just paths so it can join with the bottom of another maze
        {
            Vector2 topWallCell = new Vector2(i, mazeOriginY);

            if (GetCellv(topWallCell + south) == wall)
            {
                SetCellv(topWallCell + south, path);
                //GD.Print("set " + new Vector2(removeCell + south) + " path");
                //GD.Print("set cell+south path");
            }

            //on the top layer, if there isnt a node where there should be one due to removing the top wall, place one
            if (GetCellv(topWallCell + (south * 2)) == path && nodeTilemap.GetCellv(topWallCell + south) != node)
            {
                AddNode(topWallCell + south);
                //GD.Print("addNode " + new Vector2(topWallCell + south));
            }
        }

        GD.Print(mazeOriginY); //debug

        Node2D gameScr = GetNode<Node2D>("/root/Game"); //dependancy, really this could be in the gameScr script

        if ((int)gameScr.Get("mazesOnTheScreen") > 0) //If its not the first maze, Add paths to the floor so that you can join to the maze below
        {
            while (numUsedCells < numHoles) //Maybe change to Math.Round(width/4) <-- [must be >3]
            {
                int cellX = rnd.Next(1, width - 1);
                Vector2 cell = new Vector2(cellX, mazeOriginY + height - 1);
                if (GetCellv(cell) == wall && GetCellv(cell + north) == path && GetCellv(cell + east) == wall && GetCellv(cell + west) == wall) //makes it so each hole has 2 walls either side
                {
                    SetCellv(cell, path);
                    numUsedCells++;
                    //I deliberately made it so there are no nodes joining the 2 mazes. This is as a ghost is instanced on its own maze; if pacman goes between mazes, 
                    //it switches to being chased by the ghosts on that maze. This way, ghosts arent going between mazes and leaving 1 maze empty and 1 maze full.
                    //This also means pacman could exploit the game by just staying in between mazes, however, the ghost maze wall will stop that, forcing pacman to move up.
                }

            }
        }

    }

    private void AddNode(Vector2 nodeLocation)
    {
        TileMap nodeTilemap = GetParent().GetNode<Godot.TileMap>("NodeTilemap");

        if (nodeTilemap.GetCellv(nodeLocation) != node) //makes sure theres no duplicates... in a perfect world i would not need this
        {
            //SetCellv(nodeLocation, -1); //deletes tile so will remove wall node that collides (probably dont actually need this but just in case lol)
            nodeTilemap.SetCellv(nodeLocation, node); //turns it into an actual path node tile

            nodeList.Add(nodeLocation);

        }
        else
        {
            GD.Print("found bad node");
        }
    }
    //---------------------------------------------------End of Maze Generation Helper functions-----------------------------------------------------------

    //------------------------------------------------------Actual Maze Generation functions---------------------------------------------------------------
    private void rdfInit()
    {
        generationComplete = false;

        CorrectMazeSize();
        CreateStartingGrid();

        //startVector x and y must be odd, between 1+mazeOriginX/Y & height-1 / width-1 
        Vector2 startVector = new Vector2(1, mazeOriginY + 1); //Choose the initial cell,
        //GD.Print("StartV: " + startVector); //debug

        visited.Add(startVector); //Mark initial cell as visited,
        rdfStack.Push(startVector); //and push it to the stack,

        rdfStep();
    }

    private void rdfStep()
    {
        Vector2 prev = new Vector2(0, 0);
        while (!generationComplete)
        {
            Vector2 curr = rdfStack.Pop(); //Pop a cell from the stack and make it a current cell.
            Vector2 next = new Vector2(0, 0);

            bool found = false;

            //check neighbours in random order //N,E,S,W walls instead of their paths, so *2
            Random rnd = new Random();

            var rndDirections = directions.OrderBy(_ => rnd.Next()).ToList(); //found this online, randomly shuffle the list.

            for (int i = 0; i < rndDirections.Count; i++)
            {
                next = 2 * rndDirections[i];
                if (GetCellv(curr + next) == path && (!visited.Contains(curr + next)))
                { //If the current cell has any neighbours which have not been visited,
                    found = true;
                    break; //Choose one of the unvisited neighbours (next),
                }
            }

            if (found)
            {
                if (prev != next)
                {
                    AddNode(curr);
                }
                prev = next;

                rdfStack.Push(curr); //Push the current cell to the stack,
                SetCellv(curr + (next / 2), path); // Remove the wall between the current cell and the chosen cell,
                visited.Add(curr + next); //Mark the chosen cell as visited,
                rdfStack.Push(curr + next); //and push it to the stack.  

                backtrackCount = 0;
            }
            else
            {
                backtrackCount++;
                if (backtrackCount == 1)
                {
                    FixDeadEnds(curr);
                }
            }

            if (rdfStack.Count <= 0)
            { //While stack is not empty, (if stack is empty)
                FixDeadEnds(curr);
                PrepMazeForJoin(7); //dependancy on gameScr.Get(mazesOnTheScreen)

                generationComplete = true;

                GD.Print("Maze Generation Complete!"); //debug
                return;
            }
        }
    }

    //--------------------------------------------------End of Actual Maze Generation functions-----------------------------------------------------------------

    //------------------------------------------------------Adjacency Matrix/List stuff-------------------------------------------------------------------------

    private int ConvertVectorToInt(Vector2 temp)
    {

        if (temp.x == 0)
        {
            return (int)Math.Abs(temp.y);
        }
        else
        {
            return (int)Math.Abs(temp.x);
        }
    }

    private void PrintNodeList()
    {
        GD.Print("Printing NodeList: ");
        foreach (var thing in nodeList)
        {
            GD.Print(thing);
        }
    }

    private void GenerateNodeList() //currently not using this, if i find out the AddNode stuff doesnt work then use this instead
    {
        TileMap nodeTilemap = GetNode<TileMap>("NodeTilemap");
        for (int i = 1; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                if (nodeTilemap.GetCell(j, i) == node)
                {
                    nodeList.Add(new Vector2(j, i));
                }
            }
        }
        PrintNodeList();
    }

    private bool IfWallOrNodeBetween(Vector2 vec1, Vector2 vec2)
    {
        //vec1 should be the smaller vector
        //GD.Print("IfOnWall: " + " Vec1: " + vec1 + ", Vec2: " + vec2);
        //TileMap mazeTilemap = GetNode<TileMap>("MazeTilemap");
        TileMap nodeTilemap = GetParent().GetNode<Godot.TileMap>("NodeTilemap");
        if (vec1.x == vec2.x)
        {
            for (int y = (int)vec1.y; (int)y < vec2.y; y++)
            {
                if ((GetCell((int)vec1.x, y) == wall) || (nodeTilemap.GetCell((int)vec1.x, y) == node && y != vec1.y && y != vec2.y))
                {
                    //GD.Print("reached get cell x: " + vec1.x + ",y: " + y);
                    return true;
                }
            }
            return false;
        }
        else if (vec1.y == vec2.y)
        {
            for (int x = (int)vec1.x; (int)x < vec2.x; x++)
            {
                if (GetCell(x, (int)vec1.y) == wall || (nodeTilemap.GetCell(x, (int)vec1.y) == node && x != vec1.x && x != vec2.x))
                {
                    return true;
                }
            }
            return false;
        }
        else
        {
            return true;
        }

    }

    // private int[,] GenerateAdjMatrix()
    // {
    //     TileMap mazeTilemap = GetNode<TileMap>("MazeTilemap");

    //     int[,] adjMatrix = new int[nodeList.Count, nodeList.Count];

    //     for (int i = 0; i < nodeList.Count; i++)
    //     {
    //         //int nodeCounter = 0;
    //         for (int j = 0; j < nodeList.Count; j++)
    //         {

    //             Vector2 v1 = nodeList[i];
    //             Vector2 v2 = nodeList[j];
    //             if (v1.x == v2.x || v1.y == v2.y)
    //             {
    //                 Vector2 vec1;
    //                 Vector2 vec2;
    //                 //swaps so v1 is smaller and v2 is bigger
    //                 if (v1 <= v2)
    //                 {
    //                     vec1 = v1;
    //                     vec2 = v2;
    //                 }
    //                 else
    //                 {
    //                     vec1 = v2;
    //                     vec2 = v1;
    //                 }

    //                 //if on wall, no edge, else put weight
    //                 if (IfWallOrNodeBetween(vec1, vec2))
    //                 {
    //                     adjMatrix[i, j] = 0;
    //                 }
    //                 else
    //                 {
    //                     adjMatrix[i, j] = ConvertVectorToInt(vec2 - vec1);
    //                     //nodeCounter++;
    //                 }

    //             }
    //         }
    //     }

    //     return adjMatrix;
    // }

    // private void PrintAdjMatrix(int[,] adjMatrix)
    // {
    //     GD.Print("Actual adj matrix :");
    //     for (int i = 0; i < nodeList.Count; i++)
    //     {
    //         GD.PrintRaw("\n");
    //         for (int j = 0; j < nodeList.Count; j++)
    //         {
    //             GD.PrintRaw(adjMatrix[i, j]);
    //         }
    //     }
    // }

    private List<Tuple<Vector2, int>>[] GenerateAdjList()
    {
        TileMap mazeTilemap = GetNode<TileMap>("MazeTilemap");

        adjList = new List<Tuple<Vector2, int>>[nodeList.Count];
        //adjList is an array, size number of nodes, containing a list of tuples (neighbour nodes, weight)
        for (int i = 0; i < adjList.Length; i++) //init list
        {
            adjList[i] = new List<Tuple<Vector2, int>>();
        }



        for (int i = 0; i < nodeList.Count; i++)
        {
            for (int j = 0; j < nodeList.Count; j++)
            {

                Vector2 v1 = nodeList[i];
                Vector2 v2 = nodeList[j];
                if (v1.x == v2.x || v1.y == v2.y)
                {
                    Vector2 vec1;
                    Vector2 vec2;
                    //swaps so v1 is smaller (vec1) and v2 is bigger (vec2)
                    if (v1 <= v2)
                    {
                        vec1 = v1;
                        vec2 = v2;
                    }
                    else
                    {
                        vec1 = v2;
                        vec2 = v1;
                    }

                    int neighbourVal = ConvertVectorToInt(vec2 - vec1);
                    //if on wall, no edge, else put weight
                    if ((!IfWallOrNodeBetween(vec1, vec2)) && (neighbourVal != 0))
                    {
                        Tuple<Vector2, int> newTuple = new Tuple<Vector2, int>(nodeList[j], ConvertVectorToInt(vec2 - vec1));
                        adjList[i].Add(newTuple); //this is not working for some reason, object reference not set to instance of object
                    }
                }
            }
        }

        return adjList;
    }

    private void PrintAdjList(List<Tuple<Vector2, int>>[] adjList)
    {
        GD.Print("\nPrinting adj list");
        for (int i = 0; i < adjList.Length; i++)
        {
            GD.Print("\n");
            GD.PrintRaw(nodeList[i] + " | ");
            foreach (var item in adjList[i])
            {
                GD.PrintRaw(item + ", ");
            }
        }
    }
    //-------------------------------End of Adjacency Matrix/List stuff-----------------------------------------------------------------------------

    //--------------------------------------Other functions --------------------------------------------------------------------

    public Vector2 SetSpawn(bool spawnPacman) //probably place this somewhere else or make global idk
    {
        int x = 0;
        int y = 0;

        Random rnd = new Random();
        if (spawnPacman)
        {
            y = height - 2;
            while ((GetCell(x, y) == wall) || (GetCell(x, y + 1) == path))
            {
                x = rnd.Next(1, width);
            }
        }
        else
        {
            while (GetCell(x, y) == wall)
            {
                x = rnd.Next(1, width);
                y = rnd.Next(1, height - 2);
            }
        }
        Vector2 spawnLoc = new Vector2(x, y);
        //GD.Print("spawn" + spawnLoc); //debug

        spawnLoc = new Vector2((spawnLoc * CellSize) + (CellSize / 2));

        //GD.Print("MTWspawnLoc: " + spawnLoc); //debug
        return spawnLoc;
    }

    //--------------------------------------------End of Other Functions--------------------------------------------------------------

    //Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Node2D gameScr = GetNode<Node2D>("/root/Game");
        mazeOriginY = (int)gameScr.Get("mazeStartLoc"); //dependancy, maybe make mazeStartLoc a global variable instead?

        GD.Randomize();
        rdfInit();
        //UpdateDirtyQuadrants(); //maybe get rid of this tbh, not sure if its doing anything. Supposed to force and update to the tilemap if tiles arent updating
        //adjMatrix = GenerateAdjMatrix();
        //PrintAdjMatrix();

        GenerateAdjList();
        PrintAdjList(adjList);

        GD.Print("nodeList Count: " + nodeList.Count);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {

    }
}
