using Godot;
using System;
using System.Linq;

public partial class World : Node2D
{
	private Sprite2D? cell = null;
	private Color aliveColor;
	private Godot.Collections.Dictionary cells = new Godot.Collections.Dictionary();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print(FsLib.Say.hello("World"));

		cell = GetNode<Sprite2D>("Cell");

		if (cell != null)
		{
			cell.Hide();
			aliveColor = cell.Modulate;
		}
		else
		{
			throw new NullReferenceException("$Cell shouldn't be null!");
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseButton)
		{
			if (mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed)
			{

			}
		}
	}

	public void PlaceCell(Vector2 pos)
	{
		var gridPos = GetGridPosition(pos);
		if (!)
	}

	public Vector2 GetGridPosition(Vector2 pos)
	{
		float pixels = 32.0F;
		return pos.Snapped(new Vector2(pixels, pixels)) / pixels;
	}

	public void AddNewCell(Vector2 gridPosition)
	{
		var pos = gridPosition * 32.0F;
		var newCell = cell?.Duplicate() as Sprite2D;
		
		if (newCell is null)
		{
			throw new Exception("Cell is null!");
		}
		
		newCell.Position = pos;
		AddChild(newCell);
		newCell.Show();

		if (cells.ContainsKey(gridPosition))
		{
			throw new Exception("Cell already exists!");
		}
		else
		{
			cells.Add(gridPosition, newCell);
		}	
	}
}
