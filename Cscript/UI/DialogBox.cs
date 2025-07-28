using Godot;
using System;
using System.Collections.Generic;

public partial class DialogBox : Control, IRegisterToG
{
    [Export] public TextureRect Portrait;
    [Export] public Label NameLabel;
    [Export] public RichTextLabel DialogText;
    [Export] public Label ContinueHint;

    private Queue<DialogLine> dialogQueue = new();
    private Action onDialogFinished;
    public void ShowDialog(List<DialogLine> lines, Action onFinish = null)
    {
        dialogQueue = new Queue<DialogLine>(lines);
        onDialogFinished = onFinish;
        ShowNextLine();
        Show();
    }
    public static void SShow()
    {
        G.I.Fsm.ChangeState(Fsm.talkState);
        G.I.DialogBox.Show();
    }
    public static void SHide()
    {
        G.I.Fsm.ChangeState(Fsm.updateState);
        G.I.DialogBox.Hide();
    }
   
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_accept"))
        {
            ShowNextLine();
        }
    }

    private void ShowNextLine()
    {
        if (dialogQueue.Count == 0)
        {
            Hide();
            onDialogFinished?.Invoke();
            return;
        }

        var line = dialogQueue.Dequeue();
        Portrait.Texture = line.Portrait;
        NameLabel.Text = line.Name;
        DialogText.Text = line.Text;
    }

    public void RegisterToG(G g)
    {
        g.DialogBox = this;
	}
}
public static class DialogContext
{
    public static string CurrentName;
    public static Texture2D CurrentPortrait;
}

public class DialogLine
{
    public string Name;
    public Texture2D Portrait;
    public string Text;
    public DialogLine(string name, Texture2D portrait, string text)
    {
        Name = name;
        Portrait = portrait;
        Text = text;

        DialogContext.CurrentName = name;
        DialogContext.CurrentPortrait = portrait;
    }
    public DialogLine(Texture2D portrait, string text)
    {
        Name = DialogContext.CurrentName;
        Portrait = portrait;
        Text = text;
        DialogContext.CurrentPortrait = portrait;
    }
    public DialogLine(string text)
    {
        Name = DialogContext.CurrentName;
        Portrait = DialogContext.CurrentPortrait;
        Text = text;
    }
}

