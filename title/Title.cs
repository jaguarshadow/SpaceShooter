using Godot;
using System;

public class Title : CanvasLayer
{
    public Sprite ChosenShip { get; set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        ChosenShip = (Sprite)GetNode("Control/ShipChooser/Shipbox/SelectedShip");
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
