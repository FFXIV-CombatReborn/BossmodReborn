namespace BossMod.Endwalker.Unreal.Un5Thordan;

sealed class SwordShieldOfTheHeavens(BossModule module) : BossComponent(module)
{
    public enum Buff { None, Shield, Sword }

    private readonly List<(Actor actor, Buff buff)> _adds = [];

    public bool Active
    {
        get
        {
            for (var i = 0; i < _adds.Count; ++i)
            {
                if (AddActive(_adds[i].actor))
                    return true;
            }
            return false;
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is (uint)OID.Adelphel or (uint)OID.Janlenoux)
            _adds.Add((actor, Buff.None));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        bool mitigate = false, swap = false;
        for (var i = 0; i < _adds.Count; ++i)
        {
            var (add, buff) = (_adds[i].actor, _adds[i].buff);
            if (buff == Buff.Sword && add.CastInfo is ActorCastInfo ci && ci.TargetID == actor.InstanceID && ci.IsSpell(AID.HolyBladedance))
                mitigate = true;
            else if (buff == Buff.Shield && add.TargetID != actor.InstanceID && add.InstanceID == actor.TargetID)
                swap = true;
        }
        if (mitigate)
            hints.Add("Mitigate NOW!");
        if (swap)
            hints.Add("Swap target!");
    }

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        var inactive = 0;
        for (var i = 0; i < _adds.Count; ++i)
        {
            if (!AddActive(_adds[i].actor))
                ++inactive;
        }
        if (inactive == 2 && _adds[0].actor.Position.InCircle(_adds[1].actor.Position, 10f)) // TODO: verify range
            hints.Add("Separate adds!");

        var focus = _adds.Find(a => a.buff == Buff.Sword);
        if (focus.actor != null)
            hints.Add($"Focus on {focus.actor.Name}!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var a in _adds)
            Arena.Actor(a.actor, Colors.Enemy);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        var buff = ClassifyStatus(status.ID);
        if (buff != Buff.None)
        {
            var index = _adds.FindIndex(a => a.actor == actor);
            if (index >= 0)
                _adds[index] = (actor, buff);
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        var buff = ClassifyStatus(status.ID);
        if (buff != Buff.None)
        {
            var index = _adds.FindIndex(a => a.actor == actor);
            if (index >= 0 && _adds[index].buff == buff)
                _adds[index] = (actor, Buff.None);
        }
    }

    private static Buff ClassifyStatus(uint sid) => sid switch
    {
        (uint)SID.ShieldOfTheHeavens => Buff.Shield,
        (uint)SID.SwordOfTheHeavens => Buff.Sword,
        _ => Buff.None
    };

    private bool AddActive(Actor add) => !add.IsDestroyed && add.IsTargetable;
}

sealed class HoliestOfHoly(BossModule module) : Components.RaidwideCast(module, (uint)AID.HoliestOfHoly);

sealed class SkywardLeap(BossModule module) : Components.GenericBaitAway(module, (uint)AID.SkywardLeap, centerAtTarget: true)
{
    private static readonly AOEShapeCircle _shape = new(20); // not sure about the spread radius, 15 seems to be enough but damage goes up to 20

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.SkywardLeap)
            CurrentBaits.Add(new(Module.PrimaryActor, actor, _shape));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            if (CurrentBaits.Count > 0)
                CurrentBaits.RemoveAt(0);
        }
    }
}
