using static BossMod.Dawntrail.Raid.BruteAmbombinatorSharedBounds.BruteAmbombinatorSharedBounds;

namespace BossMod.Dawntrail.Raid.M07NBruteAbombinator;

sealed class ArenaChanges(BossModule module) : BossComponent(module)
{
    public override void OnMapEffect(byte index, uint state)
    {
        if (state == 0x00020001u)
        {
            if (index == 0x00)
            {
                Arena.Bounds = RectArena;
                Arena.Center = FinalCenter;
            }
            else if (index == 0x01)
            {
                Arena.Bounds = DefaultArena;
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.NeoBombarianSpecial)
        {
            Arena.Bounds = KnockbackArena;
            Arena.Center = KnockbackArena.Center;
        }
    }
}
