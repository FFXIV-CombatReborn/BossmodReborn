namespace BossMod.Dawntrail.Criterion.C01AMT.C012LoneSwordmaster;

sealed class SteelsbreathRelease(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.SteelsbreathReleaseCast, (uint)AID.SteelsbreathReleaseLavaCast]);
sealed class LashOfLight(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LashOfLight, new AOEShapeCone(40f, 45f.Degrees()), 2);
sealed class EchoingHush(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EchoingHush, 8f);
sealed class EchoingEight(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EchoingEightAOE, new AOEShapeCross(40f, 4f));
sealed class EchoingOrbit(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EchoingOrbitAOE, new AOEShapeDonut(8f, 60f));
sealed class MawOfTheWolf(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MawOfTheWolf, new AOEShapeRect(80f, 40f));
sealed class PlummetProximity(BossModule module) : Components.ProximityAOEs(module, (uint)AID.PlummetMeteor, 30f);
sealed class SteelsbreathBonds(BossModule module) : Components.Chains(module, (uint)TetherID.Chains, chainLength: 30f);
sealed class ResoundingSilencePuddle(BossModule module) : Components.VoidzoneAtCastTarget(module, 8f, (uint)AID.ResoundingSilenceSpread, static m => EventObjects.Active(m, (uint)OID.ResoundingSilencePuddle), 2d);
sealed class RockPuddle(BossModule module) : Components.Voidzone(module, 2.5f, static m => EventObjects.Active(m, (uint)OID.RockPuddle));

static class EventObjects
{
    public static Actor[] Active(BossModule module, uint oid)
    {
        var enemies = module.Enemies(oid);
        var count = enemies.Count;
        if (count == 0)
        {
            return [];
        }
        var result = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
            {
                result[index++] = z;
            }
        }
        return index == count ? result : result[..index];
    }
}

sealed class WaitingWounds : Components.SimpleAOEs
{
    public WaitingWounds(BossModule module) : base(module, (uint)AID.WaitingWounds, 10f, 6)
    {
        MaxDangerColor = 3;
    }
}

sealed class FromHeaven(BossModule module) : Components.UniformStackSpread(module, 5f, 5f, 2, 2)
{
    private Actor? _target;
    private int _count;
    private BitMask _forbidden;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.NearToHeavenOneCast:
            case (uint)AID.FarFromHeavenOneCast:
                _count = 1;
                Init();
                break;
            case (uint)AID.NearToHeavenTwoCast:
            case (uint)AID.FarFromHeavenTwoCast:
                _count = 2;
                Init();
                break;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        switch (iconID)
        {
            case (uint)IconID.Prey:
                _target = actor;
                Init();
                break;
            case (uint)IconID.FromHeaven1:
            case (uint)IconID.FromHeaven2:
                var slot = Raid.FindSlot(actor.InstanceID);
                if (slot >= 0)
                {
                    _forbidden.Set(slot);
                    Init();
                }
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.NearToHeavenOne or (uint)AID.NearToHeavenTwo or (uint)AID.FarFromHeavenOne or (uint)AID.FarFromHeavenTwo)
        {
            Stacks.Clear();
            Spreads.Clear();
        }
    }

    private void Init()
    {
        if (Active || _count == 0 || _target == null || _forbidden.NumSetBits() < 2)
        {
            return;
        }
        var activation = WorldState.FutureTime(5.2d);
        if (_count == 1)
        {
            AddSpread(_target, activation);
        }
        else
        {
            AddStack(_target, activation, _forbidden);
        }
    }
}

sealed class HeavensConfluence(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private static readonly AOEShapeDonut _donut = new(8f, 60f);
    private static readonly AOEShapeCircle _circle = new(8f);
    private Actor? _first;
    private Actor? _second;
    private bool _orderKnown;
    private bool _nearFirst;
    private bool _initialized;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.FarFromHeavenOneCast:
            case (uint)AID.FarFromHeavenTwoCast:
                _orderKnown = true;
                _nearFirst = false;
                Init();
                break;
            case (uint)AID.NearToHeavenOneCast:
            case (uint)AID.NearToHeavenTwoCast:
                _orderKnown = true;
                _nearFirst = true;
                Init();
                break;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        switch (iconID)
        {
            case (uint)IconID.FromHeaven1:
                _first = actor;
                Init();
                break;
            case (uint)IconID.FromHeaven2:
                _second = actor;
                Init();
                break;
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);
        if (CurrentBaits.Count != 0)
        {
            ref var b = ref CurrentBaits.Ref(0);
            if (b.Target == pc && b.Shape == _donut)
            {
                _donut.Draw(Arena, pc.Position, default, Colors.AOE);
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!EnableHints || CurrentBaits.Count == 0)
        {
            return;
        }
        ref var next = ref CurrentBaits.Ref(0);
        if (next.Target == actor)
        {
            if (PlayersClippedBy(ref next).Count != 0)
            {
                hints.Add(BaitAwayHint);
            }
        }
        else if (IsClippedBy(actor, ref next))
        {
            hints.Add("GTFO from baited aoe!");
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.HeavensConfluenceDonut or (uint)AID.HeavensConfluenceCircle)
        {
            ++NumCasts;
            if (CurrentBaits.Count != 0)
            {
                CurrentBaits.RemoveAt(0);
            }
        }
    }

    private void Init()
    {
        if (_initialized || !_orderKnown || _first == null || _second == null)
        {
            return;
        }
        var source = Module.PrimaryActor;
        CurrentBaits.Add(new(source, _first, _nearFirst ? _circle : _donut, WorldState.FutureTime(5.2d)));
        CurrentBaits.Add(new(source, _second, _nearFirst ? _donut : _circle, WorldState.FutureTime(7.3d)));
        _initialized = true;
    }
}

sealed class LavaRect(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect _rect = new(10f, 10f);
    private static readonly ArenaBoundsSquare _defaultBounds = new(20f);
    private static readonly WPos _center = new(170f, -815f);
    private static readonly ArenaBoundsCustom _lavaBounds = new(
    [
        new Rectangle(_center + new WDir(10f, -5f), 10f, 5f),
        new Rectangle(_center + new WDir(-10f, 5f), 10f, 5f),
        new Rectangle(_center + new WDir(-5f, -10f), 5f, 10f),
        new Rectangle(_center + new WDir(5f, 10f), 5f, 10f)
    ]);
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SteelsbreathReleaseLavaCast && _aoes.Count == 0)
        {
            var activation = Module.CastFinishAt(spell);
            _aoes.Add(new(_rect, _center + new WDir(10f, -10f), 180f.Degrees(), activation));
            _aoes.Add(new(_rect, _center + new WDir(10f, 10f), 90f.Degrees(), activation));
            _aoes.Add(new(_rect, _center + new WDir(-10f, 10f), default, activation));
            _aoes.Add(new(_rect, _center + new WDir(-10f, -10f), -90f.Degrees(), activation));
        }
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID != (uint)OID.LavaArena)
        {
            return;
        }
        if (state == 0x00010002u)
        {
            _aoes.Clear();
            Arena.Bounds = _lavaBounds;
            Arena.Center = _lavaBounds.Center;
        }
        else if (state == 0x00040008u)
        {
            _aoes.Clear();
            Arena.Bounds = _defaultBounds;
            Arena.Center = _center;
        }
    }
}

sealed class EchoesBait(BossModule module) : BossComponent(module)
{
    private DateTime _activation;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.EchoingEightCast:
            case (uint)AID.EchoingOrbitCast:
                _activation = Module.CastFinishAt(spell);
                break;
            case (uint)AID.EchoingHush:
                _activation = default;
                break;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_activation != default)
        {
            hints.AddForbiddenZone(new SDInvertedCircle(Arena.Center, 1f), _activation);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_activation != default)
        {
            Arena.ZoneCircleOutline(pc.Position, 8f, Colors.Danger);
        }
    }
}

sealed class EchoPredict(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut _donut = new(8f, 60f);
    private static readonly AOEShapeCross _cross = new(40f, 4f);
    private readonly List<AOEInstance> _predicted = [];
    private AOEShape? _next;
    private bool _draw;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _draw ? CollectionsMarshal.AsSpan(_predicted) : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.EchoingEightCast:
                _next = _cross;
                break;
            case (uint)AID.EchoingOrbitCast:
                _next = _donut;
                break;
            case (uint)AID.EchoingEightAOE:
            case (uint)AID.EchoingOrbitAOE:
                _predicted.Clear();
                _next = null;
                break;
            case (uint)AID.EchoingHush:
                if (_next != null)
                {
                    var activation = Module.CastFinishAt(spell, 3d);
                    var pos = spell.LocXZ;
                    _predicted.Add(new(_next, pos, default, activation));
                    if (_next == _cross)
                    {
                        _predicted.Add(new(_next, pos, 45f.Degrees(), activation));
                    }
                }
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.EchoingHush:
                _draw = true;
                break;
            case (uint)AID.EchoingEightAOE:
            case (uint)AID.EchoingOrbitAOE:
                ++NumCasts;
                break;
        }
    }
}

sealed class ResoundingSilence(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Spread, (uint)AID.ResoundingSilenceSpread, 8f, 5d)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (IsSpreadTarget(actor))
        {
            hints.ForbidDashes = true;
        }
    }
}

sealed class FangsOfTheUnderworld(BossModule module) : Components.LineStack(module, aidMarker: default, (uint)AID.FangsOfTheUnderworldStack, 5.2d, 60f, 5f, 2, 4, 3, false, (uint)IconID.Fangs)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == AidResolve && ++NumCasts >= 3)
        {
            CurrentBaits.Clear();
        }
    }
}

sealed class Plummet(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle _tiny = new(3f), _small = new(5f), _large = new(10f);
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var shape = spell.Action.ID switch
        {
            (uint)AID.PlummetTiny => _tiny,
            (uint)AID.PlummetSmall => _small,
            (uint)AID.PlummetLarge => _large,
            _ => null
        };
        if (shape != null)
        {
            _aoes.Add(new(shape, spell.LocXZ, default, Module.CastFinishAt(spell), actorID: caster.InstanceID));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.PlummetTiny or (uint)AID.PlummetSmall or (uint)AID.PlummetLarge)
        {
            ++NumCasts;
            var id = caster.InstanceID;
            for (var i = _aoes.Count - 1; i >= 0; --i)
            {
                if (_aoes[i].ActorID == id)
                {
                    _aoes.RemoveAt(i);
                    break;
                }
            }
        }
    }
}

sealed class SilentEightRock(BossModule module) : BossComponent(module)
{
    public const float RockRadius = 1.8f;
    private static readonly AOEShapeCross _cross = new(40f, 4f);
    private static readonly AOEShapeCross _crossExpanded = new(40f, 4f + RockRadius);
    private static readonly Angle _a45 = 45f.Degrees();
    private readonly List<Actor> _rocks = module.Enemies((uint)OID.Rock);
    private DateTime _activation;

    public int NumRocks => _rocks.Count;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.EchoingEightCast:
                _activation = Module.CastFinishAt(spell);
                break;
            case (uint)AID.EchoingHush:
                _activation = default;
                break;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_activation == default || pc.OID != default)
        {
            return;
        }
        _cross.Outline(Arena, pc.Position, default, Colors.Danger);
        _cross.Outline(Arena, pc.Position, _a45, Colors.Danger);

        var count = _rocks.Count;
        for (var i = 0; i < count; ++i)
        {
            Arena.ZoneCircleOutline(_rocks[i].Position, RockRadius, Colors.Object);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_activation == default || actor.OID != default)
        {
            return;
        }
        var count = _rocks.Count;
        for (var i = 0; i < count; ++i)
        {
            var pos = _rocks[i].Position;
            hints.AddForbiddenZone(_crossExpanded, pos, default, _activation);
            hints.AddForbiddenZone(_crossExpanded, pos, _a45, _activation);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_activation == default || actor.OID != default)
        {
            return;
        }
        var count = _rocks.Count;
        var pos = actor.Position;
        for (var i = 0; i < count; ++i)
        {
            var rock = _rocks[i].Position;
            if (_crossExpanded.Check(pos, rock, default) || _crossExpanded.Check(pos, rock, _a45))
            {
                hints.Add("Bait away from rocks!");
                return;
            }
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.LoneSwordmaster, Contributors = "LTS", Category = BossModuleInfo.Category.VariantCriterion, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1079u, NameID = 14323u, PlanLevel = 100)]
public sealed class C012LoneSwordmaster(WorldState ws, Actor primary) : BossModule(ws, primary, new(170f, -815f), new ArenaBoundsSquare(20f));
