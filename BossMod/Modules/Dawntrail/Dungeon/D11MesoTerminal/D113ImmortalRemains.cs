namespace BossMod.Dawntrail.Dungeon.D11MesoTerminal.D113ImmortalRemains;

public enum OID : uint
{
    ImmortalRemains = 0x48BE, // R18.0
    BygoneAerostat = 0x48BF, // R2.3
    PreservedTerror = 0x48C0, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 43826, // ImmortalRemains->player, no cast, single-target

    Recollection = 43825, // ImmortalRemains->self, 5.0s cast, range 60 circle
    Memento = 43809, // ImmortalRemains->self, 4.0+1,0s cast, single-target
    Electray = 43810, // BygoneAerostat->self, 5.0s cast, range 45 width 8 rect
    MemoryOfTheStormVisual = 43821, // ImmortalRemains->self, 4.0+1,0s cast, single-target
    MemoryOfTheStorm = 43822, // Helper->self, no cast, range 60 width 12 rect
    BombardmentBig = 43812, // Helper->location, 1.5s cast, range 14 circle
    BombardmentSmall = 43811, // Helper->location, 1.5s cast, range 3 circle

    TurmoilVisualR = 43814, // ImmortalRemains->self, no cast, single-target, right
    TurmoilVisualL = 43815, // ImmortalRemains->self, no cast, single-target, left
    Turmoil = 43816, // Helper->self, no cast, range 40 width 20 rect

    ImpressionVisual = 43817, // ImmortalRemains->self, no cast, single-target
    Impression = 43818, // Helper->location, 5.0s cast, range 10 circle
    ImpressionKB = 43819, // Helper->location, 5.0s cast, range 30 circle, knockback 11, away from source
    MemoryOfThePyreVisual = 43823, // ImmortalRemains->self, 4.0+1,0s cast, single-target
    MemoryOfThePyre = 43824, // Helper->player, 5.0s cast, single-target
    KeraunographyVisual = 43813, // Helper->self, 4.0s cast, single-target
    Keraunography = 45176 // Helper->self, 1.5s cast, range 60 width 20 rect
}

public enum IconID : uint
{
    MemoryOfTheStorm = 525 // ImmortalRemains->player
}

sealed class RecollectionImpression(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.Recollection, (uint)AID.ImpressionKB]);
sealed class MemoryOfThePyre(BossModule module) : Components.SingleTargetCast(module, (uint)AID.MemoryOfThePyre);
sealed class Electray(BossModule module) : Components.SimpleAOEGroupsByTimewindow(module, [(uint)AID.Electray], new AOEShapeRect(45f, 4f));
sealed class MemoryOfTheStorm(BossModule module) : Components.LineStack(module, iconID: (uint)IconID.MemoryOfTheStorm, (uint)AID.MemoryOfTheStorm, 5.2d, 60f, 3f, markerIsFinalTarget: false);

sealed class Bombardment(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = new(8);
    private readonly List<Actor> terrors = new(24);

    private static readonly AOEShapeCircle circleSmall = new(3f), circleBig = new(14f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.PreservedTerror)
        {
            if (id == 0x11D3)
            {
                terrors.Add(actor);
            }
            else
            {
                terrors.Remove(actor);
            }
        }
    }

    public override void OnActorNpcYell(Actor actor, ushort id)
    {
        if (terrors.Count > 6 && id is >= 18705 and <= 18708)
        {
            var count = terrors.Count;
            var pos = actor.Position;
            var big = false;
            for (var i = 0; i < count; ++i)
            {
                var t = terrors[i];
                if (t == actor)
                {
                    continue;
                }
                if (t.Position.AlmostEqual(pos, 5f))
                {
                    big = true;
                    break;
                }
            }
            var shape = big ? circleBig : circleSmall;
            var loc = (big ? pos + 3.5f * actor.Rotation.Round(1f).ToDirection() : pos).Quantized();
            AOEs.Add(new(shape, loc, default,
            AOEs.Count == 0 ? WorldState.FutureTime(9.8d) : AOEs.Ref(0).Activation, actorID: big ? 1ul : default, shapeDistance: shape.Distance(loc, default)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BombardmentSmall)
        {
            AOEs.Clear();
        }
    }

    public void UpdateAOEs(bool risky)
    {
        var aoes = CollectionsMarshal.AsSpan(AOEs);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            aoes[i].Risky = risky;
        }
    }
}

sealed class Impression(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Impression, 10f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // handled by the knockback component
    }
}

sealed class ImpressionKB(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Impression, 11f)
{
    private readonly Bombardment _aoe = module.FindComponent<Bombardment>()!;
    private bool active;
    private bool updated;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count == 0)
        {
            return;
        }
        ref readonly var c = ref Casters.Ref(0);
        var act = c.Activation;
        var innerCircle = new SDCircle(Arena.Center.Quantized(), 10f);
        if (!IsImmune(slot, act))
        {
            var aoes = CollectionsMarshal.AsSpan(_aoe.AOEs);
            var len = aoes.Length;
            var circles = new (WPos origin, float Radius)[len];
            for (var i = 0; i < len; ++i)
            {
                ref var aoe = ref aoes[i];
                circles[i] = (aoe.Origin, aoe.ActorID == default ? 4f : 15f);
            }
            // square intentionally slightly smaller to prevent sus knockback
            hints.AddForbiddenZone(new SDKnockbackInAABBSquareAwayFromOriginPlusAOECirclesMixedRadiiPlusAvoidShape(Arena.Center, c.Origin, 11f, 19f, circles, len, innerCircle), act);
        }
        else
        {
            hints.AddForbiddenZone(innerCircle, act);
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = _aoe.ActiveAOEs(slot, actor);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            if (aoes[i].Check(pos))
            {
                return true;
            }
        }
        return !Arena.InBounds(pos);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action.ID == WatchedAction)
        {
            active = true;
        }
    }

    public override void Update()
    {
        if (active)
        {
            if (!updated && _aoe.AOEs.Count == 8)
            {
                _aoe.UpdateAOEs(false);
                updated = true;
            }
            else if (updated && Casters.Count == 0)
            {
                _aoe.UpdateAOEs(true);
                updated = false;
                active = false;
            }
        }
    }
}

sealed class Turmoil(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private static readonly AOEShapeRect rect = new(40f, 10f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var id = spell.Action.ID;
        var offset = id switch
        {
            (uint)AID.TurmoilVisualR => -1f,
            (uint)AID.TurmoilVisualL => 1f,
            _ => default
        };
        if (offset != default)
        {
            _aoe = [new(rect, (caster.Position + offset * new WDir(10f, default)).Quantized(), default, WorldState.FutureTime(4.4d))];
        }
        else if (id == (uint)AID.Turmoil)
        {
            _aoe = [];
        }
    }
}

sealed class Keraunography(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> _aoes = new(2);
    private static readonly AOEShapeRect rect = new(60f, 10f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.KeraunographyVisual)
        {
            _aoes.Add(new(rect, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, 3d)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Keraunography)
        {
            _aoes.Clear();
        }
    }
}

sealed class D113ImmortalRemainsStates : StateMachineBuilder
{
    public D113ImmortalRemainsStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RecollectionImpression>()
            .ActivateOnEnter<Electray>()
            .ActivateOnEnter<MemoryOfTheStorm>()
            .ActivateOnEnter<Bombardment>()
            .ActivateOnEnter<ImpressionKB>()
            .ActivateOnEnter<Impression>()
            .ActivateOnEnter<MemoryOfThePyre>()
            .ActivateOnEnter<Turmoil>()
            .ActivateOnEnter<Keraunography>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", PrimaryActorOID = (uint)OID.ImmortalRemains, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1028u, NameID = 13974u, Category = BossModuleInfo.Category.Dungeon, Expansion = BossModuleInfo.Expansion.Dawntrail, SortOrder = 3)]
public sealed class D113ImmortalRemains(WorldState ws, Actor primary) : BossModule(ws, primary, default, new ArenaBoundsSquare(20f));
