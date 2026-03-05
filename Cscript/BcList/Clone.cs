using System.Collections.Generic;

public class MultiSpeedClone : BarrageComponent
{
    public MultiSpeedClone()
    {
        SpCost = 5;
        CoolDown = 200;
    }
    public override void Activate(ref List<BulletContext> lbc)
    {
        List<BulletContext> nlbc = [];
        foreach (var bc in lbc)
        {
            for (int v = 0; v < 3; v += 1)
            {
                BulletContext nbc = bc.Clone();
                nbc.Speed += v;
                nbc.damage /= 2;
                nlbc.Add(nbc);
            }
        }
        lbc = nlbc;
    }
}

public class ColumnClone : BarrageComponent
{
    public ColumnClone()
    {
        SpCost = 5;
        CoolDown = 200;
    }
    public override void Activate(ref List<BulletContext> lbc)
    {
        List<BulletContext> nlbc = [];
        foreach (var bc in lbc)
        {
            BulletContext nbc1 = bc.Clone();
            BulletContext nbc2 = bc.Clone();
            nbc1.Offset += 0.25f * bc.VSpeed.Normalized().Rotated(MathEx.Deg2Rad(90));
            nbc2.Offset += 0.25f * bc.VSpeed.Normalized().Rotated(MathEx.Deg2Rad(-90));
            nbc1.damage /= 2;
            nbc2.damage /= 2;
            bc.damage /= 2;
            nlbc.Add(nbc1);
            nlbc.Add(nbc2);
            nlbc.Add(bc);
        }
        lbc = nlbc;
    }
}