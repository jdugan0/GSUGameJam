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
    float zoomAmount = 0.1f;

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
        ""
    };


    public override void _Ready()
    {
        instance = this;
        Zoom = new Vector2(zoomAmount, zoomAmount);
        player = GetTree().GetFirstNodeInGroup("Player") as Movement;
        player.moveEnabled = false;
        UI.ShowCreatureDialogue("An intruder. How... intriguing.");
        UI.clickContinueLabel.Visible = true;
        //ZoomIn();
    }

    public void TextAdvance()
    {
        if (currentDialogueIndex < dialogueLines.Length - 1 && currentDialogueIndex >= 0)
        {
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
        if (Input.IsActionJustPressed("DASH") || Input.IsActionJustPressed("ATTACK") || Input.IsActionJustPressed("RIGHT"))
        {
            TextAdvance();
        if (player.mask != null)
        {
            Zoom *= 1.5f;
        }
        if (player.mask == null)
        {
            Zoom = new Vector2(defaultZoom, defaultZoom);
        }
    }
    }

    // at the beginning of the round, zoom out to show zone locations
    public void ZoomIn()
    {
        var twenBack = CreateTween().TweenProperty(this, "zoom", new Vector2(defaultZoom, defaultZoom), zoomSpeed).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
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
