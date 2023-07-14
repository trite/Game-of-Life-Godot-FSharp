using Godot;
using System;
using System.Linq;

public partial class World : Node2D
{
  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
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
}
