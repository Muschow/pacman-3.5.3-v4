using Godot;
using System;
using System.Collections.Generic;
using System.Collections; //idk if this will do anything but we can try lol
using System.Linq;

public class Movement : Node
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text"; 


    //make a function to convert the source vector from a float to a actual vector by doing MapToWorld and WorldToMap etc
    public int ConvertVecToInt(Vector2 vector)
    {
        if (vector.x == 0)
        {
            return (int)vector.y;
        }
        else if (vector.y == 0)
        {
            return (int)vector.x;
        }
        else if (vector.x == 0 && vector.y == 0)
        {
            return 0;
        }
        else
        {
            return -1; //bascially error
        }
    }

    public List<Vector2> Dijkstras(Vector2 source, Vector2 target) //takes in graph (adjMatrix) and source (Pos) Ghost MUST spawn on node
    {
        List<Vector2> pathList = new List<Vector2>();
        MazeGenerator mazeG = new MazeGenerator();
        //make all the adjList stuff static and then do nodeList = MazeGenerator.nodeList

        //to reset my changes, make the ajList properties static and then replace mazeG.nodeList with MazeGenerator.nodeList
        GD.Print("printing everything in nodelist...");
        foreach (var thing in mazeG.nodeList)
        {
            GD.Print(thing);
        }

        GD.Print("source " + source);
        GD.Print("target " + target);
        if (mazeG.nodeList.Contains(target))
        {
            GD.Print("target is a node");
        }
        else
        {
            GD.Print("target is not a node");
        }

        if (mazeG.nodeList.Contains(source))
        {
            GD.Print("source is a node");
        }
        else
        {
            GD.Print("source is not a node");
        }
        //Have a method here that makes sure source and target are nice round Vectors and not decimals or something like that
        //Im thinking WorldToMap and then MapToWorld again


        if (source == target)
        {
            pathList.Add(source);
            foreach (var thing in pathList)
            {
                GD.Print("pathlist source = target: " + thing);
            }
            return pathList;
        }

        List<Vector2> unvisited = new List<Vector2>();

        // Previous nodes in optimal path from source
        Dictionary<Vector2, Vector2> previous = new Dictionary<Vector2, Vector2>();

        // The calculated distances, set all to Infinity at start, except the start Node
        Dictionary<Vector2, int> distances = new Dictionary<Vector2, int>();

        for (int i = 0; i < mazeG.nodeList.Count; i++)
        {
            unvisited.Add(mazeG.nodeList[i]);

            // Setting the node distance to Infinity (or in this case 9999 lol)
            distances.Add(mazeG.nodeList[i], 9999);

            //previous.Add(mazeG.nodeList[i], Vector2.Zero);
        }

        distances[source] = 0;

        while (unvisited.Count != 0)
        {
            //order unvisted list by distance.
            // unvisited = (from vertex in distances
            //              orderby vertex.Value ascending
            //              select vertex.Key).ToList(); //had to learn linq just for this smh

            //this unvisited list above is creating a new list from distances dictionary every time, I need it to not do that
            unvisited = unvisited.OrderBy(Vector2 => distances[Vector2]).ToList();


            // foreach (var thing in unvisited)
            // {
            //     GD.Print("unvisited: " + thing);
            // }

            Vector2 current = new Vector2(unvisited[0]); //get node with smallest distance
            unvisited.RemoveAt(0); //remove

            if (current == target)
            {
                GD.Print("curr = " + current);
                GD.Print("target = " + target);

                GD.Print("curr == target");
                while (previous.ContainsKey(current))
                {
                    GD.Print("previous[current] " + previous[current]);
                    //insert the node onto the final result
                    pathList.Add(current);
                    current = previous[current];

                    GD.Print("current: " + current);

                }
                //insert the source onto the final result
                pathList.Add(current);
                //list.reverse either here or later so that when you return it for the ghost it leads to pacman and not the other way round
                foreach (var thing in pathList)
                {
                    GD.Print("pathlist cur = target: " + thing);
                }
                break;
            }

            for (int i = 0; i < mazeG.nodeList.Count; i++)
            {
                //GD.Print("current vec: " + current);
                int curIndex = mazeG.nodeList.IndexOf(current);

                if (curIndex == -1)
                {
                    GD.Print("Could not find current node in nodeList");

                }

                //int neighbourVal = MazeGenerator.adjMatrix[curIndex, i];
                int neighbourVal = 0;

                foreach (var neighbour in mazeG.adjList[curIndex])
                {
                    if (neighbour.Item1 == mazeG.nodeList[i])
                    {
                        neighbourVal = neighbour.Item2;
                    }
                }
                //int neighbourVal = MazeGenerator.adjList[curIndex].IndexOf(MazeGenerator.nodeList[i]).Item2;

                if (neighbourVal != 0)
                {
                    //GD.Print("neighbourVal (not 0): " + neighbourVal);
                    int alt = distances[current] + neighbourVal;
                    Vector2 neighbourNode = mazeG.nodeList[i]; //something to do with these lines
                    //GD.Print("neighbour node: " + neighbourNode);

                    if (alt < distances[neighbourNode])
                    {
                        distances[neighbourNode] = alt;
                        previous[neighbourNode] = current;
                        //GD.Print("neighbour node " + neighbourNode + " prevous neighbour node: " + previous[neighbourNode]);
                    }
                }
                //GD.Print("i: " + i);
            }
        }

        GD.Print("dijkstras complete");
        GD.Print("pathlist count " + pathList.Count);
        // foreach (Vector2 thing in pathList)
        // {
        //     GD.Print(thing);
        // }
        //path.bake() got no idea what this is supposed to do not going to lie
        return pathList;
    }



    //Called when the node enters the scene tree for the first time.
    // public override void _Ready()
    // {
    //     


    // }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
