namespace BossMod.Shadowbringers.Alliance.A24TheCompound2P;

sealed class RelentlessSpiral(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RelentlessSpiral1, 8f);
sealed class PrimeBladeCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PrimeBladeCircle1, 20f);
sealed class PrimeBladeRect(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PrimeBladeRect1, PrimeBladeTransfer.Rect);
sealed class PrimeBladeDonut(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PrimeBladeDonut1, PrimeBladeTransfer.Donut);

sealed class RelentlessSpiralTransfer(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [with(3)];
    private static readonly AOEShapeCircle circle = new(8f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Transfer2)
        {
            _aoes.Add(new(circle, source.Position.Quantized(), default, WorldState.FutureTime(8.6d)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RelentlessSpiral2)
        {
            _aoes.Clear();
        }
    }
}

sealed class PrimeBladeTransfer(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [with(4)];
    private static readonly AOEShapeCircle circle = new(20f);
    public static readonly AOEShapeDonut Donut = new(8f, 43f);
    public static readonly AOEShapeRect Rect = new(85f, 10f);
    private readonly List<(WPos source, Actor target)> _tethers = [with(4)]; // tether target can teleport after tether got applied (in the same frame), leading to incorrect locations if used directly in OnTethered
    private readonly List<(AOEShape shape, WPos origin, DateTime activation)> _casters = [with(4)];
    private readonly A24TheCompound2P bossmod = (A24TheCompound2P)module;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Transfer1 && WorldState.Actors.Find(tether.Target) is Actor t)
        {
            _tethers.Add((source.Position, t));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = spell.Action.ID switch
        {
            (uint)AID.PrimeBladeCircle2 => circle,
            (uint)AID.PrimeBladeDonut2 => Donut,
            (uint)AID.PrimeBladeRect2 => Rect, // TODO: rectangle teleport changes rotation of spell, need more long replays to find out how it works
            _ => null
        };
        if (shape != null)
        {
            _casters.Add((shape, caster.Position, Module.CastFinishAt(spell)));
        }
    }

    public override void Update()
    {
        var count = _casters.Count - 1;
        for (var i = count; i >= 0; --i)
        {
            var c = _casters[i];
            var countT = _tethers.Count - 1;
            for (var j = countT; j >= 0; --j)
            {
                var t = _tethers[j];
                if (c.origin.AlmostEqual(t.target.Position, 1f))
                {
                    if (c.shape != Rect)
                    {
                        AddAOE(c.shape, t.source, default, c.activation);
                        RemoveCasterAndTether(i, j);
                    }
                    else
                    {
                        // until we know how the teleporting rect works we just wait with drawing it until the caster teleported
                        if (t.source.AlmostEqual(bossmod.BossP2!.Position, 1f))
                        {
                            AddAOE(Rect, t.source, bossmod.BossP2.Rotation, c.activation);
                            RemoveCasterAndTether(i, j);
                            break;
                        }
                        var puppets = Module.Enemies((uint)OID.Puppet2P);
                        var countP = puppets.Count;
                        for (var k = 0; k < countP; ++k)
                        {
                            var p = puppets[k];
                            if (t.source.AlmostEqual(p.Position, 1f))
                            {
                                AddAOE(Rect, t.source, p.Rotation, c.activation);
                                RemoveCasterAndTether(i, j);
                                break;
                            }
                        }
                    }
                    break;
                }
            }
            void AddAOE(AOEShape shape, WPos position, Angle rotation, DateTime activation) => _aoes.Add(new(shape, position.Quantized(), rotation, activation));

            void RemoveCasterAndTether(int casterIndex, int tetherIndex)
            {
                _casters.RemoveAt(casterIndex);
                _tethers.RemoveAt(tetherIndex);
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.PrimeBladeCircle2:
            case (uint)AID.PrimeBladeDonut2:
            case (uint)AID.PrimeBladeRect2:
                _aoes.Clear();
                break;
            case (uint)AID.ForcedTransfer1:
            case (uint)AID.ForcedTransfer2:
                _tethers.Clear(); // other mechanics use the same tether ID
                break;
        }
    }
}
