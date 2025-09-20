namespace BossMod.Endwalker.FATE.Chi;

public enum OID : uint
{
    Boss = 0x34CB, // R16.0
    Helper1 = 0x34CC, // used for thermobaric missiles and freefall bombs
    Helper2 = 0x34CD, // used for tankbusters
    BouncingBomb1 = 0x364C, // first bomb hit
    BouncingBomb2 = 0x364D, // remaining hits
    Bunkerbuster1 = 0x361E, // used by BB with 10s casttime
    Bunkerbuster2 = 0x3505 // used by BB with 12s casttime
}

public enum AID : uint
{
    AutoAttack = 25952, // Boss->player, no cast, single-target

    AssaultCarapace1 = 25954, // Boss->self, 5.0s cast, range 120 width 32 rect
    AssaultCarapace2 = 25173, // Boss->self, 8.0s cast, range 120 width 32 rect
    CarapaceRearGuns2dot0A = 25958, // Boss->self, 8.0s cast, range 120 width 32 rect
    CarapaceForeArms2dot0A = 25957, // Boss->self, 8.0s cast, range 120 width 32 rect
    AssaultCarapace3 = 25953, // Boss->self, 5.0s cast, range 16-60 donut
    CarapaceForeArms2dot0B = 25955, // Boss->self, 8.0s cast, range 16-60 donut
    CarapaceRearGuns2dot0B = 25956, // Boss->self, 8.0s cast, range 16-60 donut
    ForeArms1 = 25959, // Boss->self, 6.0s cast, range 45 180-degree cone
    ForeArms2 = 26523, // Boss->self, 6.0s cast, range 45 180-degree cone
    ForeArms2dot0 = 25961, // Boss->self, no cast, range 45 180-degree cone
    RearGuns2dot0 = 25964, // Boss->self, no cast, range 45 180-degree cone
    RearGuns1 = 25962, // Boss->self, 6.0s cast, range 45 180-degree cone
    RearGuns2 = 26524, // Boss->self, 6.0s cast, range 45 180-degree cone
    RearGunsForeArms2dot0 = 25963, // Boss->self, 6.0s cast, range 45 180-degree cone
    ForeArmsRearGuns2dot0 = 25960, // Boss->self, 6.0s cast, range 45 180-degree cone
    HellburnerVisual = 25971, // Boss->self, no cast, single-target, circle tankbuster
    Hellburner = 25972, // Helper1->players, 5.0s cast, range 5 circle
    FreeFallBombsVisual = 25967, // Boss->self, no cast, single-target
    FreeFallBombs = 25968, // Helper1->location, 3.0s cast, range 6 circle
    MissileShowerVisual = 25969, // Boss->self, 4.0s cast, single-target
    MissileShower = 25970, // Helper2->self, no cast, range 30 circle
    Teleport = 25155, // Boss->location, no cast, single-target, boss teleports mid

    BunkerBusterVisual = 25975, // Boss->self, 3.0s cast, single-target
    BunkerBuster1 = 25101, // Helper3->self, 10.0s cast, range 20 width 20 rect
    BunkerBuster2 = 25976, // Helper6->self, 12.0s cast, range 20 width 20 rect

    BouncingBombVisual = 27484, // Boss->self, 3.0s cast, single-target
    BouncingBombFirst = 27485, // Helper4->self, 5.0s cast, range 20 width 20 rect
    BouncingBombRest = 27486, // Helper5->self, 1.0s cast, range 20 width 20 rect

    ThermobaricExplosive = 25965, // Boss->self, 3.0s cast, single-target
    ThermobaricExplosive2 = 25966 // Helper1->location, 10.0s cast, range 55 circle, damage fall off AOE
}

sealed class Bunkerbuster(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(9);
    public static readonly AOEShapeRect Square = new(10f, 10f, 10f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        ref var aoe0 = ref aoes[0];
        ref var aoeL = ref aoes[^1];
        var act0 = aoe0.Activation;
        var deadline1 = act0.AddSeconds(1d);
        var deadline2 = act0.AddSeconds(2.5d);
        var isNotLastSet = aoeL.Activation > deadline1;
        var color = Colors.Danger;
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            ref var aoe = ref aoes[i];
            if (aoe.Activation > deadline2)
                break;
            if (aoe.Activation < deadline1)
            {
                if (isNotLastSet)
                {
                    aoe.Color = color;
                }
                aoe.Risky = true;
            }
            else
            {
                aoe.Risky = false;
            }
            ++index;
        }
        return aoes[..index];
    }

    public override void OnActorCreated(Actor actor)
    {
        var activation = actor.OID switch
        {
            (uint)OID.Bunkerbuster1 => 14.8d,
            (uint)OID.Bunkerbuster2 => 16.9d,
            _ => default
        };
        if (activation != default)
        {
            _aoes.Add(new(Square, actor.Position, Angle.AnglesCardinals[1], WorldState.FutureTime(activation)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.BunkerBuster1 or (uint)AID.BunkerBuster2)
        {
            _aoes.RemoveAt(0);
        }
    }
}

sealed class BouncingBomb(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(15);

    // either 9 or 15 explosions depending on pattern
    // pattern 1: 1, 3, 5 explosions
    // pattern 2: 2, 5, 8 explosions
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        ref var aoe0 = ref aoes[0];
        ref var aoeL = ref aoes[^1];
        var act0 = aoe0.Activation;
        var deadline1 = act0.AddSeconds(1d);
        var deadline2 = act0.AddSeconds(3.5d);
        var isNotLastSet = aoeL.Activation > deadline1;
        var color = Colors.Danger;
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            ref var aoe = ref aoes[i];
            if (aoe.Activation > deadline2)
                break;
            if (aoe.Activation < deadline1)
            {
                if (isNotLastSet)
                {
                    aoe.Color = color;
                }
                aoe.Risky = true;
            }
            else
            {
                aoe.Risky = false;
            }
            ++index;
        }
        return aoes[..index];
    }

    public override void OnActorCreated(Actor actor)
    {
        var activation = actor.OID switch
        {
            (uint)OID.BouncingBomb1 => 10d,
            (uint)OID.BouncingBomb2 => 5.7d,
            _ => default
        };
        if (activation != default)
        {
            _aoes.Add(new(Bunkerbuster.Square, actor.Position, Angle.AnglesCardinals[1], WorldState.FutureTime(activation)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.BouncingBombFirst or (uint)AID.BouncingBombRest)
        {
            _aoes.RemoveAt(0);
        }
    }
}

sealed class Combos(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    private static readonly AOEShapeCone cone = new(45f, 90f.Degrees());
    private static readonly AOEShapeDonut donut = new(16f, 60f);
    private static readonly AOEShapeRect rect = new(120f, 16f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        ref var aoe0 = ref aoes[0];
        if (count > 1)
        {
            aoe0.Color = Colors.Danger;
        }
        aoe0.Risky = true;
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddAOEs(AOEShape shape, bool backwards = false)
        {
            _aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), risky: false));
            _aoes.Add(new(cone, shape == rect ? caster.Position : spell.LocXZ, spell.Rotation + (backwards ? 180f.Degrees() : default), Module.CastFinishAt(spell, 3.1d), risky: false));
        }

        switch (spell.Action.ID)
        {
            case (uint)AID.CarapaceForeArms2dot0A:
                AddAOEs(rect);
                break;
            case (uint)AID.CarapaceForeArms2dot0B:
                AddAOEs(donut);
                break;
            case (uint)AID.CarapaceRearGuns2dot0A:
                AddAOEs(rect, true);
                break;
            case (uint)AID.CarapaceRearGuns2dot0B:
                AddAOEs(donut, true);
                break;
            case (uint)AID.RearGunsForeArms2dot0:
            case (uint)AID.ForeArmsRearGuns2dot0:
                AddAOEs(cone, true);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0)
        {
            switch (spell.Action.ID)
            {
                case (uint)AID.CarapaceForeArms2dot0B:
                case (uint)AID.CarapaceRearGuns2dot0B:
                case (uint)AID.CarapaceForeArms2dot0A:
                case (uint)AID.CarapaceRearGuns2dot0A:
                case (uint)AID.ForeArmsRearGuns2dot0:
                case (uint)AID.ForeArms2dot0:
                case (uint)AID.RearGunsForeArms2dot0:
                case (uint)AID.RearGuns2dot0:
                    _aoes.RemoveAt(0);
                    break;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (_aoes.Count != 2)
        {
            return;
        }
        // make ai stay close to boss to ensure successfully dodging the combo
        ref var aoe = ref _aoes.Ref(0);
        hints.AddForbiddenZone(new SDInvertedRect(Module.PrimaryActor.Position, aoe.Rotation, 2f, 2f, 40f), aoe.Activation);
    }
}

sealed class Hellburner(BossModule module) : Components.BaitAwayCast(module, (uint)AID.Hellburner, 5f, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);

sealed class MissileShower(BossModule module) : Components.RaidwideCast(module, (uint)AID.MissileShowerVisual, "Raidwide x2");
sealed class ThermobaricExplosive(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ThermobaricExplosive2, 25f);

sealed class AssaultCarapace(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.AssaultCarapace1, (uint)AID.AssaultCarapace2], new AOEShapeRect(120f, 16f));
sealed class AssaultCarapace3(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AssaultCarapace3, new AOEShapeDonut(16f, 60f));

sealed class ForeArmsRearGuns(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ForeArms1, (uint)AID.ForeArms2,
(uint)AID.RearGuns1, (uint)AID.RearGuns2], new AOEShapeCone(45f, 90f.Degrees()));

sealed class FreeFallBombs(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FreeFallBombs, 6f);

sealed class ChiStates : StateMachineBuilder
{
    public ChiStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AssaultCarapace>()
            .ActivateOnEnter<AssaultCarapace3>()
            .ActivateOnEnter<Combos>()
            .ActivateOnEnter<ForeArmsRearGuns>()
            .ActivateOnEnter<Hellburner>()
            .ActivateOnEnter<FreeFallBombs>()
            .ActivateOnEnter<ThermobaricExplosive>()
            .ActivateOnEnter<Bunkerbuster>()
            .ActivateOnEnter<BouncingBomb>()
            .ActivateOnEnter<MissileShower>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Fate, GroupID = 1855, NameID = 10400)]
public sealed class Chi(WorldState ws, Actor primary) : BossModule(ws, primary, new(650f, default), new ArenaBoundsSquare(29.5f))
{
    protected override bool CheckPull() => base.CheckPull() && (Center - Raid.Player()!.Position).LengthSq() < 900f;
}

