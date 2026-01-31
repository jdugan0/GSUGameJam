using System;
using Godot;

public partial class GameManager : Node
{
    public static GameManager instance;
    public Mask currentSafeMask = null;

    [Export]
    public float checkTime = 10;

    public bool countdown = true;

    public float currentTime = 0;

    public Movement player { get; private set; }

    public Vector2? onGround { get; private set; }

    public override void _Ready()
    {
        instance = this;
        player = GetTree().GetFirstNodeInGroup("Player") as Movement;
    }

    public override void _Process(double delta)
    {
        if (GetTree().GetFirstNodeInGroup("Mask") == null)
        {
            onGround = null;
        }
        else
        {
            onGround = (GetTree().GetFirstNodeInGroup("Mask") as Node2D).Position;
        }
        if (currentSafeMask != null)
        {
            GD.Print(currentSafeMask.name);
        }
        // GD.Print("Meow: ", currentSafeMask == player.mask);
        if (countdown)
        {
            currentTime += (float)delta;
        }

        if (currentTime >= checkTime)
        {
            currentTime = 0;
        }
    }
}
