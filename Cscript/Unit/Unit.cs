using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

public class Unit 
{
    public Unit()
    {
        Up = new(this);
        Ua = new(this);
        Us = new(this);
        Ue = new(this);
        UnitAi = new(this);
    }
    public UnitPosition Up { get; set; }
    public UnitAttribute Ua { get; set; }
    public UnitSkill Us { get; set; }
    public UnitEvent Ue { get; set; }
    public bool dead = false;
    public float TimeEnergy { get; set; } = 0;
    public string Name { get; set; }
    public List<string> symbol { get; set; }
    public string TrName => $"u{Name}";
    public UnitAi UnitAi { get; set; }
    public int Friendness
    {
        get
        {
            if (UnitAi != null)
                return UnitAi.friendness;
            else
                return 1;
        }
    }
    public List<Status> Status { get; set; } = [];
    public Dictionary<Status, Node> StatusImages { get; set; } = [];
    public Inventory Inventory { get; set; }
    public Equipment Equipment { get; set; }
    public MemoryBag Memorys { get; set; }
    public float MemoryValue { get; set; } = 10;
    public UnitEgo Ego { get; set; }
    public void GetStatus(Status status)
    {
        status.OnGet(this, status);
    }
    public static Action OnPlayerdied;
}
public enum UnitEgo
{
    normal, elite, great, boss, eliteBoss, random
}