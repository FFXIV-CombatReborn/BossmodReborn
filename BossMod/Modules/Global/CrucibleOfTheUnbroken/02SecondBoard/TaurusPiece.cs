namespace BossMod.Global.CrucibleOfTheUnbroken.SecondBoard.TaurusPiece;

public enum OID : uint
{
    TaurusPiece = 0x4C54, // R2.240, x1
    _Gen_ = 0x4C8D, // R1.000, x10
    AethericCharge = 0x4C55, // R1.000-3.010, x0 (spawn during fight)
    TaurusPiece1 = 0x4C56, // R1.500, x0 (spawn during fight)
    AhrimanPiece = 0x4C57, // R0.900, x0 (spawn during fight)
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 50784, // TaurusPiece->player, no cast, single-target

    MortalRay = 48143, // TaurusPiece->self, 5.0s cast, range 60 circle

    RuinousExpansion1 = 48158, // TaurusPiece->self, 4.5+1.5s cast, single-target
    RuinousExpansion2 = 48160, // TaurusPiece->self, no cast, single-target
    RuinousContraction1 = 48162, // TaurusPiece->self, 4.5+1.5s cast, single-target
    RuinousContraction2 = 48164, // TaurusPiece->self, no cast, single-target
    RuinousRingVisual = 48156, // TaurusPiece->self, 4.5+1.5s cast, single-target
    RuinousRing1 = 48157, // Helper->self, 6.0s cast, range 8-50 donut
    RuinousRing2 = 48161, // Helper->self, 9.5s cast, range 8-50 donut
    RuinousRing3 = 48163, // Helper->self, 6.0s cast, range 8-50 donut
    RuinousLocusVisual = 48154, // TaurusPiece->self, 4.5+1.5s cast, single-target
    RuinousLocus1 = 48155, // Helper->self, 6.0s cast, range 8 circle
    RuinousLocus2 = 48159, // Helper->self, 6.0s cast, range 8 circle
    RuinousLocus3 = 48165, // Helper->self, 9.5s cast, range 8 circle

    RayOfIgnoranceCast = 48144, // TaurusPiece->self, 5.0+1.0s cast, single-target
    RayOfIgnorance = 48145, // Helper->player, no cast, single-target
    Burst1 = 48146, // 4C55->self, 1.5s cast, range 6 circle
    Burst2 = 48147, // 4C55->self, 1.5s cast, range 12 circle
    Burst3 = 48148, // 4C55->self, 1.5s cast, range 18 circle
    AetherialFissure = 48149, // TaurusPiece->self, 4.0s cast, single-target
    Aetherwave = 48150, // 4C56->self, 6.0s cast, range 50 width 10 rect
    Summon = 48151, // TaurusPiece->self, 3.0s cast, single-target
    MortalGazeVisual = 48152, // 4C57->self, 4.5+0.5s cast, single-target
    MortalGaze = 48153, // Helper->self, 5.0s cast, range 50 circle
    Stone = 48625, // 4C57->player, no cast, single-target
    EyesOnMe = 48372, // 4C57->self, 10.0s cast, range 60 circle
}

public enum SID : uint
{
    Doom = 5421, // TaurusPiece->player, extra=0x358
    DamageDown = 4874, // AethericCharge->player, extra=0x1
    Bleeding = 3077, // none->player, extra=0x0
    ChargeSize = 4215, // none->4C55, extra=0x1/0x2/0x3
}

public enum IconID : uint
{
    Lockon = 234, // player->self
    Gaze = 667, // 4C57->self
}
public enum TetherID : uint
{
    _Gen_Tether_chn_sinentai01p = 102, // 4C8D->TaurusPiece
}

sealed class Doom(BossModule module) : BossComponent(module)
{
    // MapEffect 3/4/5/6 N/E/S/W
    // 0x00020001 = 1st
    // 0x00200010 = 2nd (message appears at same time)
    // 0x00800040 = 3rd
    // 0x02000100 = 4th (doom cleansable)
    // 0x10000800 = light disappears
    // 0x00080004 = clear on finish
    // 2s between steps, lasts 1s after finish
    private readonly WPos[] _tiles = [
        new(520f, -7.5f),
        new(530f, 0f),
        new(520f, 7.5f),
        new(510f, 0f)
        ];
    private readonly bool[] _lightup = [false, false, false, false];
    private readonly float _halfsize = 2f;
    private bool _active = false;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Doom)
        {
            _active = true;
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Doom)
        {
            _active = false;
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is >= 3 and <= 6)
        {
            var ind = index - 3;
            _lightup[ind] = state != 0x10000800;
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_active)
        {
            var count = _tiles.Length;
            for (var i = 0; i < count; ++i)
            {
                var pos = _tiles[i];
                Arena.ZoneRect(pos, 0f.Degrees(), _halfsize, _halfsize, _halfsize, Colors.Safe);
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_active)
        {
            hints.Add("Cleanse Doom!", false);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_active)
        {
            var count = _tiles.Length;
            for (var i = 0; i < count; ++i)
            {
                // try to stay on tile if it's already charging up
                // TODO: stay on tile if doom is almost expiring but there is an imminent AOE; damage better than death
                // would forbidden shape with 4 holes and status expiration as activation make the AI better?
                var pos = _tiles[i];
                var weight = _lightup[i] ? 50f : 10f;
                hints.GoalZones.Add(AIHints.GoalSingleTarget(pos, _halfsize, weight));
            }
        }
    }
}
sealed class MortalRay(BossModule module) : Components.RaidwideCast(module, (uint)AID.MortalRay, "Raidwide + Doom");
sealed class EyesOnMe(BossModule module) : Components.CastHint(module, (uint)AID.EyesOnMe, "Kill Ahriman before it self-destructs!", true);
sealed class RuinousRing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RuinousRing1, new AOEShapeDonut(8f, 40f));
sealed class RuinousLocus(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RuinousLocus1, 8f);
sealed class RayOfIgnorance(BossModule module) : BossComponent(module)
{
    private readonly WPos[] _corners = [
        new(502f, -14f),
        new(538f, -14f),
        new(502f, 14f),
        new(538f, 14f)
        ];
    private bool _active = false;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Lockon)
        {
            _active = true;
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.AethericCharge)
        {
            _active = false;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_active)
        {
            // drop it in a corner
            var count = _corners.Length;
            for (var i = 0; i < count; ++i)
            {
                var position = _corners[i];
                //Arena.ZoneCircleOutline(position, 1f);
                hints.GoalZones.Add(AIHints.GoalSingleTarget(position, 1f));
            }
        }
    }
}
sealed class MortalGaze(BossModule module) : Components.CastGaze(module, (uint)AID.MortalGaze, maxCasts: 2);
sealed class Adds(BossModule module) : Components.Adds(module, (uint)OID.AhrimanPiece, 1);
sealed class RuinousExpansion(BossModule module) : Components.GenericAOEs(module, (uint)AID.RuinousExpansion1)
{
    private readonly List<AOEInstance> _aoes = [];
    private readonly AOEShapeCircle _circle = new(8f);
    private readonly AOEShapeDonut _donut = new(8f, 40f);
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count == 0)
        {
            return [];
        }

        return CollectionsMarshal.AsSpan(_aoes)[..1];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            var origin = spell.LocXZ;
            var rotation = spell.Rotation;
            var activation = Module.CastFinishAt(spell);
            _aoes.Add(new(_circle, origin, rotation, activation, default, true, caster.InstanceID));
            _aoes.Add(new(_donut, origin, rotation, activation.AddSeconds(3.5d), default, true, caster.InstanceID));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.RuinousLocus2 or (uint)AID.RuinousRing2)
        {
            ++NumCasts;
            _aoes.RemoveAt(0);
        }
    }
}
sealed class RuinousContraction(BossModule module) : Components.GenericAOEs(module, (uint)AID.RuinousContraction1)
{
    private readonly List<AOEInstance> _aoes = [];
    private readonly AOEShapeCircle _circle = new(8f);
    private readonly AOEShapeDonut _donut = new(8f, 40f);
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count == 0)
        {
            return [];
        }

        return CollectionsMarshal.AsSpan(_aoes)[..1];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            var origin = spell.LocXZ;
            var rotation = spell.Rotation;
            var activation = Module.CastFinishAt(spell);
            _aoes.Add(new(_donut, origin, rotation, activation, default, true, caster.InstanceID));
            _aoes.Add(new(_circle, origin, rotation, activation.AddSeconds(3.5d), default, true, caster.InstanceID));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.RuinousRing3 or (uint)AID.RuinousLocus3)
        {
            ++NumCasts;
            _aoes.RemoveAt(0);
        }
    }
}
sealed class AethericCharge(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEInstance[] _aoes = new AOEInstance[1];
    private bool _active = false;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _active ? _aoes : [];

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.AethericCharge)
        {
            var position = actor.Position;
            _aoes[0] = new(new AOEShapeCircle(6f), position);
            _active = true;
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.AethericCharge)
        {
            _active = false;
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.ChargeSize)
        {
            var extra = status.Extra;
            if (extra < 0x3)
            {
                var position = actor.Position;
                var size = status.Extra switch
                {
                    0x1 => 12f,
                    0x2 => 18f,
                    0x3 => 18f,
                    _ => 6f
                };

                _aoes[0] = new(new AOEShapeCircle(size), position);
            }
            else
            {
                _active = false;
            }
        }
    }
}

sealed class Aetherwave(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Aetherwave, new AOEShapeRect(50f, 5f))
{
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        // 2 or 3 at a time; group by activation
        var count = Casters.Count;
        if (count == 0)
        {
            return [];
        }

        var aoes = CollectionsMarshal.AsSpan(Casters);
        ref var aoe1 = ref aoes[0];
        var startTime = aoe1.Activation;
        var end = 0;

        for (var i = 0; i < count; ++i)
        {
            ref var aoe = ref aoes[i];
            var activation = aoe.Activation;
            if (Math.Abs((activation - startTime).TotalSeconds) > 1d)
            {
                break;
            }
            end++;
        }

        return aoes[..end];
    }
}

sealed class TaurusPieceStates : StateMachineBuilder
{
    public TaurusPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<EyesOnMe>()
            .ActivateOnEnter<Doom>()
            .ActivateOnEnter<MortalRay>()
            .ActivateOnEnter<RuinousRing>()
            .ActivateOnEnter<RuinousLocus>()
            .ActivateOnEnter<RayOfIgnorance>()
            .ActivateOnEnter<AethericCharge>()
            .ActivateOnEnter<Aetherwave>()
            .ActivateOnEnter<MortalGaze>()
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<RuinousExpansion>()
            .ActivateOnEnter<RuinousContraction>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.TaurusPiece, Contributors = "gynorhino", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1089u, NameID = 14546u, SortOrder = 2)]
public sealed class TaurusPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(520f, 0f), new ArenaBoundsRect(19.5f, 15f));
