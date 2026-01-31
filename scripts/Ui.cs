using Godot;
using System;

public partial class Ui : CanvasLayer
{
	private Movement player;
	[Export]
	public TextureProgressBar dashCooldownBar;
	[Export]
	public RichTextLabel creatureDialogueLabel;
	public float dialogueFadeDuration = 0.5f;





	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		player = GetTree().GetFirstNodeInGroup("Player") as Movement;
		creatureDialogueLabel.Modulate = new Color(1, 1, 1, 0);
		dashCooldownBar.MaxValue = player.dashCooldown;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		dashCooldownBar.Value = (player.dashCooldown - player.dashTimeRemaining);
	}

	public void ShowCreatureDialogue(string dialogue)
	{
		creatureDialogueLabel.Text = "[font_size=72][wave]\"" + dialogue + "\"[/wave][/font_size]";
		var alphaTween = CreateTween().TweenProperty(creatureDialogueLabel, "modulate:a", 1.0f, dialogueFadeDuration).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
	}
	public void HideCreatureDialogue()
	{
		var alphaTween = CreateTween().TweenProperty(creatureDialogueLabel, "modulate:a", 0.0f, dialogueFadeDuration).SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut);
	}
}
