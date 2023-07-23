using Godot;
using System;

public partial class ShaderWorld : Control
{
  [Export]
  public ShaderMaterial? shaderMaterial;

  [Export]
  public Vector2 impulseVal = new(0.1f, 0.1f);

  [Export]
  public float intendedSpeed = 1500f;

  [Export]
  public float ballRadius = 140.0f;

  [Export]
  public Vector2 forceAdder = new(1.08f, 1.04f);

  public bool animation_running = false;

  public bool game_running = false;

  public override void _Ready()
  {
    GetNode<RigidBody2D>("RigidBody2D").ApplyImpulse(impulseVal);
  }

  public override void _Process(double delta)
  {
    var textureNode = GetNode<TextureRect>("SubViewportContainer/SubViewport/TextureRect");

    GetNode<Label>("Label").Text = $" game_running: {game_running} \n  animation_running: {animation_running}";

    if (game_running && !animation_running)
    {
      textureNode.Material = shaderMaterial;
      ((ShaderMaterial)textureNode.Material).SetShaderParameter("global_transform", GetGlobalTransform());

      animation_running = true;
    }

    if (!game_running && animation_running)
    {
      textureNode.Texture = GetNode<SubViewport>("SubViewportContainer/SubViewport").GetTexture();
      textureNode.Material = null;
      animation_running = false;
    }

    if (game_running)
    {
      ((ShaderMaterial)textureNode.Material).SetShaderParameter("ball_radius", ballRadius);
      ((ShaderMaterial)textureNode.Material).SetShaderParameter("ball_center", GetNode<RigidBody2D>("RigidBody2D").GlobalPosition);
    }
  }

  public override void _PhysicsProcess(double delta)
  {
    var ball = GetNode<RigidBody2D>("RigidBody2D");

    if (ball.LinearVelocity.Length() < intendedSpeed)
    {
      ball.ApplyImpulse(ball.LinearVelocity.Normalized() * forceAdder * (float)delta);
    }
  }

  public override void _UnhandledInput(InputEvent @event)
  {
    if (@event.IsActionPressed("ui_accept"))
    {
      game_running = !game_running;
    }
  }
}
