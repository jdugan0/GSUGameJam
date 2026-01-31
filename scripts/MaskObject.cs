using System;
using Godot;

public partial class MaskObject : RigidBody2D
{
    [Export]
    Mask masktype;

    public void OnCollide(Node col)
    {
        GD.Print("meow");
        if (col is Movement m)
        {
            QueueFree();
            m.mask = masktype;
        }
        if (col is Enemy e)
        {
            e.mask = masktype;
            QueueFree();
        }
    }
}
