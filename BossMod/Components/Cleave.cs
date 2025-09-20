namespace BossMod.Components;

// generic component for cleaving autoattacks; shows shape outline and warns when anyone other than main target is inside
// enemy OID == 0 means 'primary actor'

[SkipLocalsInit]
public class Cleave(BossModule module, uint aid, AOEShape shape, uint[]? enemyOID = null, bool activeForUntargetable = false, bool originAtTarget = false, bool activeWhileCasting = true) : CastCounter(module, aid)
{
    public readonly AOEShape Shape = shape;
    public readonly bool ActiveForUntargetable = activeForUntargetable;
    public readonly bool ActiveWhileCasting = activeWhileCasting;
    public readonly bool OriginAtTarget = originAtTarget;
    public DateTime NextExpected;
    public readonly uint[] EnemyOID = enemyOID ?? [module.PrimaryActor.OID];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var origins = OriginsAndTargets();
        var count = origins.Count;
        if (count == 0)
            return;

        for (var i = 0; i < count; ++i)
        {
            var e = origins[i];
            if (actor != e.target && Shape.Check(actor.Position.Quantized(), e.origin.Position, e.angle))
            {
                hints.Add("GTFO from cleave!");
                break;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var origins = OriginsAndTargets();
        var count = origins.Count;
        if (count == 0)
            return;

        for (var i = 0; i < count; ++i)
        {
            var e = origins[i];
            if (actor != e.target)
                hints.AddForbiddenZone(Shape, e.origin.Position.Quantized(), e.angle, NextExpected);
            else
                AddTargetSpecificHints(ref actor, ref e.origin, ref hints);
        }
    }

    private void AddTargetSpecificHints(ref Actor actor, ref Actor source, ref AIHints hints)
    {
        var raid = Raid.WithoutSlot();
        var len = raid.Length;
        for (var i = 0; i < len; ++i)
        {
            var a = raid[i];
            if (a == actor)
                continue;
            switch (Shape)
            {
                case AOEShapeCircle circle:
                    hints.AddForbiddenZone(circle, a.Position.Quantized());
                    break;
                case AOEShapeCone cone:
                    hints.AddForbiddenZone(new SDCone(source.Position.Quantized(), 100f, source.AngleTo(a), cone.HalfAngle));
                    break;
                case AOEShapeRect rect:
                    hints.AddForbiddenZone(new SDCone(source.Position.Quantized(), 100f, source.AngleTo(a), Angle.Asin(rect.HalfWidth / (a.Position - source.Position).Length())));
                    break;
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var origins = OriginsAndTargets();
        var count = origins.Count;

        for (var i = 0; i < count; ++i)
        {
            var e = origins[i];
            Shape.Outline(Arena, e.origin.Position.Quantized(), e.angle);
        }
    }

    public virtual List<(Actor origin, Actor target, Angle angle)> OriginsAndTargets()
    {
        var enemies = Module.Enemies(EnemyOID);
        var count = enemies.Count;
        List<(Actor, Actor, Angle)> origins = new(count);
        for (var i = 0; i < count; ++i)
        {
            var enemy = enemies[i];
            if (enemy.IsDead)
                continue;

            if (!ActiveForUntargetable && !enemy.IsTargetable)
                continue;

            if (!ActiveWhileCasting && enemy.CastInfo != null)
                continue;

            var target = WorldState.Actors.Find(enemy.TargetID);
            if (target != null)
            {
                origins.Add(new(OriginAtTarget ? target : enemy, target, Angle.FromDirection(target.Position - enemy.Position)));
            }
        }
        return origins;
    }
}
