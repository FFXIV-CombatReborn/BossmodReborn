namespace BossMod.Shadowbringers.Foray.Duel.Duel1Gabriel;

sealed class Burst(BossModule module) : Components.CastTowers(module, (uint)AID.Burst, 4f);

sealed class EnhancedMobility(BossModule module) : Components.GenericKnockback(module, (uint)AID.EnhancedMobility)
{
    private Knockback[] _kb = [];
    private WPos tower;
    private readonly MagitekCannonVoidzone _aoe = module.FindComponent<MagitekCannonVoidzone>()!;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => _kb;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            _kb = [new(caster.Position, 12f, Module.CastFinishAt(spell))];
        }
        else if (spell.Action.ID == (uint)AID.Burst)
        {
            tower = spell.LocXZ;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            _kb = [];
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_kb.Length != 0)
        {
            ref readonly var kb = ref _kb[0];
            var act = kb.Activation;
            if (!IsImmune(slot, act))
            {
                var dir = (Module.PrimaryActor.Position - Arena.Center).Normalized();
                hints.AddForbiddenZone(new SDInvertedRect(tower + 12f * dir, dir, 4f, default, 1f), act);
            }
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = _aoe.ActiveAOEs(slot, actor);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (aoe.Check(pos))
            {
                return true;
            }
        }
        return !Arena.InBounds(pos);
    }
}

sealed class EnhancedMobilityFullyAnalyzedHint(BossModule module) : Components.CastHint(module, (uint)AID.EnhancedMobility, "Use manawall or tank invuln to survive!");
sealed class EnhancedMobilityWeakpoint(BossModule module) : Components.CastWeakpoint(module, (uint)AID.EnhancedMobility, 40f, default, (uint)SID.BackUnseen, (uint)SID.LeftUnseen, (uint)SID.RightUnseen);
