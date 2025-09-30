namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB1DemonTablet;

sealed class ArenaChanges(BossModule module) : BossComponent(module)
{
    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x00)
        {
            if (state == 0x00020001u)
            {
                Arena.Bounds = FTB1DemonTablet.DefaultArena;
            }
            else if (state == 0x00080004u)
            {
                Arena.Bounds = FTB1DemonTablet.CompleteArena;
            }
        }
        else if (index == 0x01 && state == 0x00020001u)
        {
            Arena.Bounds = FTB1DemonTablet.RotationArena;
        }
    }
}
