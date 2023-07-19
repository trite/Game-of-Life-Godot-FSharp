using Godot;
using System;

public partial class ShaderWorld : Control
{
  // float blue_value = 0.0f;

  // [Export]
  // public float test = 0.0f;

  // [Export]
  // public ShaderMaterial shaderMaterial;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    // GetNode<Label>("Label").Text = "Hello World!";
    GetNode<SubViewport>("SubViewportContainer/SubViewport").RenderTargetUpdateMode = SubViewport.UpdateMode.Always;
    // var x = ShaderMaterial.
    // GetNode<TextureRect>("TextureRect").Material = 
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta)
  {
    // blue_value += 0.001f;
    // // Material.SetShaderParam("blue_value", blue_value);
    // // material
    // var node = GetNode<Sprite2D>("Sprite2D");
    // if (node != null && node.Material != null && node.Material is ShaderMaterial)
    // {
    //   (node.Material as ShaderMaterial).SetShaderParameter("blue", blue_value);

    //   GetNode<Label>("Label").Text = blue_value.ToString();
    // }
  }
}
