using System;
using Godot;

public partial class PauseMenu : CanvasLayer
{
    [Export]
    public Button resumeButton;
    [Export]
    public Button quitButton;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        resumeButton.Pressed += () =>
        {
            Visible = !Visible;
            GetTree().Paused = Visible;
        };
        quitButton.Pressed += async () =>
        {
            Visible = false;
            GetTree().Paused = Visible;
            await SceneSwitcher.instance.SwitchSceneAsyncSlide("MainMenu");
            AudioManager.instance.CancelSFX("timer_ticking");
            AudioManager.instance.CancelSFX("GameEnd");
            AudioManager.instance.CancelSFX(MusicManager.instance.currentMusic);
        };
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("PAUSE"))
        {
            Visible = !Visible;
            GetTree().Paused = Visible;
        }
    }
}
