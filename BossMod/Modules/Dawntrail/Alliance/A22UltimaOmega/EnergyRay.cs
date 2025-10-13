namespace BossMod.Dawntrail.Alliance.A22UltimaOmega;

sealed class EnergyRay(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> _aoes = new(3);
    private readonly AOEShapeRect rectLong = new(48f, 10f), rectShort = new(40f, 8f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.EnergyRay)
        {
            var rot = spell.Rotation;
            AddAOE(rectShort, spell.LocXZ, rot);
            var screens = Module.Enemies([(uint)OID.ManaScreen2, (uint)OID.ManaScreen3]); // ManaScreen1 would be better, but never gets destroyed
            var count = screens.Count;
            if (count == 0)
            {
                return;
            }
            (WPos initialPosition, int[] indices)[] initialPosMap =
            [
                (new(755f, 816f), [0, 1]),
                (new(755f, 800f), [2, 3]),
                (new(820f, 784f), [4]),
                (new(820f, 816f), [5]),
                (new(755f, 784f), [6, 7]),
            ];
            (WPos firstReflect, int angleFirst, WPos secondReflect, int angleSecond)[] aoeMap =
            [
                (new(725f, 816f), 2, new(725f, 784f), 3),
                (new(745f, 816f), 2, new(745f, 800f), 0),
                (new(745f, 800f), 1, new(745f, 816f), 0),
                (new(725f, 800f), 2, new(725f, 784f), 3),
                (new(810f, 784f), 1, default, default),
                (new(810f, 816f), 2, default, default),
                (new(745f, 784f), 1, new(745f, 816f), 0),
                (new(725f, 784f), 1, new(725f, 800f), 3),
            ];
            var pos = caster.Position;
            for (var i = 0; i < 5; ++i)
            {
                ref var initial = ref initialPosMap[i];
                if (pos == initial.initialPosition)
                {
                    var indices = initial.indices;
                    var len = indices.Length;
                    for (var j = 0; j < len; ++j)
                    {
                        ref var aoes = ref aoeMap[indices[j]];
                        for (var k = 0; k < count; ++k)
                        {
                            var first = aoes.firstReflect;
                            if (screens[k].Position == first)
                            {
                                AddAOE(rectLong, first, Angle.AnglesCardinals[aoes.angleFirst], 0.7d);
                                var second = aoes.secondReflect;
                                if (second != default)
                                {
                                    AddAOE(rectShort, second, Angle.AnglesCardinals[aoes.angleSecond], 1.2d);
                                }
                                return;
                            }
                        }
                    }
                }
            }
            void AddAOE(AOEShapeRect shape, WPos position, Angle rotation, double delay = default) => _aoes.Add(new(shape, position.Quantized(), rotation, Module.CastFinishAt(spell, delay)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID is (uint)AID.EnergyRay or (uint)AID.EnergyRayReflect1 or (uint)AID.EnergyRayReflect2 or (uint)AID.EnergyRayReflect3 or (uint)AID.EnergyRayReflect4)
        {
            _aoes.RemoveAt(0);
        }
    }
}
