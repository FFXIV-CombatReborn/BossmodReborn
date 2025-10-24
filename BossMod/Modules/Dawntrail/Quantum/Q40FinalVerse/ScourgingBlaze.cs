namespace BossMod.Dawntrail.Quantum.FinalVerse.Q40EminentGrief;

[SkipLocalsInit]
sealed class ScourgingBlaze(BossModule module) : Components.Exaflare(module, 5f)
{
    private readonly List<(WDir, WPos)> crystals = new(12);
    private WDir next;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ScourgingBlazeFirst)
        {
            var count = crystals.Count;
            var pos = caster.Position;
            var crys = CollectionsMarshal.AsSpan(crystals);

            for (var i = 0; i < count; ++i)
            {
                ref var c = ref crys[i];
                var loc = c.Item2;
                if (loc.AlmostEqual(pos, 1f))
                {
                    var dir = c.Item1;
                    var offset = loc - Arena.Center;
                    var intersect = (int)Intersect.RayAABB(offset, dir, 18.1f, 14.1f);
                    var maxexplosions = intersect / 4 + 1;
                    var act = Module.CastFinishAt(spell);
                    Lines.Add(new(loc, 4f * dir, act, 1d, maxexplosions, maxexplosions, rotation: dir.ToAngle()));
                    intersect = (int)Intersect.RayAABB(offset, -dir, 18.1f, 14.1f);
                    maxexplosions = intersect / 4 + 1;
                    Lines.Add(new(loc, -4f * dir, act, 1d, maxexplosions, maxexplosions, rotation: (-dir).ToAngle()));
                    if (Lines.Count == 24)
                    {
                        crystals.Clear();
                    }
                    return;
                }
            }
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Crystal)
        {
            crystals.Add((next, actor.Position));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ScourgingBlazeVisualWE1:
            case (uint)AID.ScourgingBlazeVisualWE2:
                next = new(1f, default);
                break;
            case (uint)AID.ScourgingBlazeVisualNS1:
            case (uint)AID.ScourgingBlazeVisualNS2:
                next = new(default, 1f);
                break;
            case (uint)AID.ScourgingBlazeFirst:
                var pos = caster.Position;

                var count = Lines.Count - 1;
                for (var i = count; i >= 0; --i)
                {
                    var line = Lines[i];
                    if (line.Next.AlmostEqual(pos, 0.1f) && (WorldState.CurrentTime - line.NextExplosion).TotalSeconds < 1d)
                    {
                        AdvanceLine(line, pos);
                        if (line.ExplosionsLeft == 0)
                        {
                            Lines.RemoveAt(i);
                        }
                    }
                }
                break;
            case (uint)AID.ScourgingBlazeRest:
                var pos2 = spell.TargetXZ;

                var count2 = Lines.Count;
                for (var i = 0; i < count2; ++i)
                {
                    var line = Lines[i];
                    if (line.Next.AlmostEqual(pos2, 0.1f) && caster.Rotation.AlmostEqual(line.Rotation, Angle.DegToRad))
                    {
                        AdvanceLine(line, pos2);
                        if (line.ExplosionsLeft == 0)
                        {
                            Lines.RemoveAt(i);
                        }
                        return;
                    }
                }
                break;
        }
    }
}
