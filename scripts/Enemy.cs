using System;
using Godot;

public partial class Enemy : CharacterBody2D
{
    [Export]
    NavigationAgent2D navigationAgent2D;

    public Mask mask = null;

    [Export]
    public float maxSpeed = 750;
    [Export]
    public float stunFriction = 3000f;
    public bool stunned = false;
    [Export]
    public float stunDuration = 1.0f;
    [Export]
    public float wallStunDuration = 2.0f;
    [Export]
    public Area2D wallDetectArea;
    public float stunnedTimeRemaining = 0f;

    public override void _Ready() {}

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
        if (!navigationAgent2D.IsTargetReached() && !stunned)
        {
            Vector2 next = navigationAgent2D.GetNextPathPosition();
            Vector2 dir = (next - GlobalPosition).Normalized();

            Velocity = dir * maxSpeed;
        }
        else if (!stunned)
        {
            Velocity = Vector2.Zero;
        }
        else if (stunned)
        {
            Velocity = Velocity.MoveToward(Vector2.Zero, stunFriction * (float)delta);
            if (Velocity.Length() < 10f)
            {
                stunnedTimeRemaining -= (float)delta;
                if (stunnedTimeRemaining <= 0f)
                {
                    stunned = false;
                }
            }
        }
        MoveAndSlide();
    }

    public void BeAttacked(Movement attacker)
    {
        stunned = true;
        stunnedTimeRemaining = stunDuration;
        Velocity = (GetGlobalMousePosition() - attacker.Position).Normalized() * attacker.attackKnockback;
    }

    public void WallImpact(Node2D col)
    {
        if (col is TileMapLayer && stunned)
        {
            GD.Print(this.Name + " hit wall and is stunned longer!");
            stunnedTimeRemaining = wallStunDuration;
            Velocity = Vector2.Zero;
        }
    }
}
