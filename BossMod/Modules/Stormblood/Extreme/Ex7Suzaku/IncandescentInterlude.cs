namespace BossMod.Stormblood.Extreme.Ex7Suzaku;

sealed class IncandescentInterlude(BossModule module) : Components.GenericTowers(module)
{
    private BitMask _forbidden;
    public readonly List<Tower> TowerCache = new(4);
    private readonly int party = module.Raid.WithoutSlot(true, false, false).Length;
    private readonly RuthlessRefrain _kb = module.FindComponent<RuthlessRefrain>()!;

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Towers)
            TowerCache.Add(new(actor.Position.Quantized(), 4f, activation: WorldState.FutureTime(9.7d))); // no use to draw towers before spread markers are out
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.Burn or (uint)AID.IncandescentInterlude)
            Towers.Clear();
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (TowerCache.Count != 0 && iconID == (uint)IconID.Spreadmarker && Raid.FindSlot(actor.InstanceID) is var slot)
        {
            _forbidden[slot] = true;
            if (Towers.Count == 0)
                Towers = TowerCache;

            if (party == 8) // don't affect unsync farming
            {
                var count = Towers.Count;
                var towers = CollectionsMarshal.AsSpan(Towers);
                for (var i = 0; i < count; ++i)
                {
                    ref var t = ref towers[i];
                    t.ForbiddenSoakers = _forbidden;
                }
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (_kb.Casters.Count != 0 && party == 8) // don't affect unsync farming
        {
            var towers = Module.Enemies((uint)OID.Towers);
            var forbidden = new ShapeDistance[4];
            var a35 = 35f.Degrees();
            for (var i = 0; i < 4; ++i)
            {
                forbidden[i] = new SDCone(Ex7Suzaku.ArenaCenter, 20f, _forbidden[slot] ? Angle.AnglesCardinals[i] : Angle.AnglesIntercardinals[i], a35);
            }
            hints.AddForbiddenZone(new SDUnion(forbidden), _kb.Casters.Ref(0).Activation);
        }
    }
}
