using System;
using Godot;

public partial class Safezone : Area2D
{
    [Export]
    public Mask maskType = null;

    public override async void _Process(double delta)
    {
        if (inRegion)
        {
            waitTime += (float)delta;
        }
        if (inRegion && waitTime > 2.0f)
        {
            await GameManager.instance.Win();
        }
    }

    float waitTime = 0;

    bool inRegion = false;

    public void OnEnter(Node2D body)
    {
        if (body is Movement m)
        {
            Ui.instance.ShowCreatureDialogue("Complete my ritual, interloper.");
            GameManager.instance.currentSafeMask = maskType;
            inRegion = true;
        }
    }

    public void OnExit(Node2D body)
    {
        if (body is Movement m)
        {
            inRegion = false;
            GameManager.instance.currentSafeMask = null;
            waitTime = 0.0f;
        }
    }
}
