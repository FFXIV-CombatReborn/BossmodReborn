namespace BossMod.Endwalker.VariantCriterion.C03AAI.C030Trash1;

sealed class Hydroshot(BossModule module) : Components.GenericKnockback(module)
{
    private Actor? _caster;

    private readonly Knockback[] _oneKnockback = new Knockback[1];
    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (_caster?.CastInfo?.TargetID == actor.InstanceID)
        {
            _oneKnockback[0] = new(_caster.Position, 10f, Module.CastFinishAt(_caster.CastInfo));
            return _oneKnockback;
        }
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.NHydroshot or (uint)AID.SHydroshot)
        {
            _caster = caster;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_caster == caster)
        {
            _caster = null;
        }
    }
}

abstract class C030MonkStates : StateMachineBuilder
{
    private readonly bool _savage;

    public C030MonkStates(BossModule module, bool savage) : base(module)
    {
        _savage = savage;
        DeathPhase(0u, SinglePhase)
            .ActivateOnEnter<Hydroshot>()
            .ActivateOnEnter<Twister>();
    }

    private void SinglePhase(uint id)
    {
        Hydroshot(id, 9.1f);
        CrossAttack(id + 0x10000u, 3.4f);
        SimpleState(id + 0xFF0000u, 10f, "???");
    }

    private void Hydroshot(uint id, float delay)
    {
        Cast(id, _savage ? AID.SHydroshot : AID.NHydroshot, delay, 5f, "Single-target 1");
        Cast(id + 0x10u, _savage ? AID.SHydroshot : AID.NHydroshot, 1.5f, 5f, "Single-target 2");
        Cast(id + 0x20u, _savage ? AID.SHydroshot : AID.NHydroshot, 1.5f, 5f, "Single-target 3");
    }

    private void CrossAttack(uint id, float delay)
    {
        Cast(id, _savage ? AID.SCrossAttack : AID.NCrossAttack, delay, 5, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
    }
}

sealed class C030NMonkStates(BossModule module) : C030MonkStates(module, false);
sealed class C030SMonkStates(BossModule module) : C030MonkStates(module, true);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NMonk, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 979u, NameID = 12631u, SortOrder = 4)]
public sealed class C030NMonk(WorldState ws, Actor primary) : C030Trash1(ws, primary);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SMonk, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 980u, NameID = 12631u, SortOrder = 4)]
public sealed class C030SMonk(WorldState ws, Actor primary) : C030Trash1(ws, primary);
