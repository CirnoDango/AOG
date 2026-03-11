using Godot;
using System;
using System.Reflection;

public partial class SettingMenu : ColorRect, IRegisterToG
{
    [Export] OptionButton language;
    [Export] HSlider fps;
    [Export] HSlider fpts;
    [Export] HSlider fptq;
    [Export] Label tfps;
    [Export] Label tfpts;
    [Export] Label tfptq;
    [Export] Button bback;
    public override void _Ready()
    {
        language.GetPopup().AddThemeFontSizeOverride("font_size", 400);
        // 连接信号
        fps.ValueChanged += OnFpsChanged;
        fpts.ValueChanged += OnFptsChanged;
        fptq.ValueChanged += OnFptqChanged;
        bback.Pressed += Back;
        // 初始化显示
        UpdateLabels();
        switch(Setting.Language)
        {
            case "中文":
                language.Selected = 0;
                break;
            case "English":
            default:
                language.Selected = 1;
                break;
        }
    }

    void UpdateLabels()
    {
        tfps.Text = fps.Value.ToString();
        tfpts.Text = fpts.Value.ToString();
        tfptq.Text = fptq.Value.ToString();
    }

    void OnFpsChanged(double value)
    {
        tfps.Text = value.ToString();
    }

    void OnFptsChanged(double value)
    {
        tfpts.Text = value.ToString();
    }

    void OnFptqChanged(double value)
    {
        tfptq.Text = value.ToString();
    }
    void Back()
    {
        SaveSettings();
        G.I.SettingMenu.Visible = false;
    }
    // 保存设置
    public void SaveSettings()
    {
        Setting.FPS = (int)fps.Value;
        Engine.MaxFps = Setting.FPS;
        Setting.FPTQ = (int)fptq.Value;
        Setting.FPTS = (int)fpts.Value;
        ConfigFile config = new ConfigFile();
        var l = language.GetItemText(language.Selected);
        config.SetValue("setting", "language", l);
        config.SetValue("setting", "fps", fps.Value);
        config.SetValue("setting", "fpts", fpts.Value);
        config.SetValue("setting", "fptq", fptq.Value);
        config.Save("settings.cfg");
    }
    public void RegisterToG(G g)
    {
        g.SettingMenu = this;
    }
}
