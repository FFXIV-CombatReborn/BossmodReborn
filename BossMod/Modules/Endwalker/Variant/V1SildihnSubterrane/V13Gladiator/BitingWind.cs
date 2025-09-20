namespace BossMod.Endwalker.VariantCriterion.V1SildihnSubterrane.V13Gladiator;

sealed class BitingWindUpdraft(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BitingWindUpdraft, 6f);

sealed class BitingWindSmall(BossModule module) : Components.VoidzoneAtCastTarget(module, 4f, (uint)AID.BitingWindAOE, GetVoidzones, 0.3d)
{
    private static List<Actor> GetVoidzones(BossModule module) => module.Enemies((uint)OID.WhirlwindSmall);
}

sealed class BitingWindUpdraftVoidzone(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private bool active;
    private static readonly AOEShapeCircle circle = new(6f), circleInverted = new(6f, true);
    private DateTime activation = DateTime.MaxValue;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!active)
        {
            return [];
        }

        if (WorldState.CurrentTime > activation)
        {
            ref var aoe = ref _aoe[0];
            aoe.Color = Colors.SafeFromAOE;
            aoe.Shape = circleInverted;
        }
        return _aoe;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (active && spell.Action.ID == (uint)AID.ShatteringSteel)
        {
            activation = Module.CastFinishAt(spell, -5d);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BitingWindUpdraft)
        {
            _aoe = [new(circle, spell.LocXZ)];
            active = true;
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.WhirlwindUpdraft)
        {
            active = false;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!active)
        {
            return;
        }

        if (activation != DateTime.MaxValue)
        {
            hints.Add(WorldState.CurrentTime < activation ? "Prepare to walk into updraft!" : "Walk into updraft!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!active)
        {
            return;
        }
        base.AddAIHints(slot, actor, assignment, hints);

        // stay close to updraft
        if (activation != DateTime.MaxValue && WorldState.CurrentTime < activation)
        {
            ref var aoe = ref _aoe[0];
            hints.AddForbiddenZone(new SDInvertedCircle(aoe.Origin, 12f), activation);
        }
    }
}
