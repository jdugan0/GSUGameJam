using System;
using Godot;

public partial class Enemy : CharacterBody2D
{
    [Export]
    NavigationAgent2D navigationAgent2D;
    [Export]
    public AnimatedSprite2D enemySprite;

    public Mask mask = null;

    [Export]
    public float maxSpeed = 1000;

    public override void _Ready()
    {
        navigationAgent2D.VelocityComputed += OnVelocityComputed;
        navigationAgent2D.MaxSpeed = maxSpeed;
    }

    public enum EnemyState
    {
        HIDING,
        CHASE_PLAYER,
        PROTECT,
        GET_MASK,
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

    public override void _PhysicsProcess(double delta)
    {
        attackTimer -= (float)delta;
        if (attackTimer < 0 && enteredPlayer != null)
        {
            enteredPlayer.Hurt(this);
            attackTimer = attackTime;
        }
        Rid navMap = navigationAgent2D.GetNavigationMap();
        float playerDist = GameManager.instance.player.Position.DistanceTo(Position);
        if (GameManager.instance.onGround == null && GameManager.instance.player.mask == null)
        {
            if (mask == null)
            {
                enemyState = EnemyState.PROTECT;
                if (playerDist < 600)
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
            GameManager.instance.toProtect = this;
            if (!swapped)
            {
                Vector2 playerPos = GameManager.instance.player.Position;
                Vector2 dir = (GlobalPosition - playerPos).Normalized();
                Vector2 rawTarget = playerPos + dir * desiredDistance;
                Vector2 safeTarget = NavigationServer2D.MapGetClosestPoint(navMap, rawTarget);
                updatedPos = safeTarget;
            }
            swapped = true;
        }
        else
        {
            swapped = false;
        }
        switch (enemyState)
        {
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
                if (playerDist < 600)
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
            Vector2 next = navigationAgent2D.GetNextPathPosition();
            Vector2 dir = (next - GlobalPosition).Normalized();
            navigationAgent2D.Velocity = dir * maxSpeed;
            Velocity = computedV;
            if (Velocity.Length() <= 10f)
            {
                globalAngle = computedV.Angle();
            }
        }
        else if (!stunned)
        {
            navigationAgent2D.Velocity = Vector2.Zero;
            Velocity = computedV;
            
            if (Velocity.Length() <= 10f)
            {
                globalAngle = computedV.Angle();
            }
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
        //GD.Print(computedV.Angle());
        UpdateRotation();
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
        stunned = true;
        stunnedTimeRemaining = stunDuration / 2;
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
    public void UpdateRotation()
    {
        const float pi = (float)Math.PI;
        if (globalAngle < pi / 8 && globalAngle > -pi / 8)
            {
                enemySprite.Animation = "side";
            }
            else if (globalAngle < 3 * pi / 8 && globalAngle > pi / 8)
            {
                enemySprite.Animation = "frontDiag";
            }
            else if (globalAngle < 5 * pi / 8 && globalAngle > 3 * pi / 8)
            {
                enemySprite.Animation = "front";
            }
            else if (globalAngle < 7 * pi / 8 && globalAngle > 5 * pi / 8)
            {
                enemySprite.Animation = "frontDiag";
            }
            else if (globalAngle < -7 * pi / 8 && globalAngle > -pi)
            {
                enemySprite.Animation = "side";
            }
            else if (globalAngle < -5 * pi / 8 && globalAngle > -7 * pi / 8)
            {
                enemySprite.Animation = "backDiag";
            }
            else if (globalAngle < -3 * pi / 8 && globalAngle > -5 * pi / 8)
            {
                enemySprite.Animation = "back";
            }
            else if (globalAngle < -pi / 8 && globalAngle > -3 * pi / 8)
            {
                enemySprite.Animation = "backDiag";
            }
    }
}

