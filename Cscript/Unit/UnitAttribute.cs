using System.Collections.Generic;
public class UnitAttribute
{
    public Unit unit;
    public float SpeedGlobal = 100;
    public float SpeedCombat = 100;
    public float SpeedMove = 100;
    public float DamageBody = 100;
    public float DamageBullet = 100;
    public float HealRatio = 1f;
    public float BodyDamageAccuracy = 0;
    public float BulletDamageAccuracy = 0;
    public float DamageEvasion = 0;
    public float BulletGraze = 0;
    private int _str = 10, _dex = 10, _con = 10, _spi = 10, _mag = 10, _cun = 10;
    public int Str
    {
        get => _str - 10;
        set
        {
            int old = _str - 10;
            int now = value;
            DamageBody += (now - old) * 2;
            _str = value + 10;
        }
    }
    public int Dex
    {
        get => _dex - 10;
        set
        {
            int old = _dex - 10;
            int now = value;
            SpeedGlobal += (now - old) * 1;
            BodyDamageAccuracy += (now - old) * 0.01f;
            DamageEvasion += (now - old) * 0.01f;
            _dex = value + 10;
        }
    }
    public int Con
    {
        get => _con - 10;
        set
        {
            int old = _con - 10;
            int now = value;
            unit.MaxHp += (now - old) * 5;
            HealRatio += (now - old) * 0.02f;
            _con = value + 10;
        }
    }
    public int Spi
    {
        get => _spi - 10;
        set
        {
            int old = _spi - 10;
            int now = value;
            unit.MaxSp += (now - old) * 2;
            DamageBullet += (now - old) * 1;
            _spi = value + 10;
        }
    }
    public int Mag
    {
        get => _mag - 10;
        set
        {
            int old = _mag - 10;
            int now = value;
            unit.MaxMp += (now - old) * 1;
            DamageBullet += (now - old) * 1;
            _mag = value + 10;
        }
    }
    public int Cun
    {
        get => _cun - 10;
        set
        {
            int old = _cun - 10;
            int now = value;
            BulletDamageAccuracy += (now - old) * 0.01f;
            unit.inventory.MaxWeight += (now - old) * 2;
            unit.equipment.MaxEquipWeight += (now - old) * 2;
            _cun = value + 10;
        }
    }
    public UnitAttribute()
    {
        skillPoint = 0;
        uaPoint = 0;
    }
    private int _skillPoint;
    public int skillPoint
    {
        get { return _skillPoint; }
        set
        {
            if (unit == Player.PlayerUnit)
                G.I.Player.SkillPoint = value;

            _skillPoint = value;
        }
    }
    private int _uaPoint;
    public int uaPoint
    {
        get { return _uaPoint; }
        set
        {
            if (unit == Player.PlayerUnit)
                G.I.Player.UaPoint = value;

            _uaPoint = value;
        }
    }
    // 可选：一个构造函数初始化所有属性
    public void UnitAtt(int str = 10, int dex = 10, int con = 10, int spi = 10, int mag = 10, int cun = 10)
    {
        //return;
        Str = str - 10;
        Dex = dex - 10;
        Con = con - 10;
        Spi = spi - 10;
        Mag = mag - 10;
        Cun = cun - 10;
    }
    // 可选：一个显示真实数值的方法
    public override string ToString()
    {
        return $"Str: {Str}, Dex: {Dex}, Con: {Con}, Spi: {Spi}, Mag: {Mag}, Cun: {Cun}";
    }
}

