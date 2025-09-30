namespace BossMod.Dawntrail.Extreme.Ex1Valigarmanda;

sealed class VolcanicDrop(BossModule module) : Components.GenericAOEs(module, (uint)AID.VolcanicDropAOE)
{
    public AOEInstance[] AOE = [];

    private static readonly AOEShapeCircle _shape = new(20f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOE;

    public override void OnMapEffect(byte index, uint state)
    {
        // state transitions:
        // 00020001 - both volcanos, appear after skyruin end
        // 00200010 - active volcano, eruption start after triscourge end
        // 00800040 - active volcano, some eruption animation
        // 02000100 - active volcano, eruption end after puddles
        if (index is 0x0E or 0x0F && state == 0x00200010u)
        {
            AOE = [new(_shape, Arena.Center + new WDir(index == 0x0E ? 13f : -13f, default), default, WorldState.FutureTime(7.8d))];
        }
    }
}

sealed class VolcanicDropPuddle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VolcanicDropPuddle, 2f);
