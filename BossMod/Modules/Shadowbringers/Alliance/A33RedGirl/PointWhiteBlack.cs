namespace BossMod.Shadowbringers.Alliance.A33RedGirl;

sealed class PointBlackWhite(BossModule module) : Components.GenericAOEs(module)
{
    private readonly ArenaChanges _arena = module.FindComponent<ArenaChanges>()!;
    private readonly List<AOEInstance> _aoes = new(8);
    private static readonly AOEShapeRect rectShort = new(24f, 3f), rectLong = new(50f, 3f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorRenderflagsChanged(Actor actor, int renderflags)
    {
        if (renderflags == 0)
        {
            var oid = actor.OID;
            if (oid == (uint)OID.WhiteLance)
            {
                AddAOE(true);
            }
            else if (oid == (uint)OID.BlackLance)
            {
                AddAOE(false);
            }

            void AddAOE(bool isWhite)
            {
                var pos = actor.Position;
                var rot = actor.Rotation;
                var dir = rot.ToDirection();
                var walls = _arena.Walls;
                var isShort = false;
                for (var i = 0; i < 28; ++i)
                {
                    ref readonly var w = ref walls[i];
                    if (w.isWhite == isWhite && w.position.InRect(pos, dir, 30f, default, 3f))
                    {
                        isShort = true;
                        break;
                    }
                }
                _aoes.Add(new(isShort ? rectShort : rectLong, pos.Quantized(), rot, WorldState.FutureTime(7.8d)));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.PointBlack1 or (uint)AID.PointBlack2 or (uint)AID.PointWhite1 or (uint)AID.PointWhite2)
        {
            _aoes.Clear();
        }
    }
}
