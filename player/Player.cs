using Godot;
using System;

public class Player : Area2D
{
    // Declare member variables here. Examples:
    public enum STATES { IDLE, ALIVE, INVULN, DEAD };
    public STATES State { get; set; }

    public Sprite PlayerSprite { get; set; }

    private int speed = 300;
    private Vector2 velocity;
    private bool canShoot = false;

    [Signal]
    public delegate void PlayerShoot();

    [Export]
    public float fireRate;

    [Export]
    public PackedScene Bullet;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        PlayerSprite = (Sprite)GetNode("CollisionShape2D/Sprite");
        Connect(nameof(PlayerShoot), GetNode("/root/Main"), "_OnPlayerShoot");
        ChangeState(STATES.IDLE);
        ((Timer)GetNode("Gun/Timer")).WaitTime = fireRate;
        ((AnimationPlayer)GetNode("AnimationPlayer")).Play("hover");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        GetInput(delta);
    }

    public void GetInput(float delta)
    {
        if (canShoot && Input.IsActionPressed("shoot"))
        {
            Shoot();
        }
        if (Input.IsActionPressed("left"))
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
                break;
            case STATES.ALIVE:
                GD.Print("Player is now alive");
                canShoot = true;
                break;
            case STATES.INVULN:
                GD.Print("Player is now invuln");
                canShoot = false;
                break;
            case STATES.DEAD:
                GD.Print("Player is now dead");
                canShoot = false;
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

    // Event Handlers
    public void _OnGameStarted()
    {
        ChangeState(STATES.ALIVE);
    }

    public void _OnGunTimerTimeout()
    {
        canShoot = true;
    }
}
