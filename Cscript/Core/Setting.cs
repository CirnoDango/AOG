using Godot;

public static class Setting
{
    public static string Language { get; set; } = "中文";

    public static int FPS { get; set; } = 240;
    public static int FPTS { get; set; } = 100;
    public static int FPTQ { get; set; } = 5;
    public const float chaos = 1.5f;
    public const int imagePx = 16;
    public const float bulletScale = 0.5f;
    public const float rootnodeScale = 3f;
    public static void LoadSettings()
    {
        ConfigFile config = new ConfigFile();

        if (config.Load("settings.cfg") != Error.Ok)
            return;

        Language = (string)config.GetValue("setting", "language", "中文");
        FPS = (int)config.GetValue("setting", "fps", 240);
        FPTS = (int)config.GetValue("setting", "fpts", 100);
        FPTQ = (int)config.GetValue("setting", "fptq", 5);
    }
}
