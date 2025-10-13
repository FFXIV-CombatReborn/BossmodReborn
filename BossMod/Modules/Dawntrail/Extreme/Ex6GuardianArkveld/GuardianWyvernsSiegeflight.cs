namespace BossMod.Dawntrail.Extreme.Ex6GuardianArkveld;

sealed class GuardianWyvernsSiegeflight(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(3);
    private readonly AOEShapeRect rectFirst = new(40f, 4f), rectGuardian = new(40f, 8f), rectWyvern = new(40f, 9f);
    private readonly List<(ulong actor, WDir offset)> offsets = new(2);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        switch (id)
        {
            case (uint)AID.GuardianSiegeflight1:
            case (uint)AID.GuardianSiegeflight2:
            case (uint)AID.WyvernsSiegeflight1:
            case (uint)AID.WyvernsSiegeflight2:
                AddAOE(rectFirst, spell.LocXZ, spell.Rotation);
                SortAOEs();
                break;
            case (uint)AID.GuardianResonanceRect1:
            case (uint)AID.GuardianResonanceRect2:
                AddAOE(rectGuardian, spell.LocXZ, spell.Rotation);
                SortAOEs();
                break;
            case (uint)AID.WyvernsRadianceSides1:
            case (uint)AID.WyvernsRadianceSides2:
            case (uint)AID.WyvernsRadianceSides3:
            case (uint)AID.WyvernsRadianceSides4:
                var loc = spell.LocXZ;
                var rot = spell.Rotation;
                var dir = -4f * rot.ToDirection();
                var offset = id is (uint)AID.WyvernsRadianceSides1 or (uint)AID.WyvernsRadianceSides4 ? dir.OrthoL() : dir.OrthoR();
                AddAOE(rectWyvern, loc + offset, rot);
                offsets.Add((caster.InstanceID, offset));
                SortAOEs();
                break;
        }
        void AddAOE(AOEShapeRect rect, WPos position, Angle rotation, double delay = default) => _aoes.Add(new(rect, position, rotation, Module.CastFinishAt(spell, delay), actorID: caster.InstanceID, shapeDistance: rect.Distance(position, rotation)));
        void SortAOEs()
        {
            var count = _aoes.Count;
            if (count == 2)
            {
                var aoes = CollectionsMarshal.AsSpan(_aoes);
                ref var aoe1 = ref aoes[0];
                ref var aoe2 = ref aoes[1];
                if (aoe1.Activation > aoe2.Activation)
                {
                    (aoe1, aoe2) = (aoe2, aoe1);
                }
            }
            else if (count == 3)
            {
                _aoes.Sort(static (a, b) => a.Activation.CompareTo(b.Activation));
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.GuardianSiegeflight1:
            case (uint)AID.GuardianSiegeflight2:
            case (uint)AID.WyvernsSiegeflight1:
            case (uint)AID.WyvernsSiegeflight2:
            case (uint)AID.GuardianResonanceRect1:
            case (uint)AID.GuardianResonanceRect2:
            case (uint)AID.WyvernsRadianceSides1:
            case (uint)AID.WyvernsRadianceSides2:
            case (uint)AID.WyvernsRadianceSides3:
            case (uint)AID.WyvernsRadianceSides4:
                ++NumCasts;
                var count = _aoes.Count;
                if (count != 0)
                {
                    _aoes.RemoveAt(0);

                    if (count == 3)
                    {
                        var aoes = CollectionsMarshal.AsSpan(_aoes);
                        var offs = CollectionsMarshal.AsSpan(offsets);

                        for (var i = 0; i < 2; ++i)
                        {
                            ref var aoe = ref aoes[i];
                            for (var j = 0; j < 2; ++j)
                            {
                                ref var o = ref offs[j];
                                if (o.actor == aoe.ActorID)
                                {
                                    aoe.Origin -= o.offset;
                                    break;
                                }
                            }
                        }
                    }
                }
                break;
        }
    }
}
