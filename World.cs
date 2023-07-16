using Godot;
using System;
using System.Linq;

public partial class World : Node2D
{
  // Timer timer;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    // FsLib.World.GetNode<Timer>("Timer");
    GetNode<Timer>("Timer").Timeout += () => FsLib.World._runSimulationStep(this);
    FsLib.World._ready(this);
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta)
  {
    FsLib.World._process(this, delta);
  }

  public override void _UnhandledInput(InputEvent @event)
  {
    FsLib.World._unhandledInput(this, @event);
  }

  // public override void OnTimerTimeout()
  // {
  // }
}
