namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRN1TrinitySeeker;

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(25f, 30f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.VerdantTempest && Arena.Bounds.Radius > 26f)
        {
            _aoe = [new(donut, Arena.Center, default, Module.CastFinishAt(spell, 3.8d))];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x1B && state == 0x00020001u)
        {
            Arena.Bounds = TrinitySeeker.DefaultArena;
            Arena.Center = TrinitySeeker.DefaultArena.Center;
            _aoe = [];
        }
    }
}
