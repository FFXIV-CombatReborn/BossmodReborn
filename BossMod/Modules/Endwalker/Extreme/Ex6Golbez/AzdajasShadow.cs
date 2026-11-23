namespace BossMod.Endwalker.Extreme.Ex6Golbez;

sealed class AzdajasShadow(BossModule module) : BossComponent(module)
{
    public enum Mechanic { Unknown, CircleStack, DonutSpread }

    public Mechanic CurMechanic;

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (CurMechanic != Mechanic.Unknown)
            hints.Add($"Next mechanic: {(CurMechanic == Mechanic.CircleStack ? "out -> stack" : "in -> spread")}");
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var mechanic = spell.Action.ID switch
        {
            (uint)AID.AzdajasShadowCircleStack => Mechanic.CircleStack,
            (uint)AID.AzdajasShadowDonutSpread => Mechanic.DonutSpread,
            _ => Mechanic.Unknown
        };
        if (mechanic != Mechanic.Unknown)
            CurMechanic = mechanic;
    }
}

sealed class FlamesOfEventide(BossModule module) : Components.GenericBaitAway(module, (uint)AID.FlamesOfEventide)
{
    private readonly int[] _playerStacks = new int[PartyState.MaxPartySize];

    private readonly AOEShapeRect _shape = new(50f, 3f);

    public override void Update()
    {
        CurrentBaits.Clear();
        var target = WorldState.Actors.Find(Module.PrimaryActor.TargetID);
        if (target != null)
            CurrentBaits.Add(new(Module.PrimaryActor, target, _shape));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Module.PrimaryActor.TargetID == actor.InstanceID)
        {
            if (_playerStacks[slot] >= 2)
                hints.Add("Pass aggro!");
            var boss = Module.PrimaryActor.Position;
            var dir = Angle.FromDirection(actor.Position - boss);
            var party = Raid.WithoutSlot(false, true, true);
            for (var i = 0; i < party.Length; ++i)
            {
                var p = party[i];
                if (p != actor && _shape.Check(p.Position, boss, dir))
                {
                    hints.Add("GTFO from raid!");
                    break;
                }
            }
        }
        else if (CurrentBaits.Count > 0)
        {
            if (IsClippedBy(actor, ref CurrentBaits.Ref(0)))
                hints.Add("GTFO from tank!");
            if (_playerStacks[slot] < 2 && actor.Role == Role.Tank && Raid.FindSlot(Module.PrimaryActor.TargetID) is var tankSlot && tankSlot >= 0 && _playerStacks[tankSlot] >= 2)
                hints.Add("Taunt!");
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.FlamesOfEventide && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            _playerStacks[slot] = status.Extra;
            if (status.Extra >= 2)
                ForbiddenPlayers.Set(slot);
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.FlamesOfEventide && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            _playerStacks[slot] = 0;
            ForbiddenPlayers.Clear(slot);
        }
    }
}
