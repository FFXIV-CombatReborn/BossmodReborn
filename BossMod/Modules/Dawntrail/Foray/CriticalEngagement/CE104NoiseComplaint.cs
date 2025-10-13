namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE104NoiseComplaint;

public enum OID : uint
{
    Boss = 0x469E, // R5.0
    Chatterbird = 0x469F, // R0.9
    Deathwall = 0x4864, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{

    AutoAttack = 872, // Boss->player, no cast, single-target
    Deathwall = 41259, // Deathwall->self, no cast, range 23-30 donut

    Squash = 41189, // Boss->player, 5.0s cast, single-target
    LightningCrossingVisual1 = 41183, // Boss->self, 3.5s cast, single-target
    LightningCrossingVisual2 = 41182, // Boss->self, 3.5s cast, single-target
    LightningCrossing = 41184, // Helper->self, 3.7s cast, range 70 45-degree cone
    LightningCrossingCharge = 42984, // Helper->self, no cast, range 70 45-degree cone
    Heave1 = 43262, // Boss->self, 4.0s cast, range 60 120-degree cone
    Heave2 = 41180, // Boss->self, 2.0s cast, range 60 120-degree cone
    AgitatedGroanVisual = 41188, // Boss->self, 5.0s cast, ???
    AgitatedGroan = 41190, // Helper->self, no cast, ???
    RushingRumble = 41175, // Boss->self, 6.0s cast, single-target
    Rush = 41178, // Boss->location, no cast, width 8 rect charge
    ChatterbirdVisual = 41181, // Chatterbird->self, no cast, single-target
    Rumble = 41179, // Boss->self, 1.0s cast, range 30 circle
    BirdserkRush = 41176, // Boss->self, 6.0s cast, single-target
    LightningCharge = 41185, // Boss->self, 5.0s cast, single-target
    EpicenterShock = 41186, // Helper->self, 5.0s cast, range 12 circle
    MammothBolt = 41187, // Helper->self, 6.0s cast, range 50 circle, proximity AOE, about 25 optimal range
    RushingRumbleRampage = 41177 // Boss->self, 6.0s cast, single-target
}

public enum IconID : uint
{
    Chatterbird = 578 // Chatterbird->self
}

public enum SID : uint
{
    LightningCrossing = 2193 // none->Boss, extra=0x350/0x351 => 350 is card, 351 is intercard
}

sealed class LightningCrossingMammothBoltEpicenterShock(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(14);
    public static readonly AOEShapeCircle CircleSmall = new(12f);
    public static readonly AOEShapeCircle CircleBig = new(30f);
    public static readonly AOEShapeCone Cone = new(70f, 22.5f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var deadline = aoes[0].Activation.AddSeconds(3.5d);

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

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = spell.Action.ID switch
        {
            (uint)AID.EpicenterShock => CircleSmall,
            (uint)AID.MammothBolt => CircleBig,
            (uint)AID.LightningCrossing => Cone,
            _ => null
        };
        if (shape != null)
        {
            _aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID));
            if (_aoes.Count > 4)
            {
                _aoes.Sort(static (a, b) => a.Activation.CompareTo(b.Activation));
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.MammothBolt or (uint)AID.LightningCrossing or (uint)AID.EpicenterShock)
        {
            var count = _aoes.Count;
            var id = caster.InstanceID;
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            for (var i = 0; i < count; ++i)
            {
                if (aoes[i].ActorID == id)
                {
                    _aoes.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

sealed class Heave(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Heave1, (uint)AID.Heave2], new AOEShapeCone(60f, 60f.Degrees()));
sealed class Squash(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.Squash);
sealed class AgitatedGroan(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.AgitatedGroanVisual, (uint)AID.AgitatedGroan, 0.9d);

sealed class RushingRumbleRampage(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(6);
    private bool? lightningIsCardinal;
    private readonly List<Actor> activebirds = new(2);
    private bool showBait;
    private DateTime activation;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!showBait || activebirds.Count == 0)
        {
            return CollectionsMarshal.AsSpan(_aoes);
        }
        else
        {
            var dir = activebirds[0].Position - Arena.Center;
            AOEInstance[] bait = [new(new AOEShapeRect(dir.Length(), 4f), Arena.Center.Quantized(), Angle.FromDirection(dir), activation)];
            return bait;
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.LightningCrossing)
        {
            lightningIsCardinal = status.Extra == 0x350;
            InitIfReady();
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.LightningCrossing)
        {
            lightningIsCardinal = null;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Chatterbird)
        {
            if (_aoes.Count == 0)
            {
                activebirds.Clear();
            }
            activebirds.Add(actor);
            if (NumCasts != 1)
            {
                InitIfReady();
            }
            else
            {
                showBait = true;
                activation = WorldState.FutureTime(6.3d);
            }
        }
    }

    private void InitIfReady()
    {
        if (_aoes.Count == 0 && activebirds.Count != 0 && lightningIsCardinal != null)
        {
            var primaryPos = Module.PrimaryActor.Position;
            var birdPos = activebirds[0].Position;
            var destination = (birdPos - 6f * (birdPos - CE104NoiseComplaint.ArenaCenter).Normalized()).Quantized();
            var dir = destination - primaryPos;
            var angle = Angle.FromDirection(dir);
            _aoes.Add(new(new AOEShapeRect(dir.Length(), 4f), primaryPos.Quantized(), angle, WorldState.FutureTime(6.3d)));
            _aoes.Add(new(LightningCrossingMammothBoltEpicenterShock.CircleBig, destination.Quantized(), default, WorldState.FutureTime(9.4d)));
            var act = WorldState.FutureTime(10.5d);
            var anglecone = angle + (lightningIsCardinal == true ? default : 45f).Degrees();
            for (var i = 0; i < 4; ++i)
            {
                _aoes.Add(new(LightningCrossingMammothBoltEpicenterShock.Cone, destination, anglecone + i * 90f.Degrees(), act));
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.Rumble)
        {
            _aoes.RemoveAt(0);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var id = spell.Action.ID;
        if (id == (uint)AID.Rush)
        {
            showBait = false;
            if (++NumCasts == 5) // to make the rest of the logic easier, it always alternates between two versions after the initial tutorial
            {
                NumCasts = 1;
            }
            if (_aoes.Count != 0)
            {
                _aoes.RemoveAt(0);
            }
            if (activebirds.Count != 0)
            {
                activebirds.RemoveAt(0);
            }
        }
        else if (id == (uint)AID.LightningCrossingCharge && _aoes.Count != 0)
        {
            _aoes.RemoveAt(0);
            InitIfReady();
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (!showBait)
        {
            return;
        }
        hints.GoalZones.Add(AIHints.GoalSingleTarget(activebirds[0].Position, 12f, 5f)); // follow the charge
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (!showBait)
        {
            return;
        }
        hints.Add("Follow the charge!");
    }
}

sealed class CE104NoiseComplaintStates : StateMachineBuilder
{
    public CE104NoiseComplaintStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LightningCrossingMammothBoltEpicenterShock>()
            .ActivateOnEnter<Heave>()
            .ActivateOnEnter<Squash>()
            .ActivateOnEnter<AgitatedGroan>()
            .ActivateOnEnter<RushingRumbleRampage>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CriticalEngagement, GroupID = 1018, NameID = 44)]
public sealed class CE104NoiseComplaint(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter.Quantized(), new ArenaBoundsCircle(23f))
{
    public static readonly WPos ArenaCenter = new(461f, -363f);

    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InCircle(Arena.Center, 23f);
}
