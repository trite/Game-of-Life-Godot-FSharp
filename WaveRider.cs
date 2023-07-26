using Godot;
using System;

public partial class WaveRider : RigidBody2D
{
  [Export]
  public pathFollower target = null;

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


  // Ditch this rework of thrust, I've had time to consider a better way
  [Export]
  public float extraThrustPotentialNormalized = 0f;

  [Export]
  public bool extraThrustEnabled = false;

  // Begin new thrust stuff
  [Export]
  public float newThrustPct = 0f;

  [Export]
  public float newThrustMin = 100f;

  [Export]
  public float newThrustMax = 1000f;

  [Export]
  public float newExtraThrustMinDistance = 100f;

  [Export]
  public float newExtraThrustMaxDistance = 500f;

  [Export]
  public float newThrustAmount = 0f;


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
      // TODO: This is null a lot but everything still works?
      //         Not sure why yet.
      // GD.PrintErr("WaveRider: target is null");
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

    // TODO: Remove after rework
    extraThrustPotentialNormalized = normalize(
      extraThrustMinDistance,
      extraThrustMaxDistance,
      GlobalPosition.DistanceTo(target.GlobalPosition));

    // New with rework

    if ((facingDirection.AngleTo(directionToTarget) < extraThrustAngle)
      && (distanceToTarget > newExtraThrustMinDistance))
    {
      newThrustPct = normalize(
        newExtraThrustMinDistance,
        newExtraThrustMaxDistance,
        GlobalPosition.DistanceTo(target.GlobalPosition));
    }
    else
    {
      newThrustPct = 0f;
    }

    newThrustAmount = (newThrustPct * thrust) + newThrustMin;

    // extraThrustEnabled = facingDirection.AngleTo(directionToTarget) < extraThrustAngle;

    force = newThrustAmount * facingDirection.Normalized() * (float)delta;

    // currentThrustVisual = Mathf.Max(currentThrustVisual, currentThrustVisualMin);

    // currentThrustVisual = force.Length() / (thrust + maxExtraThrust);

    newThrustAmount = force.Length();
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
