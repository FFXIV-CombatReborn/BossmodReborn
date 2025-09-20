namespace BossMod.Dawntrail.Dungeon.D01Ihuykatumu.D011PrimePunutiy;

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
    ShoreShaker3 = 36517 // Helper->self, 9.0s cast, range 20-30 donut
}

public enum TetherID : uint
{
    BaitAway = 17 // ProdigiousPunutiy/Punutiy/PetitPunutiy->player
}

sealed class HydrowaveBait(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeCone(60f, 15f.Degrees()), (uint)TetherID.BaitAway, (uint)AID.HydrowaveBait, (uint)OID.Punutiy, 8.6d)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (ActiveBaitsOn(actor).Count != 0)
        {
            hints.AddForbiddenZone(new SDRect(Arena.Center - new WDir(default, -18f), Arena.Center - new WDir(default, 18f), 18f), WorldState.FutureTime(ActivationDelay));
        }
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether) { } // snapshot is ~0.6s after tether disappears

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.HydrowaveBait)
        {
            CurrentBaits.Clear();
        }
    }
}

sealed class Resurface(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Resurface, Inhale.Cone);
sealed class Inhale(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    public static readonly AOEShapeCone Cone = new(100f, 30f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Resurface)
        {
            _aoe = [new(Cone, new WPos(15f, -95f).Quantized(), spell.Rotation, Module.CastFinishAt(spell))]; // Resurface and Inhale origin are not identical, but almost 0.4y off
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Inhale)
        {
            if (++NumCasts == 6)
            {
                _aoe = [];
                NumCasts = 0;
            }
        }
    }
}

sealed class PunutiyPress(BossModule module) : Components.RaidwideCast(module, (uint)AID.PunutiyPress);
sealed class Hydrowave(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Hydrowave, new AOEShapeCone(60f, 15f.Degrees()));

sealed class BuryDecay(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(12f), new AOEShapeRect(35f, 5f), new AOEShapeCircle(8f), new AOEShapeCircle(4f),
    new AOEShapeRect(25f, 3f), new AOEShapeCircle(6f), new AOEShapeRect(25f, 3f), new AOEShapeRect(35f, 5f), new AOEShapeDonut(6f, 40f)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        if (count > 2)
        {
            var color = Colors.Danger;
            for (var i = 0; i < 2; ++i)
            {
                aoes[i].Color = color;
            }
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var shape = spell.Action.ID switch
        {
            (uint)AID.Bury1 => _shapes[0],
            (uint)AID.Bury2 => _shapes[1],
            (uint)AID.Bury3 => _shapes[2],
            (uint)AID.Bury4 => _shapes[3],
            (uint)AID.Bury5 => _shapes[4],
            (uint)AID.Bury6 => _shapes[5],
            (uint)AID.Bury7 => _shapes[6],
            (uint)AID.Bury8 => _shapes[7],
            (uint)AID.Decay => _shapes[8],
            _ => null
        };
        if (shape != null)
        {
            _aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0)
        {
            switch (spell.Action.ID)
            {
                case (uint)AID.Bury1:
                case (uint)AID.Bury2:
                case (uint)AID.Bury3:
                case (uint)AID.Bury4:
                case (uint)AID.Bury5:
                case (uint)AID.Bury6:
                case (uint)AID.Bury7:
                case (uint)AID.Bury8:
                case (uint)AID.Decay:

                    _aoes.RemoveAt(0);
                    break;
            }
        }
    }
}

sealed class PunutiyFlop1(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.PunutiyFlop1, 14f);
sealed class PunutiyFlop2(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.PunutiyFlop2, 6f);

sealed class ShoreShaker(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(10f), new AOEShapeDonut(10f, 20f), new AOEShapeDonut(20f, 30f)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ShoreShaker1)
        {
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.ShoreShaker1 => 0,
                (uint)AID.ShoreShaker2 => 1,
                (uint)AID.ShoreShaker3 => 2,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2d));
        }
    }
}

sealed class D011PrimePunutiyStates : StateMachineBuilder
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

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 826u, NameID = 12723u)]
public sealed class D011PrimePunutiy(WorldState ws, Actor primary) : BossModule(ws, primary, new(35f, -95f), new ArenaBoundsSquare(19.5f))
{
    private static readonly uint[] adds = [(uint)OID.Punutiy, (uint)OID.PetitPunutiy, (uint)OID.ProdigiousPunutiy];
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(this, adds);
    }
}
