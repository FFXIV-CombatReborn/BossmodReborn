namespace BossMod.Endwalker.Alliance.A11Byregot;

sealed class ArenaChanges(BossModule module) : Components.GenericAOEs(module) // arena changes excluding hammer phase
{
    private AOEInstance[] _aoe = [];
    private static readonly AOEShapeCustom square = new([new Square(A11Byregot.ArenaCenter, 25f)], [new Square(A11Byregot.ArenaCenter, 24f)]);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x00)
        {
            if (state == 0x00020001u)
            {
                Arena.Bounds = A11Byregot.DefaultBounds;
                _aoe = [];
            }
            else if (state == 0x00080004u)
            {
                Arena.Bounds = A11Byregot.StartingBounds;
            }
        }
        else if (index == 0x4F && state == 0x00080004u)
        {
            Arena.Bounds = A11Byregot.StartingBounds;
            Arena.Center = A11Byregot.ArenaCenter;
            AddAOE(WorldState.FutureTime(10.6d));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.OrdealOfThunder && Arena.Bounds == A11Byregot.StartingBounds)
        {
            AddAOE(Module.CastFinishAt(spell, 0.9d));
        }
    }

    private void AddAOE(DateTime act)
    {
        _aoe = [new(square, Arena.Center, default, act)];
    }
}
