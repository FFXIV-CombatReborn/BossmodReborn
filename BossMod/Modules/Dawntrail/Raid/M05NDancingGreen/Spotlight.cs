namespace BossMod.Dawntrail.Raid.M05NDancingGreen;

sealed class Spotlight(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(4);
    private static readonly AOEShapeCircle circle = new(2.5f);
    private bool active;
    private static readonly WPos[] threeSpotlights = [new(87.48f, 112.474f), new(112.474f, 99.992f), new(99.992f, 87.48f), new(112.474f, 87.48f), new(87.48f, 99.992f), new(99.992f, 112.474f)];
    private static readonly WPos[] fourSpotlights1 = [new(94.987f, 87.48f), new(104.997f, 112.474f), new(112.474f, 94.987f), new(87.48f, 104.997f)];
    private static readonly WPos[] fourSpotlights2 = [new(104.997f, 87.48f), new(87.48f, 94.987f), new(112.474f, 104.997f), new(94.987f, 112.474f)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        return active ? aoes.Length == 6 ? aoes[..3] : aoes : [];
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.BurnBabyBurn)
        {
            active = true;
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            var len = aoes.Length;
            var act = WorldState.FutureTime(8d);
            for (var i = 0; i < len; ++i)
            {
                ref var aoe = ref aoes[i];
                aoe.Activation = act;
            }
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (active && id is 0x2FCF or 0x2FD0)
        {
            var party = Raid.WithoutSlot(true, false, false);
            var len = party.Length;
            var first = false;
            for (var i = 0; i < len; ++i)
            {
                var p = party[i];
                if (p.FindStatus((uint)SID.BurnBabyBurn) != null)
                {
                    first = true;
                    break;
                }
            }
            if (first && _aoes.Count == 6)
            {
                _aoes.RemoveRange(0, 3);
            }
            else if (!first)
            {
                _aoes.Clear();
            }
            active = false;
        }
        else if (_aoes.Count == 0 && id == 0x11DC)
        {
            WPos[] positions = [];
            var position = actor.Position;
            if (position == new WPos(112.5f, 87.5f))
            {
                positions = threeSpotlights;
            }
            else if (position == new WPos(87.5f, 112.5f))
            {
                positions = threeSpotlights.ReverseArray();
            }
            else if (position == new WPos(95f, 87.5f))
            {
                positions = fourSpotlights1;
            }
            else if (position == new WPos(95f, 112.5f))
            {
                positions = fourSpotlights2;
            }

            var len = positions.Length;
            if (len != 0)
            {
                var col = Colors.SafeFromAOE;
                for (var i = 0; i < len; ++i)
                {
                    _aoes.Add(new(circle, positions[i], risky: false, color: col));
                }
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = _aoes.Count;
        if (count < 2 || !active)
            return;
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var countAdj = count == 6 ? 3 : count;
        var forbidden = new ShapeDistance[countAdj];

        for (var i = 0; i < countAdj; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            forbidden[i] = new SDInvertedCircle(aoe.Origin, 2.5f);
        }
        hints.AddForbiddenZone(new SDIntersection(forbidden), aoes[0].Activation);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!active)
        {
            return;
        }
        var aoes = ActiveAOEs(slot, actor);
        var len = aoes.Length;
        var isInside = false;
        var pos = actor.Position;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (aoe.Check(pos))
            {
                isInside = true;
                break;
            }
        }
        hints.Add("Go into a spotlight!", !isInside);
    }
}
