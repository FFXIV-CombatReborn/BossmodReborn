namespace BossMod.Endwalker.DeepDungeon.EurekaOrthos.DD80ProtoKaliya;

public enum OID : uint
{
    Boss = 0x3D18, // R5.0
    WeaponsDrone = 0x3D19, // R2.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 31421, // Boss->players, no cast, range 6+R 90-degree cone

    AetheromagnetismKB = 31431, // Helper->player, no cast, single-target, knockback 10, away from source
    AetheromagnetismPull = 31430, // Helper->player, no cast, single-target, pull 10, between centers
    AutoCannons = 31432, // WeaponsDrone->self, 4.0s cast, range 41+R width 5 rect
    Barofield = 31427, // Boss->self, 3.0s cast, single-target

    CentralizedNerveGasVisual = 31423, // Boss->self, 4.5s cast, range 25+R 120-degree cone
    CentralizedNerveGas = 32933, // Helper->self, 5.3s cast, range 25+R 120-degree cone
    LeftwardNerveGasVisual = 31424, // Boss->self, 4.5s cast, range 25+R 180-degree cone
    LeftwardNerveGas = 32934, // Helper->self, 5.3s cast, range 25+R 180-degree cone
    RightwardNerveGasVisual = 31425, // Boss->self, 4.5s cast, range 25+R 180-degree cone
    RightwardNerveGas = 32935, // Helper->self, 5.3s cast, range 25+R 180-degree cone
    NerveGasRingVisual = 31426, // Boss->self, 5.0s cast, range 8-30 donut
    NerveGasRing = 32930, // Helper->self, 7.2s cast, range 8-30 donut
    Resonance = 31422, // Boss->player, 5.0s cast, range 12 90-degree cone, tankbuster

    NanosporeJet = 31429 // Boss->self, 5.0s cast, range 100 circle
}

public enum SID : uint
{
    Barofield = 3420, // none->Boss, extra=0x0, damage taken when near boss
    NegativeChargePlayer = 3419, // none->player, extra=0x0
    PositiveChargePlayer = 3418, // none->player, extra=0x0
    NegativeChargeDrone = 3417, // none->WeaponsDrone, extra=0x0
    PositiveChargeDrone = 3416 // none->WeaponsDrone, extra=0x0
}

public enum TetherID : uint
{
    Magnetism = 38
}

sealed class Magnetism(BossModule module) : Components.GenericKnockback(module, stopAfterWall: true)
{
    private readonly Knockback[][] _kbs = new Knockback[4][];
    private readonly NerveGasRingAndAutoCannons _aoe = module.FindComponent<NerveGasRingAndAutoCannons>()!;
    private readonly List<Actor> drones = module.Enemies((uint)OID.WeaponsDrone);

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (_kbs[slot] != null)
        {
            var count = _aoe.AOEs.Count;
            var aoes = CollectionsMarshal.AsSpan(_aoe.AOEs);
            for (var i = 0; i < count; ++i)
            {
                if (aoes[i].Shape == NerveGasRingAndAutoCannons.donut)
                {
                    return _kbs[slot];
                }
            }
        }
        return [];
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = CollectionsMarshal.AsSpan(_aoe.AOEs);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            if (aoes[i].Check(pos))
            {
                return true;
            }
        }
        return !Arena.InBounds(pos);
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        bool IsPull(Actor target)
        => source.FindStatus((uint)SID.NegativeChargeDrone) != null && target.FindStatus((uint)SID.PositiveChargePlayer) != null ||
        source.FindStatus((uint)SID.PositiveChargeDrone) != null && target.FindStatus((uint)SID.NegativeChargePlayer) != null;

        bool IsKnockback(Actor target)
        => source.FindStatus((uint)SID.NegativeChargeDrone) != null && target.FindStatus((uint)SID.NegativeChargePlayer) != null ||
        source.FindStatus((uint)SID.PositiveChargeDrone) != null && target.FindStatus((uint)SID.PositiveChargePlayer) != null;

        if (tether.ID == (uint)TetherID.Magnetism)
        {
            var target = WorldState.Actors.Find(tether.Target)!;
            if (IsPull(target))
            {
                AddSource(false);
            }
            else if (IsKnockback(target))
            {
                AddSource(true);
            }

            void AddSource(bool isKnockback) => _kbs[Raid.FindSlot(target.InstanceID)] = [new(source.Position, 10f, WorldState.FutureTime(10d), kind: isKnockback ? Kind.AwayFromOrigin : Kind.TowardsOrigin, ignoreImmunes: true)];
        }
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Magnetism)
        {
            _kbs[Raid.FindSlot(tether.Target)] = [];
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_kbs[slot] is var kbs && kbs != null && kbs.Length != 0)
        {
            ref var kb = ref _kbs[slot][0];
            var attract = kb.Kind == Kind.TowardsOrigin;
            var center = Arena.Center;
            var count = drones.Count;

            var forbidden = new ShapeDistance[count + 1];
            forbidden[count] = new SDDonut(center, 8f, 50f);
            for (var i = 0; i < count; ++i)
            {
                var c = drones[i];
                forbidden[i] = new SDRect(c.Position, c.Rotation.ToDirection(), 43f, default, 2.5f);
            }

            var origin = kb.Origin;
            hints.AddForbiddenZone(new SDKnockbackInCircleAwayFromOriginPlusMixedAOEsPlusSingleCircleIntersection(center, origin, 19f, 10f, new SDUnion(forbidden), center, 5.5f, attract), kb.Activation);
        }
    }
}

sealed class Barofield(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(5f);
    private AOEInstance[] _aoe = [];
    public bool Active;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (!Active && spell.Action.ID is (uint)AID.Barofield or (uint)AID.NanosporeJet)
        {
            _aoe = [new(circle, spell.LocXZ, default, Module.CastFinishAt(spell, 0.7d))];
            Active = true;
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Barofield)
        {
            _aoe = [];
            Arena.Bounds = DD80ProtoKaliya.DonutArena;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Barofield)
        {
            Arena.Bounds = new ArenaBoundsCircle(20f);
            Active = false;
        }
    }
}

sealed class NerveGasRingAndAutoCannons(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = new(2);
    private static readonly AOEShapeCross cross = new(20f, 2.5f);
    private static readonly AOEShapeCross rect = new(43f, 2.5f);
    public static readonly AOEShapeDonut donut = new(8f, 30f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddAOE(AOEShape shape) => AOEs.Add(new(shape, spell.LocXZ, shape == rect ? spell.Rotation : default, Module.CastFinishAt(spell), actorID: caster.InstanceID));
        switch (spell.Action.ID)
        {
            case (uint)AID.NerveGasRing:
                AddAOE(donut);
                AddAOE(cross);
                break;
            case (uint)AID.AutoCannons:
                AddAOE(rect);
                var count = AOEs.Count;
                var aoes = CollectionsMarshal.AsSpan(AOEs);
                for (var i = 0; i < count; ++i)
                {
                    ref var aoe = ref aoes[i];
                    if (aoe.Shape == cross)
                    {
                        AOEs.RemoveAt(i);
                        return;
                    }
                }
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.NerveGasRing or (uint)AID.AutoCannons)
        {
            var count = AOEs.Count;
            var id = caster.InstanceID;
            var aoes = CollectionsMarshal.AsSpan(AOEs);
            for (var i = 0; i < count; ++i)
            {
                if (aoes[i].ActorID == id)
                {
                    AOEs.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

sealed class NerveGas(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.LeftwardNerveGas, (uint)AID.RightwardNerveGas], new AOEShapeCone(25.5f, 90f.Degrees()));

sealed class CentralizedNerveGas(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CentralizedNerveGas, new AOEShapeCone(25.5f, 60f.Degrees()));

sealed class AutoAttack(BossModule module) : Components.Cleave(module, (uint)AID.AutoAttack, new AOEShapeCone(11f, 45f.Degrees()))
{
    private readonly Barofield _aoe = module.FindComponent<Barofield>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!_aoe.Active)
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!_aoe.Active)
        {
            base.DrawArenaForeground(pcSlot, pc);
        }
    }
}

sealed class Resonance(BossModule module) : Components.BaitAwayCast(module, (uint)AID.Resonance, new AOEShapeCone(12f, 45f.Degrees()), endsOnCastEvent: true, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);

sealed class DD80ProtoKaliyaStates : StateMachineBuilder
{
    public DD80ProtoKaliyaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Barofield>()
            .ActivateOnEnter<AutoAttack>()
            .ActivateOnEnter<Resonance>()
            .ActivateOnEnter<NerveGasRingAndAutoCannons>()
            .ActivateOnEnter<NerveGas>()
            .ActivateOnEnter<CentralizedNerveGas>()
            .ActivateOnEnter<Magnetism>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus, legendoficeman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 904, NameID = 12247)]
public sealed class DD80ProtoKaliya(WorldState ws, Actor primary) : BossModule(ws, primary, arenaCenter, new ArenaBoundsCircle(20f))
{
    private static readonly WPos arenaCenter = new(-600f, -300f);
    public static readonly ArenaBoundsCustom DonutArena = new([new Polygon(arenaCenter, 20f, 64)], [new Polygon(arenaCenter.Quantized(), 5f, 64)]);
}
