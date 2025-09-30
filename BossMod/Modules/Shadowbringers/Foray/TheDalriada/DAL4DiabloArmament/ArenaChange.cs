namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL4DiabloArmament;

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private static readonly AOEShapeDonut donut = new(17f, 30f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DiabolicGateVisual)
        {
            _aoe = [new(donut, Arena.Center.Quantized(), default, Module.CastFinishAt(spell, 9.2d))];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x33)
        {
            if (state == 0x00020001u)
            {
                Arena.Bounds = DAL4DiabloArmament.SmallArena;
                Arena.Center = Arena.Center.Quantized();
                _aoe = [];
            }
            else if (state == 0x00080004u)
            {
                Arena.Bounds = DAL4DiabloArmament.DefaultArena;
                Arena.Center = DAL4DiabloArmament.ArenaCenter;
            }
        }
    }
}