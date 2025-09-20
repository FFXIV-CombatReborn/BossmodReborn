namespace BossMod.Endwalker.Dungeon.D02TowerOfBabil.D022Lugae;

public enum OID : uint
{
    Boss = 0x33FA, // R=3.9
    MagitekChakram = 0x33FB, // R=3.0
    MagitekExplosive = 0x33FC, // R=2.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    Downpour = 25333, // Boss->self, 5.0s cast, single-target
    Explosion = 25337, // MagitekExplosive->self, 7.0s cast, range 40 width 8 cross
    MagitekChakram = 25331, // Boss->self, 5.0s cast, single-target
    MagitekExplosive = 25336, // Boss->self, 5.0s cast, single-target
    MagitekMissile = 25334, // Boss->self, 3.0s cast, single-target
    MagitekRay = 25340, // Boss->self, 3.0s cast, range 50 width 6 rect
    MightyBlow = 25332, // MagitekChakram->self, 7.0s cast, range 40 width 8 rect
    SurfaceMissile = 25335, // Helper->location, 3.5s cast, range 6 circle
    ThermalSuppression = 25338 // Boss->self, 5.0s cast, range 60 circle
}

public enum SID : uint
{
    Minimum = 2504, // none->player, extra=0x14
    Breathless = 2672 // none->player, extra=0x1/0x2/0x3/0x4/0x5/0x6
}

sealed class DownpourMagitekChakram(BossModule module) : Components.GenericAOEs(module)
{
    private enum Mechanic { None, Downpour, Chakram }
    private Mechanic CurrentMechanic;
    private static readonly AOEShapeRect square = new(4f, 4f, 4f), squareInv = new(4f, 4f, 4f, invertForbiddenZone: true);
    private static readonly WPos toad = new(213f, 306f);
    private static readonly WPos mini = new(229f, 306f);
    private BitMask _status;
    private bool avoidSquares;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (CurrentMechanic == Mechanic.None)
            return [];

        var aoes = new AOEInstance[2];
        if (CurrentMechanic == Mechanic.Downpour)
        {
            var breathless = _status[slot];
            aoes[0] = new(breathless ? squareInv : square, toad, color: breathless ? Colors.SafeFromAOE : default);
            aoes[1] = new(square, mini);
        }
        else if (CurrentMechanic == Mechanic.Chakram)
        {
            var minimum = !avoidSquares && !_status[slot];
            aoes[0] = new(minimum ? squareInv : square, mini, color: minimum ? Colors.SafeFromAOE : default);
            aoes[1] = new(square, toad);
        }
        return aoes;
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID is (uint)SID.Breathless or (uint)SID.Minimum)
        {
            _status.Set(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID is (uint)SID.Breathless or (uint)SID.Minimum)
        {
            _status.Clear(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index is 0x01 or 0x02 && state == 0x00080004u)
        {
            avoidSquares = false;
            CurrentMechanic = Mechanic.None;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Downpour)
        {
            CurrentMechanic = Mechanic.Downpour;
        }
        else if (spell.Action.ID == (uint)AID.MagitekChakram)
        {
            CurrentMechanic = Mechanic.Chakram;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ThermalSuppression && CurrentMechanic != Mechanic.None)
        {
            avoidSquares = true;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (CurrentMechanic == Mechanic.Chakram && !_status[slot] && !avoidSquares)
        {
            hints.Add("Walk onto purple square!");
        }
        else if (CurrentMechanic == Mechanic.Downpour && _status[slot])
        {
            hints.Add("Walk onto green square!");
        }
    }
}

sealed class ThermalSuppression(BossModule module) : Components.RaidwideCast(module, (uint)AID.ThermalSuppression);
sealed class MightyRay(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MagitekRay, new AOEShapeRect(50f, 3f));
sealed class Explosion(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Explosion, new AOEShapeCross(40f, 4f));
sealed class SurfaceMissile(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SurfaceMissile, 6f);

sealed class D022LugaeStates : StateMachineBuilder
{
    public D022LugaeStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ThermalSuppression>()
            .ActivateOnEnter<DownpourMagitekChakram>()
            .ActivateOnEnter<MightyRay>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<SurfaceMissile>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 785, NameID = 10281)]
public sealed class D022Lugae(WorldState ws, Actor primary) : BossModule(ws, primary, new(221f, 306f), new ArenaBoundsSquare(19.5f));
