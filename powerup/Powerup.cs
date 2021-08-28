using Godot;
using Array = Godot.Collections.Array;
using System;

public class Powerup : Area2D
{
    // Declare member variables here. Examples:
    public enum TYPES { RAPID, SHIELD, LASER }

    public TYPES PowerupType { get; set; }

    private Player player;
    private int speed = 100;
    private static RandomNumberGenerator rng = new RandomNumberGenerator();
    private AudioStreamPlayer sound;

    [Signal]
    public delegate void PickedUp(string type);

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        rng.Randomize();
        player = ((Player)GetNode("/root/Main/Level/Gameplay/PlayerNode/Player"));
        Connect(nameof(PickedUp), player, "_OnPowerupPickup");
    }

    public void Start()
    {
        int rand = rng.RandiRange(1, 2);
        Sprite s = (Sprite)GetNode("Sprite");
        sound = (AudioStreamPlayer)GetNode("Sound");
        switch (rand)
        {
            case 1:
                //spawn a shield
                PowerupType = TYPES.SHIELD;
                sound.Stream = (AudioStream)ResourceLoader.Load("res://assets/sounds/PUShieldSound.wav");
                s.Frame = 1;
                break;

            case 2:
                //spawn a rapid fire
                PowerupType = TYPES.RAPID;
                sound.Stream = (AudioStream)ResourceLoader.Load("res://assets/sounds/PURapidSound.wav");
                s.Frame = 2;
                break;

            case 3:
                // spawn laser
                PowerupType = TYPES.LASER;
                s.Frame = 3;
                break;
        }
    }

    public override void _Process(float delta)
    {
        base._Process(delta); 
        GlobalPosition += (new Vector2(0, 1) * speed) * delta;
        if (GlobalPosition.y > GetViewportRect().End.y + 5) QueueFree();
    }

    public async void _OnPowerupAreaEntered(Area2D body)
    {
        if (body.Name == "Player")
        {
            sound.Play();
            ((Sprite)GetNode("Sprite")).Hide();
            EmitSignal(nameof(PickedUp), PowerupType.ToString());
            await ToSignal(sound, "finished");
            QueueFree();
        }
    }

    public void _OnScreenExit()
    {
        GD.Print("Freeing ", Name);
        QueueFree();
    }
}
