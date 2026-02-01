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
    public float attackCooldownTimer = 0f;
    public float attackDuration = 0.1f;

    [Export]
    public float attackKnockback = 2000f;
    public float attackInertia = 800f;
    public bool attackReady = true;

    public float speed;
    public float acceleration;
    public float friction;

    public bool dead = false;

    [Export(PropertyHint.Layers2DPhysics)]
    public uint dashLayer;

    public uint normalLayer;

    [Export]
    public AnimatedSprite2D playerSprite;

    [Export]
    public CollisionShape2D attackCollisionShape;

    [Export]
    public GpuParticles2D dashParticles;

    [Export]
    float dashTime;

    float dashTimer;

    private bool dashing => dashTimer <= dashTime;

    public override void _Ready()
    {
        attackCollisionShape.Disabled = true;
        playerDashCooldownBar.MaxValue = dashCooldown;
        playerDashCooldownBar.Value = playerDashCooldownBar.MaxValue;
        normalLayer = CollisionMask;
        dashTimer = dashTime;
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;

        dashTimer += dt;

        Vector2 input = Input.GetVector("LEFT", "RIGHT", "UP", "DOWN");
        if (input != Vector2.Zero)
        {
            playerSprite.Play();
        }
        else
        {
            playerSprite.Stop();
        }
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
        if (attackCooldownTimer > 0f)
        {
            attackCooldownTimer -= dt;
            if (attackCooldownTimer <= 0)
            {
                attackReady = true;
                attackCollisionShape.Disabled = true;
            }
        }
        if (dashing)
        {
            dashParticles.Emitting = true;
        }
        else
        {
            dashParticles.Emitting = false;
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
        const float pi = (float)Math.PI;
        if (Velocity.Length() < 10f)
        {
            if (angle < pi / 8 && angle > -pi / 8)
            {
                playerSprite.Animation = "sideStand";
            }
            else if (angle < 3 * pi / 8 && angle > pi / 8)
            {
                playerSprite.Animation = "sideStand";
            }
            else if (angle < 5 * pi / 8 && angle > 3 * pi / 8)
            {
                playerSprite.Animation = "frontStand";
            }
            else if (angle < 7 * pi / 8 && angle > 5 * pi / 8)
            {
                playerSprite.Animation = "sideStand";
            }
            else if (angle < -7 * pi / 8 && angle > -pi)
            {
                playerSprite.Animation = "sideStand";
            }
            else if (angle < -5 * pi / 8 && angle > -7 * pi / 8)
            {
                playerSprite.Animation = "sideStand";
            }
            else if (angle < -3 * pi / 8 && angle > -5 * pi / 8)
            {
                playerSprite.Animation = "backStand";
            }
            else if (angle < -pi / 8 && angle > -3 * pi / 8)
            {
                playerSprite.Animation = "sideStand";
            }
        }
        else
        {
            if (angle < pi / 8 && angle > -pi / 8)
            {
                playerSprite.Animation = "side";
            }
            else if (angle < 3 * pi / 8 && angle > pi / 8)
            {
                playerSprite.Animation = "frontDiag";
            }
            else if (angle < 5 * pi / 8 && angle > 3 * pi / 8)
            {
                playerSprite.Animation = "front";
            }
            else if (angle < 7 * pi / 8 && angle > 5 * pi / 8)
            {
                playerSprite.Animation = "frontDiag";
            }
            else if (angle < -7 * pi / 8 && angle > -pi)
            {
                playerSprite.Animation = "side";
            }
            else if (angle < -5 * pi / 8 && angle > -7 * pi / 8)
            {
                playerSprite.Animation = "backDiag";
            }
            else if (angle < -3 * pi / 8 && angle > -5 * pi / 8)
            {
                playerSprite.Animation = "back";
            }
            else if (angle < -pi / 8 && angle > -3 * pi / 8)
            {
                playerSprite.Animation = "backDiag";
            }
        }
        rotationNode.Rotation = angle;
        UpdateSpriteDirection();
    }

    public void Dash()
    {
        Vector2 mousePos = GetGlobalMousePosition();
        Vector2 dir = (mousePos - Position).Normalized();
        Velocity = dir * dashAmount;
        CollisionLayer = 0;
        CollisionMask = dashLayer;
        dashTimer = 0;
    }

    public void DashThroughEnemy(Node2D col)
    {
        if (col is Enemy e)
        {
            e.Stun();
        }
    }

    public void Attack()
    {
        attackReady = false;
        attackCooldownTimer = attackCooldown;
        attackCollisionShape.Disabled = false;
        Velocity = (attackArea.GlobalPosition - Position).Normalized() * attackInertia;
    }

    public void ApplyAttack(Node2D col)
    {
        if (col is Enemy enemy)
        {
            enemy.BeAttacked(this);
            Velocity = (Position - enemy.Position).Normalized() * attackKnockback;
        }
    }

    public void UpdateSpriteDirection()
    {
        if (GetGlobalMousePosition().X < Position.X)
        {
            playerSprite.FlipH = true;
        }
        else
        {
            playerSprite.FlipH = false;
        }
    }
}
