using Godot;
using System;

public class Player : Area2D
{
    // Declare member variables here. Examples:
    public enum STATES { IDLE, ALIVE, INVULN, SHIELD, DEAD };
    public STATES State { get; set; }

    public Sprite PlayerSprite { get; set; }

    private int speed = 300;
    private Vector2 velocity;
    private bool canMove = false;
    private bool canShoot = false;
    private AnimationPlayer animation;
    private Vector2 origin_position;

    [Export]
    public float FireRate { get; set; }

    [Export]
    public int Health { get; set; }

    [Export]
    public PackedScene Bullet;

    [Signal]
    public delegate void PlayerShoot();

    [Signal]
    public delegate void Dead();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        origin_position = GlobalPosition;
        PlayerSprite = (Sprite)GetNode("CollisionShape2D/Sprite");
        Connect(nameof(PlayerShoot), GetNode("/root/Main"), "_OnPlayerShoot");
        Connect(nameof(Dead), GetNode("/root/Main"), "_OnPlayerDead");
        animation = ((AnimationPlayer)GetNode("AnimationPlayer"));
        ChangeState(STATES.IDLE);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        GetInput(delta);
    }

    // TODO: Experimental touch screen controls - still need tweaking
    //public override void _Input(InputEvent @event)
    //{
    //    base._Input(@event);
    //    if (@event.GetType() == typeof(InputEventScreenDrag))
    //    {
    //        speed = 500;
    //        InputEventScreenDrag swipe = (InputEventScreenDrag)@event;
    //        if (swipe.Relative.x > 0)
    //        {
    //            //move right
    //            velocity = new Vector2(1, 0);
    //            if (GetViewportRect().End.x - 24 > Position.x) Position += (speed * velocity) * GetProcessDeltaTime();
    //        }
    //        else if (swipe.Relative.x < 0)
    //        {
    //            //move left 
    //            velocity = new Vector2(-1, 0);
    //            if (GetViewportRect().Position.x + 24 < Position.x) Position += (speed * velocity) * GetProcessDeltaTime();
    //        }
    //    }
    //    else if (@event.GetType() == typeof(InputEventScreenTouch) && canShoot)
    //    {
    //        Shoot();
    //    }
    //}

    public void GetInput(float delta)
    {
        if (State == STATES.DEAD) return;
        if (canShoot && Input.IsActionPressed("shoot"))
        {
            Shoot();
        }
        if (!canMove) return;
        else if (Input.IsActionPressed("left"))
        {
            // move left
            PlayerSprite.Frame = 0;
            velocity = new Vector2(-1, 0);
            if (GetViewportRect().Position.x + 24 < Position.x) Position += (speed * velocity) * delta;
            
        }
        else if (Input.IsActionPressed("right"))
        {
            // move right
            PlayerSprite.Frame = 2;
            velocity = new Vector2(1, 0);
            if (GetViewportRect().End.x - 24 > Position.x) Position += (speed * velocity) * delta;
        }
        else 
        {
            PlayerSprite.Frame = 1;
        }

    }

    public void ChangeState(STATES state)
    {
        switch (state)
        {
            case STATES.IDLE:
                GD.Print("Player is now idle");
                canShoot = false;
                canMove = false;
                break;
            case STATES.ALIVE:
                GD.Print("Player is now alive");
                canShoot = true;
                canMove = true;
                animation.Play("hover");
                break;
            case STATES.INVULN:
                GD.Print("Player is now invuln");
                canShoot = false;
                canMove = true;
                break;
            case STATES.SHIELD:
                GD.Print("Player is now shielded");
                canMove = true;
                break;
            case STATES.DEAD:
                GD.Print("Player is now dead");
                canShoot = false;
                canMove = false;
                velocity = new Vector2();
                EmitSignal(nameof(Dead));
                break;
        }
        State = state;
    }

    private void Shoot()
    {
        EmitSignal(nameof(PlayerShoot), Bullet, ((Position2D)GetNode("Gun")).GlobalPosition);
        canShoot = false;
        ((Timer)GetNode("Gun/Timer")).Start();
    }

    public void Explode()
    {
        ((Sprite)GetNode("CollisionShape2D/Sprite")).Hide();
        ((AnimationPlayer)GetNode("Explosion/AnimationPlayer")).Play("explode");
        ((AudioStreamPlayer)GetNode("ExplodeSound")).Play();
        ChangeState(STATES.DEAD);
    }

    // Event Handlers
    public void _OnGameStarted()
    {
        Health = 100;
        ((Main)GetNode("/root/Main")).ChangeHealth(Health);
        FireRate = 0.25f;
        ((Sprite)GetNode("CollisionShape2D/Sprite")).Show();
        ((Timer)GetNode("Gun/Timer")).WaitTime = FireRate;
        GlobalPosition = origin_position;
        ChangeState(STATES.ALIVE);
    }

    public void _OnGunTimerTimeout()
    {
        if (State == STATES.IDLE || State == STATES.DEAD) return;
        canShoot = true;
        ((Timer)GetNode("Gun/Timer")).WaitTime = FireRate;
    }

    public async void _OnPowerupPickup(string type)
    {
        if (State == STATES.DEAD || State == STATES.IDLE) return;
        switch (type)
        {
            case "RAPID":
                if (FireRate > 0.1f)
                {
                    FireRate = 0.1f;
                    await ToSignal(GetTree().CreateTimer(5f), "timeout");
                    FireRate = 0.25f;
                }
                break;
            case "SHIELD":
                if (State != STATES.SHIELD)
                {
                    ChangeState(STATES.SHIELD);
                    animation.Play("shield");
                }
                break;
            default:
                GD.Print("Picked up a different type");
                break;
        }
    }

    public void _OnAnimationFinished(string anim)
    {
        if (anim == "hurt") animation.Play("hover");
        if (anim == "shield") ChangeState(STATES.ALIVE);
    }
}
