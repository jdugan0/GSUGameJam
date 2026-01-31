using System;
using Godot;

public partial class Safezone : Node
{
    [Export]
    public Mask maskType = null;

    public override void _Process(double delta) { }

    public void OnEnter(Node2D body)
    {
        if (body is Movement m)
        {
            GameManager.instance.currentSafeMask = maskType;
        }
    }

    public void OnExit(Node2D body)
    {
        if (body is Movement m)
        {
            GameManager.instance.currentSafeMask = null;
        }
    }
}
