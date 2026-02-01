using System;
using Godot;

public partial class MaskObject : RigidBody2D
{
    [Export]
    Mask masktype;

    [Export]
    float pickupCooldown;
    float pickupTimer;

    public override void _Ready()
    {
        pickupTimer = 0;
        GD.Print("meow");
    }

    public override void _Process(double delta)
    {
        pickupTimer += (float)delta;
        foreach (var col in GetCollidingBodies())
        {
            if (pickupTimer <= pickupCooldown)
            {
                break;
            }
            if (col is Movement m)
            {
                QueueFree();
                m.mask = masktype;
                GameManager.instance.countdown = true;
                m.maskHealth = 3;
            }
            if (col is Enemy e)
            {
                e.mask = masktype;
                QueueFree();
            }
        }
    }

    public void OnCollide(Node2D col)
    {
        if (pickupTimer <= pickupCooldown)
        {
            return;
        }
        if (col is Movement m)
        {
            QueueFree();
            m.mask = masktype;
            GameManager.instance.countdown = true;
            m.maskHealth = 3;
        }
        if (col is Enemy e)
        {
            e.mask = masktype;
            QueueFree();
        }
    }
}
