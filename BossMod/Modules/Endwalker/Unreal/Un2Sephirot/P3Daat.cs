namespace BossMod.Endwalker.Unreal.Un2Sephirot;

sealed class P3Daat(BossModule module) : Components.CastCounter(module, (uint)AID.DaatRandom)
{
    private const float radius = 5f;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Raid.WithoutSlot(false, true, true).AnyInRadiusExcluding(actor, radius))
            hints.Add("Spread!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.ZoneCircleOutline(pc.Position, radius, Colors.Danger);
    }
}
