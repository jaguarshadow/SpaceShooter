using Godot;
using System;

public class Enemy : Area2D
{
    // Declare member variables here. Examples:
    private Vector2 velocity = new Vector2();
    private PathFollow2D follow;
    // private variable for shooting target 
    private bool onPath;
    private RandomNumberGenerator rng = new RandomNumberGenerator();

    public Vector2 Destination { get; set; }

    [Export]
    public int Shield { get; set; }

    [Export]
    private int speed = 400;

    [Export]
    private PackedScene Bullet;

    [Signal]
    public delegate void EnemyShoot(PackedScene bullet, Vector2 pos);

    [Signal]
    public delegate void EnemyKilled();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        rng.Randomize();
        ((Timer)GetNode("GunTimer")).WaitTime = rng.RandiRange(3, 15);
        ((Timer)GetNode("GunTimer")).Start();
        Connect(nameof(EnemyKilled), GetNode("/root/Main"), "_OnEnemyKilled");
        Connect(nameof(EnemyShoot), GetNode("/root/Main"), "_OnEnemyShoot");
        ((AnimationPlayer)GetNode("AnimationPlayer")).Play("hover");
        Path2D path = (Path2D)GetNode("EnemyPaths/Path2D");
        onPath = true;
        follow = new PathFollow2D();
        path.AddChild(follow);
        follow.Loop = false;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        if (onPath)
        {
            follow.Offset += speed * delta;
            GlobalPosition = follow.GlobalPosition;
            if (follow.UnitOffset == 1) onPath = false;
            return;
        }
        if (GlobalPosition.Round() != Destination.Round())
        {
            Move(delta);
        }
    }

    private void Move(float delta)
    {
        velocity = (Destination - GlobalPosition).Normalized();
        GlobalPosition += velocity * speed * delta;
    }

    public void Explode()
    {
        ((Sprite)GetNode("CollisionShape2D/Sprite")).Hide();
        ((AnimationPlayer)GetNode("Explosion/AnimationPlayer")).Play("explode");
        velocity = new Vector2();
    }

    public void _OnAnimationFinished(string anim)
    {
        if (anim != "explode") return;
        QueueFree();
        EmitSignal(nameof(EnemyKilled));
    }

    public void _OnGunTimerTimeout()
    {
        EmitSignal(nameof(EnemyShoot), Bullet, GlobalPosition);
        ((Timer)GetNode("GunTimer")).WaitTime = rng.RandiRange(3, 15);
        ((Timer)GetNode("GunTimer")).Start();
    }
}
