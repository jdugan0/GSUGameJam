using Godot;
using System;

public partial class Ui : CanvasLayer
{
	private Movement player;
	[Export]
	public TextureProgressBar dashCooldownBar;





	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		player = GetTree().GetFirstNodeInGroup("Player") as Movement;
		dashCooldownBar.MaxValue = player.dashCooldown;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		dashCooldownBar.Value = (player.dashCooldown - player.dashTimeRemaining);
	}
}
