namespace BossMod.Endwalker.Savage.P11SThemis;

// mirrors & spheres
sealed class ArcaneRevelation(BossModule module) : Components.GenericAOEs(module)
{
    private uint _activeMirrors;
    private uint _activeSpheres;
    private readonly List<AOEInstance> _aoes = [];

    private readonly AOEShapeRect _shapeMirror = new(50f, 5f);
    private readonly AOEShapeCircle _shapeSphere = new(15f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var (mirrors, spheres) = spell.Action.ID switch
        {
            (uint)AID.ArcaneRevelationMirrorsLight => ((uint)OID.MirrorLight, 0u),
            (uint)AID.ArcaneRevelationMirrorsDark => ((uint)OID.MirrorDark, 0u),
            (uint)AID.ArcaneRevelationSpheresLight => (0u, (uint)OID.SphereLight),
            (uint)AID.ArcaneRevelationSpheresDark => (0u, (uint)OID.SphereDark),
            (uint)AID.TwofoldRevelationLight => ((uint)OID.MirrorLight, (uint)OID.SphereLight),
            (uint)AID.TwofoldRevelationDark => ((uint)OID.MirrorDark, (uint)OID.SphereDark),
            _ => (0u, 0u)
        };
        if (mirrors != 0 || spheres != 0)
        {
            _activeMirrors = mirrors;
            _activeSpheres = spheres;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.ArcheLight or (uint)AID.ArcheDark or (uint)AID.UnluckyLotLight or (uint)AID.UnluckyLotDark)
        {
            ++NumCasts;
            _aoes.Clear();
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id != 0x1E46)
            return;
        if (actor.OID == _activeMirrors)
            AddAOE(_shapeMirror);
        else if (actor.OID == _activeSpheres)
            AddAOE(_shapeSphere);
        void AddAOE(AOEShape shape) => _aoes.Add(new(shape, actor.Position.Quantized(), actor.Rotation, WorldState.FutureTime(8.7d)));
    }
}

sealed class DismissalOverruling(BossModule module) : Components.GenericKnockback(module)
{
    private Actor? _source;

    private readonly Knockback[] _oneKnockback = new Knockback[1];
    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (_source != null)
        {
            _oneKnockback[0] = new(_source.Position, 11f, Module.CastFinishAt(_source.CastInfo));
            return _oneKnockback;
        }
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.DismissalOverrulingLightAOE or (uint)AID.DismissalOverrulingDarkAOE)
            _source = caster;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.DismissalOverrulingLightAOE or (uint)AID.DismissalOverrulingDarkAOE)
        {
            _source = null;
            ++NumCasts;
        }
    }
}

sealed class InnerLight(BossModule module) : Components.SimpleAOEs(module, (uint)AID.InnerLight, 13f);
sealed class OuterDark(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OuterDark, new AOEShapeDonut(8f, 50f));
