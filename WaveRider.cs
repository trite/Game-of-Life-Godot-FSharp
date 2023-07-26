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

  [Export]
  public float maxExtraThrust = 150f;

  [Export]
  public float extraThrustMinDistance = 100f;

  [Export]
  public float extraThrustMaxDistance = 1000f;

  [Export]
  public Vector2 currentThrust = new(0f, 0f);

  // Needs to be > 0 and <= 1
  [Export]
  public float currentThrustVisual = 0.1f;

  [Export]
  public float currentThrustVisualMin = 0.1f;

  [Export]
  public float currentThrustVisualMax = 1.0f;

  [Export]
  public float extraThrustAngle = 0.5f;

  [Export]
  public float distanceFromTarget = 0.0f;


  // Reworking thruster stuff, the previous one has become terrible
  [Export]
  public float extraThrustPotentialNormalized = 0f;

  [Export]
  public bool extraThrustEnabled = false;

  // [Export]
  // public float extraThrustWhenUnderVelocity = 100f;

  const float pi = (float)Math.PI;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta)
  {
  }

  public float normalize(float min, float max, float value)
  {
    if (max <= min)
    {
      throw new Exception("can't normalize when max <= min");
    }

    if (value <= min)
    {
      return 0f;
    }

    if (value >= max)
    {
      return 1f;
    }

    return (value - min) / (max - min);
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

    var facingDirection = new Vector2(0f, -1f).Rotated(Rotation);

    var distanceToTarget = GlobalPosition.DistanceTo(target.GlobalPosition);

    // Just used for debugging for now
    distanceFromTarget = distanceToTarget;

    // if (velocityInTargetDirection < 1.0f)
    // {
    //   force *= 2.0f;
    //   // force *= -velocityInTargetDirection * 0.05f * (float)delta;
    // }

    extraThrustPotentialNormalized = normalize(
      extraThrustMinDistance,
      extraThrustMaxDistance,
      GlobalPosition.DistanceTo(target.GlobalPosition));

    extraThrustEnabled = facingDirection.AngleTo(directionToTarget) < extraThrustAngle;

    // TODO: Leaving off here

    if ((facingDirection.AngleTo(directionToTarget) < extraThrustAngle)
      && (distanceToTarget > extraThrustMinDistance))
    // && (velocityInTargetDirection < extraThrustWhenUnderVelocity))
    {
      var extraThrust = Math.Min(
        maxExtraThrust,
        (distanceToTarget - extraThrustMinDistance)
          / (extraThrustMaxDistance - extraThrustMinDistance)
          * maxExtraThrust);

      currentThrustVisual = normalize(
        extraThrustMinDistance,
        extraThrustMaxDistance,
        GlobalPosition.DistanceTo(target.GlobalPosition));


      // currentThrustVisual = (extraThrust - extraThrustMinDistance)
      //   / (extraThrustMaxDistance - extraThrustMinDistance);

      force += extraThrust * facingDirection.Normalized() * (float)delta;
    }

    currentThrustVisual = Mathf.Max(currentThrustVisual, currentThrustVisualMin);

    currentThrustVisual = force.Length() / (thrust + maxExtraThrust);

    ApplyCentralForce(force);
    currentThrust = force;


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
