namespace BossMod.Dawntrail.Foray.FATE.ShorelineShowdown;

public enum OID : uint
{
    RegnantChimera = 0x4C7D,
    Helper = 0x233C,
    GlacipotentOrb = 0x4C80, // R2.000, x0 (spawn during fight)
    FulmipotentOrb = 0x4C7F, // R2.000, x0 (spawn during fight)
    Cacophony = 0x4B71, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 50856, // RegnantChimera->player, no cast, single-target
    TheRamsBreath = 48631, // RegnantChimera->self, 6.0s cast, range 30 120-degree cone
    TheRamsBreath1 = 48632, // RegnantChimera->self, no cast, range 30 120-degree cone
    TheRamsBreath2 = 49748, // RegnantChimera->self, no cast, range 30 120-degree cone
    TheDragonsBreath = 48629, // RegnantChimera->self, 6.0s cast, range 30 120-degree cone
    TheDragonsBreath1 = 48630, // RegnantChimera->self, no cast, range 30 120-degree cone
    TheDragonsBreath2 = 49747, // RegnantChimera->self, no cast, range 30 120-degree cone
    TheRamsVoice = 48633, // RegnantChimera->self, 4.0s cast, range 9 circle
    TheRamsVoice1 = 48635, // 4C80->location, 1.0s cast, range 12 circle
    TheDragonsVoice = 48634, // RegnantChimera->self, 4.0s cast, range 8-30 donut
    TheDragonsVoice1 = 48636, // 4C7F->location, 4.0s cast, range 8-30 donut
    Cacophony = 50113, // RegnantChimera->self, 4.0s cast, single-target
    ChaoticChorus = 50114, // 4B71->self, 1.5s cast, range 6 circle
    LeftDuobreath = 50111, // Boss->self, 5.0s cast, range 40 180-degree cone
    TheRamsBreath3 = 50116, // Boss->self, no cast, range 40 180-degree cone
    RightDuobreath = 50112, // Boss->self, 5.0s cast, range 40 180-degree cone
    TheDragonsBreath3 = 50115, // Boss->self, no cast, range 40 180-degree cone
}

public enum SID : uint
{
    Gen = 5196, // RegnantChimera/4C80->4C80/RegnantChimera, extra=0x0
    Gen1 = 5197, // RegnantChimera/4C7F->4C7F/RegnantChimera, extra=0x0
}

public enum IconID : uint
{
    TurnLeft = 547, // RegnantChimera->self
    TurnRight = 546, // RegnantChimera->self
}

sealed class TheRamsVoice(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheRamsVoice, 9f);
sealed class TheDragonsVoice(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheDragonsVoice1, new AOEShapeDonut(8f, 30f));
sealed class ChaoticChorus(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ChaoticChorus, 6f);

sealed class Breath(BossModule module) : Components.GenericRotatingAOE(module)
{
    private ActorCastInfo? spellInfo;
    private Angle increment;
    private readonly AOEShapeCone shape = new(30f, 60f.Degrees());

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        increment = iconID switch
        {
            (uint)IconID.TurnLeft => 120.0f.Degrees(),
            (uint)IconID.TurnRight => -120.0f.Degrees(),
            _ => default
        };

        InitIfReady();
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.TheRamsBreath or (uint)AID.TheDragonsBreath)
        {
            spellInfo = spell;
            InitIfReady();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.TheRamsBreath or (uint)AID.TheRamsBreath1 or (uint)AID.TheRamsBreath2 or
            (uint)AID.TheDragonsBreath or (uint)AID.TheDragonsBreath1 or (uint)AID.TheDragonsBreath2)
        {
            if (Sequences.Count > 0)
            {
                AdvanceSequence(0, WorldState.CurrentTime);
            }
        }
    }

    private void InitIfReady()
    {
        if (spellInfo != null && increment != default)
        {
            Sequences.Add(new(shape, spellInfo.LocXZ, spellInfo.Rotation, increment, Module.CastFinishAt(spellInfo), 2.7d, 3));
            spellInfo = null;
            increment = default;
        }
    }
}

sealed class GlacipotentOrb(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoesBuffer = [];
    private readonly List<Actor> iceOrbs = [];
    private readonly AOEShapeCircle shape = new(12f);
    private bool active = false;

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.GlacipotentOrb)
        {
            iceOrbs.Add(actor);
        }
    }

    public override void OnActorDeath(Actor actor)
    {
        if (actor.OID == (uint)OID.GlacipotentOrb)
        {
            iceOrbs.Remove(actor);

            if (iceOrbs.Count == 0)
            {
                active = false;
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TheRamsVoice)
        {
            active = true;
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (iceOrbs.Count == 0 || !active)
        {
            return [];
        }

        var aoes = _aoesBuffer;
        aoes.Clear();
        foreach (var orb in iceOrbs)
        {
            aoes.Add(new(shape, orb.Position, orb.Rotation));
        }

        return CollectionsMarshal.AsSpan(aoes);
    }
}

sealed class TheDragonsVoiceBoss(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheDragonsVoice, new AOEShapeDonut(8f, 30f))
{
    private readonly List<Actor> orbs = [];

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.FulmipotentOrb)
        {
            orbs.Add(actor);
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.FulmipotentOrb)
        {
            orbs.Remove(actor);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (Casters.Count == 0)
        {
            return;
        }

        if (orbs.Count <= 2)
        {
            return;
        }

        Actor? singleOrb = null;
        var bestDistance = float.MinValue;
        foreach (var orb in orbs)
        {
            var distance = float.MaxValue;
            var countO = orbs.Count;
            for (var i = 0; i < countO; ++i)
            {
                var o = orbs[i];
                if (o != orb)
                {
                    var d = (o.Position - orb.Position).LengthSq();
                    if (d < distance)
                        distance = d;
                }
            }
            if (distance > bestDistance)
            {
                bestDistance = distance;
                singleOrb = orb;
            }
        }

        if (singleOrb == null)
        {
            return;
        }

        var spellInstance = Casters[0];
        var distanceToOrb = spellInstance.Origin + (singleOrb.Position - spellInstance.Origin).Normalized() * 6f;
        hints.GoalZones.Add(AIHints.GoalProximity(distanceToOrb, 2f, 100f));
    }
}

sealed class Cacophony(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoesBuffer = [];
    private readonly List<Actor> orbs = [];
    private readonly AOEShapeCircle shape = new(6f);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Cacophony)
        {
            orbs.Add(actor);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ChaoticChorus)
        {
            orbs.Remove(caster);
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (orbs.Count == 0)
        {
            return [];
        }

        var aoes = _aoesBuffer;
        aoes.Clear();
        foreach (var orb in orbs)
        {
            aoes.Add(new(shape, orb.Position, orb.Rotation, WorldState.FutureTime(1.5f)));
        }

        return CollectionsMarshal.AsSpan(aoes);
    }
}

sealed class Duobreath(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [with(2)];
    private readonly AOEShapeCone shape = new(40f, 90f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.LeftDuobreath or (uint)AID.RightDuobreath)
        {
            var loc = spell.LocXZ;
            var rot = spell.Rotation;
            var act = Module.CastFinishAt(spell);
            AddAOE();
            AddAOE(180f.Degrees(), 3d);

            void AddAOE(Angle offset = default, double delay = default)
            {
                var pos = delay != default ? loc - 5f * rot.ToDirection() : loc;
                var rot2 = rot + offset;
                _aoes.Add(new(shape, pos, rot2, delay != default ? act.AddSeconds(delay) : act, shapeDistance: shape.Distance(pos, rot2)));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count is var count && count != 0 && spell.Action.ID is (uint)AID.LeftDuobreath or (uint)AID.RightDuobreath or (uint)AID.TheRamsBreath3 or (uint)AID.TheDragonsBreath3)
        {
            _aoes.RemoveAt(0);
            if (count == 2)
            {
                ref var aoe2 = ref _aoes.Ref(0);
                var rot = aoe2.Rotation;
                aoe2.Origin -= 5f * rot.ToDirection();
                aoe2.ShapeDistance = shape.Distance(aoe2.Origin, rot);
            }
        }
    }
}

sealed class RegnantChimeraStates : StateMachineBuilder
{
    public RegnantChimeraStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Breath>()
            .ActivateOnEnter<TheRamsVoice>()
            .ActivateOnEnter<GlacipotentOrb>()
            .ActivateOnEnter<TheDragonsVoice>()
            .ActivateOnEnter<TheDragonsVoiceBoss>()
            .ActivateOnEnter<Cacophony>()
            .ActivateOnEnter<ChaoticChorus>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(RegnantChimeraStates),
    ConfigType = null, // replace null with typeof(RegnantChimeraConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID), // replace null with typeof(AID) if applicable
    StatusIDType = typeof(SID), // replace null with typeof(SID) if applicable
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.RegnantChimera,
    Contributors = "Equilius",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.ForayFATE,
    GroupID = 1093u,
    NameID = 2076u,
    SortOrder = 5,
    PlanLevel = 0)]
public sealed class RegnantChimera : OpenWorldFate
{
    public RegnantChimera(WorldState ws, Actor primary) : base(ws, primary)
    {
        ActivateComponent<Duobreath>();
        ActivateComponent<Breath>();
    }
}
