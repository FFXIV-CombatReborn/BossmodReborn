namespace BossMod.Endwalker.Trial.T01Zodiark;

public enum OID : uint
{
    Boss = 0x324C, // R21.000
    Helper = 0x233C, // R0.500
    EsotericRay = 0x3592, // R1.000
    EsotericSquare = 0x3593, // R1.000
    EsotericTriangle = 0x3594, // R1.000
    Behemoth = 0x3659, // R1.000
    Python = 0x365A // R1.000
}

public enum AID : uint
{
    AutoAttack = 27431, // Boss->player, no cast, single-target
    AutoAttack2 = 27764, // Boss->player, no cast, single-target
    Kokytos = 27687, // Boss->self, 4.0s cast, range 60 circle
    Kokytos2 = 27743, // Boss->self, 4.0s cast, range 60 circle
    Exoterikos = 27702, // Boss->self, 5.0s cast, single-target
    Exoterikos2 = 26561, // Boss->self, 5.0s cast, single-target
    TrimorphosExoterikos = 26562, // Boss->self, 13.0s cast, single-target
    EsotericSect = 26572, // EsotericTriangle->self, 6.0s cast, range 47 60-degree cone
    EsotericDyad = 26571, // EsotericSquare->self, 6.0s cast, range 21 width 42 rect
    TripleEsotericRay = 26564, // Boss->self, 7.0s cast, single-target
    EsotericRay = 26570, // EsotericRay->self, 6.0s cast, range 42 width 14 rect
    Ania = 26581, // Boss->self, 4.0s cast, single-target
    Ania2 = 26580, // Boss->self, 4.0s cast, single-target
    AniaAOE = 27490, // Helper->player, 5.0s cast, range 3 circle
    Paradeigma = 26568, // Boss->self, 3.0s cast, single-target
    Paradeigma2 = 26559, // Boss->self, 3.0s cast, single-target
    MeteorosEidolon = 26566, // Helper->self, 0.5s cast, range 15 circle
    OpheosEidolon = 26567, // Helper->self, no cast, range 42 width 11 rect
    Phlegethon = 26576, // Boss->self, 3.0s cast, single-target
    Phlegethon2 = 26575, // Boss->self, 3.0s cast, single-target
    PhlegethonAOE = 26574, // Helper->location, 3.5s cast, range 5 circle
    Styx = 26587, // Boss->self, 5.0s cast, single-target
    Styx2 = 26586, // Boss->self, 5.0s cast, single-target
    StyxAOE = 26588, // Helper->player, no cast, range 5 circle, five hits
    CompleteControl = 26420, // Boss->self, 5.0s cast, single-target
    AstralFlow = 28361, // Boss->self, 10.0s cast, single-target
    AstralFlowCW = 26159, // Boss->self, 10.0s cast, single-target
    AstralFlowCCW = 26160, // Boss->self, 10.0s cast, single-target
    Adikia = 26584, // Boss->self, 6.0s cast, single-target
    AdikiaL = 25512, // Helper->self, 7.0s cast, range 21 circle
    AdikiaR = 26585, // Helper->self, 7.8s cast, range 21 circle
    AlgedonNW = 26578, // Boss->self, 7.0s cast, single-target
    AlgedonNE = 26577, // Boss->self, 7.0s cast, single-target
    AlgedonAOE = 26579, // Helper->self, 8.0s cast, range 60 width 30 rect
    AstralEclipse = 26563, // Boss->self, 5.0s cast, single-target
    Explosion = 26573 // Helper->self, 3.0s cast, range 10 circle
}

public enum TetherID : uint
{
    Triangle = 164, // Boss->EsotericTriangle
    Square = 171 // Boss->EsotericSquare
}

public enum IconID : uint
{
    Ania = 218, // player
    Styx = 316 // player
}

sealed class Kokytos(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.Kokytos, (uint)AID.Kokytos2], "HP reduced to 1");
sealed class Ania(BossModule module) : Components.BaitAwayCast(module, (uint)AID.AniaAOE, new AOEShapeCircle(3f), centerAtTarget: true);
sealed class Phlegethon(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PhlegethonAOE, 5f);
sealed class Adikia(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.AdikiaL, (uint)AID.AdikiaR], 21f);
sealed class Algedon(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AlgedonAOE, new AOEShapeRect(60f, 15f));

sealed class Styx(BossModule module) : Components.UniformStackSpread(module, 5f, default, 8)
{
    private int _hitsLeft;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Styx)
        {
            Stacks.Clear();
            _hitsLeft = 5;
            AddStack(WorldState.Actors.Find(targetID) ?? actor, WorldState.FutureTime(6.1d));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID != (uint)AID.StyxAOE || Stacks.Count == 0)
            return;
        if (--_hitsLeft <= 0)
        {
            Stacks.Clear();
            return;
        }
        var next = WorldState.FutureTime(1.1d);
        var stacks = CollectionsMarshal.AsSpan(Stacks);
        for (var i = 0; i < stacks.Length; ++i)
            stacks[i].Activation = next;
    }
}

sealed class Exoterikos(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Actor Source, DateTime Activation)> _sigils = [];
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCone _triangle = new(47f, 30f.Degrees());
    private static readonly AOEShapeRect _square = new(21f, 21f);
    private static readonly AOEShapeRect _ray = new(42f, 7f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _sigils.Count;
        if (count == 0)
            return [];
        var earliest = DateTime.MaxValue;
        for (var i = 0; i < count; ++i)
        {
            if (_sigils[i].Activation < earliest)
                earliest = _sigils[i].Activation;
        }
        var deadline = earliest.AddSeconds(1d);
        _aoes.Clear();
        for (var i = 0; i < count; ++i)
        {
            var (source, activation) = _sigils[i];
            if (activation < deadline && Shape(source) is AOEShape shape)
                _aoes.Add(new(shape, source.Position, source.Rotation, activation));
        }
        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (source == Module.PrimaryActor && tether.ID is (uint)TetherID.Triangle or (uint)TetherID.Square
            && WorldState.Actors.Find(tether.Target) is Actor sigil && Shape(sigil) != null)
        {
            var delay = (source.CastInfo?.Action.ID ?? default) switch
            {
                (uint)AID.TrimorphosExoterikos => 9.7d,
                (uint)AID.Exoterikos2 => 13.3d,
                _ => 11.3d
            };
            Add(sigil, WorldState.FutureTime(delay));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.EsotericSect or (uint)AID.EsotericDyad or (uint)AID.EsotericRay)
            Add(caster, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.EsotericSect or (uint)AID.EsotericDyad or (uint)AID.EsotericRay)
            Remove(caster);
    }

    private void Remove(Actor source)
    {
        for (var i = _sigils.Count - 1; i >= 0; --i)
        {
            if (_sigils[i].Source == source)
                _sigils.RemoveAt(i);
        }
    }

    private void Add(Actor source, DateTime activation)
    {
        Remove(source);
        _sigils.Add((source, activation));
    }

    private static AOEShape? Shape(Actor actor) => actor.OID switch
    {
        (uint)OID.EsotericTriangle => _triangle,
        (uint)OID.EsotericSquare => _square,
        (uint)OID.EsotericRay => _ray,
        _ => null
    };
}

sealed class Paradeigma(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(byte Index, AOEInstance AOE)> _entries = [];
    private readonly List<AOEInstance> _aoes = [];
    private Angle _rotation;
    private bool _awaitingFlow;
    private static readonly AOEShapeCircle _behemoth = new(15f);
    private static readonly AOEShapeRect _python = new(42f, 5.5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_awaitingFlow)
            return [];
        _aoes.Clear();
        for (var i = 0; i < _entries.Count; ++i)
        {
            var aoe = _entries[i].AOE;
            aoe.Origin = RotatedPosition(aoe.Origin);
            aoe.Rotation += _rotation;
            _aoes.Add(aoe);
        }
        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.Paradeigma:
            case (uint)AID.Paradeigma2:
                _entries.Clear();
                _rotation = default;
                _awaitingFlow = spell.Action.ID == (uint)AID.Paradeigma2;
                break;
            case (uint)AID.AstralFlow:
            case (uint)AID.AstralFlowCW:
            case (uint)AID.AstralFlowCCW:
                _awaitingFlow = false;
                _rotation = (spell.Action.ID == (uint)AID.AstralFlowCCW ? 90f : -90f).Degrees();
                var entries = CollectionsMarshal.AsSpan(_entries);
                for (var i = 0; i < entries.Length; ++i)
                {
                    ref var a = ref entries[i].AOE;
                    a.Activation = Module.CastFinishAt(spell, spell.Action.ID == (uint)AID.AstralFlow ? 10.8d : a.Shape == _behemoth ? 6.3d : 5.9d);
                }
                break;
            case (uint)AID.MeteorosEidolon:
                var entries2 = CollectionsMarshal.AsSpan(_entries);
                for (var i = 0; i < entries2.Length; ++i)
                {
                    ref var a = ref entries2[i].AOE;
                    if (a.Shape == _behemoth && RotatedPosition(a.Origin).AlmostEqual(caster.Position, 1f))
                        a.Activation = Module.CastFinishAt(spell);
                }
                break;
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 2)
        {
            if (state is 0x00020001u or 0x00200010u)
                _rotation = (state == 0x00020001u ? -90f : 90f).Degrees();
            return;
        }
        if (index is < 9 or > 20)
            return;
        if (state == 0x00200010u)
        {
            RemoveIndex(index);
            var center = Arena.Center;
            if (index <= 12)
            {
                var offset = new WDir((index - 9) % 2 == 0 ? -10.5f : 10.5f, index < 11 ? -10.5f : 10.5f);
                _entries.Add((index, new(_behemoth, center + offset, activation: WorldState.FutureTime(12.6d))));
            }
            else
            {
                var first = index % 2 == 1 ? -15.5f : -5.5f;
                var direction = index switch
                {
                    <= 14 => default,
                    <= 16 => 180f.Degrees(),
                    <= 18 => 90f.Degrees(),
                    _ => -90f.Degrees()
                };
                for (var i = 0; i < 2; ++i)
                {
                    var along = first + i * 21f;
                    var offset = index switch
                    {
                        <= 14 => new WDir(along, -21f),
                        <= 16 => new WDir(along, 21f),
                        <= 18 => new WDir(-21f, along),
                        _ => new WDir(21f, along)
                    };
                    _entries.Add((index, new(_python, center + offset, direction, WorldState.FutureTime(11.1d))));
                }
            }
        }
        else if (state is 0x20001000u or 0x40000004u or 0x00080004u or 0x04000004u)
        {
            var delay = state == 0x20001000u ? index <= 12 ? 5.6d : 5.05d : index <= 12 ? 0.6d : 0.06d;
            var activation = WorldState.FutureTime(delay);
            var entries = CollectionsMarshal.AsSpan(_entries);
            for (var i = 0; i < entries.Length; ++i)
            {
                if (entries[i].Index == index)
                    entries[i].AOE.Activation = activation;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.MeteorosEidolon or (uint)AID.OpheosEidolon)
        {
            for (var i = _entries.Count - 1; i >= 0; --i)
            {
                if (RotatedPosition(_entries[i].AOE.Origin).AlmostEqual(caster.Position, 1f))
                    _entries.RemoveAt(i);
            }
        }
    }

    private void RemoveIndex(byte index)
    {
        for (var i = _entries.Count - 1; i >= 0; --i)
        {
            if (_entries[i].Index == index)
                _entries.RemoveAt(i);
        }
    }

    private WPos RotatedPosition(WPos origin)
    {
        var center = Arena.Center;
        var offset = origin - center;
        return center + (_rotation.Rad < 0f ? offset.OrthoR() : _rotation.Rad > 0f ? offset.OrthoL() : offset);
    }
}

sealed class AstralEclipse(BossModule module) : Components.GenericAOEs(module, (uint)AID.Explosion)
{
    private readonly List<AOEInstance> _all = [];
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCircle _shape = new(10f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _all.Count;
        if (count == 0)
            return [];
        var earliest = DateTime.MaxValue;
        for (var i = 0; i < count; ++i)
        {
            if (_all[i].Activation < earliest)
                earliest = _all[i].Activation;
        }
        var deadline = earliest.AddSeconds(1d);
        _aoes.Clear();
        for (var i = 0; i < count; ++i)
        {
            if (_all[i].Activation < deadline)
                _aoes.Add(_all[i]);
        }
        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if ((index, state) is not ((6, 0x10000800u) or (7, 0x00200010u) or (8, 0x00800040u)))
            return;
        var center = Arena.Center;
        var activation = WorldState.FutureTime(14.1d + index - 6);
        for (var y = -1; y <= 1; ++y)
            for (var x = -1; x <= 1; ++x)
            {
                var dangerous = state switch
                {
                    0x10000800u => y == 0 || x == -y,
                    0x00200010u => x != 0 && y != 0,
                    0x00800040u => x == 0 || y == 0,
                    _ => false
                };
                if (!dangerous)
                    continue;
                var offset = index switch
                {
                    6 => new WDir(-y, -x),
                    7 => new WDir(-x, y),
                    _ => new WDir(y, x)
                };
                _all.Add(new(_shape, center + 14f * offset, activation: activation));
            }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AstralEclipse)
        {
            _all.Clear();
        }
        else if (spell.Action.ID == (uint)AID.Explosion)
        {
            var activation = Module.CastFinishAt(spell);
            var loc = spell.LocXZ;
            for (var i = _all.Count - 1; i >= 0; --i)
            {
                var a = _all[i];
                if (a.Origin.AlmostEqual(loc, 1f) && Math.Abs((a.Activation - activation).TotalSeconds) < 2d)
                    _all.RemoveAt(i);
            }
            _all.Add(new(_shape, loc, activation: activation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Explosion)
        {
            ++NumCasts;
            var soon = WorldState.FutureTime(1d);
            for (var i = _all.Count - 1; i >= 0; --i)
            {
                var a = _all[i];
                if (a.Origin.AlmostEqual(caster.Position, 1f) && a.Activation < soon)
                    _all.RemoveAt(i);
            }
        }
    }
}

sealed class T01ZodiarkStates : StateMachineBuilder
{
    public T01ZodiarkStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Kokytos>()
            .ActivateOnEnter<Ania>()
            .ActivateOnEnter<Styx>()
            .ActivateOnEnter<Exoterikos>()
            .ActivateOnEnter<Phlegethon>()
            .ActivateOnEnter<Adikia>()
            .ActivateOnEnter<Algedon>()
            .ActivateOnEnter<Paradeigma>()
            .ActivateOnEnter<AstralEclipse>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Kagekazu (ported from upstream and improved by LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 802, NameID = 10456)]
public sealed class T01Zodiark(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsSquare(20f));
