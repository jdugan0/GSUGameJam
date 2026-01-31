using System;
using Godot;

public partial class Camera : Camera2D
{
    Movement player;

    [Export]
    int playerWeight;

    public override void _Ready()
    {
        player = GetTree().GetFirstNodeInGroup("Player") as Movement;
    }

    public override void _Process(double delta)
    {
        Vector2 mousePos = GetGlobalMousePosition();
        Position = (playerWeight * player.Position + mousePos) / (1 + playerWeight);
    }
}
