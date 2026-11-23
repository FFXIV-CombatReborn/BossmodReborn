namespace BossMod.Endwalker.Savage.P8S2;

sealed class NaturalAlignment(BossModule module) : Components.GenericStackSpread(module, true)
{
    public enum Mechanic { None, StackSpread, FireIce }

    private BitMask _targets;
    private BitMask _inverse;
    private Mechanic CurMechanic;
    private Actor? CurMechanicSource;
    private bool CurMechanicInverted;
    public int CurMechanicProgress;

    private readonly List<Actor> _potentialTargets = new(PartyState.MaxPartySize);

    public override void Update()
    {
        Stacks.Clear();
        Spreads.Clear();
        if (CurMechanicProgress >= 2 || CurMechanicSource == null)
            return;

        var firstPart = CurMechanicProgress == (CurMechanicInverted ? 1 : 0);
        var potentialTargets = _potentialTargets;
        potentialTargets.Clear();
        foreach (var (s, p) in Raid.WithSlot(false, true, true))
        {
            if (!_targets[s])
                potentialTargets.Add(p);
        }
        // closest to the mechanic source first
        var sourcePos = CurMechanicSource.Position;
        for (var i = 1; i < potentialTargets.Count; ++i)
        {
            var cur = potentialTargets[i];
            var curDistSq = (cur.Position - sourcePos).LengthSq();
            var j = i - 1;
            while (j >= 0 && (potentialTargets[j].Position - sourcePos).LengthSq() > curDistSq)
            {
                potentialTargets[j + 1] = potentialTargets[j];
                --j;
            }
            potentialTargets[j + 1] = cur;
        }
        switch (CurMechanic)
        {
            case Mechanic.StackSpread:
                if (firstPart)
                {
                    // no idea how stack target is actually selected, assume it is closest...
                    var stackTarget = potentialTargets.Count != 0 ? potentialTargets[0] : null;
                    if (stackTarget != null)
                        Stacks.Add(new(stackTarget, 6, 6, 6, default, _targets));
                }
                else
                {
                    foreach (var target in potentialTargets)
                        Spreads.Add(new(target, 6));
                }
                break;
            case Mechanic.FireIce:
                if (firstPart)
                {
                    for (var i = Math.Max(0, potentialTargets.Count - 3); i < potentialTargets.Count; ++i)
                        Stacks.Add(new(potentialTargets[i], 6, 2, 2));
                }
                else
                {
                    for (var i = 0; i < potentialTargets.Count && i < 2; ++i)
                        Stacks.Add(new(potentialTargets[i], 5, 3, 3));
                }
                break;
        }
        base.Update();
    }

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (CurMechanicProgress >= 2 || CurMechanicSource == null)
            return;
        var firstPart = CurMechanicProgress == (CurMechanicInverted ? 1 : 0);
        var hint = CurMechanic switch
        {
            Mechanic.StackSpread => firstPart ? "Stack" : "Spread",
            Mechanic.FireIce => firstPart ? "Fire" : "Ice",
            _ => ""
        };
        if (hint.Length > 0)
            hints.Add($"Next NA: {hint}");
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.InverseMagicks:
                _inverse.Set(Raid.FindSlot(actor.InstanceID));
                break;
            case (uint)SID.NaturalAlignmentMechanic:
                switch (status.Extra)
                {
                    case 0x209: // initial application
                        _targets.Set(Raid.FindSlot(actor.InstanceID));
                        break;
                    case 0x1E0: // stack->spread filling progress bars
                    case 0x1E1: // stack->spread empty progress bars
                        CurMechanic = Mechanic.StackSpread;
                        CurMechanicSource = actor;
                        CurMechanicInverted = _inverse[Raid.FindSlot(actor.InstanceID)];
                        CurMechanicProgress = 0;
                        break;
                    case 0x1E2: // spread->stack filling progress bars
                    case 0x1E3: // spread->stack empty progress bars
                        CurMechanic = Mechanic.StackSpread;
                        CurMechanicSource = actor;
                        CurMechanicInverted = !_inverse[Raid.FindSlot(actor.InstanceID)];
                        CurMechanicProgress = 0;
                        break;
                    case 0x1DC: // fire->ice filling progress bars
                    case 0x1DD: // fire->ice empty progress bars
                        CurMechanic = Mechanic.FireIce;
                        CurMechanicSource = actor;
                        CurMechanicInverted = _inverse[Raid.FindSlot(actor.InstanceID)];
                        CurMechanicProgress = 0;
                        break;
                    case 0x1DE: // ice->fire filling progress bars
                    case 0x1DF: // ice->fire empty progress bars
                        CurMechanic = Mechanic.FireIce;
                        CurMechanicSource = actor;
                        CurMechanicInverted = !_inverse[Raid.FindSlot(actor.InstanceID)];
                        CurMechanicProgress = 0;
                        break;
                }
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ForcibleTrifire:
            case (uint)AID.ForcibleFireStack:
                if (CurMechanicProgress == (CurMechanicInverted ? 1 : 0))
                    ++CurMechanicProgress;
                break;
            case (uint)AID.ForcibleDifreeze:
            case (uint)AID.ForcibleFireSpread:
                if (CurMechanicProgress == (CurMechanicInverted ? 0 : 1))
                    ++CurMechanicProgress;
                break;
        }
    }
}
