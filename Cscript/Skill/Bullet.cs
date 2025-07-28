using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Bullet
{
    public Vector2 PositionFloat { get; set; }
    public Vector2I Position {  get; set; }
    public Vector2 Speed { get; set; }
    public float DistanceLeft { get; set; }
    private Vector2I lastPosition; // 上一次的浮点坐标
    public string Shape;
    public ColorBullet color;
    public Sprite2D image;
    public Unit creator;
    public Skill skill;
    public float damage;
    //点到点生成
    public Bullet(Unit user, Skill skil, float damag, Vector2 start, Vector2 target, float speed, float maxDistance, string shape, ColorBullet color, float advance = 0)
    {
        PositionFloat = start;
        Position = (Vector2I)start.Round();
        lastPosition = Position;
        Vector2 direction = (target - PositionFloat).Normalized();
        Speed = direction * speed;
        DistanceLeft = maxDistance;
        Shape = shape;
        this.color = color;
        image = GetImage();
        image.Scale /= 2;
        creator = user;
        skill = skil;
        damage = damag;
        Scene.CurrentMap.Bullets.Add(this);
        if (advance != 0)
            Update(advance);
    }
    //起点偏移+速度偏移生成
    public Bullet(Unit user, Skill skil, float damage, Vector2 start, Vector2 target, Vector2 point, Vector2 speedDir, float speed, float maxDistance, string shape, ColorBullet color, float advance = 0)
        : this(user, skil, damage, start + point.Rotated(Vector2.Right.AngleTo(target-start)), start + (point + speedDir).Rotated(Vector2.Right.AngleTo(target-start)), 
        speed, maxDistance, shape, color, advance) { }
    //起点偏移+速度角度生成
    public Bullet(Unit user, Skill skil, float damage, Vector2 start, Vector2 target, Vector2 point, float angle, float speed, float maxDistance, string shape, ColorBullet color, float advance = 0)
        : this(
        user, skil, damage, start + point.Rotated(Vector2.Right.AngleTo(target-start)),                        
        (start + point.Rotated(Vector2.Right.AngleTo(target-start))) +                    
        (target - start).Rotated(Mathf.DegToRad(angle)).Normalized() * 1000f,               
        speed, maxDistance, shape, color, advance) { }

    /// <summary>
    /// 更新子弹状态，返回本次穿越的新格子列表
    /// </summary>
    public List<Vector2I> UpdateGrid(float deltaTime)
    {
        List<Vector2I> newGrids = new();

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
    public Sprite2D GetImage()
    {
        var atlas = GD.Load<Texture2D>($"res://assets/Bullets/{Shape}.png");
        if (atlas == null)
            return null; // 如果图像不存在，直接返回 null

        var atlasTexture = new AtlasTexture
        {
            Atlas = atlas
        };
        int index = (int)color;
        int col = index % 4; // 每行4个
        int row = index / 4;

        atlasTexture.Region = new Rect2(col * 16, row * 16, 16, 16);
        Sprite2D sprite = new()
        {
            Texture = atlasTexture,
            ZIndex = -10
        };
        GameLoader.rootnode.AddChild(sprite);
        return sprite;
    }
    public void Update(float time)
    {
        foreach (Vector2I pos in UpdateGrid(time / 100))
        {
            if (!Scene.CurrentMap.CheckGrid(pos) || !Scene.CurrentMap.GetGrid(pos).IsTransparent)
            {
                Destroy();
                return;
            }
            else if(Scene.CurrentMap.GetGrid(pos).unit != null && 
                Scene.CurrentMap.GetGrid(pos).unit.friendness * creator.friendness < 0)
            {
                Active(Scene.CurrentMap.GetGrid(pos).unit);
                return;
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
    public void Active(Unit target)
    {
        skill.AwakeBullet(new SkillContext(creator, target), this);
        target.TakeBulletDamage(damage, creator, skill);
        skill.ActivateBullet(new SkillContext(creator, target), this);
        Destroy();
    }
}
public enum ColorBullet
{
    Red, DeepRed, Purple, DeepPurple,
    Blue, DeepBlue, GreenBlue, Ice,
    DeepGreen, Green, YellowGreen, Golden,
    Yellow, Orange, Black, White
}
