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
    float zoomAmount = 2.5f;

    [ExportGroup("Shake Settings")]
    [Export]
    float shakeMaxOffset = 24f;

    [Export]
    float shakeFrequency = 35f;

    [Export]
    float shakeSmoothing = 18f;

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

    float shakeTimeLeft = 0f;
    float shakeDuration = 0f;
    float shakeIntensity = 0f;
    float shakePhase = 0f;
    Vector2 shakeTargetOffset = Vector2.Zero;
    Vector2 shakeCurrentOffset = Vector2.Zero;
    readonly RandomNumberGenerator rng = new RandomNumberGenerator();

    public override void _Ready()
    {
        instance = this;
        Zoom = new Vector2(zoomAmount, zoomAmount);
        player = GetTree().GetFirstNodeInGroup("Player") as Movement;
        player.moveEnabled = false;
        UI.ShowCreatureDialogue("An intruder. How... intriguing.");
        UI.clickContinueLabel.Visible = true;
        rng.Randomize();
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
        float dt = (float)delta;

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

        UpdateShake(dt);
    }

    void UpdateShake(float dt)
    {
        if (shakeTimeLeft <= 0f)
        {
            shakeTargetOffset = Vector2.Zero;
        }
        else
        {
            shakeTimeLeft = Mathf.Max(0f, shakeTimeLeft - dt);
            shakePhase += dt * shakeFrequency;

            float t = shakeDuration <= 0f ? 0f : 1f - (shakeTimeLeft / shakeDuration);
            float amp = shakeIntensity * (1f - t);

            if (shakePhase >= 1f)
            {
                shakePhase -= 1f;
                float ox = rng.RandfRange(-1f, 1f);
                float oy = rng.RandfRange(-1f, 1f);
                Vector2 v = new Vector2(ox, oy);
                if (v.LengthSquared() > 0f)
                    v = v.Normalized();
                shakeTargetOffset = v * (amp * shakeMaxOffset);
            }
        }

        shakeCurrentOffset = shakeCurrentOffset.Lerp(
            shakeTargetOffset,
            1f - Mathf.Exp(-shakeSmoothing * dt)
        );
        Offset = shakeCurrentOffset;
    }

    public void ScreenShake(float intensity, float duration)
    {
        intensity = Mathf.Clamp(intensity, 0f, 1f);
        duration = Mathf.Max(0f, duration);

        if (duration <= 0f || intensity <= 0f)
            return;

        if (shakeTimeLeft > 0f)
            shakeIntensity = Mathf.Max(shakeIntensity, intensity);
        else
            shakeIntensity = intensity;

        shakeDuration = Mathf.Max(shakeDuration, duration);
        shakeTimeLeft = Mathf.Max(shakeTimeLeft, duration);
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
}
