namespace BossMod.RealmReborn.Quest.MSQ.TheStepsOfFaith;

public enum OID : uint
{
    Boss = 0x3A5F, // R30.0
    HordeWyvern1 = 0x3CD5, // R3.6
    HordeWyvern2 = 0x3AA2, // R3.6
    HordeWyvern3 = 0x3AA4, // R3.6
    HordeWyvern4 = 0x3AA5, // R3.6
    HordeWyvern5 = 0x3AAC, // R3.6
    HordeWyvern6 = 0x3AA7, // R3.6
    HordeWyvern7 = 0x3AAF, // R3.6
    HordeWyvern8 = 0x3ABF, // R3.6
    HordeWyvern9 = 0x3AA8, // R3.6
    HordeWyvern10 = 0x3AA9, // R3.6
    HordeDragonfly1 = 0x3A94, // R0.8
    HordeDragonfly2 = 0x3A93, // R0.8
    HordeDragonfly3 = 0x3A95, // R0.8
    HordeDragonfly4 = 0x3A96, // R0.8
    HordeDragonfly5 = 0x3A97, // R0.8
    HordeDragonfly6 = 0x3ABB, // R0.8
    HordeDragonfly7 = 0x3ABC, // R0.8
    HordeDragonfly8 = 0x3AB0, // R0.8
    HordeAevis1 = 0x3A9A, // R2.2
    HordeAevis2 = 0x3A9B, // R2.2
    HordeAevis3 = 0x3A9C, // R2.2
    HordeAevis4 = 0x3A9D, // R2.2
    HordeAevis5 = 0x3AA0, // R2.2
    HordeAevis6 = 0x3A9E, // R2.2
    HordeAevis7 = 0x3A9F, // R2.2
    HordeAevis8 = 0x3AC0, // R2.2
    HordeAevis9 = 0x3AB1, // R2.2
    HordeBiast1 = 0x3AB2, // R2.7
    HordeBiast2 = 0x3AB3, // R2.7
    HordeBiast3 = 0x3AB4, // R2.7
    HordeBiast4 = 0x3AAB, // R2.7
    HordeBiast5 = 0x3AB6, // R2.7
    HordeBiast6 = 0x3AC2, // R2.7
    HordeBiast7 = 0x3AB8, // R2.7
    HordeBiast8 = 0x3AB9, // R2.7
    HordeBiast9 = 0x3AB7, // R2.7
    HordeArmoredDragon = 0x3ABA, // R6.25
    HordeTranscendent = 0x3ABD, // R3.4
    HordeShieldDragon = 0x3AC1, // R5.3
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 6499, // horde dragons->allies, no cast, single-target

    BlazingShriekVisual = 30882, // Boss->self, no cast, single-target
    BlazingShriek = 26407, // Helper->self, 0.8s cast, range 100 width 44 rect
    FlameBreathVisual = 30877, // Boss->self, 3.3+1,7s cast, single-target
    FlameBreath = 26812, // Helper->self, 35.0s cast, range 1 width 2 rect
    FlameBreath1 = 30185, // Helper->self, 5.0s cast, range 1 width 2 rect
    FlameBreath2 = 26411, // Boss->self, 3.8+1.2s cast, range 60 width 20 rect
    FlameBreath3 = 30186, // Helper->self, 5.0s cast, range 60 width 20 rect
    FlameBreathChannel = 30884, // Helper->self, no cast, range 40 width 20 rect
    CauterizeVisual = 30878, // Boss->self, 30.5+4.5s cast, single-target
    Cauterize = 30885, // Helper->self, no cast, range 40 width 44 rect
    Touchdown = 26408, // Helper->self, 6.0s cast, range 80 circle
    FireballVisual1 = 30874, // Boss->self, 3.0+3,0s cast, single-target
    FireballVisual2 = 30876, // Boss->self, 3.0s cast, single-target
    FireballVisual3 = 28975, // Boss->self, 3.0s cast, single-target
    FireballSpread = 30875, // Helper->allies, 6.0s cast, range 6 circle
    FireballAOE = 30894, // HordeTranscendent->location, 3.5s cast, range 6 circle
    BlazingFire = 30211, // Boss->location, no cast, range 10 circle

    BodySlamVisual = 26400, // Boss->self, 4.7+1,3s cast, single-target
    BodySlam = 26401, // Helper->self, 6.0s cast, range 80 width 44 rect
    Flamisphere = 30883, // Helper->location, 8.0s cast, range 10 circle

    RipperClaw = 31262, // HordeTranscendent->self, 3.7s cast, range 9 90-degree cone
    EarthshakerAOE = 30880, // Boss->self, 4.5s cast, range 31 circle
    Earthshaker = 30887, // Helper->self, 6.5s cast, range 80 30-degree cone
    EarthrisingAOE = 26410, // Boss->self, 4.5s cast, range 31 circle
    EarthrisingCast = 30888, // Helper->self, 7.0s cast, range 8 circle
    EarthrisingRepeat = 26412, // Helper->self, no cast, range 8 circle
    SidewiseSlice = 30879, // Boss->self, 8.0s cast, range 50 120-degree cone
    ScorchingBreathVisual = 29785, // Boss->self, 15.0+5.0s cast, single-target
    ScorchingBreath = 29789, // Helper->self, no cast, range 40 width 20 rect
    SeismicShriekVisual = 30881, // Boss->self, no cast, single-target
    SeismicShriek1 = 26405, // Boss->self, no cast, range 100 circle
    SeismicShriek2 = 26406, // Boss->self, no cast, range 80 circle
    Twingaze = 28971, // Boss->self, no cast, single-target
    Levinshower = 30892, // HordeBiast2/HordeBiast4/HordeBiast5/HordeBiast6/HordeBiast7->self, no cast, range 6 120-degree cone
    DragonStomp = 30893, // HordeArmoredDragon->self, 2.0s cast, range 40 circle
    BoneShaker = 31258, // HordeTranscendent->self, no cast, range 50 circle
    MagmaticSpell = 28974, // Boss->self, no cast, single-target
    Rake = 30898, // HordeShieldDragon->player, no cast, single-target
    FallOfMan = 30187, // Helper->self, 20.0s cast, range 90 width 20 rect
}

class RipperClaw(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RipperClaw, new AOEShapeCone(9f, 45f.Degrees()));
class Levinshower(BossModule module) : Components.Cleave(module, (uint)AID.Levinshower, new AOEShapeCone(6f, 60f.Degrees()),
[(uint)OID.HordeBiast2, (uint)OID.HordeBiast4, (uint)OID.HordeBiast5, (uint)OID.HordeBiast6, (uint)OID.HordeBiast7]);
class EarthShakerAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EarthshakerAOE, 31f);
class Earthshaker(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Earthshaker, new AOEShapeCone(80f, 15f.Degrees()), 2);

class EarthrisingAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EarthrisingAOE, 31f);
class Earthrising(BossModule module) : Components.Exaflare(module, 8f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.EarthrisingCast)
        {
            Lines.Add(new(caster.Position, new(default, -7.5f), Module.CastFinishAt(spell), 1d, 5, 2));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.EarthrisingRepeat or (uint)AID.EarthrisingCast)
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
                        Lines.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

class SidewiseSlice(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SidewiseSlice, new AOEShapeCone(50f, 60f.Degrees()));

class FireballSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.FireballSpread, 6f);
class FireballAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FireballAOE, 6f);
class Flamisphere(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Flamisphere, 10f);

class BodySlam(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.BodySlam, 20f, kind: Kind.DirForward, stopAtWall: true);

class FlameBreath(BossModule module) : Components.GenericAOEs(module, (uint)AID.FlameBreathChannel)
{
    private AOEInstance[] _aoe = [];
    private static readonly AOEShapeRect rect = new(500f, 10f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FlameBreath1)
        {
            _aoe = [new(rect, Module.PrimaryActor.Position, Module.PrimaryActor.Rotation, Module.CastFinishAt(spell, 1d))];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (NumCasts >= 35)
        {
            _aoe = [];
            NumCasts = 0;
        }
    }
}

class FlameBreath2(BossModule module) : Components.GenericAOEs(module, (uint)AID.FlameBreathChannel)
{
    private AOEInstance[] _aoe = [];
    private static readonly AOEShapeRect rect = new(60f, 10f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.FlameBreath2)
        {
            NumCasts = 0;
            _aoe = [new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell))];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (NumCasts >= 14)
        {
            _aoe = [];
        }
    }
}

class Cauterize(BossModule module) : Components.GenericAOEs(module, (uint)AID.Cauterize)
{
    private Actor? Source;
    private static readonly AOEShapeRect rect = new(160f, 22f);
    private static readonly AOEShapeRect MoveIt = new(40f, 22f, 38f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Source == null)
            return [];

        if (Arena.Center.Z > 218f)
            return new AOEInstance[1] { new(MoveIt, Arena.Center) };
        else
            return new AOEInstance[1] { new(rect, Source.Position, 180f.Degrees(), Module.CastFinishAt(Source.CastInfo)) };
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            Source = Module.PrimaryActor;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            Source = null;
    }
}

class Touchdown(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Touchdown, 10f, stopAtWall: true);

class ScorchingBreath(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(100f, 10f, 100f);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ScorchingBreath)
        {
            ++NumCasts;
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (NumCasts != 0)
            return new AOEInstance[1] { new(rect, Module.PrimaryActor.Position, Module.PrimaryActor.Rotation, Module.CastFinishAt(Module.PrimaryActor.CastInfo)) };
        else
            return [];
    }
}

class ScrollingBounds(BossModule module) : BossComponent(module)
{
    public const float HalfHeight = 40f;
    public const float HalfWidth = 22f;

    public static readonly ArenaBoundsRect Bounds = new(HalfWidth, HalfHeight);

    private int Phase = 1;
    private (float Min, float Max) ZBounds = (120f, 300f);

    public override void OnMapEffect(byte index, uint state)
    {
        if (state == 0x00020001u)
        {
            if (index == 0x03)
            {
                ZBounds = (120f, 200f);
                Phase = 2;
            }
            else if (index == 0x04)
            {
                ZBounds = (-40f, 40f);
                Phase = 4;
            }
            else if (index == 0x06)
            {
                ZBounds = (-200f, -120f);
                Phase = 6;
            }
        }
        else if (state == 0x00800040u)
        {
            if (index == 0x00)
            {
                ZBounds = (-40f, 200f);
                Phase = 3;
            }
            else if (index == 0x01)
            {
                ZBounds = (-200f, 40f);
                Phase = 5;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // force player to walk south to aggro vishap (status 1268 = In Event, not actionable)
        if (Phase == 1 && !actor.InCombat && actor.FindStatus(1268u) == null)
            hints.AddForbiddenZone(new SDRect(Arena.Center, new WDir(default, 1f), 38f, 22f, 40f));
        // subsequent state transitions don't trigger until player moves into the area
        else if (Phase == 3 && actor.PosRot.Z > 25f || Phase == 5 && actor.PosRot.Z > -135f)
            hints.AddForbiddenZone(new SDRect(Arena.Center, new WDir(default, 1f), 40f, 22f, 38f));
    }

    public override void Update()
    {
        if (WorldState.Party.Player() is not Actor p)
            return;
        Arena.Center = new(0, Math.Clamp(p.PosRot.Z, ZBounds.Min + HalfHeight, ZBounds.Max - HalfHeight));
    }
}

class VishapStates : StateMachineBuilder
{
    public VishapStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FlameBreath>()
            .ActivateOnEnter<Cauterize>()
            .ActivateOnEnter<Touchdown>()
            .ActivateOnEnter<FireballSpread>()
            .ActivateOnEnter<FireballAOE>()
            .ActivateOnEnter<Flamisphere>()
            .ActivateOnEnter<BodySlam>()
            .ActivateOnEnter<SidewiseSlice>()
            .ActivateOnEnter<ScrollingBounds>()
            .ActivateOnEnter<FlameBreath2>()
            .ActivateOnEnter<EarthShakerAOE>()
            .ActivateOnEnter<Earthshaker>()
            .ActivateOnEnter<EarthrisingAOE>()
            .ActivateOnEnter<Earthrising>()
            .ActivateOnEnter<RipperClaw>()
            .ActivateOnEnter<Levinshower>()
            .ActivateOnEnter<ScorchingBreath>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70127, NameID = 3330)]
public class Vishap(WorldState ws, Actor primary) : BossModule(ws, primary, new(default, 245f), ScrollingBounds.Bounds)
{
    // vishap doesn't start targetable
    private static readonly uint[] all = [(uint)OID.HordeWyvern1, (uint)OID.HordeWyvern2, (uint)OID.HordeWyvern3, (uint)OID.HordeWyvern4, (uint)OID.HordeWyvern5,
    (uint)OID.HordeWyvern6, (uint)OID.HordeWyvern7, (uint)OID.HordeWyvern8, (uint)OID.HordeWyvern8, (uint)OID.HordeWyvern9, (uint)OID.HordeWyvern10, (uint)OID.HordeDragonfly1,
    (uint)OID.HordeDragonfly2, (uint)OID.HordeDragonfly3, (uint)OID.HordeDragonfly4, (uint)OID.HordeDragonfly5, (uint)OID.HordeDragonfly6, (uint)OID.HordeDragonfly7,
    (uint)OID.HordeDragonfly8, (uint)OID.HordeAevis1, (uint)OID.HordeAevis2, (uint)OID.HordeAevis3, (uint)OID.HordeAevis4, (uint)OID.HordeAevis5, (uint)OID.HordeAevis6,
    (uint)OID.HordeAevis7, (uint)OID.HordeAevis8, (uint)OID.HordeAevis9, (uint)OID.HordeBiast1, (uint)OID.HordeBiast2, (uint)OID.HordeBiast3, (uint)OID.HordeBiast4,
    (uint)OID.HordeBiast5, (uint)OID.HordeBiast6, (uint)OID.HordeBiast7, (uint)OID.HordeBiast8, (uint)OID.HordeBiast9, (uint)OID.HordeArmoredDragon, (uint)OID.HordeShieldDragon,
    (uint)OID.HordeTranscendent];

    protected override bool CheckPull() => PrimaryActor.InCombat;

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, all);
        Arena.Actor(PrimaryActor, allowDeadAndUntargetable: true);
    }
}

