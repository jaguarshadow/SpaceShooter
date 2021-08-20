
using Godot;
using System;
using System.Collections.Generic;

public class Main : Node2D
{
    // Declare member variables here. Examples:
    private AudioStreamPlayer musicPlayer;
    private int CameraSpeed;
    private Player player;
    private Sprite background;
    private RandomNumberGenerator rng = new RandomNumberGenerator();
    private int level_num;
    private int enemiesOnScreen = 0;

    [Signal]
    public delegate void GameStarted();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        rng.Randomize();
        level_num = 1;
        background = (Sprite)GetNode("BackgroundCanvas/ParallaxBackground/ParallaxLayer/Space");
        musicPlayer = (AudioStreamPlayer)GetNode("MusicPlayer");
        player = ((Player)GetNode("Level/Gameplay/PlayerNode/Player"));
        Connect(nameof(GameStarted), player, "_OnGameStarted");
        CameraSpeed = 50;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(float delta)
    {
        Camera2D camera = (Camera2D)GetNode("Camera2D");
        camera.Position += (new Vector2(0, -1) * CameraSpeed) * delta;
    }

    private async void SpawnEnemies()
    {
        // Spawn enemies and send them to a formation! 
        Control EnemyContainer = (Control)GetNode("Level/Gameplay/EnemyContainer");
        TileMap formation;
        if (level_num == 1) formation = (TileMap)EnemyContainer.GetNode("Formation3");
        else formation = (TileMap)EnemyContainer.GetNode("Formation2");
        GD.Print(formation.GetUsedCells());
        for (int i = 0; i < formation.GetUsedCells().Count; i++)
        {
            Enemy e = (Enemy)(GD.Load<PackedScene>("res://enemy/Enemy.tscn")).Instance();
            enemiesOnScreen += 1;
            if (level_num == 2) ((Sprite)e.GetNode("CollisionShape2D/Sprite")).Frame = 2;
            e.Destination = formation.MapToWorld((Vector2)formation.GetUsedCells()[i]) + new Vector2(formation.CellSize/2);
            EnemyContainer.AddChild(e);
            await ToSignal(GetTree().CreateTimer(0.2f), "timeout");
        }
    }

    // Event Handlers

    public void _OnPlayerShoot(PackedScene bullet, Vector2 pos)
    {
        Bullet b = (Bullet)bullet.Instance();
        b.Target = "enemy";
        b.Start(pos, Vector2.Up);
        GetNode("Level/Gameplay").AddChild(b);
        b.PlaySound();
    }

    public void _OnEnemyShoot(PackedScene bullet, Vector2 pos)
    {
        Bullet b = (Bullet)bullet.Instance();
        b.Target = "player";
        b.Start(pos, Vector2.Down);
        GetNode("Level/Gameplay").AddChild(b);
        b.PlaySound();
    }

    public async void _OnPlayButtonPressed()
    {
        background.Texture = (Texture)ResourceLoader.Load(string.Format("res://assets/backgrounds/BlueNebula{0}.png", rng.RandiRange(1, 7)));
        musicPlayer.Stream = (AudioStream)ResourceLoader.Load("res://assets/music/loading.wav");
        musicPlayer.Play();
        CameraSpeed = 200;
        Sprite chosenShip = ((Title)GetNode("Title")).ChosenShip;
        if (chosenShip.Frame > 0)
        {
            player.PlayerSprite.Set("region_rect", new Rect2(-1, chosenShip.Frame * 8, 26, 8));
        }
        ((Control)GetNode("Title/Control")).Hide();
        ((Control)GetNode("Level/Gameplay")).Show();
        EmitSignal(nameof(GameStarted));
        await ToSignal(GetTree().CreateTimer(2.0f), "timeout");
        SpawnEnemies();
    }

    public void _OnEnemyKilled()
    {
        enemiesOnScreen -= 1;
        if (enemiesOnScreen > 0) return;
        level_num = 2;
        SpawnEnemies();
    }
}
