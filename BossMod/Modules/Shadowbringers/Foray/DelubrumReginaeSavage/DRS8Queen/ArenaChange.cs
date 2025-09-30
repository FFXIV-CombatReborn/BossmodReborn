namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS8Queen;

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    public override bool KeepOnPhaseChange => true;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.EmpyreanIniquityAOE && Arena.Bounds.Radius > 26f)
        {
            _aoe = [new(Queen.ArenaChange, Arena.Center, default, Module.CastFinishAt(spell, 4.8d))];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x19)
        {
            if (state is 0x00020001u or 0x00400001u)
            {
                Arena.Bounds = Queen.DefaultArena;
                _aoe = [];
            }
            else if (state == 0x00200010u)
            {
                Arena.Bounds = new ArenaBoundsSquare(25f);
                _aoe = [];
            }
        }
    }
}
