using System;
using Godot;

public partial class MaskObject : RigidBody2D
{
    [Export]
    Mask masktype;

    public void OnCollide(Node2D col)
    {
        GD.Print("meowwww");
        if (col is Movement m)
        {
            QueueFree();
            m.mask = masktype;
            GameManager.instance.countdown = true;
        }
        if (col is Enemy e)
        {
            GD.Print("meow");
            e.mask = masktype;
            QueueFree();
        }
    }
}
