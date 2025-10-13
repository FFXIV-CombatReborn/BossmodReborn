namespace BossMod.Dawntrail.Extreme.Ex6GuardianArkveld;

sealed class WyvernsRadianceCleave(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    private readonly AOEShapeRect rect = new(80f, 14f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        switch (id)
        {
            case (uint)AID.WyvernsRadianceCleaveFirstR1:
            case (uint)AID.WyvernsRadianceCleaveFirstR2:
            case (uint)AID.WyvernsRadianceCleaveFirstL1:
            case (uint)AID.WyvernsRadianceCleaveFirstL2:
                var loc = spell.LocXZ;
                var rot = spell.Rotation;
                var dir = 30f * rot.ToDirection();
                var offset = id is (uint)AID.WyvernsRadianceCleaveFirstR1 or (uint)AID.WyvernsRadianceCleaveFirstR2 ? dir.OrthoL() : dir.OrthoR();
                AddAOE(loc, rot);
                AddAOE(loc + offset, rot, 4.1d);
                break;
            case (uint)AID.WyvernsRadianceCleaveSecondR1:
            case (uint)AID.WyvernsRadianceCleaveSecondR2:
            case (uint)AID.WyvernsRadianceCleaveSecondL1:
            case (uint)AID.WyvernsRadianceCleaveSecondL2:
                if (_aoes.Count != 0) // ensure pixel perfectness as soon as cast starts
                {
                    ref var aoe = ref _aoes.Ref(0);
                    aoe.Origin = spell.LocXZ;
                    aoe.ShapeDistance = rect.Distance(aoe.Origin, aoe.Rotation);
                }
                break;
        }
        void AddAOE(WPos position, Angle rotation, double delay = default) => _aoes.Add(new(rect, position, rotation, Module.CastFinishAt(spell, delay), shapeDistance: rect.Distance(position, rotation)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (id is (uint)AID.WyvernsRadianceCleaveFirstR1 or (uint)AID.WyvernsRadianceCleaveFirstR2 or (uint)AID.WyvernsRadianceCleaveFirstL1 or (uint)AID.WyvernsRadianceCleaveFirstL2
        or (uint)AID.WyvernsRadianceCleaveSecondR1 or (uint)AID.WyvernsRadianceCleaveSecondR2 or (uint)AID.WyvernsRadianceCleaveSecondL1 or (uint)AID.WyvernsRadianceCleaveSecondL2)
        {
            var count = _aoes.Count;
            ++NumCasts;
            if (count != 0)
            {
                _aoes.RemoveAt(0);
                if (count == 2)
                {
                    var dir = 16f * spell.Rotation.ToDirection();
                    var offset = id is (uint)AID.WyvernsRadianceCleaveFirstR1 or (uint)AID.WyvernsRadianceCleaveFirstR2 ? dir.OrthoR() : dir.OrthoL();
                    ref var aoe = ref CollectionsMarshal.AsSpan(_aoes)[0];
                    aoe.Origin += offset;
                    aoe.ShapeDistance = rect.Distance(aoe.Origin, aoe.Rotation);
                }
            }
        }
    }
}

// drawing these is mostly redundant, I believe it is pretty much impossible to get hit by these without also getting hit by the cleave
sealed class ChainbladeBlow(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ChainbladeBlowFirst1, (uint)AID.ChainbladeBlowFirst2,
(uint)AID.ChainbladeBlowFirst3, (uint)AID.ChainbladeBlowFirst4,
(uint)AID.ChainbladeBlowFirst5, (uint)AID.ChainbladeBlowFirst6, (uint)AID.ChainbladeBlowFirst7, (uint)AID.ChainbladeBlowFirst8,
(uint)AID.ChainbladeBlowRest1, (uint)AID.ChainbladeBlowRest2, (uint)AID.ChainbladeBlowRest3, (uint)AID.ChainbladeBlowRest4,
(uint)AID.ChainbladeBlowRest5, (uint)AID.ChainbladeBlowRest6, (uint)AID.ChainbladeBlowRest7, (uint)AID.ChainbladeBlowRest8], new AOEShapeRect(40f, 2f));
