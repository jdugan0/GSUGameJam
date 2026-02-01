using System;
using System.Threading.Tasks;
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

    [Export]
    public float attackTime;
    float attackTimer;

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

    [Export]
    public int health;

    [Export]
    float maskLaunchVelocity;

    [Export]
    PackedScene maskScene;

    public int maskHealth;

    [Export]
    float protectionTimer;

    [Export]
    public AnimatedSprite2D maskSprite;

    public override void _Ready()
    {
        maskSprite.Visible = false;
        attackCollisionShape.Disabled = true;
        playerDashCooldownBar.MaxValue = dashCooldown;
        playerDashCooldownBar.Value = playerDashCooldownBar.MaxValue;
        normalLayer = CollisionMask;
        dashTimer = dashTime;
        maskHealth = 0;
        protectionTimer = 1.0f;
        AudioManager.instance.CancelSFX(MusicManager.instance.currentMusic);
        MusicManager.instance.currentMusic = "music_main";
        AudioManager.instance.PlaySFX("music_main");
    }

    public void Hurt(Enemy e)
    {
        if (protectionTimer > 0)
        {
            return;
        }
        AudioManager.instance.PlaySFX("test_ow");
        protectionTimer = 1.0f;
        Velocity = -(e.Position - Position).Normalized() * 3000;
        if (mask != null)
        {
            maskHealth--;
            if (maskHealth == 1)
            {
                AudioManager.instance.CancelSFX(MusicManager.instance.currentMusic);
                MusicManager.instance.currentMusic = "music_neardeath_mask";
                AudioManager.instance.PlaySFX("music_neardeath_mask");
            }
            if (maskHealth == 0)
            {
                AudioManager.instance.PlaySFX("mask_lose");
                MaskObject m = maskScene.Instantiate<MaskObject>();
                m.LinearVelocity =
                    (GlobalPosition - e.GlobalPosition).Normalized() * maskLaunchVelocity;
                var world = GetTree().CurrentScene;
                m.GlobalPosition = GlobalPosition;
                mask = null;
                world.CallDeferred(MethodName.AddChild, m);
            }
        }
        else
        {
            health--;
            if (health == 1)
            {
                AudioManager.instance.CancelSFX(MusicManager.instance.currentMusic);
                MusicManager.instance.currentMusic = "music_neardeath";
                AudioManager.instance.PlaySFX("music_neardeath");
            }
            if (health == 0)
            {
                Kill();
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;

        dashTimer += dt;

        protectionTimer -= (float)delta;

        Vector2 input = Input.GetVector("LEFT", "RIGHT", "UP", "DOWN");
        // if (protectionTimer > 0)
        // {
        //     input = Vector2.Zero;
        // }
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
            maskSprite.Visible = false;
            Ui.instance.vignette.Visible = false;
        }
        else
        {
            speed = MaskMaxSpeed;
            acceleration = MaskAcceleration;
            friction = MaskFriction;
            maskSprite.Visible = true;
            Ui.instance.vignette.Visible = true;
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
            }
        }
        if (attackTimer > 0)
        {
            attackTimer -= dt;
            if (attackTimer <= 0)
            {
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

    public async Task Kill()
    {
        dead = true;
        Visible = false;
        ProcessMode = ProcessModeEnum.Disabled;
        await SceneSwitcher.instance.SwitchSceneAsyncSlide("LoseScreen");
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
                maskSprite.Animation = "side";
            }
            else if (angle < 3 * pi / 8 && angle > pi / 8)
            {
                playerSprite.Animation = "frontDiag";
                maskSprite.Animation = "frontDiag";
            }
            else if (angle < 5 * pi / 8 && angle > 3 * pi / 8)
            {
                playerSprite.Animation = "front";
                maskSprite.Animation = "front";
            }
            else if (angle < 7 * pi / 8 && angle > 5 * pi / 8)
            {
                playerSprite.Animation = "frontDiag";
                maskSprite.Animation = "frontDiag";
            }
            else if (angle < -7 * pi / 8 && angle > -pi)
            {
                playerSprite.Animation = "side";
                maskSprite.Animation = "side";
            }
            else if (angle < -5 * pi / 8 && angle > -7 * pi / 8)
            {
                playerSprite.Animation = "backDiag";
                maskSprite.Animation = "backDiag";
            }
            else if (angle < -3 * pi / 8 && angle > -5 * pi / 8)
            {
                playerSprite.Animation = "back";
                maskSprite.Animation = "back";
            }
            else if (angle < -pi / 8 && angle > -3 * pi / 8)
            {
                playerSprite.Animation = "backDiag";
                maskSprite.Animation = "backDiag";
            }
        }
        rotationNode.Rotation = angle;
        UpdateSpriteDirection();
    }

    public void Dash()
    {
        AudioManager.instance.PlaySFX("dash");
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
        AudioManager.instance.PlaySFX("woosh");
        attackReady = false;
        attackCooldownTimer = attackCooldown;
        attackCollisionShape.Disabled = false;
        Velocity = (attackArea.GlobalPosition - Position).Normalized() * attackInertia;
        attackTimer = attackTime;
    }

    public void ApplyAttack(Node2D col)
    {
        if (col is Enemy enemy)
        {
            AudioManager.instance.PlaySFX("punch");
            enemy.BeAttacked(this);
            Velocity = (Position - enemy.Position).Normalized() * attackKnockback;
        }
    }

    public void UpdateSpriteDirection()
    {
        if (GetGlobalMousePosition().X < Position.X)
        {
            playerSprite.FlipH = true;
            maskSprite.FlipH = true;
        }
        else
        {
            playerSprite.FlipH = false;
            maskSprite.FlipH = false;
        }
    }
}
