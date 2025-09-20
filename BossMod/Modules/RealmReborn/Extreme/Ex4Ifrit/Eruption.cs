namespace BossMod.RealmReborn.Extreme.Ex4Ifrit;

// TODO: revise & generalize to 'baited aoe' component, with nice utilities for AI
class Eruption(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EruptionAOE, Radius)
{
    private DateTime _baitDetectDeadline;
    public BitMask Baiters;

    public const float Radius = 8f;

    public override void Update()
    {
        if (Casters.Count == 0)
            Baiters.Reset();
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        switch (spell.Action.ID)
        {
            case (uint)AID.Eruption:
                _baitDetectDeadline = WorldState.FutureTime(1d);
                break;
            case (uint)AID.EruptionAOE:
                if (WorldState.CurrentTime < _baitDetectDeadline)
                {
                    var baiter = Raid.WithoutSlot(false, true, true).Closest(spell.LocXZ);
                    if (baiter != null)
                        Baiters.Set(Raid.FindSlot(baiter.InstanceID));
                }
                break;
        }
    }
}
