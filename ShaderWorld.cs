using Godot;
using System;

public partial class ShaderWorld : Control
{
  [Export]
  public ShaderMaterial shaderMaterial;

  public bool animation_running = false;

  public bool game_running = false;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
	// GetNode<SubViewport>("SubViewportContainer/SubViewport").RenderTargetUpdateMode = SubViewport.UpdateMode.Always;
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta)
  {
	GetNode<Label>("Label").Text = $" game_running: {game_running} \n  animation_running: {animation_running}";

	if (game_running && !animation_running)
	{
	  var node = GetNode<TextureRect>("SubViewportContainer/SubViewport/TextureRect");
	  node.Material = shaderMaterial;
	  ((ShaderMaterial)node.Material).SetShaderParameter("global_transform", GetGlobalTransform());

	  animation_running = true;
	}

	if (!game_running && animation_running)
	{
	  GetNode<TextureRect>("SubViewportContainer/SubViewport/TextureRect").Texture = GetNode<SubViewport>("SubViewportContainer/SubViewport").GetTexture();
	  GetNode<TextureRect>("SubViewportContainer/SubViewport/TextureRect").Material = null;
	  animation_running = false;
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
