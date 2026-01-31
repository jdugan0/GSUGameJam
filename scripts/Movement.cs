using System;
using Godot;

public partial class Movement : CharacterBody2D
{
    [Export]
    public float MaxSpeed = 2400f;

    [Export]
    public float Acceleration = 15000f;

    [Export]
    public float Friction = 8000f;
    [Export]
    public float dashAmount = 4250.0f;

    [Export]
    public float dashCooldown = 3.0f;
    public bool dashAvailable = true;
    public float dashTimeRemaining = 3.0f;
    public Mask mask;
    public bool moveEnabled = true;
    [Export]
    public TextureProgressBar playerDashCooldownBar;

    public override void _Ready()
    {
        playerDashCooldownBar.MaxValue = dashCooldown;
        playerDashCooldownBar.Value = playerDashCooldownBar.MaxValue;
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;

        Vector2 input = Input.GetVector("LEFT", "RIGHT", "UP", "DOWN");
        Vector2 targetVelocity = input * MaxSpeed;

        float rate = input == Vector2.Zero ? Friction : Acceleration;
        Velocity = Velocity.MoveToward(targetVelocity, rate * dt);

        if (dashAvailable == false)
        {
            dashTimeRemaining -= dt;
            if (dashTimeRemaining <= 0)
            {
                dashAvailable = true;
            }
        }

        if (Input.IsActionJustPressed("DASH") && moveEnabled)
        {
            if (dashAvailable)
            {
                Dash();
                dashAvailable = false;
                dashTimeRemaining = dashCooldown;
            }
        }
        if (moveEnabled)
        {
            MoveAndSlide();
        }
    }

    public override void _Process(double delta)
    {
        playerDashCooldownBar.Value = (dashCooldown - dashTimeRemaining);
    }


    public void Dash()
    {
        Vector2 mousePos = GetGlobalMousePosition();
        Vector2 dir = (mousePos - Position).Normalized();
        Velocity = dir * dashAmount;
    }

}
