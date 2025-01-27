﻿namespace BossMod.Dawntrail.Dungeon.D01Ihuykatumu.D011PrimePunutiy;

public enum OID : uint
{
    Boss = 0x4190, // R7.99
    PalmTree1 = 0x1EBA45,
    PalmTree2 = 0x1EBA43,
    PalmTree3 = 0x1EBA46,
    PalmTree4 = 0x1EBA44,
    IhuykatumuFlytrap = 0x4194, // R1.6
    ProdigiousPunutiy = 0x4191, // R4.23
    Punutiy = 0x4192, // R2.82
    PetitPunutiy = 0x4193, // R2.115
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Boss/PetitPunutiy/Punutiy/ProdigiousPunutiy->player, no cast, single-target

    PunutiyPress = 36492, // Boss->self, 5.0s cast, range 60 circle
    Hydrowave = 36493, // Boss->self, 4.0s cast, range 60 30-degree cone
    Inhale = 36496, // Helper->self, no cast, range 100 ?-degree cone

    ResurfaceVisual = 36495, // Boss->self, 7.0s cast, single-target
    Resurface = 36494, // Boss->self, 5.0s cast, range 100 60-degree cone

    Bury1 = 36497, // Helper->self, 4.0s cast, range 12 circle
    Bury2 = 36500, // Helper->self, 4.0s cast, range 35 width 10 rect
    Bury3 = 36498, // Helper->self, 4.0s cast, range 8 circle
    Bury4 = 36501, // Helper->self, 4.0s cast, range 4 circle
    Bury5 = 36499, // Helper->self, 4.0s cast, range 25 width 6 rect
    Bury6 = 36502, // Helper->self, 4.0s cast, range 6 circle
    Bury7 = 36503, // Helper->self, 4.0s cast, range 25 width 6 rect
    Bury8 = 36504, // Helper->self, 4.0s cast, range 35 width 10 rect

    Decay = 36505, // IhuykatumuFlytrap->self, 7.0s cast, range 6-40 donut

    SongOfThePunutiy = 36506, // Boss->self, 5.0s cast, single-target

    PunutiyFlop1 = 36508, // ProdigiousPunutiy->player, 8.0s cast, range 14 circle
    PunutiyFlop2 = 36513, // PetitPunutiy->player, 8.0s cast, range 6 circle

    HydrowaveBaitVisual1 = 36509, // Punutiy->self, 8.0s cast, single-target
    HydrowaveBaitVisual2 = 36511, // Punutiy->self, no cast, single-target
    HydrowaveBaitVisual3 = 36510, // Helper->player, no cast, single-target
    HydrowaveBait = 36512, // Helper->self, no cast, range 60 30-degree cone

    ShoreShakerVisual = 36514, // Boss->self, 4.0+1.0s cast, single-target
    ShoreShaker1 = 36515, // Helper->self, 5.0s cast, range 10 circle
    ShoreShaker2 = 36516, // Helper->self, 7.0s cast, range 10-20 donut
    ShoreShaker3 = 36517, // Helper->self, 9.0s cast, range 20-30 donut
}

public enum TetherID : uint
{
    BaitAway = 17, // ProdigiousPunutiy/Punutiy/PetitPunutiy->player
}

class HydrowaveBait(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeCone(60, 15.Degrees()), (uint)TetherID.BaitAway, ActionID.MakeSpell(AID.HydrowaveBait), (uint)OID.Punutiy, 8.6f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (CurrentBaits.Any(x => x.Target == actor))
            hints.AddForbiddenZone(ShapeDistance.Rect(Arena.Center - new WDir(0, -18), Arena.Center - new WDir(0, 18), 18), WorldState.FutureTime(ActivationDelay));
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether) { } // snapshot is ~0.6s after tether disappears

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.HydrowaveBait)
            CurrentBaits.Clear();
    }
}

class Resurface(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Resurface), new AOEShapeCone(100, 30.Degrees()));
class Inhale(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private static readonly AOEShapeCone cone = new(100, 30.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Resurface)
            _aoe = new(cone, new(15, -95), spell.Rotation, Module.CastFinishAt(spell)); // Resurface and Inhale origin are not identical, but almost 0.4y off
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Inhale)
            if (++NumCasts == 6)
            {
                _aoe = null;
                NumCasts = 0;
            }
    }
}

class PunutiyPress(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.PunutiyPress));
class Hydrowave(BossModule module) : Components.SimpleAOEs(module, ActionID.MakeSpell(AID.Hydrowave), new AOEShapeCone(60, 15.Degrees()));

class BuryDecay(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(12), new AOEShapeRect(35, 5), new AOEShapeCircle(8), new AOEShapeCircle(4),
    new AOEShapeRect(25, 3), new AOEShapeCircle(6), new AOEShapeRect(25, 3), new AOEShapeRect(35, 5), new AOEShapeDonut(6, 40)];
    private static readonly HashSet<AID> castEnd = [AID.Bury1, AID.Bury2, AID.Bury3, AID.Bury4, AID.Bury5, AID.Bury6, AID.Bury7, AID.Bury8, AID.Decay];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        List<AOEInstance> aoes = new(count);
        for (var i = 0; i < count; ++i)
        {
            var aoe = _aoes[i];
            if (i < 2)
                aoes.Add(count > 1 ? aoe with { Color = Colors.Danger } : aoe);
            else if (i > 1)
                aoes.Add(aoe);
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddAOE(AOEShape shape)
        => _aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
        switch ((AID)spell.Action.ID)
        {
            case AID.Bury1:
                AddAOE(_shapes[0]);
                break;
            case AID.Bury2:
                AddAOE(_shapes[1]);
                break;
            case AID.Bury3:
                AddAOE(_shapes[2]);
                break;
            case AID.Bury4:
                AddAOE(_shapes[3]);
                break;
            case AID.Bury5:
                AddAOE(_shapes[4]);
                break;
            case AID.Bury6:
                AddAOE(_shapes[5]);
                break;
            case AID.Bury7:
                AddAOE(_shapes[6]);
                break;
            case AID.Bury8:
                AddAOE(_shapes[7]);
                break;
            case AID.Decay:
                AddAOE(_shapes[8]);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && castEnd.Contains((AID)spell.Action.ID))
            _aoes.RemoveAt(0);
    }
}

class PunutiyFlop1(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.PunutiyFlop1), 14);
class PunutiyFlop2(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.PunutiyFlop2), 6);

class ShoreShaker(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(10), new AOEShapeDonut(10, 20), new AOEShapeDonut(20, 30)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ShoreShaker1)
            AddSequence(Arena.Center, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (Sequences.Count > 0)
        {
            var order = (AID)spell.Action.ID switch
            {
                AID.ShoreShaker1 => 0,
                AID.ShoreShaker2 => 1,
                AID.ShoreShaker3 => 2,
                _ => -1
            };
            AdvanceSequence(order, Arena.Center, WorldState.FutureTime(2));
        }
    }
}

class D011PrimePunutiyStates : StateMachineBuilder
{
    public D011PrimePunutiyStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Inhale>()
            .ActivateOnEnter<Resurface>()
            .ActivateOnEnter<PunutiyPress>()
            .ActivateOnEnter<Hydrowave>()
            .ActivateOnEnter<HydrowaveBait>()
            .ActivateOnEnter<BuryDecay>()
            .ActivateOnEnter<PunutiyFlop1>()
            .ActivateOnEnter<PunutiyFlop2>()
            .ActivateOnEnter<ShoreShaker>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 826, NameID = 12723)]
public class D011PrimePunutiy(WorldState ws, Actor primary) : BossModule(ws, primary, new(35, -95), new ArenaBoundsSquare(19.5f))
{
    private static readonly uint[] adds = [(uint)OID.Punutiy, (uint)OID.PetitPunutiy, (uint)OID.ProdigiousPunutiy];
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies(adds));
    }
}
