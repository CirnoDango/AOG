using Godot;
using System;
using System.Collections.Generic;

public class Bullet
{
    public Vector2 PositionFloat { get; set; }
    public Vector2I Position {  get; set; }
    public Vector2 Speed { get; set; }
    public Vector2 Acceleration { get; set; } = Vector2.Zero;
    public float DistanceLeft { get; set; }
    public Vector2I lastPosition; // 上一次的浮点坐标
    public ShapeBullet Shape;
    public ColorBullet color;
    public Sprite2D image;
    public Unit creator;
    public Unit tracing;
    public Skill skill;
    public Damage damage;
    public float accuracy;
    public float crit = 0;
    public float timeUsed;
    public Action<Bullet, float> OverrideUpdateEvents { get; set; }
    public Action<Bullet, float> NewUpdateEvents { get; set; }
    public static Bullet CreateBullet(Unit user, Skill skil, Damage damag, Vector2 start, Vector2 target,
        float speed, float maxDistance, ShapeBullet shape, ColorBullet color, float advance = 0, Grid trace = null)
    {
        Bullet b = new(user, skil, damag, start, target,
        speed, maxDistance, shape, color, advance, trace);
        user.Ue.CreateBullet(b);
        return b;
    }
    //起点偏移+速度偏移生成
    public static Bullet CreateBullet(Unit user, Skill skil, Damage damage, Vector2 start, Vector2 target, Vector2 point, Vector2 speedDir,
        float speed, float maxDistance, ShapeBullet shape, ColorBullet color, float advance = 0, Grid trace = null)
    {
        return CreateBullet(user, skil, damage, start + point.Rotated(Vector2.Right.AngleTo(target - start)), start + (point + speedDir).Rotated(Vector2.Right.AngleTo(target - start)),
        speed, maxDistance, shape, color, advance, trace);
    }
    //起点偏移+速度角度生成
    public static Bullet CreateBullet(Unit user, Skill skil, Damage damage, Vector2 start, Vector2 target, Vector2 point,
        float angle, float speed, float maxDistance, ShapeBullet shape, ColorBullet color, float advance = 0, Grid trace = null)
    {
        return CreateBullet(user, skil, damage, start + point.Rotated(Vector2.Right.AngleTo(target - start)),
        (start + point.Rotated(Vector2.Right.AngleTo(target - start))) +
        (target - start).Rotated(Mathf.DegToRad(angle)).Normalized() * 1000f,
        speed, maxDistance, shape, color, advance, trace);
    }
    //点到点生成
    public Bullet(Unit user, Skill skil, Damage damag, Vector2 start, Vector2 target, 
        float speed, float maxDistance, ShapeBullet shape, ColorBullet color, float advance = 0, Grid trace = null)
    {
        PositionFloat = start;
        Position = (Vector2I)start.Round();
        lastPosition = Position;
        Vector2 direction = (target - PositionFloat).Normalized();
        Speed = direction * speed;
        DistanceLeft = maxDistance;
        Shape = shape;
        this.color = color;
        image = GetImage(color, shape);
        image.Scale *= Setting.bulletScale;
        creator = user;
        skill = skil;
        damage = damag;
        accuracy = user.Ua.BulletDamageAccuracy;
        Scene.CurrentMap.Bullets.Add(this);
        if (advance != 0)
            Update(advance);
        if (trace != null)
        {
            var unit = trace.unit;
            if (unit != null)
            {
                tracing = unit;
                if (unit == user && user.Up.RandomEnemyInVision(out Unit u))
                    tracing = u;
            }
        }
    }
    

    /// <summary>
    /// 更新子弹状态，返回本次穿越的新格子列表
    /// </summary>
    public List<Vector2I> UpdateGrid(float deltaTime)
    {
        List<Vector2I> newGrids = [];

        if (DistanceLeft <= 0)
            return newGrids;

        // 移动前记录旧位置
        lastPosition = Position;

        // 计算实际移动距离
        float travelDistance = Speed.Length() * deltaTime;
        if (travelDistance > DistanceLeft)
            travelDistance = DistanceLeft;

        // 更新位置
        PositionFloat += Speed.Normalized() * travelDistance;
        DistanceLeft -= travelDistance;

        // 加速度
        if (tracing != null && Scene.CurrentMap.Units.Contains(tracing))
        {
            Speed = Speed.Length() * (Speed + 2f * deltaTime * (Vector2)(tracing.Up.Position - Position)).Normalized();
        }
        else
        {
            Speed += Acceleration * deltaTime;
        }
            // 用整数格子记录路径
            Vector2I oldPos = lastPosition;
        Vector2I newPos = (Vector2I)PositionFloat.Round();

        if (oldPos != newPos)
        {
            var path = MathEx.GetLine(oldPos, newPos);
            // 只记录中间新格子（不包含起点）
            foreach (var p in path)
            {
                if (p != oldPos)
                    newGrids.Add(p);
            }
        }
        
        Position = newPos;
        return newGrids;
    }
    public static Sprite2D GetImage(ColorBullet color, ShapeBullet shape)
    {
        var sprite = ReadImage(color, shape);
        Root.rootnode.AddChild(sprite);
        return sprite;
        
    }
    public static Sprite2D ReadImage(ColorBullet color, ShapeBullet shape)
    {
        var atlas = GD.Load<Texture2D>($"res://Assets/Bullets/{color}.png");
        if (atlas == null)
            return null; // 如果图像不存在，直接返回 null

        var atlasTexture = new AtlasTexture
        {
            Atlas = atlas,
        };

        int index = (int)shape;
        int col = index % 10;
        int row = index / 10;

        atlasTexture.Region = new Rect2(col * 32, row * 32, 32, 32);

        Sprite2D sprite = new()
        {
            Texture = atlasTexture,
            ZIndex = -10,
            Scale = new Vector2(Setting.imagePx / 16, Setting.imagePx / 16),
        };

        return sprite;
    }
    
    public void Update(float time)
    {
        if (OverrideUpdateEvents == null)
            DefaultUpdate(time);
        else
            OverrideUpdateEvents.Invoke(this, time);
        NewUpdateEvents?.Invoke(this, time);
        timeUsed += time;
    }
    public void DefaultUpdate(float time)
    {
        foreach (Vector2I pos in UpdateGrid(time / 100))
        {
            var grid = Scene.CurrentMap.GetGrid(pos);
            if (!Scene.CurrentMap.CheckGrid(pos) || !grid.IsTransparent)
            {

                Destroy();
                return;
            }
            GameEvents.BulletMove(this);
            var target = grid.unit;
            if (target != null && target.Friendness * creator.Friendness < 0)
            {
                // 命中检定
                float hitChance = MathEx.Logistic(0.8f, accuracy - target.Ua.DamageEvasion);
                if (GD.Randf() < hitChance)
                {
                    Active(target);
                    return;
                }
            }
        }

        if (DistanceLeft <= 0 || image == null)
        {
            Destroy();
            return;
        }
        // 设置位置
        image.Position = PositionFloat * Setting.imagePx;
        // 设置朝向：因为默认朝上（0°），所以旋转方向就是速度向量的角度
        if (Speed.LengthSquared() > 0) // 避免除0
        {
            image.Rotation = Mathf.Atan2(Speed.Y, Speed.X) + Mathf.Pi / 2 - Mathf.Pi * AngleOfShapeBullet() / 180;
        }
    }
    public void Destroy(float particleRate = 1)
    {
        if(particleRate != 1)
            Animation.CreateParticle(Position, ColorOfBullet(), (int)(SizeOfBullet() * particleRate));
        else
            Animation.CreateParticle(PositionFloat, ColorOfBullet(), (int)(SizeOfBullet() * particleRate));
        Scene.CurrentMap.Bullets.Remove(this);
        image.QueueFree();
    }
    public Action<Bullet, Unit> OnActive;
    public Action<Bullet, Unit> OverrideActive;
    public bool Piercing = false;
    public void Active(Unit target)
    {
        OnActive?.Invoke(this, target);
        if(OverrideActive == null)
        {
            skill.AwakeBullet(new SkillContext(creator, target), this);
            target.Ua.TakeBulletDamage(damage, creator, skill, crit);
            skill.ActivateBullet(new SkillContext(creator, target), this);
            if (!Piercing)
                Destroy(3);
        }
        else
        {
            OverrideActive?.Invoke(this, target);
        }
        
    }
    public float AngleOfShapeBullet()
    {
        float[] angles = [
            0   ,0  ,0  ,0  ,0  ,225,225,225,225,30 ,
            0   ,90 ,225,225,225,225,90 ,0  ,0  ,0  ,
            45  ,0  ,180,180,0  ,45 ,0  ,180,180,180,
            45  ,0  ,0  ,0  ,0  ,0  ,45 ,45 ,0  ,0  ,
            0   ,0  ,180,0  ,0  ,0  ,0  ,0  ,0  ,0,
            45  ,0  ,90 ,0  ,0  ,0  ,0  ,0  ,0  ,0];
        return angles[(int)Shape];
    }
    public int SizeOfBullet()
    {
        int[] sizes = [
             4, 12, 24, 12, 32, 16,  8, 16, 16, 16,
            16, 12,  8, 12,  8, 16, 16, 12, 24, 12,
            16, 4,  24, 12, 16, 16, 32, 16, 16, 16,
            16, 24,  0,  0,  0,  0,  4,  8,  8, 12,
            16, 16, 16, 16, 16, 16,  0,  0,  0,  0,
            16, 16,  8,  8,  0,  0,  0,  0,  0,  0];
        return sizes[(int)Shape];
    }
    public Color ColorOfBullet()
    {
        Color[] colors = [
            Colors.Red, Colors.Orange, Colors.Brown, Colors.Gold, Colors.Yellow,
            Colors.Olive, Colors.Lime, Colors.Green, Colors.Teal, Colors.Turquoise,
            Colors.Cyan, Colors.LightCyan, Colors.DodgerBlue, Colors.Blue, Colors.Indigo,
            Colors.Violet, Colors.Magenta, Colors.DeepPink, Colors.Pink, Colors.White];

        return colors[(int)color];
    }
}
public enum ColorBullet
{
    Red, Orange, Brown, Gold, Yellow,
    Olive, Lime, Green, Teal, Turquoise,
    Cyan, Ice, Azure, Blue, Indigo, 
    Violet, Magenta, Rose, Pink, White
}

public enum ShapeBullet
{
    Micro, Small, Medium, Ring, Big, Knife, ArrowSmall, Laser, LaserSide, Square,
    Butterfly, ArrowBig, Grain, ArrowMedium, GrainBlack, GrainBig, Bullet, Star, StarBig, Coin,
    Water, MicroBlack, Heart, Drop, S, ArrowBow, Light, Music, StopNote, Bubble,
    Branch, Yinyang, e2, e3, e4, e5, Line, LineDouble, Cross, Curse,
    Human, Dog, Bird, Frog, Turtle, Fish, e6, e7, e8, e9,
    LaserBlack, CoinDark, ArrowKnife, Cloud, e10, e11, e12, e13, e14, e15
}
