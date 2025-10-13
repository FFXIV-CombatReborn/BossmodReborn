namespace BossMod.Dawntrail.Alliance.A12Fafnir;

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeDonut donut = new(30f, 35f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DarkMatterBlast && Arena.Bounds != A12Fafnir.DefaultBounds)
        {
            _aoe = [new(donut, Arena.Center, default, Module.CastFinishAt(spell, 1.1d))];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x22 && state == 0x00020001u)
        {
            Arena.Bounds = A12Fafnir.DefaultBounds;
            _aoe = [];
        }
    }
}

sealed class DragonBreathArenaChange(BossModule module) : BossComponent(module)
{
    public override bool KeepOnPhaseChange => true;

    private Angle initialRot;
    private DateTime started;
    private static readonly Circle circle = new(A12Fafnir.ArenaCenter, 16f);

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == (uint)OID.FireVoidzone)
        {
            if (state == 0x00010002u) // outer arena starts to turn unsafe
            {
                Arena.Bounds = A12Fafnir.FireArena;
            }
            else if (state == 0x00040008u) // outer arena starts to turn safe again
            {
                initialRot = actor.Rotation;
                started = WorldState.CurrentTime;
            }
        }
    }

    public override void Update()
    {
        if (started != default)
        {
            var time = (WorldState.CurrentTime - started).TotalSeconds;
            var angle = initialRot - ((float)time * 30f).Degrees(); // 30° of the outer arena turn safe again per second
            if (time >= 12d)
            {
                started = default;
                Arena.Bounds = A12Fafnir.DefaultBounds;
                Arena.Center = A12Fafnir.ArenaCenter;
                return;
            }
            ArenaBoundsCustom refresh = new([circle, new Cone(A12Fafnir.ArenaCenter, 30f, angle, initialRot)]);
            Arena.Bounds = refresh;
            Arena.Center = refresh.Center;
        }
    }
}
