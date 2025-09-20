namespace BossMod.Stormblood.Foray.BaldesionArsenal.BA4ProtoOzma;

sealed class Holy(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Holy, 3f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            ref readonly var c = ref Casters.Ref(0);
            var act = c.Activation;
            if (!IsImmune(slot, act))
                hints.AddForbiddenZone(new SDInvertedCircle(Module.PrimaryActor.Position, 21.5f), act);
        }
    }
}

sealed class ShootingStar(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.ShootingStar, 8f, shape: new AOEShapeCircle(26f))
{
    private readonly TransitionAttacks _aoe = module.FindComponent<TransitionAttacks>()!;
    private static readonly Angle a60 = 60f.Degrees(), am60 = -60f.Degrees(), a180 = 180f.Degrees(), a120 = 120f.Degrees(), am120 = -120f.Degrees(), a30 = 30f.Degrees();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = Casters.Count;
        if (count == 0)
            return;
        ref readonly var c = ref Casters.Ref(0);
        var act = c.Activation;
        if (IsImmune(slot, act))
            return;

        var transitionAOE = _aoe.AOEs.Count != 0 ? _aoe.AOEs.Ref(0).Shape : null;
        var forbidden = new ShapeDistance[transitionAOE != null ? count : 2 * count];
        var index = 0;
        var casters = CollectionsMarshal.AsSpan(Casters);
        for (var i = 0; i < count; ++i)
        {
            ref readonly var caster = ref casters[i];
            var pos = caster.Origin;
            void AddForbiddenCone(Angle direction) => forbidden[index++] = new SDInvertedCone(pos, 3.5f, direction, a30);

            switch ((int)pos.X)
            {
                case -38:
                    if (transitionAOE != TransitionAttacks.Donut)
                        AddForbiddenCone(a60);
                    if (transitionAOE != TransitionAttacks.Circle)
                        AddForbiddenCone(am120);
                    break;
                case -17:
                    if (transitionAOE != TransitionAttacks.Donut)
                        AddForbiddenCone(a180);
                    if (transitionAOE != TransitionAttacks.Circle)
                        AddForbiddenCone(default);
                    break;
                case 4:
                    if (transitionAOE != TransitionAttacks.Donut)
                        AddForbiddenCone(am60);
                    if (transitionAOE != TransitionAttacks.Circle)
                        AddForbiddenCone(a120);
                    break;
            }
        }
        hints.AddForbiddenZone(new SDIntersection(forbidden), act);
    }
}
