using System;
using System.Security.Principal;
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
    float zoomAmount = 2.5f;

    [ExportGroup("Screen Shake")]
    [Export]
    float shakeSampleRate = 40f;

    [Export]
    float shakeSmoothing = 25f;

    [Export]
    float shakeFadePower = 2.0f;

    private readonly RandomNumberGenerator rng = new();
    private float shakeTimeLeft;
    private float shakeDurationTotal;
    private float shakeAmplitude;
    private float shakeSampleTimer;
    private Vector2 shakeTarget;
    private Vector2 shakeCurrent;
    private Vector2 lastAppliedShake;

    public static Camera instance;
    public int currentDialogueIndex = 0;
    public string[] dialogueLines = new string[]
    {
        "An intruder. How... intriguing.",
        "It's forbidden for an outsider to wear the [color=purple]Mask[/color]...",
        "Yet that is what you seek, is it not?",
        "Very well then.",
        "Complete my ritual and bring the [color=purple]Mask[/color] to the [color=green]Altar[/color].",
        "Or succumb to the fog of the farthest world.",
        "Sanguinem occultorum effundere. Little one.",
        "",
    };

    public override void _Ready()
    {
        instance = this;
        rng.Randomize();
        Zoom = new Vector2(zoomAmount, zoomAmount);
        player = GetTree().GetFirstNodeInGroup("Player") as Movement;
        player.moveEnabled = false;
        UI.ShowCreatureDialogue("An intruder. How... intriguing.");
        UI.clickContinueLabel.Visible = true;
    }

    public void TextAdvance()
    {
        Ui.instance.advanceEnabled = false;
        if (currentDialogueIndex < dialogueLines.Length - 1 && currentDialogueIndex >= 0)
        {
            GD.Print("Advancing dialogue", currentDialogueIndex);
            currentDialogueIndex++;
            if (dialogueLines[currentDialogueIndex] == "")
            {
                UI.FinishDialogue();
                ZoomIn();
                return;
            }
            UI.NextCreatureDialogue(dialogueLines[currentDialogueIndex]);
        }
    }

    public override void _Process(double delta)
    {
        Vector2 mousePos = GetGlobalMousePosition();
        Position = (playerWeight * player.Position + mousePos) / (1 + playerWeight);

        if (Input.IsActionJustPressed("SKIP_DIALOGUE"))
        {
            currentDialogueIndex = -1;
            UI.FinishDialogue();
            ZoomIn();
            return;
        }
        if (
            (
                Input.IsActionJustPressed("DASH")
                || Input.IsActionJustPressed("ATTACK")
                || Input.IsActionJustPressed("RIGHT")
            ) && Ui.instance.advanceEnabled
        )
        {
            TextAdvance();
        }

        if (player.mask != null && player.moveEnabled)
        {
            Zoom = new Vector2(defaultZoom * 1.5f, defaultZoom * 1.5f);
        }
        if (player.mask == null && player.moveEnabled)
        {
            Zoom = new Vector2(defaultZoom, defaultZoom);
        }

        ApplyShake((float)delta);
    }

    public void ZoomIn()
    {
        var twenBack = CreateTween()
            .TweenProperty(this, "zoom", new Vector2(defaultZoom, defaultZoom), zoomSpeed)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
        twenBack.Finished += () =>
        {
            player.moveEnabled = true;
            var enemies = GetTree().GetNodesInGroup("Enemy");
            foreach (var enemy in enemies)
            {
                (enemy as Enemy).disable = false;
            }
        };
    }

    public void ScreenShake(float amplitude, float duration)
    {
        if (duration <= 0f || amplitude <= 0f)
            return;

        shakeDurationTotal = Math.Max(shakeDurationTotal, duration);
        shakeTimeLeft = Math.Max(shakeTimeLeft, duration);
        shakeAmplitude = Math.Max(shakeAmplitude, amplitude);

        if (shakeSampleTimer <= 0f)
            shakeSampleTimer = 1f / Math.Max(1f, shakeSampleRate);
    }

    private void ApplyShake(float delta)
    {
        Vector2 rawOffset = Offset - lastAppliedShake;

        if (shakeTimeLeft <= 0f)
        {
            shakeCurrent = Vector2.Zero;
            shakeTarget = Vector2.Zero;
            lastAppliedShake = Vector2.Zero;
            Offset = rawOffset;
            shakeDurationTotal = 0f;
            shakeAmplitude = 0f;
            shakeSampleTimer = 0f;
            return;
        }

        shakeTimeLeft = Math.Max(0f, shakeTimeLeft - delta);

        float interval = 1f / Math.Max(1f, shakeSampleRate);
        shakeSampleTimer -= delta;

        while (shakeSampleTimer <= 0f)
        {
            shakeSampleTimer += interval;
            shakeTarget = RandomInUnitCircle();
        }

        float t = shakeDurationTotal > 0f ? (shakeTimeLeft / shakeDurationTotal) : 0f;
        float fade = Mathf.Pow(Mathf.Clamp(t, 0f, 1f), shakeFadePower);
        Vector2 target = shakeTarget * (shakeAmplitude * fade);

        float lerpT = 1f - Mathf.Exp(-Mathf.Max(0.01f, shakeSmoothing) * delta);
        shakeCurrent = shakeCurrent.Lerp(target, lerpT);

        lastAppliedShake = shakeCurrent;
        Offset = rawOffset + shakeCurrent;
    }

    private Vector2 RandomInUnitCircle()
    {
        float angle = rng.RandfRange(0f, Mathf.Tau);
        float radius = Mathf.Sqrt(rng.Randf());
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
    }
}
