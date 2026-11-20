namespace BossMod.Dawntrail.Savage.M07SBruteAbombinator;

sealed class HurricaneForce(BossModule module) : Components.CastHint(module, (uint)AID.HurricaneForce, "Plant enrage!", true);

sealed class QuarrySwamp(BossModule module) : Components.CastLineOfSightAOE(module, (uint)AID.QuarrySwamp, 60f)
{
    public override ReadOnlySpan<Actor> BlockerActors() => CollectionsMarshal.AsSpan(Module.Enemies((uint)OID.BloomingAbomination));

    public override void Update()
    {
        if (Casters.Count != 0 && BlockerActors().Length != 0)
        {
            Safezones.Clear();
            Refresh();
            AddSafezone(Module.CastFinishAt(Casters[0].CastInfo));
        }
    }
}

sealed class SporeSac(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SporeSac, 8f);
sealed class Pollen(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Pollen, 8f);
sealed class RootsOfEvil(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RootsOfEvil, 12f);
sealed class CrossingCrosswinds(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CrossingCrosswinds, new AOEShapeCross(50f, 5f));
sealed class WindingWildwinds(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WindingWildwinds, new AOEShapeDonut(5f, 60f));

sealed class AddInterruptHint(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> castersDonut = [with(2)];
    private readonly List<Actor> castersCross = [with(2)];

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var countD = castersDonut.Count;
        for (var i = 0; i < countD; ++i)
        {
            var e = hints.FindEnemy(castersDonut[i]);
            e?.ShouldBeInterrupted = true;
        }
        if (countD != 0)
            return;
        var countC = castersCross.Count;
        for (var i = 0; i < countC; ++i)
        {
            var e = hints.FindEnemy(castersCross[i]);
            e?.ShouldBeInterrupted = true;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action.ID == (uint)AID.WindingWildwinds)
        {
            castersDonut.Add(caster);
        }
        else if (spell.Action.ID == (uint)AID.CrossingCrosswinds)
        {
            castersCross.Add(caster);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
        if (spell.Action.ID == (uint)AID.WindingWildwinds)
        {
            castersDonut.Remove(caster);
        }
        else if (spell.Action.ID == (uint)AID.CrossingCrosswinds)
        {
            castersCross.Remove(caster);
        }
    }

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (castersDonut.Count != 0)
        {
            hints.Add("Interrupt donut caster!");
            return;
        }

        if (castersCross.Count != 0)
        {
            hints.Add("Interrupt cross caster!");
        }
    }
}
