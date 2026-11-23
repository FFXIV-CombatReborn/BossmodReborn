namespace BossMod.Endwalker.Savage.P10SPandaemonium;

sealed class WickedStep(BossModule module) : Components.GenericKnockback(module)
{
    private readonly Actor?[] _towers = [null, null];

    private const float _towerRadius = 4f;
    private const float _knockbackRadius = 36f;

    private readonly List<Knockback> _sourcesBuffer = [];
    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        var len = _towers.Length;
        if (len == 0)
            return [];

        var sources = _sourcesBuffer;
        sources.Clear();

        for (var i = 0; i < len; ++i)
        {
            ref readonly var s = ref _towers[i];
            if (s != null && s.Position.InCircle(actor.Position, _towerRadius))
            {
                sources.Add(new(s.Position, _knockbackRadius, Module.CastFinishAt(s.CastInfo), ignoreImmunes: true));
            }
        }

        return CollectionsMarshal.AsSpan(sources);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        var soaking = (_towers[0]?.Position.InCircle(actor.Position, _towerRadius) ?? false) || (_towers[1]?.Position.InCircle(actor.Position, _towerRadius) ?? false);
        var shouldSoak = actor.Role == Role.Tank;
        if (soaking != shouldSoak)
            hints.Add(shouldSoak ? "Soak the tower!" : "GTFO from tower!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        var len = _towers.Length;
        if (len == 0)
            return;
        for (var i = 0; i < len; ++i)
        {
            ref var t = ref _towers[i];
            if (t != null)
                Components.GenericTowers.DrawTower(Arena, t.Position, _towerRadius, pc.Role == Role.Tank);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var index = ActionToIndex(spell.Action);
        if (index >= 0)
            _towers[index] = caster;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var index = ActionToIndex(spell.Action);
        if (index >= 0)
        {
            _towers[index] = null;
            ++NumCasts;
        }
    }

    private static int ActionToIndex(ActionID aid) => aid.ID switch
    {
        (uint)AID.WickedStepAOE1 => 0,
        (uint)AID.WickedStepAOE2 => 1,
        _ => -1
    };
}
