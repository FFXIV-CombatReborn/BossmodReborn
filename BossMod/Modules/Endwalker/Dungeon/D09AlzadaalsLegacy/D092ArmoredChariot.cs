namespace BossMod.Endwalker.Dungeon.D09AlzadaalsLegacy.D092ArmoredChariot;

public enum OID : uint
{
    Boss = 0x386C, // R=6.4
    ArmoredDrudge = 0x386D, // R1.8
    Voidzone = 0x1EB69C,
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 29132, // Boss->player, no cast, single-target

    ArticulatedBits = 28441, // Boss->self, 3.0s cast, range 6 circle //Persistent AOE under boss

    AssaultCannonVisual1First = 28442, // ArmoredDrudge->self, 8.0s cast, single-target
    AssaultCannonVisual2First = 28443, // ArmoredDrudge->self, 8.0s cast, single-target

    AssaultCannonLong = 28444, // Helper->self, no cast, range 40 width 8 rect
    AssaultCannonShort = 28445, // Helper->self, no cast, range 28 width 8 rect
    CannonReflectionVisual = 28454, // Helper->self, 8.0s cast, single-target
    CannonReflection = 28455, // Helper->self, no cast, range 30 90-degree cone

    DiffusionRay = 28446, // Boss->self, 5.0s cast, range 40 circle

    // Initiates model state changes
    ReflectionNESW1 = 28448, // Boss->self, no cast, single-target
    ReflectionNESW2 = 28449, // Boss->self, no cast, single-target
    ReflectionNWSE1 = 28450, // Boss->self, no cast, single-target
    ReflectionNWSE2 = 28451, // Boss->self, no cast, single-target
    ReflectionShieldDisappears1 = 28452, // Boss->self, no cast, single-target
    ReflectionShieldDisappears2 = 28453, // Boss->self, no cast, single-target

    Assail1Wave = 28456, // Boss->self, no cast, single-target
    Assail2Waves = 28457, // Boss->self, no cast, single-target

    GravitonCannon = 29555, // Helper->player, 8.5s cast, range 6 circle, spread
    RailCannon = 28447 // Boss->player, 5.0s cast, single-target
}

public enum SID : uint
{
    CannonOrder = 2552, // none->ArmoredDrudge, extra=0x180/0x181 (first/second wave)
    ReflectionShield = 2195 // none->Boss, extra=0x182/0x183 (NE+SW/NW+SE)
}

sealed class Voidzone(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(6f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnActorEState(Actor actor, ushort state)
    {
        if (state == 0x0004 && actor.OID == (uint)OID.Voidzone)
        {
            _aoe = [];
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ArticulatedBits)
        {
            _aoe = [new(circle, Arena.Center, default, Module.CastFinishAt(spell, 0.8d))];
        }
    }
}

sealed class AssaultCannon(BossModule module) : Components.GenericAOEs(module)
{
    private enum Type { None, OneWave, TwoWaves }
    private Type currentType;
    private readonly List<AOEInstance> _aoesCones = [];
    private readonly List<AOEInstance> _aoesRects = [];
    private readonly List<Actor> _activeDrudges = [];
    private static readonly AOEShapeCone cone = new(30f, 45f.Degrees());
    private static readonly AOEShapeRect rectShort = new(28f, 4f);
    private static readonly AOEShapeRect rectLong = new(40f, 4f);
    private static readonly WPos[] cornerPositions = [new(-20.5f, -202.5f), new(20.5f, -161.5f), new(-20.5f, -161.5f), new(20.5f, -202.5f)];
    private int numCastsReflections;
    private int numCastsCannons;

    private AOEInstance[] _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var countCones = _aoesCones.Count;
        var countRects = _aoesRects.Count;
        var total = countCones + countRects;
        if (total == 0)
            return [];

        var type = currentType == Type.TwoWaves;
        var maxCone = type && countCones > 2 ? 2 : countCones;
        var maxRect = type && countRects > 2 ? 2 : countRects;
        var adjtotal = maxCone + maxRect;
        var aoesLen = adjtotal;
        if (_aoesBuffer.Length < aoesLen)
            _aoesBuffer = new AOEInstance[aoesLen];
        var aoes = _aoesBuffer;
        var index = 0;

        for (var i = 0; i < maxCone; ++i)
            aoes[index++] = _aoesCones[i];
        for (var i = 0; i < maxRect; ++i)
            aoes[index++] = _aoesRects[i];
        return aoes.AsSpan(0, aoesLen);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x1E43)
        {
            _activeDrudges.Add(actor);
        }
        else if (id == 0x1E39)
        {
            _activeDrudges.Remove(actor);
        }
    }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (_aoesCones.Count == 0)
        {
            if (currentType == Type.TwoWaves)
                AddTwoWaveAOEs(modelState);
            else if (currentType == Type.OneWave)
                AddOneWaveAOEs(modelState);
        }
    }

    private void AddTwoWaveAOEs(byte modelState)
    {
        var activationFirst = WorldState.FutureTime(7.1d);
        var activationSecond = WorldState.FutureTime(15d);

        if (modelState == 4)
        {
            AddConeAOEs(activationFirst, Angle.AnglesIntercardinals[2], Angle.AnglesIntercardinals[0]);
            AddConeAOEs(activationSecond, Angle.AnglesIntercardinals[3], Angle.AnglesIntercardinals[1]);
        }
        else if (modelState == 5)
        {
            AddConeAOEs(activationFirst, Angle.AnglesIntercardinals[3], Angle.AnglesIntercardinals[1]);
            AddConeAOEs(activationSecond, Angle.AnglesIntercardinals[2], Angle.AnglesIntercardinals[0]);
        }
    }

    private void AddOneWaveAOEs(byte modelState)
    {
        var activation = WorldState.FutureTime(6.9d);

        if (modelState == 4)
            AddConeAOEs(activation, Angle.AnglesIntercardinals[2], Angle.AnglesIntercardinals[0]);
        else if (modelState == 5)
            AddConeAOEs(activation, Angle.AnglesIntercardinals[3], Angle.AnglesIntercardinals[1]);

        var count = _activeDrudges.Count;
        for (var i = 0; i < count; ++i)
        {
            var drudge = _activeDrudges[i];
            var shape = cornerPositions.Contains(drudge.Position) ? rectShort : rectLong;
            _aoesRects.Add(new(shape, drudge.Position.Quantized(), drudge.Rotation, activation));
        }
    }

    private void AddConeAOEs(DateTime activation, params Angle[] angles)
    {
        for (var i = 0; i < 2; ++i)
            _aoesCones.Add(new(cone, Arena.Center.Quantized(), angles[i], activation));
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.CannonOrder)
        {
            var activation = status.Extra == 0x180 ? 6.9d : 15d;
            _aoesRects.Add(new(rectShort, actor.Position.Quantized(), actor.Rotation, WorldState.FutureTime(activation)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Assail1Wave)
            currentType = Type.OneWave;
        else if (spell.Action.ID == (uint)AID.Assail2Waves)
            currentType = Type.TwoWaves;
        else if (currentType == Type.TwoWaves)
        {
            if (spell.Action.ID == (uint)AID.CannonReflection && _aoesCones.Count > 0)
            {
                if (++numCastsReflections == 14)
                {
                    _aoesCones.RemoveRange(0, 2);
                    numCastsReflections = 0;
                    if (_aoesCones.Count == 0)
                        currentType = Type.None;
                }
            }
            else if (spell.Action.ID == (uint)AID.AssaultCannonShort && _aoesRects.Count > 0)
                if (++numCastsCannons == 14)
                {
                    _aoesRects.RemoveRange(0, 2);
                    numCastsCannons = 0;
                }
        }
        else if (currentType == Type.OneWave)
        {
            if (spell.Action.ID == (uint)AID.CannonReflection)
            {
                if (++numCastsReflections == 14)
                {
                    _aoesCones.Clear();
                    _aoesRects.Clear();
                    numCastsReflections = 0;
                    currentType = Type.None;
                }
            }
        }
    }
}

sealed class DiffusionRay(BossModule module) : Components.RaidwideCast(module, (uint)AID.DiffusionRay);
sealed class RailCannon(BossModule module) : Components.SingleTargetCast(module, (uint)AID.RailCannon);
sealed class GravitonCannon(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.GravitonCannon, 6f);

sealed class D092ArmoredChariotStates : StateMachineBuilder
{
    public D092ArmoredChariotStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AssaultCannon>()
            .ActivateOnEnter<Voidzone>()
            .ActivateOnEnter<RailCannon>()
            .ActivateOnEnter<DiffusionRay>()
            .ActivateOnEnter<GravitonCannon>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 844, NameID = 11239)]
public sealed class D092ArmoredChariot(WorldState ws, Actor primary) : BossModule(ws, primary, new(default, -182f), new ArenaBoundsSquare(19.5f));
