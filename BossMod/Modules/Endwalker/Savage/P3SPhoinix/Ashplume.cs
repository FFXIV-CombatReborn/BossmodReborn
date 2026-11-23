namespace BossMod.Endwalker.Savage.P3SPhoinix;

// state related to ashplumes (normal or parts of gloryplume)
// normal ashplume is boss cast (with different IDs depending on stack/spread) + instant aoe some time later
// gloryplume is one instant cast with animation only soon after boss cast + instant aoe some time later
sealed class Ashplume : BossComponent
{
    public enum State { UnknownGlory, Stack, Spread, Done }

    public State CurState;

    private const float _stackRadius = 8;
    private const float _spreadRadius = 6;

    public Ashplume(BossModule module) : base(module)
    {
        CurState = (Module.PrimaryActor.CastInfo?.Action.ID ?? 0) switch
        {
            (uint)AID.ExperimentalAshplumeStack => State.Stack,
            (uint)AID.ExperimentalAshplumeSpread => State.Spread,
            (uint)AID.ExperimentalGloryplumeSingle or (uint)AID.ExperimentalGloryplumeMulti => State.UnknownGlory, // instant cast turns this into correct state ~3 sec after cast end
            _ => State.Done
        };
        if (CurState == State.Done)
            ReportError($"Failed to initialize ashplume component, unexpected cast {Module.PrimaryActor.CastInfo?.Action}");
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (CurState == State.Stack)
        {
            // note: it seems to always target 1 tank & 1 healer, so correct stacks are always tanks+dd and healers+dd
            var numStacked = 0;
            var haveTanks = actor.Role == Role.Tank;
            var haveHealers = actor.Role == Role.Healer;
            foreach (var pair in Raid.WithoutSlot(false, true, true))
            {
                if (!(pair != actor && pair.Position.InCircle(actor.Position, _stackRadius)))
                    continue;
                ++numStacked;
                haveTanks |= pair.Role == Role.Tank;
                haveHealers |= pair.Role == Role.Healer;
            }
            if (numStacked != 3)
            {
                hints.Add("Stack in fours!");
            }
            else if (haveTanks && haveHealers)
            {
                hints.Add("Incorrect stack!");
            }
        }
        else if (CurState == State.Spread)
        {
            if (Raid.WithoutSlot(false, true, true).AnyInRadiusExcluding(actor, _spreadRadius))
            {
                hints.Add("Spread!");
            }
        }
    }

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (CurState == State.Stack)
            hints.Add("Stack!");
        else if (CurState == State.Spread)
            hints.Add("Spread!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (CurState is State.UnknownGlory or State.Done)
            return;

        // draw all raid members, to simplify positioning
        var aoeRadius = CurState == State.Stack ? _stackRadius : _spreadRadius;
        foreach (var player in Raid.WithoutSlot(false, true, true))
        {
            if (player == pc)
            {
                continue;
            }
            var isinaoe = player.Position.InCircle(pc.Position, aoeRadius);
            Arena.Actor(player, isinaoe ? Colors.PlayerInteresting : Colors.PlayerGeneric, drawWorld: isinaoe ? true : null);
        }

        // draw circle around pc
        Arena.ZoneCircleOutline(pc.Position, aoeRadius, Colors.Danger);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ExperimentalGloryplumeSpread:
                CurState = State.Spread;
                break;
            case (uint)AID.ExperimentalGloryplumeStack:
                CurState = State.Stack;
                break;
            case (uint)AID.ExperimentalGloryplumeSpreadAOE:
            case (uint)AID.ExperimentalGloryplumeStackAOE:
            case (uint)AID.ExperimentalAshplumeSpreadAOE:
            case (uint)AID.ExperimentalAshplumeStackAOE:
                CurState = State.Done;
                break;
        }
    }
}
