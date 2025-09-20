namespace BossMod.Endwalker.Alliance.A14Naldthal;

class TippedScales(BossModule module) : Components.GenericAOEs(module, (uint)AID.TippedScalesAOE)
{
    private readonly List<(Actor, int weight)> weightsByActor = new(27);
    private static readonly WDir eastBoundary = new(50f, default), dir = new(1f, default);
    private static readonly AOEShapeRect rect = new(50f, 50f);
    private static readonly Angle a90 = 90f.Degrees();
    private DateTime activation;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = weightsByActor.Count;
        if (count == 0)
            return [];
        int westWeight = 0, eastWeight = 0;

        var actors = CollectionsMarshal.AsSpan(weightsByActor);
        for (var i = 0; i < count; ++i)
        {
            ref readonly var a = ref actors[i];
            {
                if (a.Item1.Position.InRect(Arena.Center, eastBoundary, 50f))
                    eastWeight += a.weight;
                else
                    westWeight += a.weight;
            }
        }

        if (Math.Abs(westWeight - eastWeight) < 3)
            return [];
        else
            return new AOEInstance[1] { new(rect, Arena.Center, westWeight > eastWeight ? a90 : -a90, activation, Colors.SafeFromAOE, false) };
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        var count = weightsByActor.Count;
        if (count == 0)
            return;
        int westWeight = 0, eastWeight = 0;

        var actors = CollectionsMarshal.AsSpan(weightsByActor);
        for (var i = 0; i < count; ++i)
        {
            ref readonly var actor = ref actors[i];
            {
                if (actor.Item1.Position.InRect(Arena.Center, eastBoundary, 50f))
                    eastWeight += actor.weight;
                else
                    westWeight += actor.weight;
            }
        }

        if (Math.Abs(westWeight - eastWeight) < 3)
            hints.Add($"Suggestion: Stay! (Total weight west: {westWeight}, total weight east: {eastWeight})");
        else
            hints.Add($"Suggestion: Move {(westWeight > eastWeight ? "east" : "west")!} (Total weight west: {westWeight}, total weight east: {eastWeight})");
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Weight)
            switch (status.Extra)
            {
                case 0x1BD:
                    weightsByActor.Add((actor, 1)); // players
                    break;
                case 0x1BC:
                    weightsByActor.Add((actor, 8)); // vessels with 2 big weights, todo: verify weight value
                    break;
                case 0x1BB:
                    weightsByActor.Add((actor, 12)); // vessels with 3 big weights, todo: verify weight value
                    break;
            }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == (uint)AID.TippedScales)
            weightsByActor.Clear();
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TippedScales)
            activation = Module.CastFinishAt(spell);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var aoes = ActiveAOEs(slot, actor);
        if (aoes.Length == 0)
            return;
        ref readonly var aoe = ref aoes[0];
        // rect is offset by one so it doesn't switch sides every frame if there are multiple AI users...
        hints.AddForbiddenZone(new SDInvertedRect(Arena.Center + (aoe.Rotation == a90 ? dir : -dir), aoe.Rotation == a90 ? dir : -dir, 30f, default, 30f), aoe.Activation);
    }
}
