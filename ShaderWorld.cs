using Godot;
using System;

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
  public Vector2 forceAdder = new(1.08f, 1.04f);

  [Export]
  public bool ballEnabled = false;

  public bool animation_running = false;

  public bool game_running = false;

  public float lowSpeedXCounter = 0f;
  public float lowSpeedXThreshhold = 1000f;
  public float lowSpeedX = 100f;

  public float lowSpeedYCounter = 0f;
  public float lowSpeedYThreshhold = 1000f;
  public float lowSpeedY = 100f;

  public bool resetRequested = false;
  public bool resetFinished = false;

  public override void _Ready()
  {
    GetNode<RigidBody2D>("RigidBody2D").ApplyImpulse(impulseVal);
  }

  public override void _Process(double delta)
  {
    var textureNode = GetNode<TextureRect>("SimulationContainer/SimulationViewport/Simulation");

    GetNode<Label>("Label").Text =
      $"game_running: {game_running}\n" +
      $"animation_running: {animation_running}\n" +
      $"ball_speed: {GetNode<RigidBody2D>("RigidBody2D").LinearVelocity.Length()}\n" +
      $"ball_position: {GetNode<RigidBody2D>("RigidBody2D").GlobalPosition}\n" +
      $"ball_enabled: {ballEnabled}\n" +
      $"low_speed_x_counter: {lowSpeedXCounter}\n" +
      $"low_speed_y_counter: {lowSpeedYCounter}\n";

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
      game_running = true;
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
      ((ShaderMaterial)textureNode.Material).SetShaderParameter("ball_radius", ballRadius);
      ((ShaderMaterial)textureNode.Material).SetShaderParameter("ball_center", GetNode<RigidBody2D>("RigidBody2D").GlobalPosition);
      ((ShaderMaterial)textureNode.Material).SetShaderParameter("ball_enabled", ballEnabled);
    }
  }

  public override void _PhysicsProcess(double delta)
  {
    var ball = GetNode<RigidBody2D>("RigidBody2D");

    if (ball.LinearVelocity.Length() < intendedSpeed)
    {
      // TODO: Maybe add a check on X and Y, if either is too small apply a large bump in some direction
      var impulse = ball.LinearVelocity.Normalized() * forceAdder * (float)delta;

      lowSpeedXCounter = ball.LinearVelocity.X < lowSpeedX ? lowSpeedXCounter + (float)delta : 0f;
      lowSpeedYCounter = ball.LinearVelocity.Y < lowSpeedY ? lowSpeedYCounter + (float)delta : 0f;

      if (lowSpeedXCounter > lowSpeedXThreshhold)
      {
        impulse.X = 1.0f;
      }

      if (lowSpeedYCounter > lowSpeedYThreshhold)
      {
        impulse.Y = 1.0f;
      }

      // if (impulse.X < 1.0f)
      // {
      //   lowSpeedXCounter += (float)delta;
      // }
      // else
      // {
      //   lowSpeedXCounter = 0f;
      // }

      // if (impulse.X < 1.0f) impulse.X = 1.0f;
      // if (impulse.Y < 1.0f) impulse.Y = 1.0f;
      ball.ApplyImpulse(impulse);
    }
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
      // var textureNode = GetNode<TextureRect>("SimulationContainer/SimulationViewport/Simulation");
      // textureNode.Texture = startingImage;
      GD.Print("Starting simulation reset...");
    }
  }
}
