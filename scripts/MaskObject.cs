using System;
using Godot;

public partial class MaskObject : RigidBody2D
{
    [Export]
    Mask masktype;

    public void OnCollide(Node2D col)
    {
        if (col is Movement m)
        {
            QueueFree();
            m.mask = masktype;
            GameManager.instance.countdown = true;
        }
        if (col is Enemy e)
        {
            e.mask = masktype;
            QueueFree();
        }
    }
}
