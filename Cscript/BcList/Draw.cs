using System.Collections.Generic;

public class Draw2 : BarrageComponent
{
    public Draw2()
    {
        CoolDown = 200;
        draw = 2;
        group = "Draw2";
    }
    public override void Activate(ref List<BulletContext> lbc)
    {
        return;
    }
}
