using System;
using Godot;

public partial class Camera : Camera2D
{
    [Export]
    Movement player;

    [Export]
    int playerWeight;

    public override void _Process(double delta)
    {
        Vector2 mousePos = GetGlobalMousePosition();
        Position = (playerWeight * player.Position + mousePos) / (1 + playerWeight);
    }
}
