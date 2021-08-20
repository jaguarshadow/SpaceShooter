using Godot;
using System;

public class Bullet : Area2D
{
    // Declare member variables here. Examples:

    private Vector2 Velocity;

    [Export]
    private int speed = 800;

    public string Target { get; set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

    }

    public void Start(Vector2 pos, Vector2 dir)
    {
        GlobalPosition = pos;
        Velocity = new Vector2(speed, 0).Rotated(dir.Angle());
    }


    public void PlaySound()
    {
        ((AudioStreamPlayer)GetNode("Sound")).Play();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        Position += Velocity * delta;
    }

    public void _OnBulletAreaEntered(Area2D body)
    {
        if (Target == "enemy" && body.IsInGroup("enemies"))
        {
            ((AnimationPlayer)body.GetNode("AnimationPlayer")).Play("hurt");
            ((Enemy)body).Shield -= 50;
            QueueFree();
            if (((Enemy)body).Shield <= 0)
            {
                ((Enemy)body).Explode();
            }
        }

        if (Target == "enemy" && body.IsInGroup("player"))
        {
            ((AnimationPlayer)body.GetNode("AnimationPlayer")).Play("hurt");
            ((Enemy)body).Shield -= 50;
            QueueFree();
            if (((Enemy)body).Shield <= 0)
            {
                ((Enemy)body).Explode();
            }
        }
    }

    public void _OnBulletScreenExit()
    {
        QueueFree();
    }
}
