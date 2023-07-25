using Godot;
using System;

public partial class WaveRider : RigidBody2D
{
  [Export]
  public Sprite2D? target = null;

  [Export]
  public float maxRotationSpeed = (float)Math.PI / 4.0f;

  [Export]
  public float thrust = 100f;

  const float pi = (float)Math.PI;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta)
  {
  }

  public override void _PhysicsProcess(double delta)
  {
    if (target == null)
    {
      GD.PrintErr("WaveRider: target is null");
      return;
    }

    if (!target.IsInsideTree())
    {
      GD.PrintErr("WaveRider: target is not in tree");
      return;
    }

    var force = new Vector2(0.0f, -thrust).Rotated(Rotation) * (float)delta;

    var directionToTarget = (target.GlobalPosition - GlobalPosition).Normalized();

    var velocityInTargetDirection = LinearVelocity.Dot(directionToTarget);

    if (velocityInTargetDirection < -1.0f)
    {
      // force *= 2.0f;
      force *= -velocityInTargetDirection * 5.0f * (float)delta;
    }

    ApplyCentralForce(force);


    // Angle to target
    var targetAngle = directionToTarget.Angle() + pi / 2.0f;

    // Angle difference to target
    var angleDifference = targetAngle - Rotation;

    // Get smallest angle difference
    angleDifference = ((angleDifference + pi) % (pi * 2.0f)) - pi;

    // Clamp angle difference to max rotation speed
    angleDifference = Mathf.Clamp(
      angleDifference,
      -maxRotationSpeed * (float)delta,
      maxRotationSpeed * (float)delta);

    // Rotate
    Rotation += angleDifference;
  }
}
