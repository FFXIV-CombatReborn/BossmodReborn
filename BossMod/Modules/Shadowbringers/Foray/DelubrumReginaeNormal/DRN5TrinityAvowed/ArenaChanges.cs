namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN5TrinityAvowed;

sealed class ArenaChanges(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GloryOfBozja && Arena.Bounds.Radius > 25f)
        {
            _aoe = [new(TrinityAvowed.ArenaChange1, Arena.Center, default, Module.CastFinishAt(spell, 0.7d))];
        }
        else if (spell.Action.ID == (uint)AID.FlamesOfBozjaAOE)
        {
            _aoe = [new(TrinityAvowed.ArenaChange2, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell))];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (state == 0x00020001u)
        {
            if (index == 0x11)
            {
                Arena.Bounds = TrinityAvowed.DefaultArena;
                _aoe = [];
            }
            else if (index == 0x12)
            {
                Arena.Bounds = TrinityAvowed.ShimmeringShotArena;
                Arena.Center = TrinityAvowed.EastRemovedCenter;
                _aoe = [];
            }
            else if (index == 0x13)
            {
                Arena.Bounds = TrinityAvowed.ShimmeringShotArena;
                Arena.Center = TrinityAvowed.WestRemovedCenter;
                _aoe = [];
            }
        }
        else if (state == 0x00080004u && index is 0x12 or 0x13)
        {
            Arena.Bounds = TrinityAvowed.DefaultArena;
            Arena.Center = TrinityAvowed.ArenaCenter;
        }
    }
}
