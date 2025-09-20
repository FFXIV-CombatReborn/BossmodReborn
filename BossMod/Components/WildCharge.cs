namespace BossMod.Components;

// generic 'wild charge': various mechanics that consist of charge aoe on some target that other players have to stay in; optionally some players can be marked as 'having to be closest to source' (usually tanks)
[SkipLocalsInit]
public class GenericWildCharge(BossModule module, float halfWidth, uint aid = default, float fixedLength = default) : CastCounter(module, aid)
{
    public enum PlayerRole
    {
        Ignore, // player completely ignores the mechanic; no hints for such players are displayed
        Target, // player is charge target
        TargetNotFirst, // player is charge target, and has to hide behind other raid member
        Share, // player has to stay inside aoe
        ShareNotFirst, // player has to stay inside aoe, but not as a closest raid member
        Avoid, // player has to avoid aoe
    }

    public readonly float HalfWidth = halfWidth;
    public readonly float FixedLength = fixedLength; // if == 0, length is up to target
    public Actor? Source; // if null, mechanic is not active
    public DateTime Activation;
    public PlayerRole[] PlayerRoles = new PlayerRole[PartyState.MaxAllies];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Source == null)
            return;

        switch (PlayerRoles[slot])
        {
            case PlayerRole.Ignore:
            case PlayerRole.Target: // TODO: consider hints for target?..
                break; // nothing to advise
            case PlayerRole.TargetNotFirst:
                if (EnumerateAOEs(slot).Any(aoe => InAOE(aoe, actor)))
                    hints.Add("GTFO from other charges!");
                else if (!AnyRoleCloser(GetAOEForTarget(Source.Position, actor.Position), PlayerRole.Share, PlayerRole.Share, (actor.Position - Source.Position).LengthSq()))
                    hints.Add("Hide behind tank!");
                break;
            case PlayerRole.Share:
            case PlayerRole.ShareNotFirst:
                var badShare = false;
                var numShares = 0;
                foreach (var aoe in EnumerateAOEs().Where(aoe => InAOE(aoe, actor)))
                {
                    if (++numShares > 1)
                        break;

                    badShare = PlayerRoles[slot] == PlayerRole.Share
                        ? AnyRoleCloser(aoe, PlayerRole.ShareNotFirst, PlayerRole.TargetNotFirst, (actor.Position - Source.Position).LengthSq())
                        : !AnyRoleCloser(aoe, PlayerRole.Share, PlayerRole.Target, (actor.Position - Source.Position).LengthSq());
                }
                if (numShares == 0)
                    hints.Add("Stay inside charge!");
                else if (numShares > 1)
                    hints.Add("Stay in single charge!");
                else if (badShare)
                    hints.Add(PlayerRoles[slot] == PlayerRole.Share ? "Move closer to charge source!" : "Hide behind tank!");
                break;
            case PlayerRole.Avoid:
                if (EnumerateAOEs().Any(aoe => InAOE(aoe, actor)))
                    hints.Add("GTFO from charge!");
                break;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Source == null)
            return;
        var forbiddenInverted = new List<ShapeDistance>();
        var forbidden = new List<ShapeDistance>();
        switch (PlayerRoles[slot])
        {
            case PlayerRole.Ignore:
                break;
            case PlayerRole.Target:
            case PlayerRole.TargetNotFirst: // TODO: consider some hint to hide behind others?..
                // TODO: improve this - for now, just stack with closest player...
                if (Source != null)
                {
                    var closest = Raid.WithSlot().WhereSlot(i => PlayerRoles[i] is PlayerRole.Share or PlayerRole.ShareNotFirst).Actors().Closest(actor.Position);
                    if (closest != null)
                    {
                        var stack = GetAOEForTarget(Source.Position, closest.Position);
                        forbiddenInverted.Add(new SDInvertedRect(stack.origin, stack.dir, stack.length, 0, HalfWidth * 0.5f));
                    }
                }
                break;
            case PlayerRole.Share: // TODO: some hint to be first in line...
            case PlayerRole.ShareNotFirst:
                foreach (var aoe in EnumerateAOEs())
                    forbiddenInverted.Add(new SDInvertedRect(aoe.origin, aoe.dir, aoe.length, 0, HalfWidth));
                break;
            case PlayerRole.Avoid:
                foreach (var aoe in EnumerateAOEs())
                    forbiddenInverted.Add(new SDRect(aoe.origin, aoe.dir, aoe.length, 0, HalfWidth));
                break;
        }

        foreach (var aoe in EnumerateAOEs())
            // TODO add separate "tankbuster" hint for PlayerRole.Share if there are any ShareNotFirsts in the party
            hints.AddPredictedDamage(Raid.WithSlot().Where(p => InAOE(aoe, p.Item2)).Mask(), Activation);

        if (forbiddenInverted.Count != 0)
        {
            hints.AddForbiddenZone(new SDOutsideOfUnion([.. forbiddenInverted]), Activation);
        }
        if (forbidden.Count != 0)
        {
            hints.AddForbiddenZone(new SDUnion([.. forbidden]), Activation);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Source == null || PlayerRoles[pcSlot] == PlayerRole.Ignore)
            return;

        foreach (var aoe in EnumerateAOEs())
        {
            var dangerous = PlayerRoles[pcSlot] == PlayerRole.Avoid; // TODO: reconsider this condition
            Arena.ZoneRect(aoe.origin, aoe.dir, aoe.length, 0, HalfWidth, dangerous ? Colors.AOE : Colors.SafeFromAOE);
        }
    }

    private (WPos origin, WDir dir, float length) GetAOEForTarget(WPos sourcePos, WPos targetPos)
    {
        var toTarget = targetPos - sourcePos;
        var length = FixedLength > 0 ? FixedLength : toTarget.Length();
        var dir = toTarget.Normalized();
        return (sourcePos, dir, length);
    }

    protected bool InAOE((WPos origin, WDir dir, float length) aoe, Actor actor) => actor.Position.InRect(aoe.origin, aoe.dir, aoe.length, 0, HalfWidth);

    protected IEnumerable<(WPos origin, WDir dir, float length)> EnumerateAOEs(int targetSlotToSkip = -1)
    {
        if (Source == null)
            yield break;
        foreach (var (i, p) in Module.Raid.WithSlot().WhereSlot(i => i != targetSlotToSkip && PlayerRoles[i] is PlayerRole.Target or PlayerRole.TargetNotFirst))
            yield return GetAOEForTarget(Source.Position, p.Position);
    }

    private bool AnyRoleCloser((WPos origin, WDir dir, float length) aoe, PlayerRole role1, PlayerRole role2, float thresholdSq)
        => Raid.WithSlot().Any(ia => (PlayerRoles[ia.Item1] == role1 || PlayerRoles[ia.Item1] == role2) && InAOE(aoe, ia.Item2) && (ia.Item2.Position - aoe.origin).LengthSq() < thresholdSq);
}
