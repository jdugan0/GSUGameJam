using System;
using Godot;

public partial class Movement : CharacterBody2D
{
    [ExportGroup("Non-Mask Movement")]
    [Export]
    public float MaxSpeed = 1200f;

    [Export]
    public float Acceleration = 7500f;

    [Export]
    public float Friction = 8000f;

    [ExportGroup("Mask Movement")]
    public float MaskMaxSpeed = 600f;
    public float MaskAcceleration = 3000f;
    public float MaskFriction = 160000f;

    [ExportGroup("Dash Settings")]
    [Export]
    public float dashAmount = 3500.0f;

    [Export]
    public float dashCooldown = 3.0f;
    public bool dashAvailable = true;
    public float dashTimeRemaining = 3.0f;

    [Export]
    public Mask mask;
    public bool moveEnabled = true;

    [ExportGroup("MISC")]
    [Export]
    public TextureProgressBar playerDashCooldownBar;

    [Export]
    public Node2D rotationNode;

    [Export]
    public Area2D attackArea;

    [Export]
    public float attackCooldown = 0.3f;
    public float attackDuration = 0.1f;
    public bool attackReady = true;

    public float speed;
    public float acceleration;
    public float friction;

    public bool dead = false;

    [Export(PropertyHint.Layers2DPhysics)]
    public uint dashLayer;

    public uint normalLayer;

    private bool dashing => Velocity.Length() > MaxSpeed * 1.1;

    public override void _Ready()
    {
        attackArea.Monitorable = false;
        attackArea.Monitoring = false;
        attackArea.GetChild<CollisionShape2D>(0).Disabled = true;
        playerDashCooldownBar.MaxValue = dashCooldown;
        playerDashCooldownBar.Value = playerDashCooldownBar.MaxValue;
        normalLayer = CollisionMask;
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;

        Vector2 input = Input.GetVector("LEFT", "RIGHT", "UP", "DOWN");

        if (mask == null)
        {
            speed = MaxSpeed;
            acceleration = Acceleration;
            friction = Friction;
        }
        else
        {
            speed = MaskMaxSpeed;
            acceleration = MaskAcceleration;
            friction = MaskFriction;
        }
        Vector2 targetVelocity = input * speed;

        float rate = input == Vector2.Zero ? friction : acceleration;
        Velocity = Velocity.MoveToward(targetVelocity, rate * dt);

        if (dashAvailable == false)
        {
            dashTimeRemaining -= dt;
            if (dashTimeRemaining <= 0)
            {
                dashAvailable = true;
            }
        }

        if (Input.IsActionJustPressed("DASH") && moveEnabled && mask == null)
        {
            if (dashAvailable)
            {
                Dash();
                dashAvailable = false;
                dashTimeRemaining = dashCooldown;
            }
        }
        if (Input.IsActionPressed("ATTACK") && moveEnabled && attackReady)
        {
            Attack();
        }
        if (moveEnabled)
        {
            MoveAndSlide();
            UpdateRotation();
        }
    }

    public void Kill()
    {
        dead = true;
        Visible = false;
        ProcessMode = ProcessModeEnum.Disabled;
    }

    public override void _Process(double delta)
    {
        playerDashCooldownBar.Value = (dashCooldown - dashTimeRemaining);
        if (dashing)
        {
            CollisionLayer = 0;
            CollisionMask = dashLayer;
        }
        else
        {
            CollisionLayer = 1;
            CollisionMask = normalLayer;
        }
    }

    public void UpdateRotation()
    {
        Vector2 mousePos = GetGlobalMousePosition();
        Vector2 dir = (mousePos - Position).Normalized();
        float angle = dir.Angle();
        rotationNode.Rotation = angle;
    }

    public void Dash()
    {
        Vector2 mousePos = GetGlobalMousePosition();
        Vector2 dir = (mousePos - Position).Normalized();
        Velocity = dir * dashAmount;
        CollisionLayer = 0;
        CollisionMask = dashLayer;
    }

    public void DashThroughEnemy(Node2D col)
    {
        if (col is Enemy e) { }
    }

    public void Attack()
    {
        attackReady = false;
        attackArea.Monitorable = true;
        attackArea.Monitoring = true;
        attackArea.GetChild<CollisionShape2D>(0).Disabled = false;
        //actual attack
        var bodies = attackArea.GetOverlappingBodies();
        GD.Print(bodies);
        foreach (var body in bodies)
        {
            //
        }

        //spawn animation
        GetTree().CreateTimer(attackDuration).Timeout += () =>
        {
            attackArea.Monitorable = false;
            attackArea.Monitoring = false;
            attackArea.GetChild<CollisionShape2D>(0).Disabled = true;

            GetTree().CreateTimer(attackCooldown).Timeout += () =>
            {
                attackReady = true;
            };
        };
    }
}
