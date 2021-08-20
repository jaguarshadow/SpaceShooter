using Godot;
using System;

public class Title : CanvasLayer
{
    // Declare member variables here.
    private AudioStreamPlayer musicPlayer;
    private int CameraSpeed;
    private Player player;
    private Sprite background;
    private RandomNumberGenerator rng = new RandomNumberGenerator();

    public Sprite ChosenShip { get; set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        ChosenShip = (Sprite)GetNode("Control/ShipChooser/Shipbox/SelectedShip");
        background = (Sprite)GetNode("/root/Main/BackgroundCanvas/ParallaxBackground/ParallaxLayer/Space");
        musicPlayer = (AudioStreamPlayer)GetNode("MusicPlayer");
        player = ((Player)GetNode("/root/Main/Level/Gameplay/PlayerNode/Player"));
    }

    public void _OnLeftPressed()
    {
        if (ChosenShip.Frame == 0) ChosenShip.Frame = 4;
        else ChosenShip.Frame -= 1;
    }
    
    public void _OnRightPressed()
    {
        if (ChosenShip.Frame == 4) ChosenShip.Frame = 0;
        else ChosenShip.Frame += 1;
    }
}
