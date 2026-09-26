namespace BossMod.Components;

// generic component for cleaving autoattacks; shows shape outline and warns when anyone other than main target is inside
// enemy OID == 0 means 'primary actor'

public class Cleave(BossModule module, uint aid, AOEShape shape, uint[]? enemyOID = null, bool activeForUntargetable = false, bool originAtTarget = false, bool activeWhileCasting = true,
    int? arenaProjectionLayer = null, bool? restrictToArenaProjectionLayer = false) : CastCounter(module, aid)
{
    public readonly AOEShape Shape = shape;
    public readonly bool ActiveForUntargetable = activeForUntargetable;
    public readonly bool ActiveWhileCasting = activeWhileCasting;
    public readonly bool OriginAtTarget = originAtTarget;
    public DateTime NextExpected;
    public readonly uint[] EnemyOID = enemyOID ?? [module.PrimaryActor.OID];
    public int? ArenaProjectionLayer = arenaProjectionLayer;
    public bool? RestrictToArenaProjectionLayer = restrictToArenaProjectionLayer;
    public bool AllowPetTargets;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!ArenaProjectionLayerParticipantApplies(actor, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
        {
            return;
        }

        var origins = OriginsAndTargets();
        var count = origins.Count;
        if (count == 0)
        {
            return;
        }

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
        if (!ArenaProjectionLayerApplies(actor, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
        {
            return;
        }

        var origins = OriginsAndTargets();
        var count = origins.Count;
        if (count == 0)
        {
            return;
        }

        for (var i = 0; i < count; ++i)
        {
            var e = origins[i];
            if (actor != e.target)
            {
                hints.AddForbiddenZone(Shape, e.origin.Position.Quantized(), e.angle, NextExpected, arenaProjectionLayer: ArenaProjectionLayerForAI(ArenaProjectionLayer, RestrictToArenaProjectionLayer));
            }
            else
            {
                AddTargetSpecificHints(ref actor, ref e.origin, ref hints);
            }
        }
    }

    private void AddTargetSpecificHints(ref Actor actor, ref Actor source, ref AIHints hints)
    {
        var sourcePosition = source.Position;
        var raid = Raid.WithoutSlot();
        var len = raid.Length;
        for (var i = 0; i < len; ++i)
        {
            var a = raid[i];
            // A lot of mechanics don't care if the pet is hit or not, e.g. even if the pet is inside the bait it doesn't do anything to them
            // Also check to ensure the pet isn't dead (swapping pets will make them stay there for like 2 seconds)
            if (!AllowPetTargets && a.Type == ActorType.Pet && !a.IsDead)
            {
                continue;
            }

            if (a == actor)
            {
                continue;
            }
            if (!ArenaProjectionLayerParticipantApplies(a, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
            {
                continue;
            }

            var radius = a.Type == ActorType.Pet ? a.HitboxRadius : 0f;

            ShapeDistance zone;

            switch (Shape)
            {
                case AOEShapeCircle circle:
                    {
                        var distance = Shape.Distance(a.Position.Quantized(), a.Rotation);
                        zone = radius > 0f ? new ExpandedBaitZone(distance, radius) : distance;
                        break;
                    }

                case AOEShapeCone cone:
                    {
                        var distance = (a.Position - sourcePosition).Length();
                        var halfAngle = distance <= radius ? new Angle(MathF.PI) : cone.HalfAngle + Angle.Asin(Math.Clamp(radius / distance, 0f, 1f));
                        zone = BaitAimZone(sourcePosition, source.AngleTo(a) - cone.DirectionOffset, halfAngle, cone.Radius);
                        break;
                    }

                case AOEShapeRect rect:
                    {
                        var distance = (a.Position - sourcePosition).Length();
                        var halfAngle = RectBaitHalfAngle(distance, rect.HalfWidth, radius);
                        zone = BaitAimZone(sourcePosition, source.AngleTo(a) - rect.DirectionOffset, halfAngle, rect.LengthFront);
                        break;
                    }
                default:
                    continue;
            }

            hints.AddForbiddenZone(zone, NextExpected, arenaProjectionLayer: ArenaProjectionLayerForAI(ArenaProjectionLayer, RestrictToArenaProjectionLayer));
        }
    }

    private sealed class ExpandedBaitZone(ShapeDistance inner, float radius) : ShapeDistance {
        public override float Distance(in WPos p) => inner.Distance(p) - radius;
    }

    private static ShapeDistance BaitAimZone(WPos source, Angle direction, Angle halfAngle, float zoneRadius) {
        return halfAngle.Rad >= MathF.PI ? new SDCircle(source, zoneRadius) : new SDCone(source, zoneRadius, direction, halfAngle);
    }

    // Angular interval in which a circular hitbox intersects a forward, infinitely long rectangle
    private static Angle RectBaitHalfAngle(float distance, float halfWidth, float radius) {
        // The party member (pet, since players count as a point) overlaps the source: changing aim cannot avoid it
        if (distance <= radius) {
            return new Angle(MathF.PI);
        }

        // Normal case: contact is against one of the rectangle's long sides
        if (distance >= halfWidth + radius) {
            return Angle.Asin(Math.Clamp((halfWidth + radius) / distance, 0f, 1f));
        }

        var distanceSq = distance * distance;
        var widthSq = halfWidth * halfWidth;
        var radiusSq = radius * radius;
        var rightAngle = new Angle(Angle.HalfPi);

        // Close to the source: contact is against the rectangle's rear face
        if (distanceSq <= widthSq + radiusSq) {
            return rightAngle + Angle.Asin(Math.Clamp(radius / distance, 0f, 1f));
        }

        // Otherwise, contact is around a rear corner halfWidth is positive here; zero width takes the normal-case branch
        var cosAngle = (distanceSq + widthSq - radiusSq) / (2f * distance * halfWidth);

        return rightAngle + Angle.Acos(Math.Clamp(cosAngle, -1f, 1f));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var origins = OriginsAndTargets();
        var count = origins.Count;

        for (var i = 0; i < count; ++i)
        {
            var e = origins[i];
            using (Arena.WorldProjectionLayer(ArenaProjectionLayer, RestrictToArenaProjectionLayer))
            {
                Shape.Outline(Arena, e.origin.Position.Quantized(), e.angle);
            }
        }
    }

    public virtual List<(Actor origin, Actor target, Angle angle)> OriginsAndTargets()
    {
        var enemies = Module.Enemies(EnemyOID);
        var count = enemies.Count;
        List<(Actor, Actor, Angle)> origins = [with(count)];
        for (var i = 0; i < count; ++i)
        {
            var enemy = enemies[i];
            if (enemy.IsDead)
            {
                continue;
            }

            if (!ActiveForUntargetable && !enemy.IsTargetable)
            {
                continue;
            }

            if (!ActiveWhileCasting && enemy.CastInfo != null)
            {
                continue;
            }

            var target = WorldState.Actors.Find(enemy.TargetID);
            if (target != null && ArenaProjectionLayerParticipantApplies(target, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
            {
                origins.Add(new(OriginAtTarget ? target : enemy, target, Angle.FromDirection(target.Position - enemy.Position)));
            }
        }
        return origins;
    }
}
