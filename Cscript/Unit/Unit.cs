using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

public partial class Unit 
{
    private Grid _currentPosition;
    public Grid CurrentGrid
    {
        get => _currentPosition;
        set
        {
            _currentPosition = value;
            if (_currentPosition != null && sprite != null)
            {
                sprite.Position = _currentPosition.Position * 16; // 16是格子大小
            }
        }
    }
    public Vector2I Position
    {
        get => CurrentGrid != null ? CurrentGrid.Position : new Vector2I(-1, -1);
    }
    public Sprite2D sprite;
    private ColorRect hpBarAhead;
    private ColorRect spBarAhead;

    public float MaxHp = 100;
    public float CurrentHp = 100;
    private float _maxSp = 40;
    public float MaxSp
    {
        get => _maxSp;
        set { if (_maxSp != 0) { _maxSp = value; } }
    }
    public float CurrentSp = 0;
    public float MaxMp = 10;
    public float CurrentMp = 10;
    public float TimeEnergy { get; set; } = 0;
    public float imageSizeFactor = 0.16f;
    public string Name;
    public int bestDistance;
    public string TrName => $"u{Name}";
    public List<(SkillInstance skill, float weight)> skills = [];
    public SkillInstance GetSkill(string name) => skills.FirstOrDefault(x => x.skill.Name == name).skill;
    public UnitAi unitAi;
    public UnitAttribute Ua { get; set; }
    public int friendness = -1;
    public SpellCard currentSpellcard;
    public List<Status> Status = [];
    public Dictionary<Status, Node> StatusImages = [];
    public static Action OnPlayerdied;
    private int _vision = 12;
    public int Vision
    {
        get => _vision;
        set
        {
            _vision = value;
            if (this == Player.PlayerUnit)
                RefreshVision();
        }
    }
    public Inventory inventory;
    public Equipment equipment;
    public MemoryBag Memorys;
    public float MemoryValue = 10;

    public UnitEgo Ego { get; set; }
}

public enum UnitEgo
{
    normal, elite, great, boss, eliteBoss, random
}