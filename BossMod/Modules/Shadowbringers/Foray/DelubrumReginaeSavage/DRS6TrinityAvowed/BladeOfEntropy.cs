namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS6TrinityAvowed;

// note: instead of trying to figure out cone intersections and shit, we use the fact that clones are always positioned on grid and just check each cell
sealed class BladeOfEntropy(BossModule module) : TemperatureAOE(module)
{
    private readonly List<(Actor caster, WDir dir, int temperature)> _casters = [];

    private readonly AOEShapeRect _shapeCell = new(5f, 5f, 5f);

    private readonly List<AOEInstance> _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var playerTemp = Math.Clamp(Temperature(actor), -2, +2);
        var aoes = _aoesBuffer;
        aoes.Clear();
        for (var x = -2; x <= +2; ++x)
        {
            for (var z = -2; z <= +2; ++z)
            {
                var cellCenter = Arena.Center + 10f * new WDir(x, z);
                var temperature = 0;
                var numClips = 0;
                DateTime activation = default;
                var count = _casters.Count;
                for (var i = 0; i < count; ++i)
                {
                    var c = _casters[i];
                    activation = Module.CastFinishAt(c.caster.CastInfo);
                    if (c.dir.Dot(cellCenter - c.caster.Position) > 0)
                    {
                        temperature = c.temperature;
                        if (++numClips > 1)
                            break;
                    }
                }

                if (numClips > 1)
                    aoes.Add(new(_shapeCell, cellCenter, default, activation));
                else if (activation != default && temperature == -playerTemp)
                    aoes.Add(new(_shapeCell, cellCenter, default, activation, Colors.SafeFromAOE, false));
            }
        }
        return CollectionsMarshal.AsSpan(aoes);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.BladeOfEntropyAC11:
            case (uint)AID.BladeOfEntropyBC11:
                _casters.Add((caster, spell.Rotation.ToDirection(), -1));
                break;
            case (uint)AID.BladeOfEntropyAC21:
            case (uint)AID.BladeOfEntropyBC21:
                _casters.Add((caster, spell.Rotation.ToDirection(), -2));
                break;
            case (uint)AID.BladeOfEntropyAH11:
            case (uint)AID.BladeOfEntropyBH11:
                _casters.Add((caster, spell.Rotation.ToDirection(), +1));
                break;
            case (uint)AID.BladeOfEntropyAH21:
            case (uint)AID.BladeOfEntropyBH21:
                _casters.Add((caster, spell.Rotation.ToDirection(), +2));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.BladeOfEntropyAC11 or (uint)AID.BladeOfEntropyBC11 or (uint)AID.BladeOfEntropyAC21 or (uint)AID.BladeOfEntropyBC21
        or (uint)AID.BladeOfEntropyAH11 or (uint)AID.BladeOfEntropyBH11 or (uint)AID.BladeOfEntropyAH21 or (uint)AID.BladeOfEntropyBH21)
        {
            var count = _casters.Count;
            for (var i = 0; i < count; ++i)
            {
                if (_casters[i].caster == caster)
                {
                    _casters.RemoveAt(i);
                    return;
                }
            }
        }
    }
}
