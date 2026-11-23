namespace BossMod.Endwalker.Raid.P12NAthena;

public enum OID : uint
{
    Boss = 0x3F23, // R10.000, x2
    Helper = 0x233C, // R0.500, x28, Helper type
    Anthropos = 0x3F24, // R2.000, x8, ray of light / white flame casters
    LogouIdea = 0x3F25, // R1.000, x11
    ThymouIdea = 0x3F26, // R1.000, x11
    AnthroposAdd = 0x3F27, // R2.000, x8
    SuperchainOrigin = 0x3F28, // R0.500, x20
    SuperchainCircle = 0x3F29, // R0.600, x14
    SuperchainDonut = 0x3F2A, // R0.600, x6
    Wing = 0x3F65, // R1.000, x20, trinity wings and enchainment platforms
    ActorFF38F = 0xFF38F, // R0.500, x1, EventNpc type
    Exit = 0x1E850B, // R0.500, x2, EventObj type
    Actor1EA1A1 = 0x1EA1A1, // R2.000, x2, EventObj type
}

public enum AID : uint
{
    SuperchainTheory = 33468, // Boss->self, 5.0s cast, single-target
    SuperchainBurst = 33469, // Helper->self, 1.0s cast, range 7 circle
    SuperchainCoil = 33470, // Helper->self, 1.0s cast, range 6-70 donut
    UnnaturalEnchainment = 33471, // Boss->self, 7.0s cast, single-target
    Sample = 33472, // Helper->self, no cast, range 10 width 20 rect, the tile then falls away
    TrinityOfSoulsFirstR = 33473, // Boss->self, 10.0s cast, range 60 180-degree cone
    TrinityOfSoulsFirstL = 33474, // Boss->self, 10.0s cast, range 60 180-degree cone
    TrinityOfSoulsMidR = 33475, // Boss->self, no cast, range 60 180-degree cone
    TrinityOfSoulsMidL = 33476, // Boss->self, no cast, range 60 180-degree cone
    TrinityOfSoulsLastR = 33477, // Boss->self, no cast, range 60 180-degree cone
    TrinityOfSoulsLastL = 33478, // Boss->self, no cast, range 60 180-degree cone
    Paradeigma = 33479, // Boss->self, 3.0s cast, single-target
    RayOfLight = 33480, // Anthropos->self, 5.0s cast, range 60 width 10 rect
    WhiteFlame = 33481, // Anthropos->self, no cast, single-target
    WhiteFlameAOE = 33482, // Helper->self, no cast, range 100 width 4 rect, along the tether
    UltimaBlade = 33483, // Boss->self, no cast, single-target
    UltimaBladeAOE = 33484, // Helper->self, no cast, range 60 circle
    Palladion = 33485, // Boss->self, 8.0s cast, single-target
    PalladionJump = 33486, // Boss->location, no cast, range 6 circle, on the next marked player
    PalladionShockwave = 33487, // Helper->players, no cast, width 4 rect charge, from the last spot to the target
    PalladionEnd = 33488, // Boss->self, no cast, single-target
    PalladionCut = 33489, // Helper->self, 5.8s cast, range 20 width 40 rect, the middle band
    TheossUltima = 33490, // Boss->self, 7.0s cast, range 60 circle
    ClearCut = 33491, // AnthroposAdd->self, 2.5s cast, range 4 270-degree cone
    RayOfLightAdd = 33492, // AnthroposAdd->self, 4.0s cast, range 60 width 10 rect
    Glaukopis = 33493, // Boss->self/players, 5.0s cast, range 60 width 5 rect, tankbuster line
    Dialogos = 33494, // Boss->players, 5.0s cast, range 6 circle, party stack
    DialogosSpread = 33495, // Helper->players, 5.0s cast, range 6 circle
    Parthenos = 33496, // Boss->self, 7.0s cast, range 120 width 16 rect
    OnTheSoul = 33497, // Boss->self, 5.0s cast, range 60 circle
    Ability33556 = 33556, // LogouIdea/ThymouIdea->location, no cast, single-target
    Ability33557 = 33557, // Boss->location, no cast, single-target
    AutoAttack = 34425, // Boss->player, no cast, single-target
    AutoAttackAdd = 34721, // AnthroposAdd->player, no cast, single-target
}

public enum IconID : uint
{
    Dialogos = 62, // player
    PalladionOrSpread = 139, // player
    WingTLFirst = 421, // Wing
    WingTRFirst = 422, // Wing
    WingML = 423, // Wing
    WingMR = 424, // Wing
    WingBLLast = 433, // Wing
    WingBRLast = 434, // Wing
    Glaukopis = 471, // player
}

public enum TetherID : uint
{
    WhiteFlame = 17, // Anthropos->player
    SuperchainCircle = 228, // SuperchainCircle->SuperchainOrigin
    SuperchainDonut = 229, // SuperchainDonut->SuperchainOrigin
    UnnaturalEnchainment = 232, // Wing->Boss
}

sealed class RayOfLight(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RayOfLight, new AOEShapeRect(60, 5));
sealed class RayOfLightAdd(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RayOfLightAdd, new AOEShapeRect(60, 5));
sealed class WhiteFlame(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeRect(100, 2), (uint)TetherID.WhiteFlame, (uint)AID.WhiteFlameAOE);
sealed class ClearCut(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ClearCut, new AOEShapeCone(4, 135.Degrees()));
sealed class PalladionCut(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PalladionCut, new AOEShapeRect(20, 20));
sealed class TheossUltima(BossModule module) : Components.RaidwideCast(module, (uint)AID.TheossUltima);
sealed class Parthenos(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Parthenos, new AOEShapeRect(120, 8));
sealed class OnTheSoul(BossModule module) : Components.RaidwideCast(module, (uint)AID.OnTheSoul);
sealed class Glaukopis(BossModule module) : Components.BaitAwayCast(module, (uint)AID.Glaukopis, new AOEShapeRect(60, 2.5f));
sealed class Dialogos(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.Dialogos, 6);
sealed class DialogosSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.DialogosSpread, 6);

sealed class TrinityOfSouls(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone _shape = new(60, 90.Degrees());
    private readonly List<AOEInstance> _aoes = [];
    private Actor? _caster;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Count != 0 ? CollectionsMarshal.AsSpan(_aoes)[..1] : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.TrinityOfSoulsFirstR or (uint)AID.TrinityOfSoulsFirstL)
        {
            _caster = caster;
            _aoes.Clear();
            _aoes.Add(new(_shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        Angle? offset = iconID switch
        {
            (uint)IconID.WingML or (uint)IconID.WingBLLast => 90.Degrees(),
            (uint)IconID.WingMR or (uint)IconID.WingBRLast => -90.Degrees(),
            _ => null
        };
        if (offset != null && _caster != null && _aoes.Count > 0)
            _aoes.Add(new(_shape, _caster.Position, _caster.Rotation + offset.Value, _aoes[^1].Activation.AddSeconds(2.6f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.TrinityOfSoulsFirstR or (uint)AID.TrinityOfSoulsFirstL or (uint)AID.TrinityOfSoulsMidR or (uint)AID.TrinityOfSoulsMidL or (uint)AID.TrinityOfSoulsLastR or (uint)AID.TrinityOfSoulsLastL)
        {
            ++NumCasts;
            if (_aoes.Count > 0)
                _aoes.RemoveAt(0);
        }
    }
}

sealed class Superchain(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle _circle = new(7);
    private static readonly AOEShapeDonut _donut = new(6, 70);
    private readonly List<(Actor mover, AOEShape shape)> _pending = [];
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var deadline = _aoes[0].Activation.AddSeconds(1d);
        var n = 1;
        while (n < count && _aoes[n].Activation < deadline)
            ++n;
        return CollectionsMarshal.AsSpan(_aoes)[..n];
    }

    public override void Update()
    {
        for (var i = _pending.Count - 1; i >= 0; --i)
        {
            var (mover, shape) = _pending[i];
            if (WorldState.Actors.Find(mover.Tether.Target) is var origin && origin != null)
            {
                _aoes.Add(new(shape, origin.Position, default, WorldState.FutureTime(8.9f + 0.33f * (mover.Position - origin.Position).Length())));
                _aoes.Sort(static (a, b) => a.Activation.CompareTo(b.Activation));
                _pending.RemoveAt(i);
            }
            else if (mover.Tether.ID == 0)
            {
                _pending.RemoveAt(i);
            }
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        AOEShape? shape = tether.ID switch
        {
            (uint)TetherID.SuperchainCircle => _circle,
            (uint)TetherID.SuperchainDonut => _donut,
            _ => null
        };
        if (shape != null)
            _pending.Add((source, shape));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.SuperchainBurst or (uint)AID.SuperchainCoil)
        {
            ++NumCasts;
            var shape = spell.Action.ID == (uint)AID.SuperchainBurst ? (AOEShape)_circle : _donut;
            var index = _aoes.FindIndex(a => a.Shape == shape && a.Origin.AlmostEqual(caster.Position, 1));
            if (index >= 0)
                _aoes.RemoveAt(index);
        }
    }
}

sealed class PalladionJumps(BossModule module) : Components.GenericAOEs(module, default, "GTFO from the charge!")
{
    private static readonly AOEShapeCircle _landing = new(6);
    private readonly List<Actor> _targets = [];
    private WPos _from;
    private bool _active;

    private readonly List<AOEInstance> _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoesBuffer.Clear();
        if (!_active || _targets.Count == 0)
            return CollectionsMarshal.AsSpan(_aoesBuffer);
        var target = _targets[0];
        if (target == actor)
            return CollectionsMarshal.AsSpan(_aoesBuffer);
        var dir = target.Position - _from;
        if (dir.LengthSq() > 1)
            _aoesBuffer.Add(new(new AOEShapeRect(dir.Length(), 2), _from, Angle.FromDirection(dir)));
        _aoesBuffer.Add(new(_landing, target.Position));
        return CollectionsMarshal.AsSpan(_aoesBuffer);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        var order = _targets.IndexOf(actor);
        if (_active && order >= 0)
            hints.Add(order == 0 ? "Jump on you: move away from the party!" : $"Jump order: {order + 1}", order == 0 && Raid.WithoutSlot().AnyInRadiusExcluding(actor, _landing.Radius));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Palladion)
        {
            _active = true;
            _targets.Clear();
            _from = caster.Position;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (_active && iconID == (uint)IconID.PalladionOrSpread)
            _targets.Add(actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.PalladionJump:
                ++NumCasts;
                _from = spell.TargetXZ;
                if (_targets.Count > 0)
                    _targets.RemoveAt(0);
                break;
            case (uint)AID.PalladionEnd:
                _active = false;
                _targets.Clear();
                break;
        }
    }
}

sealed class UnnaturalEnchainment(BossModule module) : Components.GenericAOEs(module, (uint)AID.Sample)
{
    private static readonly AOEShapeRect _shape = new(5, 10, 5);
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.UnnaturalEnchainment)
            _aoes.Add(new(_shape, source.Position, default, WorldState.FutureTime(8.2f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Sample)
        {
            ++NumCasts;
            _aoes.RemoveAll(a => a.Origin.AlmostEqual(caster.Position, 1));
        }
    }
}

sealed class Tiles(BossModule module) : BossComponent(module)
{
    private static readonly ArenaBoundsSquare _square = new(20);
    private int _removed;

    public static WDir TileCenter(int i) => new((i & 1) == 0 ? -10 : 10, -15 + 10 * (i >> 1));

    public override void OnMapEffect(byte index, uint state)
    {
        if (index > 7)
            return;
        var prev = _removed;
        if (state is 0x00020001 or 0x00200001)
            _removed |= 1 << index;
        else if (state == 0x00080004)
            _removed &= ~(1 << index);
        if (prev == _removed)
            return;
        if (_removed == 0)
        {
            Arena.Bounds = _square;
            Arena.Center = new(100f, 100f);
            return;
        }
        var pieces = new List<Shape>(8);
        var center = new WPos(100f, 100f);
        for (var i = 0; i < 8; ++i)
        {
            if ((_removed & (1 << i)) == 0)
                pieces.Add(new Rectangle(center + TileCenter(i), 10f, 5f));
        }
        if (pieces.Count > 0)
        {
            // custom bounds are centered on their own bounding box, so the arena center has to follow the bounds
            var bounds = new ArenaBoundsCustom([.. pieces]);
            Arena.Bounds = bounds;
            Arena.Center = bounds.Center;
        }
    }
}

sealed class PalladionArenaEdge(BossModule module) : Components.GenericAOEs(module, default, "Stay inside the octagon!")
{
    private const float OctagonRadius = 13.5f;
    private const float ShowLead = 5;
    private static readonly AOEShapeCustom _shape = BuildShape();
    private bool _active;
    private DateTime _activation;

    private static AOEShapeCustom BuildShape()
    {
        var center = new WPos(100f, 100f);
        return new(center, [new Square(center, 20f)], [new Polygon(center, OctagonRadius, 8)]);
    }

    private readonly List<AOEInstance> _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoesBuffer.Clear();
        if (_active && WorldState.FutureTime(ShowLead) >= _activation)
            _aoesBuffer.Add(new(_shape, Module.Center, default, _activation));
        return CollectionsMarshal.AsSpan(_aoesBuffer);
    }

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (_active && _activation > WorldState.CurrentTime)
            hints.Add("Raidwide, then the floor outside the octagon bleeds");
    }

    public override void OnMapEffect(byte index, uint state)
    {
        switch (index, state)
        {
            case (10, 0x00020001):
                _active = true;
                _activation = WorldState.CurrentTime;
                break;
            case (10, 0x00080004):
            case (9, 0x00080004):
            case (8, 0x00080004):
                _active = false;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.UltimaBlade)
        {
            _active = true;
            _activation = WorldState.FutureTime(7.4f);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.PalladionCut)
            _active = false;
    }
}

sealed class P12NAthenaStates : StateMachineBuilder
{
    public P12NAthenaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RayOfLight>()
            .ActivateOnEnter<RayOfLightAdd>()
            .ActivateOnEnter<WhiteFlame>()
            .ActivateOnEnter<ClearCut>()
            .ActivateOnEnter<PalladionCut>()
            .ActivateOnEnter<TheossUltima>()
            .ActivateOnEnter<Parthenos>()
            .ActivateOnEnter<OnTheSoul>()
            .ActivateOnEnter<Glaukopis>()
            .ActivateOnEnter<Dialogos>()
            .ActivateOnEnter<DialogosSpread>()
            .ActivateOnEnter<TrinityOfSouls>()
            .ActivateOnEnter<Superchain>()
            .ActivateOnEnter<PalladionJumps>()
            .ActivateOnEnter<UnnaturalEnchainment>()
            .ActivateOnEnter<Tiles>()
            .ActivateOnEnter<PalladionArenaEdge>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Nonu (ported from upstream and modified by LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 942, NameID = 12377)]
public sealed class P12NAthena(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(20));
