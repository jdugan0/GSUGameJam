using System;
using Godot;

public partial class WinScreen : Control
{
    [Export]
    public TextureButton mainMenuButton;

    [Export]
    public bool lose;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        mainMenuButton.Pressed += async () =>
        {
            AudioManager.instance.CancelSFX(MusicManager.instance.currentMusic);
            if (GameManager.instance.currentlevel == GameManager.instance.levelOrder.Length - 1)
            {
                await SceneSwitcher.instance.SwitchSceneAsyncSlide("MainMenu");
            }
            else
            {
                if (!lose)
                {
                    GameManager.instance.currentlevel++;
                }
                await SceneSwitcher.instance.SwitchSceneAsyncSlide(
                    GameManager.instance.levelOrder[GameManager.instance.currentlevel]
                );
            }
        };
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) { }
}
