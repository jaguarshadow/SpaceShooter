
using Godot;
using Array = Godot.Collections.Array;
using System;
using System.Collections.Generic;
using System.Linq;

public class Main : Node2D
{
    // Declare member variables here. Examples:
    private AudioStreamPlayer musicPlayer;
    private int CameraSpeed;
    private Player player;
    private Sprite background;
    private RandomNumberGenerator rng = new RandomNumberGenerator();
    private int level_num;
    private int wave_num;
    private List<Enemy> enemiesOnScreen = new List<Enemy>();
    private int score;

    [Signal]
    public delegate void GameStarted();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        rng.Randomize();
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
        Array EnemyFormations = GetNode(string.Format("Level/Gameplay/EnemyFormations/Level{0}", level_num)).GetChildren();
        TileMap formation = (TileMap)EnemyFormations[rng.RandiRange(0, EnemyFormations.Count - 1)];
        GD.Print(formation.GetUsedCells());
        for (int i = 0; i < formation.GetUsedCells().Count; i++)
        {
            Enemy e = (Enemy)(GD.Load<PackedScene>("res://enemy/Enemy.tscn")).Instance();
            enemiesOnScreen.Add(e);
            ((Sprite)e.GetNode("CollisionShape2D/Sprite")).Frame = rng.RandiRange(0,5);
            e.Destination = formation.MapToWorld((Vector2)formation.GetUsedCells()[i]) + new Vector2(formation.CellSize/2);
            GetNode("Level/Gameplay").AddChild(e);
            await ToSignal(GetTree().CreateTimer(0.2f), "timeout");
        }
    }

    public void SpawnPowerup(Vector2 pos)
    {
        Powerup pu = (Powerup)GD.Load<PackedScene>("res://powerup/Powerup.tscn").Instance();
        pu.GlobalPosition = pos;
        pu.Start();
        GD.Print("Type: ", pu.PowerupType);
        GetNode("Level/Gameplay").AddChild(pu);
        
    }

    public void ChangeScore(int score)
    {
        ((Label)GetNode("Level/Gameplay/UI/MarginContainerLeft/Score")).Text = "Score: " + score;
    }

    public void ChangeHealth(int health)
    {
        ((ProgressBar)GetNode("Level/Gameplay/UI/MarginContainerRight/HealthContainer/HealthBar")).Value = health;
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
        score = 0;
        ChangeScore(score);
        background.Texture = (Texture)ResourceLoader.Load(string.Format("res://assets/backgrounds/BlueNebula{0}.png", rng.RandiRange(1, 7)));
        musicPlayer.Stream = (AudioStream)ResourceLoader.Load("res://assets/music/loading.wav");
        musicPlayer.Play();
        CameraSpeed = 200;
        Sprite chosenShip = ((Title)GetNode("Title")).ChosenShip;
        player.PlayerSprite.Set("region_rect", new Rect2(-1, chosenShip.Frame * 8, 26, 8));
        level_num = 1;
        wave_num = 1;
        ((Control)GetNode("Title/Control")).Hide();
        ((Control)GetNode("Level/Gameplay")).Show();
        EmitSignal(nameof(GameStarted));
        await ToSignal(GetTree().CreateTimer(2.0f), "timeout");
        SpawnEnemies();
    }

    public void _OnEnemyKilled(Enemy e)
    {
        score += 10;
        ChangeScore(score);
        if (rng.RandiRange(1, 100) < 16)
        {
            GD.Print("spawning powerup...");
            SpawnPowerup(e.GlobalPosition);
        }
        enemiesOnScreen.Remove(e);
        if (enemiesOnScreen.Count > 0) return;

        wave_num += 1;
        if (wave_num == 3)
        {
            level_num += 1;
            wave_num = 1;
        }
        if (level_num > 5) level_num = 5;
        SpawnEnemies();
    }

    public async void _OnPlayerDead()
    {
        await ToSignal(GetTree().CreateTimer(3f), "timeout");
        foreach (Enemy e in enemiesOnScreen)
        {
            e.QueueFree();
        }
        enemiesOnScreen.Clear();
        ((Control)GetNode("Level/Gameplay")).Hide();
        background.Texture = (Texture)ResourceLoader.Load("res://assets/backgrounds/BlueNebula7.png");
        musicPlayer.Stream = (AudioStream)ResourceLoader.Load("res://assets/music/menu.wav");
        musicPlayer.Play();
        CameraSpeed = 50;
        ((Control)GetNode("Title/Control")).Show();
    }
}
