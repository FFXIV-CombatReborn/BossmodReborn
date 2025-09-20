namespace BossMod.Dawntrail.Alliance.A12Fafnir;

sealed class HurricaneWingRaidwide(BossModule module) : Components.CastCounterMulti(module, [(uint)AID.HurricaneWingRaidwideAOE1, (uint)AID.HurricaneWingRaidwideAOE2, (uint)AID.HurricaneWingRaidwideAOE3,
    (uint)AID.HurricaneWingRaidwideAOE4, (uint)AID.HurricaneWingRaidwideAOE5, (uint)AID.HurricaneWingRaidwideAOE6,
    (uint)AID.HurricaneWingRaidwideAOE7, (uint)AID.HurricaneWingRaidwideAOE8, (uint)AID.HurricaneWingRaidwideAOE9]);

sealed class HurricaneWingAOE(BossModule module) : Components.GenericAOEs(module)
{
    public override bool KeepOnPhaseChange => true;

    public readonly List<AOEInstance> AOEs = new(4);

    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(9f), new AOEShapeDonut(9f, 16f), new AOEShapeDonut(16f, 23f), new AOEShapeDonut(23f, 30f)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs.Count != 0 ? CollectionsMarshal.AsSpan(AOEs)[..1] : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var shape = ShapeForAction(spell.Action.ID);
        if (shape != null)
        {
            NumCasts = 0;
            AOEs.Add(new(shape, spell.LocXZ, default, Module.CastFinishAt(spell), actorID: caster.InstanceID));
            if (AOEs.Count >= 4)
            {
                AOEs.Sort((a, b) => a.Activation.CompareTo(b.Activation));
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var shape = ShapeForAction(spell.Action.ID);
        if (shape != null)
        {
            ++NumCasts;
            var aoes = CollectionsMarshal.AsSpan(AOEs);
            var len = aoes.Length;
            var id = caster.InstanceID;
            for (var i = 0; i < len; ++i)
            {
                ref var aoe = ref aoes[i];
                if (aoe.ActorID == id)
                {
                    AOEs.RemoveAt(i);
                    return;
                }
            }
        }
    }

    private static AOEShape? ShapeForAction(uint aid) => aid switch
    {
        (uint)AID.HurricaneWingLongExpanding1 or (uint)AID.HurricaneWingShortExpanding1 or (uint)AID.HurricaneWingLongShrinking4 or (uint)AID.HurricaneWingShortShrinking4 => _shapes[0],
        (uint)AID.HurricaneWingLongExpanding2 or (uint)AID.HurricaneWingShortExpanding2 or (uint)AID.HurricaneWingLongShrinking3 or (uint)AID.HurricaneWingShortShrinking3 => _shapes[1],
        (uint)AID.HurricaneWingLongExpanding3 or (uint)AID.HurricaneWingShortExpanding3 or (uint)AID.HurricaneWingLongShrinking2 or (uint)AID.HurricaneWingShortShrinking2 => _shapes[2],
        (uint)AID.HurricaneWingLongExpanding4 or (uint)AID.HurricaneWingShortExpanding4 or (uint)AID.HurricaneWingLongShrinking1 or (uint)AID.HurricaneWingShortShrinking1 => _shapes[3],
        _ => null
    };
}

sealed class GreatWhirlwindLarge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GreatWhirlwindLarge, 10f)
{
    public override bool KeepOnPhaseChange => true;
}

sealed class GreatWhirlwindSmall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GreatWhirlwindSmall, 3f)
{
    public override bool KeepOnPhaseChange => true;
}

sealed class Whirlwinds(BossModule module) : Components.GenericAOEs(module)
{
    public override bool KeepOnPhaseChange => true;

    private const float Length = 5f;
    private static readonly AOEShapeCapsule capsuleSmall = new(3f, Length), capsuleBig = new(9f, Length);
    private static readonly AOEShapeCircle circleSmall = new(3f), circleBig = new(9f);
    private readonly List<Actor> _smallWhirldwinds = new(3), _bigWhirldwinds = new(3);
    public bool Active => _smallWhirldwinds.Count != 0 || _bigWhirldwinds.Count != 0;
    private static readonly Angle a180 = 180f.Degrees();
    private bool moving;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var countSmall = _smallWhirldwinds.Count;
        var countBig = _bigWhirldwinds.Count;
        var total = countSmall + countBig;
        if (total == 0)
            return [];
        Span<AOEInstance> aoes = new AOEInstance[total];
        for (var i = 0; i < countSmall; ++i)
        {
            var w = _smallWhirldwinds[i];
            aoes[i] = new(moving ? capsuleSmall : circleSmall, w.Position, w.Rotation);
        }
        for (var i = 0; i < countBig; ++i)
        {
            var w = _bigWhirldwinds[i];
            aoes[i + countSmall] = new(moving ? capsuleBig : circleBig, w.Position, w.Rotation);
        }
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GreatWhirlwindLarge)
        {
            _bigWhirldwinds.Add(caster);
            moving = false;
        }
        else if (spell.Action.ID == (uint)AID.GreatWhirlwindSmall)
            _smallWhirldwinds.Add(caster);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.BitingWind && id == 0x1E3C)
            _smallWhirldwinds.Remove(actor);
        else if (actor.OID == (uint)OID.RavagingWind && id == 0x1E39)
            _bigWhirldwinds.Remove(actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (!moving && spell.Action.ID is (uint)AID.GreatWhirlwindLargeAOE or (uint)AID.GreatWhirlwindSmallAOE)
            moving = true;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var countSmall = _smallWhirldwinds.Count;
        var countBig = _bigWhirldwinds.Count;
        var total = countSmall + countBig;
        if (countSmall == 0 && countBig == 0)
            return;
        var forbiddenImminent = new ShapeDistance[total];
        var forbiddenFuture = new ShapeDistance[total];

        const float length = Length + 6f;
        for (var i = 0; i < countBig; ++i)
        {
            var w = _bigWhirldwinds[i];
            forbiddenFuture[i] = new SDCapsule(w.Position, !moving ? w.Rotation + a180 : w.Rotation, length, 10f);
            forbiddenImminent[i] = new SDCircle(w.Position, 10f);
        }
        for (var i = 0; i < countSmall; ++i)
        {
            var w = _smallWhirldwinds[i];
            forbiddenImminent[i + countBig] = new SDCircle(w.Position, 5f);
            forbiddenFuture[i + countBig] = new SDCapsule(w.Position, !moving ? w.Rotation + a180 : w.Rotation, length, 5f);
        }

        hints.AddForbiddenZone(new SDUnion(forbiddenFuture), WorldState.FutureTime(1.5d));
        hints.AddForbiddenZone(new SDUnion(forbiddenImminent));
    }
}

sealed class HorridRoarPuddle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HorridRoarPuddle, 4f)
{
    public override bool KeepOnPhaseChange => true;
}

sealed class HorridRoarSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.HorridRoarSpread, 8f)
{
    public override bool KeepOnPhaseChange => true;
}
