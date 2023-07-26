using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;


public partial class ShaderWorld : Control
{
  [Export]
  public ShaderMaterial? shaderMaterial;

  [Export]
  public CompressedTexture2D? startingImage;

  [Export]
  public Vector2 impulseVal = new(0.1f, 0.1f);

  [Export]
  public float intendedSpeed = 1500f;

  [Export]
  public float ballRadius = 140.0f;

  [Export]
  public float ballRadiusMin = 5f;

  [Export]
  public float ballRadiusMax = 20f;

  [Export]
  public Vector2 forceAdder = new(1.08f, 1.04f);

  [Export]
  public bool ballEnabled = false;

  [Export]
  public int maxFPS = 100;

  [Export]
  public float targetProgressModifier = 1.0f;

  public bool animation_running = false;

  public bool game_running = false;

  public float lowSpeedXCounter = 0f;
  public float lowSpeedXThreshhold = 1000f;
  public float lowSpeedX = 100f;

  public float lowSpeedYCounter = 0f;
  public float lowSpeedYThreshhold = 1000f;
  public float lowSpeedY = 100f;

  public bool resetRequested = false;
  public bool resetGameRunning = false;
  public bool resetFinished = false;

  public List<WaveRider> waveRiders = new();

  public PathFollow2D? targetPathFollow = null;

  public RandomNumberGenerator rng = new RandomNumberGenerator();

  [Export]
  public float waveRiderMinThrustRangeMin = 500f;

  [Export]
  public float waveRiderMinThrustRangeMax = 1500f;

  [Export]
  public float waveRiderMaxThrustRangeMin = 3000f;

  [Export]
  public float waveRiderMaxThrustRangeMax = 5000f;

  [Export]
  public float waveRiderMaxRotationSpeedMin = (float)Math.PI / 4.0f;

  [Export]
  public float waveRiderMaxRotationSpeedMax = (float)Math.PI / 2.0f;

  public void SpawnWaveRider()
  {
    if (GetNode<WaveRider>("waveRider").Duplicate() is WaveRider rider)
    {
      rider.target = GetNode<Sprite2D>("targetPath/targetPathFollow/target");
      rider.maxRotationSpeed = (float)Math.PI / 4.0f;

      // Random position in a 3240/1240 area (-100px on each side)
      rider.Position = new Vector2(rng.RandfRange(100f, 3340f), rng.RandfRange(100f, 1340f));

      // Random rotation
      rider.Rotation = rng.RandfRange(0f, (float)Math.PI * 2.0f);

      // Random value ranges for various wave rider behaviors
      rider.newThrustMin = rng.RandfRange(waveRiderMinThrustRangeMin, waveRiderMinThrustRangeMax);
      rider.newThrustMax = rng.RandfRange(waveRiderMaxThrustRangeMin, waveRiderMaxThrustRangeMax);
      rider.maxRotationSpeed = rng.RandfRange(waveRiderMaxRotationSpeedMin, waveRiderMaxRotationSpeedMax);


      AddChild(rider);
      rider.Show();

      waveRiders.Add(rider);
    }
  }

  public override void _Ready()
  {
    // GetNode<RigidBody2D>("RigidBody2D").ApplyImpulse(impulseVal);

    targetPathFollow = GetNode<PathFollow2D>("targetPath/targetPathFollow");

    SpawnWaveRider();
  }

  public string VecFloor(Vector2 vec)
  {
    return $"({Math.Floor(vec.X)}, {Math.Floor(vec.X)})";
  }

  public override void _Process(double delta)
  {
    Engine.MaxFps = maxFPS;

    if (targetPathFollow != null)
      targetPathFollow.Progress += targetProgressModifier * (float)delta;

    var textureNode = GetNode<TextureRect>("SimulationContainer/SimulationViewport/Simulation");

    GetNode<Label>("Label").Text =
      $"game_running: {game_running}\n" +
      $"animation_running: {animation_running}\n" +
      $"FPS: {Engine.GetFramesPerSecond()}\n" +
      $"ship_speed: {Math.Floor(waveRiders[0].LinearVelocity.Length())}\n" +
      $"ship_position: {VecFloor(waveRiders[0].GlobalPosition)}\n" +
      $"ship_thrust: {waveRiders[0].currentThrust.Length()}\n" +
      $"ship_thrust_visual: {waveRiders[0].currentThrustVisual}\n" +
      $"ship_distance_from_target: {Math.Floor(waveRiders[0].distanceFromTarget)}\n" +
      $"ship_new_thrust_pct: {waveRiders[0].newThrustPct}\n" +
      $"ship_new_thrust_amount: {waveRiders[0].newThrustAmount}\n" +
      $"ball_radius: {Math.Floor(ballRadius)}\n";

    if (resetRequested && !resetFinished)
    {
      if (game_running || animation_running)
      {
        game_running = false;
      }
      else
      {
        textureNode.Texture = startingImage;
        resetFinished = true;
      }
    }
    else if (resetRequested && resetFinished)
    {
      resetRequested = false;
      resetFinished = false;

      game_running = resetGameRunning;
      resetGameRunning = false;
    }

    if (game_running && !animation_running)
    {
      textureNode.Material = shaderMaterial;
      ((ShaderMaterial)textureNode.Material).SetShaderParameter("global_transform", GetGlobalTransform());

      animation_running = true;
    }

    if (!game_running && animation_running)
    {
      textureNode.Texture = GetNode<SubViewport>("SimulationContainer/SimulationViewport").GetTexture();
      textureNode.Material = null;
      animation_running = false;
    }

    if (game_running)
    {
      ballRadius = ballRadiusMin + (waveRiders[0].newThrustPct * (ballRadiusMax - ballRadiusMin));

      var ballPositionsAndSizes = new Array<Vector3>();

      foreach (var rider in waveRiders)
      {
        ballPositionsAndSizes.Add(new Vector3(rider.GlobalPosition.X, rider.GlobalPosition.Y, ballRadius));
      }

      ((ShaderMaterial)textureNode.Material).SetShaderParameter("ball_positions_and_sizes", ballPositionsAndSizes);

      ((ShaderMaterial)textureNode.Material).SetShaderParameter("ball_radius", ballRadius);
      // ((ShaderMaterial)textureNode.Material).SetShaderParameter("ball_center", GetNode<RigidBody2D>("RigidBody2D").GlobalPosition);
      ((ShaderMaterial)textureNode.Material).SetShaderParameter("ball_center", waveRiders[0].GlobalPosition);
      ((ShaderMaterial)textureNode.Material).SetShaderParameter("ball_enabled", ballEnabled);
    }
  }

  public override void _PhysicsProcess(double delta)
  {
    // var ball = GetNode<RigidBody2D>("RigidBody2D");

    // if (ball.LinearVelocity.Length() < intendedSpeed)
    // {
    //   // TODO: Maybe add a check on X and Y, if either is too small apply a large bump in some direction
    //   var impulse = ball.LinearVelocity.Normalized() * forceAdder * (float)delta;

    //   lowSpeedXCounter = ball.LinearVelocity.X < lowSpeedX ? lowSpeedXCounter + (float)delta : 0f;
    //   lowSpeedYCounter = ball.LinearVelocity.Y < lowSpeedY ? lowSpeedYCounter + (float)delta : 0f;

    //   if (lowSpeedXCounter > lowSpeedXThreshhold)
    //   {
    //     impulse.X = 1.0f;
    //   }

    //   if (lowSpeedYCounter > lowSpeedYThreshhold)
    //   {
    //     impulse.Y = 1.0f;
    //   }

    //   // if (impulse.X < 1.0f)
    //   // {
    //   //   lowSpeedXCounter += (float)delta;
    //   // }
    //   // else
    //   // {
    //   //   lowSpeedXCounter = 0f;
    //   // }

    //   // if (impulse.X < 1.0f) impulse.X = 1.0f;
    //   // if (impulse.Y < 1.0f) impulse.Y = 1.0f;
    //   ball.ApplyImpulse(impulse);
    // }
  }

  public override void _UnhandledInput(InputEvent @event)
  {
    if (@event.IsActionPressed("ui_accept"))
    {
      game_running = !game_running;
    }

    if (@event.IsActionPressed("ball_emitter_toggle"))
    {
      ballEnabled = !ballEnabled;
    }

    if (@event.IsActionPressed("reset_simulation"))
    {
      resetRequested = true;
      resetGameRunning = game_running;
      // var textureNode = GetNode<TextureRect>("SimulationContainer/SimulationViewport/Simulation");
      // textureNode.Texture = startingImage;
      GD.Print("Starting simulation reset...");
    }

    if (@event.IsActionPressed("spawn_wave_rider"))
    {
      SpawnWaveRider();
    }
  }
}
