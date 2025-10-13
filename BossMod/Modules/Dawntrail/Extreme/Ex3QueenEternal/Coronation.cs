namespace BossMod.Dawntrail.Extreme.Ex3QueenEternal;

sealed class Coronation(BossModule module) : Components.GenericAOEs(module, (uint)AID.RuthlessRegalia)
{
    public struct Group
    {
        public required Actor Source;
        public Actor? LeftPartner;
        public Actor? RightPartner;

        public readonly bool Contains(Actor player) => LeftPartner == player || RightPartner == player;
    }

    public readonly List<Group> Groups = new(4);
    private DateTime _activation;

    private static readonly AOEShapeRect _shape = new(100f, 6f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Groups.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            var g = Groups[i];
            aoes[i] = new(_shape, g.Source.Position, g.Source.Rotation, _activation);
        }
        return aoes;
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        var count = Groups.Count;
        var index = -1;
        for (var i = 0; i < count; ++i)
        {
            var g = Groups[i];
            if (g.Contains(pc))
            {
                index = i;
                break;
            }
        }
        return index >= 0 && Groups[index].Contains(player) ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (ref var g in Groups.AsSpan())
        {
            Arena.Actor(g.Source, Colors.Object, true);
            if (g.Contains(pc))
            {
                if (g.LeftPartner != null)
                    Arena.AddLine(g.LeftPartner.Position, g.Source.Position);
                if (g.RightPartner != null)
                    Arena.AddLine(g.RightPartner.Position, g.Source.Position);
            }
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID is (uint)TetherID.CoronationL or (uint)TetherID.CoronationR)
        {
            _activation = WorldState.FutureTime(10.1d);
            var count = Groups.Count;
            var index = -1;
            for (var i = 0; i < count; ++i)
            {
                var g = Groups[i];
                if (g.Source.InstanceID == tether.Target)
                {
                    index = i;
                    break;
                }
            }
            if (index < 0 && WorldState.Actors.Find(tether.Target) is var target && target != null)
            {
                index = Groups.Count;
                Groups.Add(new() { Source = target });
            }
            if (index >= 0)
            {
                ref var group = ref Groups.Ref(index);
                ref var partner = ref tether.ID == (uint)TetherID.CoronationL ? ref group.LeftPartner : ref group.RightPartner;
                if (partner != null)
                    ReportError($"Both {source} and {partner} have identical tether");
                partner = source;
            }
        }
    }
}

sealed class AtomicRay(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.AtomicRayAOE, 16f);
