using System;
using Godot;

[GlobalClass]
public partial class Mask : Resource
{
    [Export]
    public Color color;

    [Export]
    public Texture2D sprite;

    [Export]
    public string name;
}
