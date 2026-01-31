using System;
using Godot;

public partial class Camera : Camera2D
{
    Movement player;

    [Export]
    int playerWeight;

    [Export]
    Ui UI;

    [ExportGroup("Zoom Settings")]
    [Export]
    float defaultZoom = 0.5f;

    [Export]
    float zoomSpeed = 2.0f;

    [Export]
    float zoomDuration = 4.0f;

    [Export]
    float zoomAmount = 0.1f;

    [Signal]
    public delegate void ZoomStartEventHandler();

    [Signal]
    public delegate void ZoomFinishEventHandler();
    public bool zooming;

    public static Camera instance;

    public override void _Ready()
    {
        instance = this;
        player = GetTree().GetFirstNodeInGroup("Player") as Movement;
        UI.dialogueFadeDuration = zoomSpeed;
        // ZoomStart += () =>
        // {
        //     UI.ShowCreatureDialogue("Look around to find the zones!");
        //     zooming = true;
        // };
        // ZoomFinish += () =>
        // {
        //     UI.HideCreatureDialogue();
        //     zooming = false;
        // };
        // RoundZoom();
    }

    public override void _Process(double delta)
    {
        Vector2 mousePos = GetGlobalMousePosition();
        Position = (playerWeight * player.Position + mousePos) / (1 + playerWeight);
    }

    // at the beginning of the round, zoom out to show zone locations
    public void RoundZoom()
    {
        player.moveEnabled = false;
        var twen = CreateTween()
            .TweenProperty(this, "zoom", new Vector2(zoomAmount, zoomAmount), zoomSpeed)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
        EmitSignal(nameof(ZoomStart));
        twen.Finished += () =>
        {
            var timer = GetTree().CreateTimer(zoomDuration);
            timer.Timeout += () =>
            {
                var twenBack = CreateTween()
                    .TweenProperty(this, "zoom", new Vector2(defaultZoom, defaultZoom), zoomSpeed)
                    .SetTrans(Tween.TransitionType.Sine)
                    .SetEase(Tween.EaseType.InOut);
                EmitSignal(nameof(ZoomFinish));
                player.moveEnabled = true;
            };
        };
    }
}
