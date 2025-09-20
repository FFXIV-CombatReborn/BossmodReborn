namespace BossMod.Components;

// component for breakable chains - Note that chainLength for AI considers the minimum distance needed for a chain-pair to be broken (assuming perfectly stacked at cast)

[SkipLocalsInit]
public class Chains(BossModule module, uint tetherID, uint aid = default, float chainLength = default, bool spreadChains = true) : CastCounter(module, aid)
{
    public readonly uint TID = tetherID;
    public bool TethersAssigned;
    private readonly Actor?[] _partner = new Actor?[PartyState.MaxAllies];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_partner[slot] != null)
            hints.Add(spreadChains ? "Break the chains!" : "Stay with partner!");
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return _partner[pcSlot] == player ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == TID)
        {
            TethersAssigned = true;
            var target = WorldState.Actors.Find(tether.Target);
            if (target != null)
            {
                SetPartner(source.InstanceID, target);
                SetPartner(target.InstanceID, source);
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_partner[pcSlot] is var partner && partner != null)
            Arena.AddLine(pc.Position, partner.Position, spreadChains ? default : Colors.Safe);
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == TID)
        {
            SetPartner(source.InstanceID, null);
            SetPartner(tether.Target, null);
        }
    }

    private void SetPartner(ulong source, Actor? target)
    {
        var slot = Raid.FindSlot(source);
        if (slot >= 0)
            _partner[slot] = target;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_partner[slot] is var partner && partner != null)
            hints.AddForbiddenZone(spreadChains ? new SDCircle(partner.Position, (partner.Position - actor.Position).Length() + 1f) : new SDInvertedCircle(partner.Position, chainLength), WorldState.FutureTime(10d));
    }
}
