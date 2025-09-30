namespace BossMod.Dawntrail.Alliance.A23Kamlanaut;

sealed class ArenaChanges(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private static readonly AOEShapeDonut donut = new(20f, 40f);
    private static readonly AOEShapeCircle circle = new(5);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ProvingGround)
        {
            _aoe = [new(circle, spell.LocXZ, default, Module.CastFinishAt(spell))];
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.ProvingGroundVoidzone)
        {
            _aoe = [];
            Arena.Bounds = Arena.Bounds.Radius == 29.5f ? A23Kamlanaut.P1ArenaDonut : A23Kamlanaut.P2ArenaWithBridgesDonut;
        }
    }

    public override void OnActorRenderflagsChange(Actor actor, int renderflags)
    {
        if (renderflags == 256 && actor.OID == (uint)OID.ProvingGroundVoidzone)
        {
            Arena.Bounds = Arena.Bounds.Radius == 29.5f ? A23Kamlanaut.P1Arena : A23Kamlanaut.P2ArenaWithBridges;
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        switch (index)
        {
            case 0x00: // p1/p2 transition
                switch (state)
                {
                    case 0x00020001u:
                        _aoe = [new(A23Kamlanaut.P1p2transition, Arena.Center, default, WorldState.FutureTime(5.1d))];
                        break;
                    case 0x00200010u:
                        SetArena(A23Kamlanaut.P2ArenaWithBridges);
                        break;
                }
                break;
            case 0x63: // bridges
                switch (state)
                {
                    case 0x00200010u:
                        _aoe = [new(donut, A23Kamlanaut.ArenaCenter, default, WorldState.FutureTime(4.3d))];
                        break;
                    case 0x00020001u:
                        SetArena(A23Kamlanaut.P2Arena);
                        _aoe = [];
                        break;
                    case 0x00080004u:
                        SetArena(A23Kamlanaut.P2ArenaWithBridges);
                        break;
                }
                break;
        }
        void SetArena(ArenaBoundsCustom arena)
        {
            Arena.Bounds = arena;
            Arena.Center = arena.Center;
            _aoe = [];
        }
    }
}
