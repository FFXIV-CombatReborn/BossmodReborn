namespace BossMod.Endwalker.Savage.P4S2Hesperos;

// state related to nearsight & farsight mechanics
sealed class NearFarSight : BossComponent
{
    public enum State { Near, Far, Done }

    public State CurState;
    private BitMask _targets;
    private BitMask _inAOE;

    private const float _aoeRadius = 5;

    public NearFarSight(BossModule module) : base(module)
    {
        CurState = (Module.PrimaryActor.CastInfo?.Action.ID ?? 0) switch
        {
            (uint)AID.Nearsight => State.Near,
            (uint)AID.Farsight => State.Far,
            _ => State.Done
        };
        if (CurState == State.Done)
            ReportError($"Failed to initialize near/far sight, unexpected cast {Module.PrimaryActor.CastInfo?.Action}");
    }

    private readonly List<(int, Actor)> _sorted = new(PartyState.MaxPartySize);

    public override void Update()
    {
        _targets = _inAOE = default;
        if (CurState == State.Done)
            return;

        Raid.WithSlot(false, true, true).SortedByRange(Module.PrimaryActor.Position, _sorted);
        var first = CurState == State.Near ? 0 : Math.Max(0, _sorted.Count - 2);
        for (var k = first; k < _sorted.Count && k < first + 2; ++k)
        {
            var (i, player) = _sorted[k];
            _targets.Set(i);
            _inAOE |= Raid.WithSlot(false, true, true).MaskInRadiusExcluding(player, _aoeRadius);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_targets.None())
            return;

        var isTarget = _targets[slot];
        var shouldBeTarget = actor.Role == Role.Tank;
        var isFailing = isTarget != shouldBeTarget;
        var shouldBeNear = CurState == State.Near ? shouldBeTarget : !shouldBeTarget;
        hints.Add(shouldBeNear ? "Stay near boss" : "Stay on max melee", isFailing);
        if (_inAOE[slot])
        {
            hints.Add("GTFO from tanks!");
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_targets.None())
            return;

        foreach ((var i, var player) in Raid.WithSlot(false, true, true))
        {
            if (_targets[i])
            {
                Arena.Actor(player, Colors.Danger, drawWorld: true);
                Arena.ZoneCircleOutline(player.Position, _aoeRadius, Colors.Danger);
            }
            else
            {
                var isinAOE = _inAOE[i];
                Arena.Actor(player, isinAOE ? Colors.PlayerInteresting : Colors.PlayerGeneric, drawWorld: isinAOE ? true : null);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.NearsightAOE or (uint)AID.FarsightAOE)
            CurState = State.Done;
    }
}
