namespace BossMod.Shadowbringers.Extreme.Ex5Varis;

public enum OID : uint
{
    Boss = 0x2D45,
    Helper = 0x233C,
    PhantomVaris = 0x2D46,
    TerminusEst = 0x2D48,
    IgnisEst = 0x2D49,
    VentusEst = 0x2D4A,
    Bladesblood = 0x2D4B,
    MagitekTurretI = 0x2D4C,
    MagitekTurretII = 0x2D4D,
    ConfinesOfMemory = 0x2D4E,
    Gunshield = 0x2DAC
}

public enum AID : uint
{
    TerminusEstVisual = 19635,
    TerminusEst = 19636,
    IgnisEstVisual = 19637,
    IgnisEstLine = 19638,
    IgnisEstCircle = 19653,
    VentusEstVisual = 19654,
    VentusEstLine = 19655,
    VentusEstDonut = 19656,
    FestinaLente = 19657,
    Altius = 19658,
    VivereMilitareEstVisual = 19660,
    VivereMilitareEst = 19661,
    Shockwave = 19662,
    AleaIactaEst1 = 19666,
    AleaIactaEst2 = 19667,
    AleaIactaEst3 = 19668,
    AleaIactaEst4 = 19669,
    AleaIactaEst5 = 19670,
    ElectrifiedGunshield = 19671,
    LoadedGunshield = 19672,
    ReinforcedGunshield = 19673,
    MagitekShock = 19674,
    MagitekShielding1 = 19675,
    MagitekShielding2 = 19676,
    Repay = 19677,
    MagitekBurstVisual = 19678,
    MagitekBurst = 19679,
    AltiusEnrage = 19681,
    MagitekTorch = 19683,
    MagitekSpark = 19684,
    FortiusVisual = 19685,
    FortiusFirst = 19687,
    FortiusRotateVisual = 19688,
    FortiusRest = 19689,
    Reinforcements = 19690,
    AssaultCannon = 19691,
    SelfDetonate = 19692,
    AetherochemicalGrenado = 19693,
    Citius = 19696,
    MagitekTorchVisual = 20047,
    MagitekSparkVisual = 20048,
    FestinaLenteClone = 20107
}

public enum IconID : uint
{
    FestinaLente = 161,
    RotateCW = 167,
    RotateCCW = 168
}

public enum TetherID : uint
{
    AetherochemicalGrenado = 84
}

sealed class Altius(BossModule module) : Components.RaidwideCast(module, (uint)AID.Altius);
sealed class AltiusEnrage(BossModule module) : Components.CastHint(module, (uint)AID.AltiusEnrage, "Destroy the Gunshield before the enrage!", true);
sealed class VivereMilitareEst(BossModule module) : Components.CastHint(module, (uint)AID.VivereMilitareEst, "Kill Bladesblood before the cast ends!", true);
sealed class SelfDetonate(BossModule module) : Components.RaidwideCast(module, (uint)AID.SelfDetonate);
sealed class Citius(BossModule module) : Components.CastSharedTankbuster(module, (uint)AID.Citius, new AOEShapeCone(60f, 45f.Degrees()));
sealed class IgnisEstCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IgnisEstCircle, 10f);
sealed class VentusEstDonut(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VentusEstDonut, new AOEShapeDonut(10f, 40f));
sealed class AleaIactaEst(BossModule module) : Components.SimpleAOEGroupsByTimewindow(module, [(uint)AID.AleaIactaEst1, (uint)AID.AleaIactaEst2, (uint)AID.AleaIactaEst3,
    (uint)AID.AleaIactaEst4, (uint)AID.AleaIactaEst5], new AOEShapeCone(50f, 105f.Degrees()), 2.5d);
sealed class AssaultCannon(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AssaultCannon, new AOEShapeRect(75f, 1f));
sealed class AetherochemicalGrenado(BossModule module) : Components.TankbusterTether(module, (uint)AID.AetherochemicalGrenado, (uint)TetherID.AetherochemicalGrenado, 3f);
sealed class MagitekTorch(BossModule module) : Components.CastHint(module, (uint)AID.MagitekTorchVisual, "Line stack on a random player: stack together!");
sealed class ReinforcedGunshield(BossModule module) : Components.DirectionalParry(module, [(uint)OID.Boss]);

sealed class FestinaLente(BossModule module) : Components.UniformStackSpread(module, 6f, default, 4, 4)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.FestinaLente)
            AddStack(actor, WorldState.FutureTime(5.1d));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.FestinaLente or (uint)AID.FestinaLenteClone)
            Stacks.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
    }
}

sealed class TerminusEst(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect _rect = new(50f, 4f);
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is (uint)OID.TerminusEst or (uint)OID.IgnisEst or (uint)OID.VentusEst)
            _aoes.Add(new(_rect, actor.Position.Quantized(), actor.Rotation, WorldState.FutureTime(9d), actorID: actor.InstanceID));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.TerminusEst or (uint)AID.IgnisEstLine or (uint)AID.VentusEstLine)
        {
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            var len = aoes.Length;
            for (var i = 0; i < len; ++i)
            {
                ref var aoe = ref aoes[i];
                if (aoe.ActorID == caster.InstanceID)
                {
                    aoe.Origin = caster.Position.Quantized();
                    aoe.Rotation = spell.Rotation;
                    aoe.Activation = Module.CastFinishAt(spell);
                    return;
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.TerminusEst or (uint)AID.IgnisEstLine or (uint)AID.VentusEstLine)
        {
            ++NumCasts;
            Remove(caster.InstanceID);
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID is (uint)OID.TerminusEst or (uint)OID.IgnisEst or (uint)OID.VentusEst)
            Remove(actor.InstanceID);
    }

    private void Remove(ulong id)
    {
        var count = _aoes.Count;
        for (var i = 0; i < count; ++i)
        {
            if (_aoes[i].ActorID == id)
            {
                _aoes.RemoveAt(i);
                return;
            }
        }
    }
}

sealed class GunshieldGauge(BossModule module) : BossComponent(module)
{
    public uint Kind;
    public DateTime Full;
    private DateTime _startTime;
    private uint _startValue;
    private bool _started;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ElectrifiedGunshield:
            case (uint)AID.LoadedGunshield:
            case (uint)AID.ReinforcedGunshield:
                Kind = spell.Action.ID;
                Full = WorldState.FutureTime(60d);
                _started = false;
                break;
            case (uint)AID.MagitekShock:
            case (uint)AID.MagitekBurstVisual:
            case (uint)AID.MagitekShielding1:
            case (uint)AID.MagitekShielding2:
                Kind = default;
                break;
        }
    }

    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        if (updateID != 0x8000000Cu || param1 != 0x27u || Kind == default)
            return;
        var now = WorldState.CurrentTime;
        if (!_started)
        {
            _started = true;
            _startTime = now;
            _startValue = param2;
            return;
        }
        var elapsed = (now - _startTime).TotalSeconds;
        if (elapsed < 0.5d || param2 <= _startValue)
            return;
        var rate = (param2 - _startValue) / elapsed;
        Full = now.AddSeconds(Math.Max(0d, 10000d - param2) / rate);
    }

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (Kind == default)
            return;
        var left = Math.Max(0d, (Full - WorldState.CurrentTime).TotalSeconds);
        if (left > 30d)
            return;
        var name = Kind switch
        {
            (uint)AID.ElectrifiedGunshield => "Knockback",
            (uint)AID.LoadedGunshield => "Spreads",
            _ => "Directional parry"
        };
        hints.Add($"{name} in {left:f1}s");
    }
}

sealed class MagitekShock(BossModule module) : Components.GenericKnockback(module)
{
    private readonly GunshieldGauge _gauge = module.FindComponent<GunshieldGauge>()!;
    private Knockback[] _kb = [];

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => _kb;

    public override void Update()
    {
        if (_gauge.Kind != (uint)AID.ElectrifiedGunshield)
        {
            _kb = [];
            return;
        }
        var activation = _gauge.Full.AddSeconds(0.3d);
        _kb = (activation - WorldState.CurrentTime).TotalSeconds < 6d ? [new(Module.PrimaryActor.Position, 10f, activation)] : [];
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.MagitekShock)
        {
            ++NumCasts;
            _kb = [];
        }
    }
}

sealed class LoadedGunshield(BossModule module) : Components.UniformStackSpread(module, default, 8f)
{
    private readonly GunshieldGauge _gauge = module.FindComponent<GunshieldGauge>()!;

    public override void Update()
    {
        if (Spreads.Count == 0 && _gauge.Kind == (uint)AID.LoadedGunshield)
        {
            var activation = _gauge.Full.AddSeconds(3.2d);
            if ((activation - WorldState.CurrentTime).TotalSeconds < 7d)
                AddSpreads(Raid.WithoutSlot(false, true, true), activation);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.MagitekBurst)
            Spreads.Clear();
    }
}

sealed class FortiusBait(BossModule module) : Components.GenericBaitAway(module, (uint)AID.FortiusFirst)
{
    private static readonly AOEShapeCone _cone = new(50f, 11.25f.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID != (uint)AID.FortiusVisual)
            return;
        var party = Raid.WithoutSlot(false, true, true);
        var activation = Module.CastFinishAt(spell, 0.8d);
        for (var i = 0; i < party.Length; ++i)
            CurrentBaits.Add(new(caster, party[i], _cone, activation));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            CurrentBaits.Clear();
        }
    }
}

sealed class FortiusRotation(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone _cone = new(50f, 11.25f.Degrees());
    private readonly List<AOEInstance> _aoes = [];
    private readonly Dictionary<ulong, int> _left = [];
    private Angle _step;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.RotateCW)
            _step = -20f.Degrees();
        else if (iconID == (uint)IconID.RotateCCW)
            _step = 20f.Degrees();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var id = spell.Action.ID;
        if (id == (uint)AID.FortiusFirst)
        {
            if (_step == default)
                return;
            _left[caster.InstanceID] = 3;
            _aoes.Add(new(_cone, caster.Position.Quantized(), spell.Rotation + _step, WorldState.FutureTime(3.1d), actorID: caster.InstanceID));
        }
        else if (id == (uint)AID.FortiusRest)
        {
            ++NumCasts;
            var count = _aoes.Count;
            for (var i = 0; i < count; ++i)
            {
                if (_aoes[i].ActorID != caster.InstanceID)
                    continue;
                var left = _left.GetValueOrDefault(caster.InstanceID) - 1;
                if (left <= 0)
                {
                    _aoes.RemoveAt(i);
                    _left.Remove(caster.InstanceID);
                }
                else
                {
                    _left[caster.InstanceID] = left;
                    _aoes[i] = new(_cone, caster.Position.Quantized(), spell.Rotation + _step, WorldState.FutureTime(3.1d), actorID: caster.InstanceID);
                }
                break;
            }
            if (_aoes.Count == 0)
                _step = default;
        }
    }
}

sealed class MagitekSpark(BossModule module) : Components.GenericBaitAway(module, (uint)AID.MagitekSpark)
{
    private static readonly AOEShapeCone _cone = new(50f, 5f.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID != (uint)AID.MagitekSparkVisual)
            return;
        var party = Raid.WithoutSlot(false, true, true);
        var activation = Module.CastFinishAt(spell, 0.1d);
        for (var i = 0; i < party.Length; ++i)
            CurrentBaits.Add(new(caster, party[i], _cone, activation));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            CurrentBaits.Clear();
        }
    }
}

sealed class Ex5VarisStates : StateMachineBuilder
{
    public Ex5VarisStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GunshieldGauge>()
            .ActivateOnEnter<Altius>()
            .ActivateOnEnter<AltiusEnrage>()
            .ActivateOnEnter<VivereMilitareEst>()
            .ActivateOnEnter<SelfDetonate>()
            .ActivateOnEnter<Citius>()
            .ActivateOnEnter<IgnisEstCircle>()
            .ActivateOnEnter<VentusEstDonut>()
            .ActivateOnEnter<AleaIactaEst>()
            .ActivateOnEnter<AssaultCannon>()
            .ActivateOnEnter<AetherochemicalGrenado>()
            .ActivateOnEnter<MagitekTorch>()
            .ActivateOnEnter<ReinforcedGunshield>()
            .ActivateOnEnter<FestinaLente>()
            .ActivateOnEnter<TerminusEst>()
            .ActivateOnEnter<MagitekShock>()
            .ActivateOnEnter<LoadedGunshield>()
            .ActivateOnEnter<FortiusBait>()
            .ActivateOnEnter<FortiusRotation>()
            .ActivateOnEnter<MagitekSpark>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LTS", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 725, NameID = 9341, PlanLevel = 80)]
public sealed class Ex5Varis(WorldState ws, Actor primary) : BossModule(ws, primary, new(35f, -683f), new ArenaBoundsSquare(20f))
{
    private static readonly uint[] _adds = [(uint)OID.Bladesblood, (uint)OID.MagitekTurretI, (uint)OID.MagitekTurretII, (uint)OID.Gunshield, (uint)OID.ConfinesOfMemory];

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.ConfinesOfMemory => 3,
                (uint)OID.Bladesblood or (uint)OID.Gunshield or (uint)OID.MagitekTurretI => 2,
                _ => e.Priority
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(this, _adds);
    }
}
