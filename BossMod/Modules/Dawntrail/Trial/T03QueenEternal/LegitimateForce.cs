namespace BossMod.Dawntrail.Trial.T03QueenEternal;

sealed class LegitimateForce(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    private static readonly AOEShapeRect rect = new(20f, 40f);
    private static readonly WDir offset1 = new(default, 20f), offset2 = new(default, 8f);
    private readonly Besiegement _aoe = module.FindComponent<Besiegement>()!;
    private static readonly ShapeDistance stayInBounds = new SDIntersection([
        new SDInvertedRect(T03QueenEternal.LeftSplitCenter + offset2, T03QueenEternal.LeftSplitCenter - offset2, 4f),
        new SDInvertedRect(T03QueenEternal.RightSplitCenter + offset2, T03QueenEternal.RightSplitCenter - offset2, 4f)]);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0 || _aoe.AOEs.Count != 0)
        {
            return [];
        }
        var aoes = CollectionsMarshal.AsSpan(_aoes);

        if (count > 1 && aoes[0].Rotation != aoes[1].Rotation)
        {
            ref var aoe0 = ref aoes[0];
            aoe0.Color = Colors.Danger;
            return aoes;
        }
        return aoes[..1];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.LegitimateForceLL:
                AddAOEs(caster, -90f, -90f);
                break;
            case (uint)AID.LegitimateForceLR:
                AddAOEs(caster, -90f, 90f);
                break;
            case (uint)AID.LegitimateForceRR:
                AddAOEs(caster, 90f, 90f);
                break;
            case (uint)AID.LegitimateForceRL:
                AddAOEs(caster, 90f, -90f);
                break;
        }

        void AddAOEs(Actor caster, float first, float second)
        {
            AddAOE(first);
            AddAOE(second, 3.1d);
            void AddAOE(float offset, double delay = default) => _aoes.Add(new(rect, caster.Position, spell.Rotation + offset.Degrees(), Module.CastFinishAt(spell, delay)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count != 0)
        {
            switch (spell.Action.ID)
            {
                case (uint)AID.LegitimateForceLL:
                case (uint)AID.LegitimateForceLR:
                case (uint)AID.LegitimateForceRR:
                case (uint)AID.LegitimateForceRL:
                case (uint)AID.LegitimateForceR:
                case (uint)AID.LegitimateForceL:
                    _aoes.RemoveAt(0);
                    break;
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var count = _aoes.Count;
        var besiegeCount = _aoe.AOEs.Count;
        var gravityBounds = Arena.Bounds == T03QueenEternal.SplitGravityBounds;
        if (count != 0 && Arena.Center != T03QueenEternal.SplitArena.Center || besiegeCount == 0 && count == 2 && gravityBounds)
        {
            hints.AddForbiddenZone(new SDInvertedRect(Arena.Center + offset1, Arena.Center - offset1, 3f), aoes[0].Activation);
        }
        else if (count != 2 && besiegeCount == 0 && gravityBounds)
        {
            hints.AddForbiddenZone(stayInBounds, count != 0 ? aoes[0].Activation : default);
        }
    }
}
