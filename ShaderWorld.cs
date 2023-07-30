using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.Arm;

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
  public float waveRiderMaxRotationSpeedMin = (float)Math.PI / 8.0f;

  [Export]
  public float waveRiderMaxRotationSpeedMax = (float)Math.PI / 4.0f;

  [Export]
  public float targetProgressModifierMin = 100f;

  [Export]
  public float targetProgressModifierMax = 300f;

  // TODO: Should be able to search for these instead of manually generating the
  //         list, grouping targets/paths under a single node might make it easier
  // [Export]
  // public Array<Sprite2D> targets = new();

  // [Export]
  // public Array<PathFollow2D> targetPathFollows = new();

  [Export]
  public Array<pathFollower> targets = new();

  [Export]
  public float scaleMin = 0.1f;

  [Export]
  public float scaleMax = 0.5f;

  [Export]
  public bool vBarrierEnabled = true;

  public bool scrollBackground = false;

  public bool scrollBackgroundJustFired = false;

  [Export]
  public float scrollBackgroundAmount = 5.0f;

  [Export]
  public bool scrollBackgroundEnabled = true;

  public RenderingDevice? renderingDevice = null;

  public pathFollower GetRandomTarget(string riderName)
  {
    var result = targets[(int)Math.Floor(rng.RandfRange(0f, targets.Count - 1))];
    GD.Print($"Setting target for {riderName} to: {result.Name}");
    return result;
  }

  public void SpawnWaveRider()
  {
    if (GetNode<WaveRider>("waveRider").Duplicate() is WaveRider rider)
    {
      // rider.target = GetNode<Sprite2D>("targetPath/targetPathFollow/target");
      rider.maxRotationSpeed = (float)Math.PI / 4.0f;

      // Random position in a 3240/1240 area (-100px on each side)
      rider.Position = new Vector2(rng.RandfRange(100f, 3340f), rng.RandfRange(100f, 1340f));

      // Random rotation
      rider.Rotation = rng.RandfRange(0f, (float)Math.PI * 2.0f);

      // Random value ranges for various wave rider behaviors
      rider.newThrustMin = rng.RandfRange(waveRiderMinThrustRangeMin, waveRiderMinThrustRangeMax);
      rider.newThrustMax = rng.RandfRange(waveRiderMaxThrustRangeMin, waveRiderMaxThrustRangeMax);
      rider.maxRotationSpeed = rng.RandfRange(waveRiderMaxRotationSpeedMin, waveRiderMaxRotationSpeedMax);

      // Pick a random starting target
      rider.target = GetRandomTarget(rider.Name);

      // Just setting the scale won't work
      // rider.Scale *= rng.RandfRange(scaleMin, scaleMax);
      // var collision = rider.GetNode<CollisionShape2D>("waveRiderCollisionBox");
      // var sprite = rider.GetNode<Sprite2D>("waveRiderSprite");

      var scale = rng.RandfRange(scaleMin, scaleMax);

      rider.GetNode<CollisionShape2D>("waveRiderCollisionBox").Scale = new Vector2(scale, scale);
      rider.GetNode<Sprite2D>("waveRiderSprite").Scale = new Vector2(scale, scale);

      // collision.Scale = new Vector2(scale, scale);
      // sprite.Scale = new Vector2(scale, scale);

      AddChild(rider);
      rider.Show();

      waveRiders.Add(rider);
    }
  }

  public void DespawnWaveRider()
  {
    if (waveRiders.Count > 0)
    {
      var rider = waveRiders[0];
      waveRiders.RemoveAt(0);
      rider.QueueFree();
    }
  }

  public void RandomizeTargetsParameters()
  {

    foreach (var rider in waveRiders)
    {
      rider.target = GetRandomTarget(rider.Name);
    }

    foreach (var target in targets)
    {
      target.targetProgressModifier = rng.RandfRange(-targetProgressModifier, targetProgressModifier);
    }
  }

  public override void _Ready()
  {
    renderingDevice = RenderingServer.CreateLocalRenderingDevice();

    var shaderFile = GD.Load<RDShaderFile>("res://compute_shader_simulation.glsl");
    var shaderBytecode = shaderFile.GetSpirV();
    var shader = renderingDevice.ShaderCreateFromSpirV(shaderBytecode);


    // Prepare our data. We use floats in the shader, so we need 32 bit.
    var input = new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
    var inputBytes = new byte[input.Length * sizeof(float)];
    Buffer.BlockCopy(input, 0, inputBytes, 0, inputBytes.Length);

    // Create a storage buffer that can hold our float values.
    // Each float has 4 bytes (32 bit) so 10 x 4 = 40 bytes
    var buffer = renderingDevice.StorageBufferCreate((uint)inputBytes.Length, inputBytes);

    // Create a uniform to assign the buffer to the rendering device
    var uniform = new RDUniform
    {
      UniformType = RenderingDevice.UniformType.StorageBuffer,
      Binding = 0
    };
    uniform.AddId(buffer);
    var uniformSet = renderingDevice.UniformSetCreate(new Array<RDUniform> { uniform }, shader, 0);

    // Create a compute pipeline
    var pipeline = renderingDevice.ComputePipelineCreate(shader);
    var computeList = renderingDevice.ComputeListBegin();
    renderingDevice.ComputeListBindComputePipeline(computeList, pipeline);
    renderingDevice.ComputeListBindUniformSet(computeList, uniformSet, 0);
    renderingDevice.ComputeListDispatch(computeList, xGroups: 5, yGroups: 1, zGroups: 1);
    renderingDevice.ComputeListEnd();

    // Submit to GPU and wait for sync
    renderingDevice.Submit();
    renderingDevice.Sync();

    // Read back the data from the buffers
    var outputBytes = renderingDevice.BufferGetData(buffer);
    var output = new float[input.Length];
    Buffer.BlockCopy(outputBytes, 0, output, 0, outputBytes.Length);
    GD.Print("Input: ", string.Join(", ", input));
    GD.Print("Output: ", string.Join(", ", output));

    // GetNode<RigidBody2D>("RigidBody2D").ApplyImpulse(impulseVal);

    // targetPathFollow = GetNode<PathFollow2D>("targetPath/targetPathFollow");

    // Find all the targets and add them to the list
    // TODO: Couldn't get this to work, manually adding the nodes for now instead
    // var nodes = GetTree().GetNodesInGroup("targets");

    // foreach (var node in nodes)
    // {
    //   if (node is PathFollower target)
    //   {
    //     targets.Add(target);
    //   }
    // }

    foreach (var target in targets)
    {
      target.ProgressRatio = rng.RandfRange(0f, 1f);
      // TODO: Speed needs to be moved from ShaderWorld to the PathFollow2D nodes
      //         Then we can randomize the speed here
    }

    RandomizeTargetsParameters();

    SpawnWaveRider();
  }

  public string VecFloor(Vector2 vec)
  {
    return $"({Math.Floor(vec.X)}, {Math.Floor(vec.X)})";
  }

  public override void _Process(double delta)
  {
    Engine.MaxFps = maxFPS;

    // if (targetPathFollow != null)
    //   targetPathFollow.Progress += targetProgressModifier * (float)delta;

    foreach (var target in targets)
    {
      // TODO: Once progress rate is moved to the PathFollow2D node this
      //         should use it instead of `targetProgressModifier`
      target.Progress += target.targetProgressModifier * (float)delta;
    }

    var textureNode = GetNode<TextureRect>("SimulationContainer/SimulationViewport/Simulation");

    var fpsLimitText = maxFPS == 0 ? "Unlimited" : maxFPS.ToString();

    GetNode<Label>("Label").Text =
      $"FPS and simulation run rate (current / limit): {Engine.GetFramesPerSecond()} / {fpsLimitText}\n" +
      $"Game running? (ENTER): {game_running}\n" +
      $"Animation running?: {animation_running}\n" +
      $"Thrust generates chaos? (E) : {ballEnabled}\n" +
      $"1-way barrier on? (V) : {vBarrierEnabled}\n" +
      $"Scroll background? (Z): {scrollBackgroundEnabled}\n" +
      $"Wave riders: {waveRiders.Count}\n";

    // $"ship_speed: {Math.Floor(waveRiders[0].LinearVelocity.Length())}\n" +
    // $"ship_position: {VecFloor(waveRiders[0].GlobalPosition)}\n" +
    // $"ship_thrust: {waveRiders[0].currentThrust.Length()}\n" +
    // $"ship_thrust_visual: {waveRiders[0].currentThrustVisual}\n" +
    // $"ship_distance_from_target: {Math.Floor(waveRiders[0].distanceFromTarget)}\n" +
    // $"ship_new_thrust_pct: {waveRiders[0].newThrustPct}\n" +
    // $"ship_new_thrust_amount: {waveRiders[0].newThrustAmount}\n" +
    // $"ball_radius: {Math.Floor(ballRadius)}\n";

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

      // This shouldn't be needed anymore
      // ((ShaderMaterial)textureNode.Material).SetShaderParameter("global_transform", GetGlobalTransform());

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
      var ballPositionsAndSizes = new Array<Vector3>();

      foreach (var rider in waveRiders)
      {
        var rawScale = rider.GetNode<Sprite2D>("waveRiderSprite").Scale;

        var riderRadius = rawScale.X + rawScale.Y / 2f
          * (ballRadiusMin + (rider.newThrustPct * (ballRadiusMax - ballRadiusMin)));

        ballPositionsAndSizes.Add(new Vector3(rider.GlobalPosition.X, rider.GlobalPosition.Y, riderRadius));
      }

      ((ShaderMaterial)textureNode.Material).SetShaderParameter("ball_positions_and_sizes", ballPositionsAndSizes);
      ((ShaderMaterial)textureNode.Material).SetShaderParameter("ball_enabled", ballEnabled);

      ((ShaderMaterial)textureNode.Material).SetShaderParameter("scroll_background_amount", scrollBackgroundAmount);
      ((ShaderMaterial)textureNode.Material).SetShaderParameter("scroll_enabled", scrollBackgroundEnabled);

      // Always pass this value on to the shader
      // ((ShaderMaterial)textureNode.Material).SetShaderParameter("scroll_background", scrollBackground);
      // ((ShaderMaterial)textureNode.Material).SetShaderParameter("scroll_background", true);

      // Once a true has been passed we want to pass false until the next time one comes along
      if (scrollBackground)
      {
        QueueRedraw();
      }

      if (scrollBackgroundJustFired)
      {
        QueueRedraw();
        scrollBackgroundJustFired = false;
      }


      // These should all be able to go now:
      // ((ShaderMaterial)textureNode.Material).SetShaderParameter("ball_radius", ballRadius);
      // // ((ShaderMaterial)textureNode.Material).SetShaderParameter("ball_center", GetNode<RigidBody2D>("RigidBody2D").GlobalPosition);
      // ((ShaderMaterial)textureNode.Material).SetShaderParameter("ball_center", waveRiders[0].GlobalPosition);
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

    if (@event.IsActionPressed("despawn_wave_rider"))
    {
      DespawnWaveRider();
    }

    if (@event.IsActionPressed("vbarrier_toggle"))
    {
      vBarrierEnabled = !vBarrierEnabled;
      ((ShaderMaterial)GetNode<TextureRect>("SimulationContainer/SimulationViewport/Simulation").Material)
      .SetShaderParameter("v_barrier_enabled", vBarrierEnabled);
    }

    if (@event.IsActionPressed("spawn_multiple_wave_riders"))
    {
      for (int i = 0; i < 9; i++)
      {
        SpawnWaveRider();
      }
    }

    if (@event.IsActionPressed("despawn_multiple_wave_riders"))
    {
      if (waveRiders.Count > 0)
      {
        for (int i = 0; i < Mathf.Min(9, waveRiders.Count - 1); i++)
        {
          DespawnWaveRider();
        }
      }
      else
      {
        GD.Print("No wave riders to despawn");
      }
    }

    if (@event.IsActionPressed("toggle_scroll"))
    {
      scrollBackgroundEnabled = !scrollBackgroundEnabled;
      if (scrollBackgroundEnabled)
      {
        GetNode<Timer>("ScrollBackground").Start();
      }
      else
      {
        GetNode<Timer>("ScrollBackground").Stop();
      }
    }
  }

  // ChangeTargets timer calls this
  private void OnChangeTargetsTimeout()
  {
    RandomizeTargetsParameters();
  }

  // ScrollBackground timer calls this
  private void OnScrollBackgroundTimeout()
  {
    // ((ShaderMaterial)GetNode<Simulation>
    //   ("SimulationContainer/SimulationViewport/Simulation")
    //   .Material)
    // .SetShaderParameter("scroll_background", true);
    scrollBackground = true;
  }

  public override void _Draw()
  {

    if (game_running)
    {
      ((ShaderMaterial)GetNode<Simulation>
        ("SimulationContainer/SimulationViewport/Simulation")
        .Material)
      .SetShaderParameter("scroll_background", scrollBackground);

      if (scrollBackground)
      {
        scrollBackground = false;
        scrollBackgroundJustFired = true;
      }
    }

  }
}




