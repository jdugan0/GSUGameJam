using Godot;
using System;

public partial class MusicManager : Node
{
	public String currentMusic = "";
	public static MusicManager instance;
	public override void _Ready()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            this.QueueFree();
        }
    }
}
