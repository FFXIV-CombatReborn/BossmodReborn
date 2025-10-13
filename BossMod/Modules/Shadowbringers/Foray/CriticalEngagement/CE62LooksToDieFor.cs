namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE62LooksToDieFor;

public enum OID : uint
{
    Boss = 0x31C9, // R5.950, x1
    PurpleLevin = 0x31CA, // R1.000-2.000, spawn during fight
    BallOfFire = 0x31CB, // R1.000, spawn during fight
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target

    Thundercall = 23964, // Boss->self, 4.0s cast, single-target, visual (summon orbs)
    ThundercallVisual = 23965, // Helper->self, 4.5s cast, single-target, ???
    ThundercallAOE = 23966, // Helper->self, no cast, ???, raidwide
    LightningBolt = 23967, // Boss->self, 3.0s cast, single-target, visual (explode orbs)
    LightningBoltAOE = 23968, // Helper->location, 4.0s cast, range 4 circle
    DistantClap = 23969, // PurpleLevin->self, 1.0s cast, range 4-10 donut
    TwistingWinds = 23970, // Boss->self, 5.0s cast, range 80 width 10 rect aoe with knockback 30
    CloudToGround = 23971, // Boss->self, no cast, single-target, visual (exaflares)
    CloudToGroundFirst = 23972, // Helper->self, 5.0s cast, range 5 circle
    CloudToGroundRest = 23973, // Helper->self, no cast, range 5 circle
    Flame = 23974, // Boss->self, 4.0s cast, single-target, visual (raidwide + spawn flames)
    FlameAOE = 23975, // Helper->self, no cast, ???, raidwide
    Burn = 23976, // BallOfFire->self, 1.0s cast, range 8 circle
    Forelash = 23977, // Boss->self, 5.0s cast, range 40 180-degree cone with knockback 15
    Backlash = 23978, // Boss->self, 5.0s cast, range 40 180-degree cone with knockback 15
    Charybdis = 23979, // Boss->self, 4.0s cast, single-target, visual (set hp to 1)
    CharybdisAOE = 23980, // Helper->self, no cast, ???, set hp to 1
    Roar = 23981, // Boss->self, 8.0s cast, single-target, visual (raidwide after charybdis)
    RoarAOE = 23982, // Helper->self, no cast, ???, raidwide
    Levinbolt = 23983, // Boss->self, no cast, single-target, visual (spread)
    LevinboltAOE = 23984, // Helper->players, 5.0s cast, range 6 circle spread
    SerpentsEdge = 23985, // Boss->player, 5.0s cast, single-target, tankbuster
    Deathwall = 24711 // Helper->self, no cast, range 20-30 donut
}

sealed class Thundercall(BossModule module) : Components.RaidwideCast(module, (uint)AID.Thundercall, "Raidwide + summon lighting orbs");

sealed class LightningBoltDistantClap(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCircle _shapeBolt = new(4f);
    private static readonly AOEShapeDonut _shapeClap = new(4f, 10f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.LightningBoltAOE)
        {
            _aoes.Add(new(_shapeBolt, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.LightningBoltAOE:
                var count1 = _aoes.Count;
                var pos1 = spell.TargetXZ;
                var act = WorldState.FutureTime(6.1d);
                for (var i = 0; i < count1; ++i)
                {
                    if (_aoes[i].Origin.AlmostEqual(pos1, 1f))
                    {
                        _aoes[i] = new(_shapeClap, pos1, default, act);
                        break;
                    }
                }
                break;
            case (uint)AID.DistantClap:
                {
                    var count2 = _aoes.Count;
                    var pos2 = caster.Position;
                    for (var i = 0; i < count2; ++i)
                    {
                        if (_aoes[i].Origin.AlmostEqual(pos2, 1f))
                        {
                            _aoes.RemoveAt(i);
                            return;
                        }
                    }
                }
                break;
        }
    }
}

sealed class TwistingWinds(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TwistingWinds, new AOEShapeRect(80f, 5f));

sealed class CloudToGround(BossModule module) : Components.Exaflare(module, 5f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.CloudToGroundFirst)
        {
            Lines.Add(new(caster.Position, 5f * spell.Rotation.ToDirection(), Module.CastFinishAt(spell), 1.1d, 4, 2));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.CloudToGroundFirst or (uint)AID.CloudToGroundRest)
        {
            var count = Lines.Count;
            var pos = caster.Position;
            for (var i = 0; i < count; ++i)
            {
                var line = Lines[i];
                if (line.Next.AlmostEqual(pos, 1f))
                {
                    AdvanceLine(line, pos);
                    if (line.ExplosionsLeft == 0)
                    {
                        Lines.RemoveAt(i);
                    }
                    return;
                }
            }
            ReportError($"Failed to find entry for {caster.InstanceID:X}");
        }
    }
}

sealed class Flame(BossModule module) : Components.RaidwideCast(module, (uint)AID.Flame, "Raidwide + summon flame orbs");

sealed class Burn(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(12);
    private static readonly AOEShapeCircle _shape = new(8f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var deadline = aoes[0].Activation.AddSeconds(1d);

        var index = 0;
        while (index < count)
        {
            ref var aoe = ref aoes[index];
            if (aoe.Activation >= deadline)
            {
                break;
            }
            ++index;
        }

        return aoes[..index];
    }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (actor.OID == (uint)OID.BallOfFire && animState1 == 1)
            _aoes.Add(new(_shape, actor.Position.Quantized(), default, WorldState.FutureTime(5d)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Burn)
        {
            _aoes.RemoveAt(0);
        }
    }
}

sealed class Lash(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Forelash, (uint)AID.Backlash], new AOEShapeCone(40f, 90f.Degrees()));
sealed class Charybdis(BossModule module) : Components.CastHint(module, (uint)AID.Charybdis, "Set hp to 1");
sealed class Roar(BossModule module) : Components.RaidwideCast(module, (uint)AID.Roar);
sealed class Levinbolt(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.LevinboltAOE, 6f);
sealed class SerpentsEdge(BossModule module) : Components.SingleTargetCast(module, (uint)AID.SerpentsEdge);

sealed class CE62LooksToDieForStates : StateMachineBuilder
{
    public CE62LooksToDieForStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Thundercall>()
            .ActivateOnEnter<LightningBoltDistantClap>()
            .ActivateOnEnter<TwistingWinds>()
            .ActivateOnEnter<CloudToGround>()
            .ActivateOnEnter<Flame>()
            .ActivateOnEnter<Burn>()
            .ActivateOnEnter<Lash>()
            .ActivateOnEnter<Charybdis>()
            .ActivateOnEnter<Roar>()
            .ActivateOnEnter<Levinbolt>()
            .ActivateOnEnter<SerpentsEdge>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CriticalEngagement, GroupID = 778, NameID = 30)] // bnpcname=9925
public sealed class CE62LooksToDieFor(WorldState ws, Actor primary) : BossModule(ws, primary, new(-200f, -580f), new ArenaBoundsCircle(20f))
{
    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InCircle(Arena.Center, 20f);
}
