namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL1Gauntlet;

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private static readonly AOEShapeCustom square = new([new Square(DAL1Gauntlet.ArenaCenter, 30f)], [new Square(DAL1Gauntlet.ArenaCenter, 23f)]);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SuppressiveMagitekRays && Arena.Bounds.Radius > 23f)
        {
            _aoe = [new(square, Arena.Center, default, Module.CastFinishAt(spell, 1.5d))];
        }
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x31 && state == 0x00020001u)
        {
            Arena.Bounds = new ArenaBoundsSquare(23f);
            _aoe = [];
        }
    }
}
