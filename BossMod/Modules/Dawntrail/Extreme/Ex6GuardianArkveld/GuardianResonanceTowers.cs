namespace BossMod.Dawntrail.Extreme.Ex6GuardianArkveld;

sealed class GuardianResonanceTowers(BossModule module) : Components.GenericTowers(module)
{
    private BitMask forbidden = GetTanks(module);
    private bool aoesStarted, aoesEnded;

    private static BitMask GetTanks(BossModule module)
    {
        var party = module.Raid.WithSlot(true, false, false);
        var len = party.Length;
        BitMask forbidden = default;
        for (var i = 0; i < len; ++i)
        {
            ref var p = ref party[i];
            if (p.Item2.Role == Role.Tank)
            {
                forbidden.Set(p.Item1);
            }
        }
        return forbidden;
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Towers.Count != 0)
        {
            hints.Add("Bait 3 AOEs before going into the towers!");
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        var radius = spell.Action.ID switch
        {
            (uint)AID.GuardianResonanceTower1 => 4f,
            (uint)AID.GuardianResonanceTower2 => 2f,
            _ => default
        };
        if (radius > 0f)
        {
            var loc = spell.LocXZ;
            Towers.Add(new(loc, radius, forbiddenSoakers: radius == 2f ? forbidden : ~forbidden, activation: Module.CastFinishAt(spell), actorID: (ulong)radius,
            shapeDistance: new SDCircle(loc, radius + 4.5f), invertedShapeDistance: new SDInvertedCircle(loc, radius)));
        }
        else if (id == (uint)AID.GuardianResonanceAOE)
        {
            if (!aoesStarted)
            {
                aoesStarted = true;
                var towers = CollectionsMarshal.AsSpan(Towers);
                var f = ~(BitMask)default;
                for (var i = 0; i < 6; ++i)
                {
                    towers[i].ForbiddenSoakers = f;
                }
            }
            else if (!aoesEnded && (Towers.Ref(0).Activation - WorldState.CurrentTime).TotalSeconds < 4d)
            {
                var towers = CollectionsMarshal.AsSpan(Towers);
                for (var i = 0; i < 6; ++i)
                {
                    ref var t = ref towers[i];
                    t.ForbiddenSoakers = t.ActorID == 2ul ? forbidden : ~forbidden;
                }
                aoesEnded = true;
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GuardianResonanceTower1)
        {
            ++NumCasts;
        }
    }
}
