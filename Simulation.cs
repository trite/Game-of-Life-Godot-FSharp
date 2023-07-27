using Godot;
using System;

public partial class Simulation : TextureRect
{
  // [Export]
  // public bool scrollBackground = false;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta)
  {
    // if (Material is ShaderMaterial material)
    // {

    //   if (scrollBackground)
    //   {
    //     material.SetShaderParameter("scroll_background", true);
    //     scrollBackground = false;
    //   }
    //   else
    //   {
    //     material.SetShaderParameter("scroll_background", false);
    //   }

    //   // TODO: debugging only
    //   material.SetShaderParameter("scroll_background", true);
    // }
  }
}
