namespace BossMod.Dawntrail.Extreme.Ex5Necron;

sealed class GrandCross(BossModule module) : Components.RaidwideCast(module, (uint)AID.GrandCross);
sealed class GrandCrossRW(BossModule module) : Components.RaidwideCast(module, (uint)AID.GrandCrossProximity);
sealed class NeutronRing(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.NeutronRingVisual, (uint)AID.NeutronRing, 2.6d);

sealed class GrandCrossArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private readonly AOEShapeDonut donut = new(9f, 60f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ArenaChangeVisual)
        {
            _aoe = [new(donut, Trial.T05Necron.Necron.ArenaCenter, default, Module.CastFinishAt(spell, 1.1d))];
        }
    }

    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        if (updateID == 0x8000000D && param1 == 0x2u)
        {
            Arena.Bounds = Trial.T05Necron.Necron.CircleArena;
            _aoe = [];
        }
    }
}

sealed class GrandCrossBait(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GrandCrossBait, 3f);
sealed class GrandCrossSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.GrandCrossSpread, 3f);

sealed class Shock(BossModule module) : Components.GenericTowers(module)
{
    private BitMask forbidden;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Shock)
        {
            Towers.Add(new(spell.LocXZ, 3f, forbiddenSoakers: forbidden, activation: Module.CastFinishAt(spell)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Shock)
        {
            Towers.Clear();
            forbidden = default;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.GrandCross && Raid.FindSlot(actor.InstanceID) is var slot)
        {
            forbidden[slot] = true;
            var count = Towers.Count;
            var towers = CollectionsMarshal.AsSpan(Towers);
            for (var i = 0; i < count; ++i)
            {
                towers[i].ForbiddenSoakers = forbidden;
            }
        }
    }
}

sealed class GrandCrossRect(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    private static readonly AOEShapeRect rectPredict = new(50f, 2.25f, 50f), rect = new(100f, 2f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        var oid = source.OID;
        var offset = oid switch
        {
            (uint)OID.AzureAether1 => 41f.Degrees(),
            (uint)OID.AzureAether2 => -153f.Degrees(),
            _ => default
        };
        if (offset != default)
        {
            var center = Arena.Center;
            _aoes.Add(new(rectPredict, center, Angle.FromDirection(center - source.Position) + offset, WorldState.FutureTime(oid == (uint)OID.AzureAether1 ? 7.6d : 5.6d)));
            if (_aoes.Count > 1)
            {
                var aoes = CollectionsMarshal.AsSpan(_aoes);
                ref var aoe1 = ref aoes[0];
                ref var aoe2 = ref aoes[1];
                if (aoe1.Activation > aoe2.Activation)
                {
                    (aoe1, aoe2) = (aoe2, aoe1);
                }
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.GrandCrossRect) // replace prediction with actual AOE
        {
            _aoes.Ref(0) = new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GrandCrossRect)
        {
            ++NumCasts;
            if (_aoes.Count != 0)
            {
                _aoes.RemoveAt(0);
            }
        }
    }
}

sealed class GrandCrossProx(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GrandCrossProximity, new AOEShapeRect(100f, 5f));
