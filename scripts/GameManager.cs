using System;
using System.Threading.Tasks;
using Godot;

public partial class GameManager : Node
{
    public static GameManager instance;
    public Mask currentSafeMask = null;

    [Export]
    public float checkTime = 10;

    public bool countdown = false;

    public float currentTime = 0;

    public Movement player { get; private set; }

    public Vector2? onGround { get; private set; }

    public Enemy toProtect = null;

    bool playingEndSound = false;

    public override void _Ready()
    {
        instance = this;
        player = GetTree().GetFirstNodeInGroup("Player") as Movement;
    }

    public override async void _PhysicsProcess(double delta)
    {
        if (!playingEndSound && countdown && checkTime - currentTime <= 12.0)
        {
            playingEndSound = true;
            AudioManager.instance.PlaySFX("GameEnd");
        }
        //GD.Print(countdown);
        if (GetTree().GetFirstNodeInGroup("Mask") == null)
        {
            onGround = null;
        }
        else
        {
            onGround = (GetTree().GetFirstNodeInGroup("Mask") as Node2D).Position;
        }

        if (countdown)
        {
            currentTime += (float)delta;
        }

        if (currentTime >= checkTime)
        {
            currentTime = 0;
            // countdown = false;
            if (currentSafeMask == null || player.mask == null || player.mask != currentSafeMask)
            {
                await player.Kill();
            }
            else
            {
                await Win();
            }
        }
    }

    public async Task Win()
    {
        if (countdown)
        {
            AudioManager.instance.CancelSFX("timer_ticking");
        }
        AudioManager.instance.CancelSFX(MusicManager.instance.currentMusic);
        MusicManager.instance.currentMusic = "music_game_win";
        AudioManager.instance.PlaySFX("music_game_win");
        await SceneSwitcher.instance.SwitchSceneAsyncSlide("WinScreen");
    }
}
