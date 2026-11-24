namespace BossMod.Shadowbringers.Alliance.A13Engels;

sealed class MarxSmashLR(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(30f, 30f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var rotation = spell.Action.ID switch
        {
            (uint)AID.MarxSmashLeftBoss => 90f,
            (uint)AID.MarxSmashRightBoss => -90f,
            _ => default
        };
        if (rotation != default)
        {
            _aoe = [new(rect, Arena.Center, rotation.Degrees(), Module.CastFinishAt(spell, 1.7d))];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.MarxSmashLeft or (uint)AID.MarxSmashRight)
        {
            ++NumCasts;
            _aoe = [];
        }
    }
}

sealed class MarxSmashOutside(BossModule module) : Components.GenericAOEs(module, (uint)AID.MarxSmashOutsideFirst)
{
    private static readonly AOEShapeRect rectFar = new(30f, 30f, 5f);
    private static readonly AOEShapeRect rectSide = new(60f, 10f);
    private readonly List<AOEInstance> _aoes = [with(3)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            var center = Arena.Center;
            var act1 = Module.CastFinishAt(spell, 1.6d);
            var act2 = act1.AddSeconds(4.7d);
            _aoes.Clear();
            _aoes.Add(new(rectFar, center, default, act1, Colors.Danger));
            _aoes.Add(new(rectSide, center + new WDir(15f, -30f), default, act2));
            _aoes.Add(new(rectSide, center + new WDir(-15f, -30f), default, act2));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.MarxSmashOutsideFar:
                if (_aoes.Count == 3)
                {
                    _aoes.RemoveAt(0);
                    var aoes = CollectionsMarshal.AsSpan(_aoes);
                    aoes[0].Color = aoes[1].Color = Colors.Danger;
                }
                break;
            case (uint)AID.MarxSmashOutsideLeft:
            case (uint)AID.MarxSmashOutsideRight:
                ++NumCasts;
                _aoes.Clear();
                break;
        }
    }
}

sealed class MarxSmashInside(BossModule module) : Components.GenericAOEs(module, (uint)AID.MarxSmashInsideFirst)
{
    private static readonly AOEShapeRect rectNear = new(30f, 30f);
    private static readonly AOEShapeRect rectMiddle = new(30f, 15f, 30f);
    private readonly List<AOEInstance> _aoes = [with(2)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            var center = Arena.Center;
            var act1 = Module.CastFinishAt(spell, 1.6d);
            _aoes.Clear();
            _aoes.Add(new(rectNear, center, 180f.Degrees(), act1, Colors.Danger));
            _aoes.Add(new(rectMiddle, center, default, act1.AddSeconds(4.7d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.MarxSmashInsideNear:
                if (_aoes.Count == 2)
                {
                    _aoes.RemoveAt(0);
                    _aoes.Ref(0).Color = Colors.Danger;
                }
                break;
            case (uint)AID.MarxSmashInsideMiddle:
                ++NumCasts;
                _aoes.Clear();
                break;
        }
    }
}
