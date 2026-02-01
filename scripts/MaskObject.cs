using System;
using System.Linq;
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
            if (col is Movement m)
            {
                QueueFree();
                m.mask = masktype;
                GameManager.instance.countdown = true;
                m.maskHealth = 3;
            }
            if (col is Enemy e)
            {
                if (pickupTimer <= pickupCooldown)
                {
                    continue;
                }
                e.mask = masktype;
                QueueFree();
            }
        }
    }

    public void OnCollide(Node2D col)
    {
        if (col is Movement m)
        {
            AudioManager.instance.PlaySFX("mask_get");
            AudioManager.instance.CancelSFX(MusicManager.instance.currentMusic);
            MusicManager.instance.currentMusic = "music_spooky";
            AudioManager.instance.PlaySFX("music_spooky");
            QueueFree();
            m.mask = masktype;
            GameManager.instance.countdown = true;
            m.maskHealth = 3;
            foreach (Enemy en in GetTree().GetNodesInGroup("Enemy").OfType<Enemy>().ToArray())
            {
                en.Stun();
            }
        }
        if (col is Enemy e)
        {
            if (pickupTimer <= pickupCooldown)
            {
                return;
            }
            e.mask = masktype;
            QueueFree();
        }
    }
}
