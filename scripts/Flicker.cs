using Godot;

public partial class Flicker : PointLight2D
{
    [Export]
    public float MinEnergy = 0.7f;

    [Export]
    public float MaxEnergy = 1.2f;

    [Export]
    public float FlickerSpeed = 12f;

    private float t;

    public override void _Ready()
    {
        MinEnergy = MinEnergy * Energy;
        MaxEnergy = MaxEnergy * Energy;
    }

    public override void _Process(double delta)
    {
        t += (float)delta * FlickerSpeed;
        float noise =
            Mathf.Sin(t * 3.1f) + Mathf.Sin(t * 7.3f) * 0.5f + Mathf.Sin(t * 13.7f) * 0.25f;
        noise = (noise + 1.75f) / 3.5f;
        Energy = Mathf.Lerp(MinEnergy, MaxEnergy, noise);
    }
}
