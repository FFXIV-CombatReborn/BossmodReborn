namespace BossMod.Dawntrail.Alliance.A14ShadowLord;

sealed class DarkNebula(BossModule module) : Components.GenericKnockback(module)
{
    private const float Length = 4f;
    private const float HalfWidth = 1.75f;

    public readonly List<Actor> Casters = new(4);

    private static readonly Angle a90 = 90f.Degrees();
    private static readonly WDir[] directions = [45f.Degrees().ToDirection(), -135f.Degrees().ToDirection(), -45f.Degrees().ToDirection(), 135f.Degrees().ToDirection()];

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        var count = Casters.Count;
        if (count == 0)
            return [];
        var max = count > 2 ? 2 : count;
        var sources = new Knockback[max];
        for (var i = 0; i < max; ++i)
        {
            var caster = Casters[i];
            var dir = caster.CastInfo?.Rotation ?? caster.Rotation;
            var kind = dir.ToDirection().OrthoL().Dot(actor.Position - caster.Position) >= 0 ? Kind.DirLeft : Kind.DirRight;
            sources[i] = new(caster.Position, 20f, Module.CastFinishAt(caster.CastInfo), null, dir, kind);
        }
        return sources;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.DarkNebulaShort or (uint)AID.DarkNebulaLong)
            Casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.DarkNebulaShort or (uint)AID.DarkNebulaLong)
        {
            ++NumCasts;
            Casters.Remove(caster);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count == 0)
            return;

        var forbidden = new List<ShapeDistance>(2);
        var caster0 = Casters[0];

        var rot = (int)caster0.Rotation.Deg;
        (int[], WDir) indices = rot switch
        {
            -45 => ([2, 0], directions[2]),
            -135 => ([1, 2], directions[3]),
            134 => ([3, 1], directions[0]),
            44 => ([0, 3], directions[1]),
            _ => default
        };
        var act = Module.CastFinishAt(Casters[0].CastInfo);
        if (Casters.Count == 1)
        {
            if (indices != default)
            {
                for (var i = 0; i < 2; ++i)
                {
                    forbidden.Add(new SDInvertedRect(A14ShadowLord.Circles[indices.Item1[i]].Center, indices.Item2, Length, default, HalfWidth));
                }
            }
            hints.AddForbiddenZone(new SDIntersection([.. forbidden]), act);
        }
        else
        {
            var caster1 = Casters[1];
            var rotationMatch = caster0.Rotation.AlmostEqual(caster1.Rotation + a90, Angle.DegToRad);
            var circleIndex = rotationMatch ? indices.Item1[0] : indices.Item1[1];
            hints.AddForbiddenZone(new SDInvertedRect(A14ShadowLord.Circles[circleIndex].Center, indices.Item2, Length, 0f, HalfWidth), act);
        }
    }
}
