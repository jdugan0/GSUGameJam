using Godot;
using System;

public partial class TitleScreen : Control
{
	[Export]
	public MarginContainer mainScreen;
	[Export]
	public MarginContainer configScreen;
	[Export]
	public TextureButton startButton;
	[Export]
	public TextureButton configButton;
	[Export]
	public Button exitButton;
	[Export]
	public Button configBackButton;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		mainScreen.Visible = true;
		configScreen.Visible = false;
		configButton.Pressed += () =>
		{
			mainScreen.Visible = false;
			configScreen.Visible = true;
		};
		configBackButton.Pressed += () =>
		{
			mainScreen.Visible = true;
			configScreen.Visible = false;
		};
		exitButton.Pressed += () =>
		{
			GetTree().Quit();
		};
		startButton.Pressed += () =>
		{
			//
		};
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
