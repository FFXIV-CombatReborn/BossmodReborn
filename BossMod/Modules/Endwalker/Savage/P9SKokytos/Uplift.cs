namespace BossMod.Endwalker.Savage.P9SKokytos;

class Uplift(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Uplift, new AOEShapeRect(4, 8));

class ArenaChanges(BossModule module) : BossComponent(module)
{
    public override void OnMapEffect(byte index, uint state)
    {
        if (state == 0x00080004 && index is 2 or 3)
            Module.Arena.Bounds = P9SKokytos.arena;
        if (state == 0x00020001)
        {
            if (index == 2)
                Module.Arena.Bounds = P9SKokytos.arenaUplift0;
            if (index == 3)
                Module.Arena.Bounds = P9SKokytos.arenaUplift45;
        }
    }
}
