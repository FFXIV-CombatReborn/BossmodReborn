namespace BossMod.Endwalker.Dungeon.D11LapisManalis.D111Albion;

public enum OID : uint
{
    Albion = 0x3CFE, //R=4.6
    WildBeasts = 0x3D03, //R=0.5
    WildBeasts1 = 0x3CFF, // R1.32
    WildBeasts2 = 0x3D00, // R1.7
    WildBeasts3 = 0x3D02, // R4.0
    WildBeasts4 = 0x3D01, // R2.85
    IcyCrystal = 0x3D04, // R2.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 872, // Albion->player, no cast, single-target
    Teleport = 32812, // Albion->location, no cast, single-target, Albion teleports mid

    CallOfTheMountain = 31356, // Albion->self, 3.0s cast, single-target, Albion calls wild beasts
    WildlifeCrossing = 31357, // WildBeasts->self, no cast, range 7 width 10 rect
    AlbionsEmbrace = 31365, // Albion->player, 5.0s cast, single-target

    RightSlam = 32813, // Albion->self, 5.0s cast, range 80 width 20 rect
    LeftSlam = 32814, // Albion->self, 5.0s cast, range 80 width 20 rect

    KnockOnIceVisual = 31358, // Albion->self, 4.0s cast, single-target
    KnockOnIce = 31359, // Helper->self, 6.0s cast, range 5 circle

    Icebreaker = 31361, // Albion->IcyCrystal, 5.0s cast, range 17 circle
    IcyThroesVisual = 31362, // Albion->self, no cast, single-target
    IcyThroes1 = 32783, // Helper->self, 5.0s cast, range 6 circle
    IcyThroes2 = 32697, // Helper->self, 5.0s cast, range 6 circle
    IcyThroesSpread = 31363, // Helper->player, 5.0s cast, range 6 circle
    RoarOfAlbion = 31364 // Albion->self, 7.0s cast, range 60 circle
}

sealed class WildlifeCrossing(BossModule module) : Components.GenericAOEs(module)
{
    struct Stampede(bool active, WPos pos, Angle rot, List<Actor> beasts)
    {
        public bool Active = active;
        public WPos Position = pos;
        public Angle Rotation = rot;
        public int Count;
        public DateTime Reset;
        public List<Actor> Beasts = beasts;
    }

    private static readonly AOEShapeRect rect = new(20f, 5f, 20f);
    private readonly Stampede[] _stampedes = [default, default];
    private readonly List<AOEInstance> _aoes = new(2);
    private int numActiveStampedes;
    private readonly List<Actor> animals = new(40);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is (uint)OID.WildBeasts1 or (uint)OID.WildBeasts2 or (uint)OID.WildBeasts3 or (uint)OID.WildBeasts4)
        {
            animals.Add(actor);
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID is (uint)OID.WildBeasts1 or (uint)OID.WildBeasts2 or (uint)OID.WildBeasts3 or (uint)OID.WildBeasts4)
        {
            animals.Remove(actor);
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (state != 0x00020001u)
        {
            return;
        }

        WPos pos = index switch
        {
            0x21 => new(4f, -759f),
            0x25 => new(44f, -759f),
            0x22 => new(4f, -749f),
            0x26 => new(44f, -749f),
            0x23 => new(4f, -739f),
            0x27 => new(44f, -739f),
            0x24 => new(4f, -729f),
            0x28 => new(44f, -729f),
            _ => default
        };
        if (pos == default)
        {
            return;
        }

        _stampedes[numActiveStampedes] = new Stampede(true, pos, pos.X == 4f ? Angle.AnglesCardinals[3] : Angle.AnglesCardinals[0], []);
        ++numActiveStampedes;
    }

    public override void Update()
    {
        if (numActiveStampedes == 0)
        {
            return;
        }
        _aoes.Clear();
        for (var i = 0; i < 2; ++i)
        {
            ref var s = ref _stampedes[i];
            if (!s.Active)
            {
                continue;
            }
            var reset = s.Reset;
            if (reset != default && WorldState.CurrentTime > reset)
            {
                _stampedes[i] = default;
                --numActiveStampedes;
                if (numActiveStampedes == 0)
                {
                    _aoes.Clear();
                }
                return;
            }

            var count = animals.Count;

            HashSet<Actor> updatedBeasts = [.. s.Beasts];
            for (var j = 0; j < count; ++j)
            {
                var b = animals[j];
                if (b.Position.InRect(s.Position, s.Rotation, default, 10f, 5f))
                {
                    updatedBeasts.Add(b);
                }
            }
            var currentbeasts = s.Beasts = [.. updatedBeasts];
            _aoes.Add(currentbeasts.Count == 0 ? new(rect, s.Position, Angle.AnglesCardinals[3])
            : new(new AOEShapeRect((currentbeasts[0].Position - currentbeasts[^1].Position).Length() + 30f, 5f), new(currentbeasts[^1].PosRot.X, s.Position.Z), s.Rotation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.WildlifeCrossing)
        {
            var len = _stampedes.Length;
            var posZ = caster.PosRot.Z;
            for (var i = 0; i < len; ++i)
            {
                ref var s = ref _stampedes[i];
                if (Math.Abs(posZ - s.Position.Z) < 1f)
                {
                    if (++s.Count == 30)
                    {
                        s.Reset = WorldState.FutureTime(0.5d);
                    }
                }
            }
        }
    }
}

sealed class Icebreaker(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Icebreaker, 17f);

sealed class IcyThroes(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.IcyThroes1, (uint)AID.IcyThroes2], 6f);
sealed class IcyThroesSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.IcyThroesSpread, 6f);
sealed class KnockOnIce(BossModule module) : Components.SimpleAOEs(module, (uint)AID.KnockOnIce, 5f);

sealed class Slam(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.RightSlam, (uint)AID.LeftSlam], new AOEShapeRect(80f, 10f));
sealed class AlbionsEmbrace(BossModule module) : Components.SingleTargetCast(module, (uint)AID.AlbionsEmbrace);

sealed class RoarOfAlbion(BossModule module) : Components.CastLineOfSightAOE(module, (uint)AID.RoarOfAlbion, 60f)
{
    public override ReadOnlySpan<Actor> BlockerActors() => CollectionsMarshal.AsSpan(Module.Enemies((uint)OID.IcyCrystal));
}

sealed class D111AlbionStates : StateMachineBuilder
{
    public D111AlbionStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WildlifeCrossing>()
            .ActivateOnEnter<Slam>()
            .ActivateOnEnter<AlbionsEmbrace>()
            .ActivateOnEnter<Icebreaker>()
            .ActivateOnEnter<KnockOnIce>()
            .ActivateOnEnter<IcyThroes>()
            .ActivateOnEnter<IcyThroesSpread>()
            .ActivateOnEnter<RoarOfAlbion>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.Albion, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 896u, NameID = 11992u, Category = BossModuleInfo.Category.Dungeon, Expansion = BossModuleInfo.Expansion.Endwalker, SortOrder = 2)]
public sealed class D111Albion(WorldState ws, Actor primary) : BossModule(ws, primary, new(24f, -744f), new ArenaBoundsSquare(19.5f));
