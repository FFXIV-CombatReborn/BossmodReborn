namespace BossMod.Endwalker.VariantCriterion.V2MountRokkon.V24Shishio;

sealed class FocusedTremorYokiUzu(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(30f, 20f);
    private static readonly AOEShapeDonut donut = new(23f, 40f, true);
    private AOEInstance[] _aoe = [];
    private AOEInstance[] _aoeCache = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoe.Length != 0)
        {
            ref var aoe = ref _aoe[0];
            if (aoe.Shape == rect)
            {
                aoe.Activation = actor.FindStatus((uint)SID.SixFulmsUnder)?.ExpireAt ?? DateTime.MaxValue;
            }
        }
        return _aoe;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.YokiUzu)
        {
            _aoeCache = [.. _aoe];
            _aoe = [new(donut, spell.LocXZ, default, Module.CastFinishAt(spell), Colors.SafeFromAOE)];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.YokiUzu)
        {
            _aoe = [.. _aoeCache];
            _aoeCache = [];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (state == 0x00020001u)
        {
            var center = Arena.Center;
            _aoe = index switch
            {
                0x67 => [new(rect, center - new WDir(20f, default), 90f.Degrees())],
                0x65 => [new(rect, center - new WDir(-20f, default), -90f.Degrees())],
                0x66 => [new(rect, center - new WDir(default, 20f))],
                0x68 => [new(rect, center - new WDir(default, -20f), 180f.Degrees())],
                _ => []
            };
        }
        else if (state == 0x00080004u && index is 0x65 or 0x66 or 0x67 or 0x68)
        {
            _aoe = [];
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        var expireAt = actor.FindStatus((uint)SID.SixFulmsUnder)?.ExpireAt ?? DateTime.MaxValue;
        if (_aoe.Length != 0)
        {
            ref var aoe = ref _aoe[0];
            if (aoe.Shape == donut && (expireAt - WorldState.CurrentTime).TotalSeconds <= 8d)
            {
                hints.ActionsToExecute.Push(ActionDefinitions.IDSprint, actor, ActionQueue.Priority.High);
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_aoe.Length != 0)
        {
            ref var aoe = ref _aoe[0];
            if (aoe.Shape == rect)
            {
                base.AddHints(slot, actor, hints);
            }
            else if (!aoe.Check(actor.Position))
            {
                hints.Add("Go into quicksand to avoid AOE!");
            }
        }
    }
}
