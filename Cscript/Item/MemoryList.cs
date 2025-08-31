#pragma warning disable IDE1006 // 命名样式
public class mUaAny : Memory
{
    public mUaAny()
    {
        Name = "mUaAny";
        Weight = 1f;
        Description = "+1属性点";
    }

    public void OnEquip(Unit unit)
    {
        unit.Ua.uaPoint += 1;
    }
}
public class mSkill : Memory
{
    public mSkill()
    {
        Name = "mSkill";
        Weight = 5f;
        Description = "+2技能点";
    }

    public void OnEquip(Unit unit)
    {
        unit.Ua.skillPoint += 2;
    }
}
public class mUaStr : Memory
{
    public mUaStr()
    {
        Name = "mUaStr";
        Weight = 3f;
        Description = "+4力量";
    }

    public void OnEquip(Unit unit)
    {
        unit.Ua.Str += 4;
    }
}

public class mUaDex : Memory
{
    public mUaDex()
    {
        Name = "mUaDex";
        Weight = 3f;
        Description = "+4敏捷";
    }

    public void OnEquip(Unit unit)
    {
        unit.Ua.Dex += 4;
    }
}

public class mUaCon : Memory
{
    public mUaCon()
    {
        Name = "mUaCon";
        Weight = 3f;
        Description = "+4体质";
    }

    public void OnEquip(Unit unit)
    {
        unit.Ua.Con += 4;
    }
}

public class mUaSpi : Memory
{
    public mUaSpi()
    {
        Name = "mUaSpi";
        Weight = 3f;
        Description = "+4灵力";
    }

    public void OnEquip(Unit unit)
    {
        unit.Ua.Spi += 4;
    }
}

public class mUaMag : Memory
{
    public mUaMag()
    {
        Name = "mUaMag";
        Weight = 3f;
        Description = "+4魔力";
    }

    public void OnEquip(Unit unit)
    {
        unit.Ua.Mag += 4;
    }
}

public class mUaCun : Memory
{
    public mUaCun()
    {
        Name = "mUaCun";
        Weight = 3f;
        Description = "+4灵巧";
    }

    public void OnEquip(Unit unit)
    {
        unit.Ua.Cun += 4;
    }
}

