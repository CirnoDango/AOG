using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;

public class Bullet
{
    public Vector2 PositionFloat { get; set; }
    public Vector2I Position {  get; set; }
    public Vector2 Speed { get; set; }
    public float DistanceLeft { get; set; }
    private Vector2I lastPosition; // 上一次的浮点坐标
    public ShapeBullet Shape;
    public ColorBullet color;
    public Sprite2D image;
    public Unit creator;
    public Unit tracing;
    public Skill skill;
    public float damage;
    public float accuracy;
    public static Bullet CreateBullet(Unit user, Skill skil, float damag, Vector2 start, Vector2 target,
        float speed, float maxDistance, ShapeBullet shape, ColorBullet color, float advance = 0, Grid trace = null)
    {
        Bullet b = new(user, skil, damag, start, target,
        speed, maxDistance, shape, color, advance, trace);
        GameEvents.CreateBullet(user, b);
        return b;
    }
    //起点偏移+速度偏移生成
    public static Bullet CreateBullet(Unit user, Skill skil, float damage, Vector2 start, Vector2 target, Vector2 point, Vector2 speedDir,
        float speed, float maxDistance, ShapeBullet shape, ColorBullet color, float advance = 0, Grid trace = null)
    {
        return CreateBullet(user, skil, damage, start + point.Rotated(Vector2.Right.AngleTo(target - start)), start + (point + speedDir).Rotated(Vector2.Right.AngleTo(target - start)),
        speed, maxDistance, shape, color, advance, trace);
    }
    //起点偏移+速度角度生成
    public static Bullet CreateBullet(Unit user, Skill skil, float damage, Vector2 start, Vector2 target, Vector2 point,
        float angle, float speed, float maxDistance, ShapeBullet shape, ColorBullet color, float advance = 0, Grid trace = null)
    {
        return CreateBullet(user, skil, damage, start + point.Rotated(Vector2.Right.AngleTo(target - start)),
        (start + point.Rotated(Vector2.Right.AngleTo(target - start))) +
        (target - start).Rotated(Mathf.DegToRad(angle)).Normalized() * 1000f,
        speed, maxDistance, shape, color, advance, trace);
    }
    //点到点生成
    public Bullet(Unit user, Skill skil, float damag, Vector2 start, Vector2 target, 
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
        image.Scale /= 2;
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
                if (unit == user && user.RandomEnemyInVision(out Unit u))
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

        // 追踪弹的加速度
        if (tracing != null && Scene.CurrentMap.Units.Contains(tracing))
        {
            Speed = Speed.Length() * (Speed + 1.5f * deltaTime * (Vector2)(tracing.Position - Position))
                .Normalized();
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
        GameLoader.rootnode.AddChild(sprite);
        return sprite;
        
    }
    public static Sprite2D ReadImage(ColorBullet color, ShapeBullet shape)
    {
        var atlas = GD.Load<Texture2D>($"res://Assets/Bullets/{color}.png");
        if (atlas == null)
            return null; // 如果图像不存在，直接返回 null

        var atlasTexture = new AtlasTexture
        {
            Atlas = atlas
        };
        int index = (int)shape;
        int col = index % 10; // 每行4个
        int row = index / 10;

        atlasTexture.Region = new Rect2(col * 32, row * 32, 32, 32);
        Sprite2D sprite = new()
        {
            Texture = atlasTexture,
            ZIndex = -10
        };
        return sprite;
    }
    public void Update(float time)
    {
        foreach (Vector2I pos in UpdateGrid(time / 100))
        {
            var grid = Scene.CurrentMap.GetGrid(pos);
            if (!Scene.CurrentMap.CheckGrid(pos) || !grid.IsTransparent)
            {
                Destroy();
                return;
            }

            var target = grid.unit;
            if (target != null && target.friendness * creator.friendness < 0)
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
        image.Position = PositionFloat * 16;
        // 设置朝向：因为默认朝上（0°），所以旋转方向就是速度向量的角度
        if (Speed.LengthSquared() > 0) // 避免除0
        {
            image.Rotation = Mathf.Atan2(Speed.Y, Speed.X) + Mathf.Pi / 2;
        }
    
}
    public void Destroy()
    {
        Scene.CurrentMap.Bullets.Remove(this);
        image.QueueFree();
    }
    public Action<Unit> OnActive;
    public void Active(Unit target)
    {
        OnActive?.Invoke(target);
        skill.AwakeBullet(new SkillContext(creator, target), this);
        target.TakeBulletDamage(damage, creator, skill);
        skill.ActivateBullet(new SkillContext(creator, target), this);
        Destroy();
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
