using System;
using Godot;

public partial class Ui : CanvasLayer
{
    private Movement player;

    [Export]
    public TextureProgressBar dashCooldownBar;

    [Export]
    public RichTextLabel creatureDialogueLabel;
    public float dialogueFadeDuration = 0.6f;

    [Export]
    public Label timer;

    [Export]
    CanvasLayer pauseMenu;

    [Export]
    public ColorRect vignette;
    public static Ui instance;

    [Export]
    public RichTextLabel clickContinueLabel;

    [Export]
    PackedScene maskHealth;

    [Export]
    PackedScene normalHealth;

    [Export]
    HBoxContainer healthContainer;
    public bool advanceEnabled = false;
    [Export]
    public TextureProgressBar candle;

    [Export]
    public ColorRect lowHealth;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        player = GetTree().GetFirstNodeInGroup("Player") as Movement;
        creatureDialogueLabel.Modulate = new Color(1, 1, 1, 0);
        dashCooldownBar.MaxValue = player.dashCooldown;
        instance = this;
        candle.Visible = false;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        foreach (var x in healthContainer.GetChildren())
        {
            healthContainer.RemoveChild(x);
            x.QueueFree();
        }
        if (player.health == 1)
        {
            lowHealth.Visible = true;
        }
        // GD.Print(heaplthContainer.GetChildCount());
        if (player.mask != null)
        {
            dashCooldownBar.Visible = false;
            for (int i = 0; i < player.maskHealth; i++)
            {
                healthContainer.AddChild(maskHealth.Instantiate());
            }
        }
        else
        {
            dashCooldownBar.Visible = true;
            for (int i = 0; i < player.health; i++)
            {
                // GD.Print(i);
                healthContainer.AddChild(normalHealth.Instantiate());
            }
        }
        dashCooldownBar.Value = (player.dashCooldown - player.dashTimeRemaining);
        if (GameManager.instance.countdown)
        {
            
            candle.Visible = true;
            timer.Visible = true;
            timer.Text =
                "TIMER: "
                + Math.Round(
                    (GameManager.instance.checkTime - GameManager.instance.currentTime) * 10
                ) / 10;
            Vector2 n = new Vector2(candle.Size.X, 240 - (240* GameManager.instance.currentTime / GameManager.instance.checkTime));
            candle.CustomMinimumSize = n;
            Math.Round(GameManager.instance.currentTime);
        }
        else
        {
            timer.Visible = false;
        }
    }

    public void ShowCreatureDialogue(string dialogue)
    {
        creatureDialogueLabel.Text =
            "[font_size=72][shake]\"" + dialogue + "\"[/shake][/font_size]";
        var alphaTween = CreateTween()
            .TweenProperty(creatureDialogueLabel, "modulate:a", 1.0f, dialogueFadeDuration)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
        alphaTween.Finished += () =>
        {
            Ui.instance.advanceEnabled = true;
        };
    }

    public void NextCreatureDialogue(string dialogue)
    {
        Ui.instance.advanceEnabled = false;
        var alphaTween = CreateTween()
            .TweenProperty(creatureDialogueLabel, "modulate:a", 0.0f, dialogueFadeDuration)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
        alphaTween.Finished += () =>
        {
            clickContinueLabel.Visible = false;
            ShowCreatureDialogue(dialogue);
        };
    }

    public void FinishDialogue()
    {
        var alphaTween = CreateTween()
            .TweenProperty(creatureDialogueLabel, "modulate:a", 0.0f, dialogueFadeDuration)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
    }
}
