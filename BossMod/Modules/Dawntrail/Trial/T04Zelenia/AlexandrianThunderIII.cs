namespace BossMod.Dawntrail.Trial.T04Zelenia;

sealed class AlexandrianThunderIII(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(16f, 30f.Degrees());
    private static readonly AOEShapeCircle circle = new(4f);
    private readonly List<AOEInstance> _aoes = new(4);
    private readonly bool[] activeTiles = new bool[6];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var result = new AOEInstance[count];
        Span<bool> visited = stackalloc bool[6];
        const float slice = 1f / 60f;

        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (aoe.Shape == circle)
            {
                result[index++] = aoe;
            }
            else // filter aoes that hit the same spot more than once since that looks ugly and wastes many cpu cycles
            {
                var sliceIdx = (int)MathF.Round((180f - aoe.Rotation.Deg) * slice);
                if (!visited[sliceIdx])
                {
                    visited[sliceIdx] = true;
                    result[index++] = aoe;
                }
            }
        }
        return result.AsSpan()[..index];
    }

    public override void OnMapEffect(byte index, uint state)
    {
        // arena slices:
        // 0x04: 180°  → index 0
        // 0x05: 120°  → index 1
        // 0x06: 60°   → index 2
        // 0x07: 0°    → index 3
        // 0x08: -60°  → index 4
        // 0x09: -120° → index 5
        if (index is >= 0x04 and <= 0x09)
        {
            switch (state)
            {
                case 0x00400100u:
                    activeTiles[index - 0x04] = true;
                    break;
                case 0x00040020u:
                    activeTiles[index - 0x04] = false;
                    break;
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.AlexandrianThunderIII1 or (uint)AID.AlexandrianThunderIII2)
        {
            Span<bool> visited = stackalloc bool[6];
            var pos = Arena.Center;
            var loc = spell.LocXZ;
            var rot = 30f.Degrees();
            var id = caster.InstanceID;
            var act = Module.CastFinishAt(spell);

            _aoes.Add(new(circle, loc, default, act, actorID: id));

            for (var i = 0; i < 6; ++i)
            {
                if (!activeTiles[i] || visited[i])
                    continue;

                var intersects = Intersect.CircleCone(loc, 4f, pos, 16f, (180f - 60f * i).Degrees().ToDirection(), rot);

                if (!intersects)
                    continue;

                var left = i;
                while (true)
                {
                    var prev = (left - 1 + 6) % 6;
                    if (!activeTiles[prev] || visited[prev])
                        break;
                    left = prev;
                }

                var right = i;
                while (true)
                {
                    var next = (right + 1) % 6;
                    if (!activeTiles[next] || visited[next])
                        break;
                    right = next;
                }

                var j = left;
                while (true)
                {
                    if (!visited[j])
                    {
                        visited[j] = true;
                        _aoes.Add(new(cone, pos, (180f - 60f * j).Degrees(), act, actorID: id));
                    }

                    if (j == right)
                        break;

                    j = (j + 1) % 6;
                }
                break;
            }
        }
        else if (spell.Action.ID is (uint)AID.AlexandrianThunderIVCircle2 or (uint)AID.AlexandrianThunderIVDonut2 && _aoes.Count == 0)
        {
            var pos = Arena.Center;
            var act = Module.CastFinishAt(spell);
            for (var i = 0; i < 6; ++i)
            {
                if (activeTiles[i])
                {
                    _aoes.Add(new(cone, pos, (180f - 60f * i).Degrees(), act));
                }
            }
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x1E46)
        {
            var rotRounded = (int)actor.Rotation.Deg;
            var index = -1;
            switch (rotRounded)
            {
                case 143: // 82.679, 110
                case -143: // 117.321, 110
                case 180: // 100, 120
                    index = 0;
                    break;
                case 83: // 82.679, 90
                case 156: // 100, 120
                case 119: // 82.679, 110
                    index = 1;
                    break;
                case 96: // 82.679, 110
                case 23: // 100, 80
                case 59: // 82.679, 90
                    index = 2;
                    break;
                case 0: // 100, 80
                case -36: // 117.321, 90
                case 36: // 82.679, 90
                    index = 3;
                    break;
                case -96: // 117.321, 110
                case -23: // 100, 80
                case -60: // 117.321, 90
                    index = 4;
                    break;
                case -120: // 117.321, 110
                case -156: // 100, 120
                case -83: // 117.321, 90
                    index = 5;
                    break;

            }
            if (index >= 0)
            {
                activeTiles[index] = true;
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.AlexandrianThunderIII1:
            case (uint)AID.AlexandrianThunderIII2:
                var count = _aoes.Count - 1;
                var id = caster.InstanceID;
                for (var i = count; i >= 0; --i)
                {
                    if (_aoes[i].ActorID == id)
                    {
                        _aoes.RemoveAt(i);
                    }
                }
                break;
            case (uint)AID.AlexandrianThunderIVCircle2:
            case (uint)AID.AlexandrianThunderIVDonut2:
                if (++NumCasts == 2)
                {
                    _aoes.Clear();
                    NumCasts = 0;
                }
                break;
        }
    }
}
