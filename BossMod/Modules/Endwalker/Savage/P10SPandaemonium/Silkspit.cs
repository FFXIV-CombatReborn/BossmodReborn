namespace BossMod.Endwalker.Savage.P10SPandaemonium;

class Silkspit(BossModule module) : Components.UniformStackSpread(module, default, 7f)
{
    private readonly List<Actor> _pillars = module.Enemies((uint)OID.Pillar);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (IsSpreadTarget(actor) && _pillars.InRadius(actor.Position, SpreadRadius).Any())
            hints.Add("GTFO from pillars!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);
        Arena.Actors(_pillars, Colors.Object, true);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SilkspitAOE)
            Spreads.Clear();
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Silkspit)
            AddSpread(actor, WorldState.FutureTime(8));
    }
}
