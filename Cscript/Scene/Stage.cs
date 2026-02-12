using Godot;
using System.Threading.Tasks;

public partial class Stage : Node
{
    public TaskCompletionSource clickTcs;
    public async Task Click()
    {
        clickTcs = new TaskCompletionSource();
        await clickTcs.Task;
    }
    public static Texture2D L(string name)
    {
        return GD.Load<Texture2D>($"res://Assets/Portraits/{name}.png");
    }
    public override void _Input(InputEvent @event)
    {
        if (clickTcs != null &&
            @event is InputEventMouseButton mouseEvent &&
            mouseEvent.Pressed)
        {
            clickTcs.TrySetResult();
        }
    }

    public async Task ShowDialogSequence(params DialogLine[] lines)
    {
        foreach (var line in lines)
        {
            G.I.DialogBox.ShowDialog([line]);
            await Click();
        }
    }
}

