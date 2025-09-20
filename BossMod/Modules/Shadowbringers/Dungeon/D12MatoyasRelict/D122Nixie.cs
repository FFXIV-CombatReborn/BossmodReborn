namespace BossMod.Shadowbringers.Dungeon.D12MatoyasRelict.D122Nixie;

public enum OID : uint
{
    Boss = 0x307F, // R2.4
    Icicle = 0x3081, // R1.0
    UnfinishedNixie = 0x3080, // R1.2
    Geyser = 0x1EB0C7,
    CloudPlatform = 0x1EA1A1, // R0.5s-2.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 22932, // Boss->player, no cast, single-target
    Teleport = 22933, // Boss->location, no cast, single-target

    CrashSmash = 22927, // Boss->self, 3.0s cast, single-target
    CrackVisual = 23481, // Icicle->self, 5.0s cast, single-target
    Crack = 22928, // Icicle->self, no cast, range 80 width 3 rect

    ShowerPower = 22929, // Boss->self, 3.0s cast, single-target
    Gurgle = 22930, // Helper->self, no cast, range 60 width 10 rect

    PitterPatter = 22920, // Boss->self, 3.0s cast, single-target
    Sploosh = 22926, // Helper->self, no cast, range 6 circle, geysirs, no dmg, just throwing player around
    FallDamage = 22934, // Helper->player, no cast, single-target

    SinginInTheRain = 22921, // UnfinishedNixie->self, 40.0s cast, single-target
    SeaShantyVisual = 22922, // Boss->self, no cast, single-target
    SeaShanty = 22924, // Helper->self, no cast, ???
    SeaShantyEnrage = 22923, // Helper->self, no cast, ???

    SplishSplash = 22925, // Boss->self, 3.0s cast, single-target
    Sputter = 22931 // Helper->player, 5.0s cast, range 6 circle, spread
}

public enum TetherID : uint
{
    Crack = 8, // Icicle->player
    Gurgle = 3 // Boss->Helper
}

class Gurgle(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(60f, 5f);
    private readonly List<AOEInstance> _aoes = new(3);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001u && index is > 0x12 and < 0x1B)
        {
            var posX = index < 0x17 ? -20f : 20f;
            var posZ = posX == -20f ? -165 + (index - 0x13) * 10f : -165f + (index - 0x17) * 10f;
            var rot = posX == -20f ? Angle.AnglesCardinals[3] : Angle.AnglesCardinals[0];
            _aoes.Add(new(rect, new WPos(posX, posZ).Quantized(), rot, WorldState.FutureTime(9d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Gurgle)
            _aoes.Clear();
    }
}

class Crack(BossModule module) : Components.GenericBaitAway(module, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    private static readonly AOEShapeRect rect = new(80f, 1.5f);

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Crack)
        {
            var target = WorldState.Actors.Find(tether.Target);
            if (target is Actor t)
                CurrentBaits.Add(new(source, t, rect, WorldState.FutureTime(5.4d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Crack)
        {
            CurrentBaits.Clear();
        }
    }
}

sealed class GeysersCloudPlatform(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle circle = new(6f);
    private readonly List<AOEInstance> _aoes = new(5);
    private bool active;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count != 0 && Arena.Bounds.Radius == 19.5f)
        {
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            if (active)
            {
                ulong id = default; // id of geyer closest to the platform
                var minDistanceSq = float.MaxValue;
                for (var i = 0; i < count; ++i)
                {
                    ref var aoe = ref aoes[i];
                    var distanceSq = (aoe.Origin - D122Nixie.CloudCenter).LengthSq();
                    if (distanceSq < minDistanceSq)
                    {
                        minDistanceSq = distanceSq;
                        id = aoe.ActorID;
                    }
                }
                if (id != default)
                {
                    for (var i = 0; i < count; ++i)
                    {
                        ref var aoe = ref aoes[i];
                        if (aoe.ActorID == id)
                        {
                            aoe.Shape.InvertForbiddenZone = true;
                            aoe.Color = Colors.SafeFromAOE;
                        }
                    }
                }
            }
            return aoes;
        }
        return [];
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x12)
        {
            if (state == 0x00020001u)
            {
                active = true;
            }
            else if (state == 0x00080004u)
            {
                active = false;
            }
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Geyser)
        {
            _aoes.Add(new(circle, actor.Position.Quantized(), default, WorldState.FutureTime(3.9d), actorID: actor.InstanceID));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Sploosh)
        {
            var count = _aoes.Count;
            var pos = caster.Position;
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            for (var i = 0; i < count; ++i)
            {
                ref var aoe = ref aoes[i];
                if (aoe.Origin.AlmostEqual(pos, 1f))
                {
                    _aoes.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);
        var r = Arena.Bounds.Radius;
        var onCloud = pc.Position.InRect(D122Nixie.CloudCenter, 9.5f, 5.5f);
        if (r == 19.5f && onCloud)
        {
            SetArena(new ArenaBoundsRect(9.5f, 5.5f), D122Nixie.CloudCenter);
        }
        else if (r == 9.5f && !onCloud)
        {
            SetArena(new ArenaBoundsSquare(19.5f), D122Nixie.ArenaCenter);
        }

        void SetArena(ArenaBounds bounds, WPos center)
        {
            Arena.Bounds = bounds;
            Arena.Center = center;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (active && Arena.Bounds.Radius == 19.5f)
        {
            var aoes = ActiveAOEs(slot, actor);
            var len = aoes.Length;
            var isRisky = true;
            var color = Colors.SafeFromAOE;
            for (var i = 0; i < len; ++i)
            {
                ref readonly var aoe = ref aoes[i];
                if (aoe.Color == color && aoe.Check(actor.Position))
                {
                    isRisky = false;
                    break;
                }
            }
            hints.Add("Go to correct geyser and wait for erruption!", isRisky);
        }
        else
        {
            base.AddHints(slot, actor, hints);
        }
    }
}

class Sputter(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Sputter, 6f);

class D122NixieStates : StateMachineBuilder
{
    public D122NixieStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Gurgle>()
            .ActivateOnEnter<Crack>()
            .ActivateOnEnter<Sputter>()
            .ActivateOnEnter<GeysersCloudPlatform>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 746u, NameID = 9738u)]
public class D122Nixie(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, new ArenaBoundsSquare(19.5f))
{
    public static readonly WPos ArenaCenter = new(default, -150f);
    public static readonly WPos CloudCenter = new(default, -175f);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.UnfinishedNixie));
    }
}
