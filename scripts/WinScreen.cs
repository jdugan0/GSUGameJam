using System;
using Godot;

public partial class WinScreen : Control
{
    [Export]
    public TextureButton mainMenuButton;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        mainMenuButton.Pressed += async () =>
        {
            AudioManager.instance.CancelSFX(MusicManager.instance.currentMusic);
            await SceneSwitcher.instance.SwitchSceneAsyncSlide("MainMenu");
        };
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) { }
}
