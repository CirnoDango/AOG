using Godot;

public partial class DamageNumber : Control
{
    private Label label;
    private int lifetime = 100;
    private float speed = 9;

    public override void _Ready()
    {
        label = GetNode<Label>("Label");
        speed = (float)GD.RandRange(7d, 11d);
    }

    public void Init(int damage, Color color)
    {
        label.Text = damage.ToString();
        label.Modulate = color;
    }

    public override void _Process(double delta)
    {
        if (lifetime > 60)
            Position += new Vector2(0, -speed);
        lifetime--;
        if (lifetime <= 0)
            QueueFree();
    }
}