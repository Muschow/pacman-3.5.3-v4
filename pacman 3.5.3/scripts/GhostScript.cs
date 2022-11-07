using Godot;
using System;
using System.Collections.Generic;

public class GhostScript : CharacterScript
{

    protected override void MoveAnimManager(Vector2 masVector)
    {
        AnimatedSprite ghostEyes = GetNode<AnimatedSprite>("GhostEyes"); //not sure whether to put it in here for readabillity or in each ready so theres less calls

        masVector = masVector.Normalized();
        if (masVector == Vector2.Up)
        {
            ghostEyes.Play("up");
        }
        else if (masVector == Vector2.Down)
        {
            ghostEyes.Play("down");
        }
        else if (masVector == Vector2.Right)
        {
            ghostEyes.Play("right");
        }
        else if (masVector == Vector2.Left)
        {
            ghostEyes.Play("left");
        }
    }
    //As GhostScript is a base class, it will not be in the scene tree so ready and process are not needed
    // Called when the node enters the scene tree for the first time.

    public override void _Ready()
    {
        //1,36*32 +16,16
        Position = new Vector2(1 * 32 + 16, 35 * 32 + 16); //temp starting pos
        TileMap mazeTm = GetNode<TileMap>("/root/Game/MazeContainer/Maze/MazeTilemap");

        Movement moveScr = new Movement();
        List<Vector2> paths = moveScr.Dijkstras(new Vector2(1, 1), new Vector2(1, 35));
        foreach (Vector2 thing in paths)
        {
            GD.Print(thing);
        }
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
