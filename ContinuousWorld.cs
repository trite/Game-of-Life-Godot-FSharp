using Godot;
using Godot.Collections;


public partial class ContinuousWorld : Control
{
  [Export]
  public ShaderMaterial? shaderMaterial;

  public enum SimulationState
  {
    RunRequested,
    Running,
    PauseRequested,
    Paused
  }

  [Export]
  public Array<float> kernel = new() {
    0f, 0f, 1f, 1f, 1f, 0f, 0f,
    0f, 1f, 1f, 1f, 1f, 1f, 0f,
    1f, 1f, 1f, 1f, 1f, 1f, 1f,
    1f, 1f, 1f, 0f, 1f, 1f, 1f,
    1f, 1f, 1f, 1f, 1f, 1f, 1f,
    0f, 1f, 1f, 1f, 1f, 1f, 0f,
    0f, 0f, 1f, 1f, 1f, 0f, 0f
  };

  [Export]
  public int kernelSize = 7;

  [Export]
  public float alivenessFuncCurvePeak = 1f;

  [Export]
  public float alivenessFuncCurveCenter = 0.375f;

  [Export]
  public float alivenessFuncCurveStdev = 0.125f;

  public SimulationState simulationState = SimulationState.Paused;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    var kernelSum = 0f;

    foreach (var value in kernel)
    {
      kernelSum += value;
    }

    shaderMaterial?.SetShaderParameter("kernel", kernel);
    shaderMaterial?.SetShaderParameter("kernel_size", kernelSize);
    shaderMaterial?.SetShaderParameter("kernel_sum", kernelSum);
    shaderMaterial?.SetShaderParameter("aliveness_func_curve_peak", alivenessFuncCurvePeak);
    shaderMaterial?.SetShaderParameter("aliveness_func_curve_center", alivenessFuncCurveCenter);
    shaderMaterial?.SetShaderParameter("aliveness_func_curve_stdev", alivenessFuncCurveStdev);
  }

  public void RunRequested()
  {
    var textureNode = GetNode<TextureRect>("SimViewportContainer/SimViewport/Simulation");
    textureNode.Material = shaderMaterial;
    simulationState = SimulationState.Running;
  }

  public void PauseRequested()
  {
    var textureNode = GetNode<TextureRect>("SimViewportContainer/SimViewport/Simulation");
    textureNode.Texture = GetNode<SubViewport>("SimViewportContainer/SimViewport").GetTexture();
    textureNode.Material = null;
    simulationState = SimulationState.Paused;
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta)
  {
    if (simulationState == SimulationState.RunRequested)
    {
      RunRequested();
    }

    if (simulationState == SimulationState.PauseRequested)
    {
      PauseRequested();
    }

    GetNode<Label>("Label").Text =
      $"Simulation state: {simulationState}" +
      $"FPS: {Engine.GetFramesPerSecond()}";
  }

  public override void _UnhandledInput(InputEvent @event)
  {
    if (@event.IsActionPressed("ui_accept"))
    {
      simulationState = simulationState switch
      {
        SimulationState.Running => SimulationState.PauseRequested,
        SimulationState.Paused => SimulationState.RunRequested,
        _ => simulationState
      };
    }

    GD.Print("Toggling game state");
  }
}
