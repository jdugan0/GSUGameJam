using System;
using Godot;

public partial class Enemy : CharacterBody2D
{
    [Export]
    NavigationAgent2D navigationAgent2D;

    public Mask mask = null;

    [Export]
    public float maxSpeed = 1000;

    public override void _Ready() { }

    public override void _PhysicsProcess(double delta)
    {
        if (Camera.instance.zooming)
        {
            navigationAgent2D.TargetPosition = Position;
            return;
        }
        if (GameManager.instance.onGround != null)
        {
            navigationAgent2D.TargetPosition = GameManager.instance.onGround.Value;
        }
        else if (mask == null)
        {
            navigationAgent2D.TargetPosition = GameManager.instance.player.Position;
        }
        else
        {
            navigationAgent2D.TargetPosition = Position;
        }
        if (!navigationAgent2D.IsTargetReached())
        {
            Vector2 next = navigationAgent2D.GetNextPathPosition();
            Vector2 dir = (next - GlobalPosition).Normalized();

            Velocity = dir * maxSpeed;
        }
        else
        {
            Velocity = Vector2.Zero;
        }
        MoveAndSlide();
    }
}
