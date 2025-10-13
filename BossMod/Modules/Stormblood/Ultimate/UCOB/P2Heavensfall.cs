namespace BossMod.Stormblood.Ultimate.UCOB;

class P2Heavensfall(BossModule module) : Components.GenericKnockback(module, (uint)AID.Heavensfall)
{
    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        return new Knockback[1] { new(Arena.Center, 11f, ignoreImmunes: true) }; // TODO: activation
    }
}

class P2HeavensfallPillar(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];

    private static readonly AOEShapeRect _shape = new(5f, 5f, 5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID != (uint)OID.EventHelper)
            return;
        switch (state)
        {
            case 0x00040008u: // appear
                _aoe = [new(_shape, actor.Position, actor.Rotation)];
                break;
            // 0x00100020: ? 0.5s after appear
            // 0x00400080: ? 4.0s after appear
            // 0x01000200: ? 5.8s after appear
            // 0x04000800: ? 7.5s after appear
            // 0x10002000: ? 9.4s after appear
            case 0x40008000u: // disappear (11.1s after appear)
                _aoe = [];
                break;
        }
    }
}

class P2ThermionicBurst(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ThermionicBurst, new AOEShapeCone(24.5f, 11.25f.Degrees()));

class P2MeteorStream : Components.UniformStackSpread
{
    public int NumCasts;

    public P2MeteorStream(BossModule module) : base(module, default, 4f)
    {
        AddSpreads(Raid.WithoutSlot(true, true, true), WorldState.FutureTime(5.6d));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.MeteorStream)
        {
            ++NumCasts;
            {
                var count = Spreads.Count;
                var id = spell.MainTargetID;
                for (var i = 0; i < count; ++i)
                {
                    if (Spreads[i].Target.InstanceID == id)
                    {
                        Spreads.RemoveAt(i);
                        return;
                    }
                }
            }
        }
    }
}

class P2HeavensfallDalamudDive(BossModule module) : Components.GenericBaitAway(module, (uint)AID.DalamudDive, true, true)
{
    private readonly Actor? _target = module.WorldState.Actors.Find(module.PrimaryActor.TargetID);

    private static readonly AOEShapeCircle _shape = new(5f);

    public void Show()
    {
        if (_target != null)
        {
            CurrentBaits.Add(new(_target, _target, _shape));
        }
    }
}
