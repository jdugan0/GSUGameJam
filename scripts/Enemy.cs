using System;
using System.Threading.Tasks;
using Godot;

public partial class Enemy : CharacterBody2D
{
    [Export]
    NavigationAgent2D navigationAgent2D;

    [Export]
    public AnimatedSprite2D enemySprite;

    [Export]
    public Mask mask = null;

    [Export]
    public float maxSpeed = 1000;

    uint initalLayerMask;
    uint initalLayer;

    [Export]
    PointLight2D light;

    public override void _Ready()
    {
        navigationAgent2D.VelocityComputed += OnVelocityComputed;
        navigationAgent2D.MaxSpeed = maxSpeed;
        if (mask != null)
        {
            GameManager.instance.toProtect = this;
        }
        initalLayer = CollisionLayer;
        initalLayerMask = CollisionMask;
        maskParticles.Emitting = false;
        lightColor = light.Color;
    }

    public enum EnemyState
    {
        HIDING,
        CHASE_PLAYER,
        PROTECT,
        GET_MASK,
        CAMP,
    }

    public EnemyState enemyState = EnemyState.GET_MASK;

    [Export]
    public float desiredDistance;

    [Export]
    public float protectDistance;

    public Vector2 computedV;

    private bool swapped;
    Vector2 updatedPos;

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
    public float globalAngle = 0f;

    [Export]
    float maskLaunchVelocity;

    [Export]
    PackedScene maskScene;

    int health = 2;

    Movement enteredPlayer;

    [Export]
    float attackTime;
    float attackTimer = 0;

    [Export]
    public AnimatedSprite2D maskSprite;

    [Export]
    public GpuParticles2D maskParticles;

    [Export]
    public GpuParticles2D stunImpactParticles;

    [Export]
    public GpuParticles2D stunnedParticles;

    bool hidden = false;

    [Export]
    Node2D campPos;
    public bool disable = true;
    Color lightColor;

    [Export]
    float attackDelay = 1.0f;
    float attackDelayTimer = 0.0f;

    [Export]
    Color maskColor;

    bool tryingAttack = false;

    public void PlayerEnter(Node2D col)
    {
        if (col is Movement m)
        {
            enteredPlayer = m;
        }
    }

    public void PlayerExit(Node2D col)
    {
        if (col is Movement m)
        {
            enteredPlayer = null;
        }
    }

    public override async void _PhysicsProcess(double delta)
    {
        if (disable)
        {
            return;
        }
        if (mask != null)
        {
            light.Color = maskColor;
        }
        else
        {
            light.Color = lightColor;
        }
        if (stunned)
        {
            Modulate = new Color(Colors.White, 0.8f);
            enemySprite.Animation = "hurt";
            maskSprite.Animation = "hurt";
        }
        else
        {
            Modulate = Colors.White;
        }
        attackTimer -= (float)delta;
        attackDelayTimer -= (float)delta;
        // GD.Print(attackDelayTimer);

        if (attackDelayTimer <= 0 && tryingAttack && enteredPlayer != null)
        {
            tryingAttack = false;
            attackTimer = attackTime;
            await enteredPlayer.Hurt(this);
        }
        if (attackDelayTimer <= 0)
        {
            tryingAttack = false;
        }
        if (attackTimer < 0 && enteredPlayer != null && !stunned && attackDelayTimer <= 0)
        {
            attackDelayTimer = attackDelay;
            tryingAttack = true;
        }
        Rid navMap = navigationAgent2D.GetNavigationMap();
        float playerDist = GameManager.instance.player.Position.DistanceTo(Position);
        if (GameManager.instance.player.mask != null)
        {
            enemyState = EnemyState.CAMP;
            if (playerDist < 1000)
            {
                enemyState = EnemyState.CHASE_PLAYER;
            }
        }
        else if (GameManager.instance.onGround == null && GameManager.instance.player.mask == null)
        {
            if (mask == null)
            {
                enemyState = EnemyState.PROTECT;
                if (playerDist < 1000)
                {
                    enemyState = EnemyState.CHASE_PLAYER;
                }
            }
            else
            {
                enemyState = EnemyState.HIDING;
            }
        }
        else if (GameManager.instance.onGround != null)
        {
            enemyState = EnemyState.GET_MASK;
            if (playerDist < 300)
            {
                enemyState = EnemyState.CHASE_PLAYER;
            }
        }
        else
        {
            enemyState = EnemyState.CHASE_PLAYER;
        }
        if (mask != null)
        {
            maskSprite.Visible = true;
            maskParticles.Emitting = true;
            GameManager.instance.toProtect = this;
            if (!swapped)
            {
                Vector2 playerPos = GameManager.instance.player.Position;
                Vector2 dir = (GlobalPosition - playerPos).Normalized();
                Vector2 rawTarget = playerPos + dir * desiredDistance;
                Vector2 safeTarget = NavigationServer2D.MapGetClosestPoint(navMap, rawTarget);
                updatedPos = safeTarget;
                hidden = false;
            }
            swapped = true;
        }
        else
        {
            swapped = false;
            maskSprite.Visible = false;
            maskParticles.Emitting = false;
        }
        if (playerDist > 2000)
        {
            hidden = true;
        }
        if (playerDist > 2000)
        {
            hidden = true;
        }
        // GD.Print(enemyState);
        switch (enemyState)
        {
            case EnemyState.CAMP:
                navigationAgent2D.TargetPosition = campPos.GlobalPosition;
                break;
            case EnemyState.GET_MASK:
                if (GameManager.instance.onGround != null)
                {
                    navigationAgent2D.TargetPosition = GameManager.instance.onGround.Value;
                }
                break;
            case EnemyState.CHASE_PLAYER:
                navigationAgent2D.TargetPosition = GameManager.instance.player.Position;
                break;
            case EnemyState.HIDING:
                if (playerDist < 600 && hidden)
                {
                    Vector2 playerPos = GameManager.instance.player.Position;
                    Vector2 dir = (GlobalPosition - playerPos).Normalized();
                    Vector2 rawTarget = playerPos + dir * 600;
                    Vector2 safeTarget = NavigationServer2D.MapGetClosestPoint(navMap, rawTarget);
                    updatedPos = safeTarget;
                }
                navigationAgent2D.TargetPosition = updatedPos;
                break;
            case EnemyState.PROTECT:
                Vector2 protectPos = GameManager.instance.toProtect.Position;
                Vector2 dirNew = (GlobalPosition - protectPos).Normalized();
                Vector2 rawTarget1 = protectPos + dirNew * protectDistance;
                Vector2 safeTarget1 = NavigationServer2D.MapGetClosestPoint(navMap, rawTarget1);
                navigationAgent2D.TargetPosition = safeTarget1;
                break;
        }
        if (!navigationAgent2D.IsTargetReached() && !stunned)
        {
            stunnedParticles.Emitting = false;
            Vector2 next = navigationAgent2D.GetNextPathPosition();
            Vector2 dir = (next - GlobalPosition).Normalized();
            navigationAgent2D.Velocity = dir * maxSpeed;
            Velocity = computedV;
        }
        else if (!stunned)
        {
            stunnedParticles.Emitting = false;
            navigationAgent2D.Velocity = Vector2.Zero;
            Velocity = computedV;
        }
        else if (stunned)
        {
            stunnedParticles.Emitting = true;
            Velocity = Velocity.MoveToward(Vector2.Zero, stunFriction * (float)delta);
            if (Velocity.Length() < 10f)
            {
                stunnedTimeRemaining -= (float)delta;
                if (stunnedTimeRemaining <= 0f)
                {
                    CollisionLayer = initalLayer;
                    CollisionMask = initalLayerMask;
                    stunned = false;
                }
            }
        }
        if (Velocity.Length() >= 10f)
        {
            globalAngle = computedV.Angle();
        }
        if (Velocity.X < 0)
        {
            enemySprite.FlipH = true;
            maskSprite.FlipH = true;
        }
        else
        {
            enemySprite.FlipH = false;
            maskSprite.FlipH = false;
        }
        if (!stunned)
        {
            UpdateRotation();
        }
        MoveAndSlide();
    }

    private void OnVelocityComputed(Vector2 safeVelocity)
    {
        computedV = safeVelocity;
    }

    public void BeAttacked(Movement attacker)
    {
        stunned = true;
        stunnedTimeRemaining = stunDuration;
        if (mask != null)
        {
            health--;
            if (health == 0)
            {
                MaskObject m = maskScene.Instantiate<MaskObject>();
                m.LinearVelocity =
                    (GlobalPosition - GameManager.instance.player.GlobalPosition).Normalized()
                    * maskLaunchVelocity;
                var world = GetTree().CurrentScene;
                m.GlobalPosition = GlobalPosition;
                mask = null;
                world.CallDeferred(MethodName.AddChild, m);
                health = 2;
            }
        }
        Velocity =
            (GetGlobalMousePosition() - attacker.Position).Normalized() * attacker.attackKnockback;
    }

    public void Stun()
    {
        stunImpactParticles.Emitting = true;
        stunned = true;
        stunnedTimeRemaining = stunDuration;
        CollisionLayer = 0;
        CollisionMask = 0;
    }

    public void WallImpact(Node2D col)
    {
        if (col is TileMapLayer && stunned)
        {
            stunImpactParticles.Emitting = true;
            GD.Print(this.Name + " hit wall and is stunned longer!");
            stunnedTimeRemaining = wallStunDuration;
            Velocity = Vector2.Zero;
        }
    }

    public void UpdateRotation()
    {
        const float pi = (float)Math.PI;
        if (globalAngle < pi / 8 && globalAngle > -pi / 8)
        {
            enemySprite.Animation = "side";
            maskSprite.Animation = "side";
        }
        else if (globalAngle < 3 * pi / 8 && globalAngle > pi / 8)
        {
            enemySprite.Animation = "frontDiag";
            maskSprite.Animation = "frontDiag";
        }
        else if (globalAngle < 5 * pi / 8 && globalAngle > 3 * pi / 8)
        {
            enemySprite.Animation = "front";
            maskSprite.Animation = "front";
        }
        else if (globalAngle < 7 * pi / 8 && globalAngle > 5 * pi / 8)
        {
            enemySprite.Animation = "frontDiag";
            maskSprite.Animation = "frontDiag";
        }
        else if (globalAngle < -7 * pi / 8 && globalAngle > -pi)
        {
            enemySprite.Animation = "side";
            maskSprite.Animation = "side";
        }
        else if (globalAngle < -5 * pi / 8 && globalAngle > -7 * pi / 8)
        {
            enemySprite.Animation = "backDiag";
            maskSprite.Animation = "backDiag";
        }
        else if (globalAngle < -3 * pi / 8 && globalAngle > -5 * pi / 8)
        {
            enemySprite.Animation = "back";
            maskSprite.Animation = "back";
        }
        else if (globalAngle < -pi / 8 && globalAngle > -3 * pi / 8)
        {
            enemySprite.Animation = "backDiag";
            maskSprite.Animation = "backDiag";
        }
    }
}
