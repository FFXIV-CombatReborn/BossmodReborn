namespace BossMod.Global.Quest.FF15Collab.Iseultalon;

public enum OID : uint
{
    Boss = 0x252C, //R=5.0
    Helper = 0x25B4,
    Helper2 = 0x2523,
    Helper3 = 0x260C,
    Helper4 = 0x25B3,
    Helper5 = 0x273D,
    Noctis = 0x2650
}

public enum AID : uint
{
    AutoAttack2 = 872, // Boss->player, no cast, single-target
    Stomp = 14605, // Boss->location, 3.0s cast, range 80 circle
    DeathRay = 14602, // Boss->self, 4.0s cast, single-target
    DeathRay2 = 14603, // 273D->self, 4.0s cast, range 40 width 2 rect
    DeathRay3 = 14604, // Boss->self, no cast, range 40 width 22 rect
    NeedleShot = 14606, // Boss->Noctis, 5.0s cast, range 5 circle
    Thunderbolt = 15082, // Boss->self, 5.0s cast, single-target
    Thunderbolt2 = 15083, // 260C->player/Noctis, no cast, range 5 circle
    unknown = 14608, // Boss->self, no cast, single-target
    Electrocution = 14609, // 25B3->location, 10.0s cast, range 3 circle, tower
    Electrocution2 = 14981, // 25B3->location, 10.0s cast, range 3 circle, tower
    FatalCurrent = 14610, // Helper/Helper2->self, 2.0s cast, range 80 circle, tower fail
    TailWhip = 14607 // Boss->self, 3.0s cast, range 12 270-degree cone
}

public enum IconID : uint
{
    Stackmarker = 93, // 2650
    Spreadmarker = 129 // player/2650
}

class Thunderbolt(BossModule module) : Components.UniformStackSpread(module, default, 5f)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Spreadmarker)
            AddSpread(actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Thunderbolt2)
            Spreads.Clear();
    }
}

class Electrocution(BossModule module) : Components.GenericTowers(module)
{ // Noctis always goes to soak this tower, except on first cast as a tutorial
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Electrocution)
        {
            if (NumCasts > 0)
                Towers.Add(new(DeterminePosition(caster, spell), 3, forbiddenSoakers: Raid.WithSlot(true, true, false).WhereActor(p => p.InstanceID == Raid.Player()!.InstanceID).Mask()));
            else
                Towers.Add(new(DeterminePosition(caster, spell), 3));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.Electrocution or (uint)AID.FatalCurrent)
        {
            Towers.RemoveAll(t => t.Position.AlmostEqual(DeterminePosition(caster, spell), 1));
            ++NumCasts;
        }
    }

    private WPos DeterminePosition(Actor caster, ActorCastInfo spell) => spell.TargetID == caster.InstanceID ? caster.Position : WorldState.Actors.Find(spell.TargetID)?.Position ?? spell.LocXZ;
}

class Electrocution2(BossModule module) : Components.CastTowers(module, (uint)AID.Electrocution2, 3f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Towers.Count > 0)
            hints.AddForbiddenZone(new SDInvertedCircle(Towers[0].Position, 3f));
    }
}

class Stomp(BossModule module) : Components.RaidwideCast(module, (uint)AID.Stomp);

class DeathRay : Components.SimpleAOEs
{
    public DeathRay(BossModule module) : base(module, (uint)AID.DeathRay2, new AOEShapeRect(40f, 1f))
    {
        Color = Colors.Danger;
    }
}

class TailWhip(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TailWhip, new AOEShapeCone(12f, 135f.Degrees()));

class DeathRay2(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(40, 11);
    private DateTime _activation;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
            return new AOEInstance[1] { new(rect, Module.PrimaryActor.Position, Module.PrimaryActor.Rotation, _activation) };
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.DeathRay)
            _activation = WorldState.FutureTime(6.1d);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.DeathRay3)
            _activation = default;
    }
}

class NeedleShot(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.NeedleShot, 5f);

class IseultalonStates : StateMachineBuilder
{
    public IseultalonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Thunderbolt>()
            .ActivateOnEnter<Electrocution>()
            .ActivateOnEnter<Electrocution2>()
            .ActivateOnEnter<DeathRay>()
            .ActivateOnEnter<DeathRay2>()
            .ActivateOnEnter<Stomp>()
            .ActivateOnEnter<NeedleShot>()
            .ActivateOnEnter<TailWhip>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68695, NameID = 7895)]
public class Iseultalon(WorldState ws, Actor primary) : BossModule(ws, primary, new(-289, -30), new ArenaBoundsCircle(25)) // note the arena is actually a 6 sided polygon
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Noctis), Colors.Vulnerable);
    }
}
