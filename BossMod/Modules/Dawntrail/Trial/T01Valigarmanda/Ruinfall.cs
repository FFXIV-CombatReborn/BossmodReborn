namespace BossMod.Dawntrail.Trial.T01Valigarmanda;

sealed class RuinfallAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RuinfallAOE, 6f);

sealed class RuinfallKB(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.RuinfallKB, 21f, stopAfterWall: true, kind: Kind.DirForward)
{
    private readonly RuinfallTower _tower = module.FindComponent<RuinfallTower>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count == 0)
            return;
        if (actor.Role != Role.Tank)
        {
            ref readonly var c = ref Casters.Ref(0);
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(Module.PrimaryActor.Position, new WDir(default, 1f), 1f, default, 20f), c.Activation);
            return;
        }
        var towers = _tower.Towers;
        var count = towers.Count;
        if (count == 0)
            return;
        ref var t0 = ref towers.Ref(0);
        var isDelayDeltaLow = (t0.Activation - WorldState.CurrentTime).TotalSeconds < 5d;
        var isActorInsideTower = false;
        if (t0.IsInside(actor))
            isActorInsideTower = true;
        if (isDelayDeltaLow && isActorInsideTower)
            hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.ArmsLength), actor, ActionQueue.Priority.High);
    }
}

sealed class RuinfallTower(BossModule module) : Components.CastTowers(module, (uint)AID.RuinfallTower, 6f, 2, 2)
{
    public override void Update()
    {
        if (Towers.Count == 0)
            return;
        var forbidden = Raid.WithSlot(false, true, true).WhereActor(p => p.Role != Role.Tank).Mask();
        foreach (ref var t in Towers.AsSpan())
            t.ForbiddenSoakers = forbidden;
    }
}
