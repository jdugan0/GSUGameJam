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
    }

    public void OnCollide(Node2D col)
    {
        if (pickupTimer <= pickupCooldown)
        {
            return;
        }
        if (col is Movement m)
        {
            AudioManager.instance.PlaySFX("mask_get");
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
