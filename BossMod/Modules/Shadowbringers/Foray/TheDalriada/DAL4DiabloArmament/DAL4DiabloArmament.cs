namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL4DiabloArmament;

sealed class AdvancedDeathIV(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AdvancedDeathIV, 10f);
sealed class AethericBoomExplosionDiabolicGateVoidSystemsOverload(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.AethericBoom, (uint)AID.AethericExplosion,
(uint)AID.DiabolicGate, (uint)AID.VoidSystemsOverload]);

sealed class AdvancedNox(BossModule module) : Components.SimpleExaflare(module, 10f, (uint)AID.AdvancedNoxFirst, (uint)AID.AdvancedNoxRest, 10f, 1.1d, 5, 5, true);

sealed class AccelerationBomb(BossModule module) : Components.StayMove(module, 2.5d)
{
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.AccelerationBomb && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            PlayerStates[slot] = new(Requirement.Stay, status.ExpireAt);
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.AccelerationBomb && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            PlayerStates[slot] = default;
        }
    }
}

sealed class AssaultCannon(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AssaultCannon, new AOEShapeRect(100f, 3f));
sealed class DeadlyDealingAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeadlyDealingAOE, 6f);
sealed class DeadlyDealingKB(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.DeadlyDealing, 30f, stopAtWall: true)
{
    private readonly AdvancedNox _exa = module.FindComponent<AdvancedNox>()!;
    private readonly AssaultCannon _aoe = module.FindComponent<AssaultCannon>()!;

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var aoes = CollectionsMarshal.AsSpan(_aoe.Casters);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var aoe = ref aoes[i];
            if (aoe.Check(pos))
            {
                return true;
            }
        }
        var exas = _exa.ActiveAOEs(slot, actor);
        var len2 = exas.Length;
        for (var i = 0; i < len2; ++i)
        {
            ref readonly var aoe = ref exas[i];
            if (aoe.Check(pos))
            {
                return true;
            }
        }
        return !Arena.InBounds(pos);
    }
}

sealed class AdvancedDeathRay(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(70f, 4f), (uint)IconID.AdvancedDeathRay, (uint)AID.AdvancedDeathRay, 5.9d, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);
sealed class PillarOfShamashBait(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(70f, 2f), (uint)IconID.PillarOfShamash, (uint)AID.PillarOfShamashBait);
sealed class PillarOfShamashStack(BossModule module) : Components.LineStack(module, aidMarker: (uint)AID.PillarOfShamashMarker, (uint)AID.PillarOfShamashStack, 6d, 70f, 4f, 8, 8);
sealed class Explosion(BossModule module) : Components.SimpleAOEGroupsByTimewindow(module, [(uint)AID.Explosion1, (uint)AID.Explosion2,
(uint)AID.Explosion3, (uint)AID.Explosion4, (uint)AID.Explosion5, (uint)AID.Explosion6, (uint)AID.Explosion7, (uint)AID.Explosion8, (uint)AID.Explosion9],
new AOEShapeRect(60f, 11f))
{
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Casters.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = CollectionsMarshal.AsSpan(Casters);
        ref readonly var aoe0 = ref aoes[0];
        var deadline = aoe0.Activation.AddSeconds(TimeWindowInSeconds);
        var rot = aoe0.Rotation;

        var index = 0;
        while (index < count)
        {
            ref readonly var aoe = ref aoes[index];
            if (aoe.Activation >= deadline && aoe.Rotation == rot)
            {
                break;
            }
            ++index;
        }
        return aoes[..index];
    }
}

sealed class LightPseudopillar(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LightPseudopillar, 10f);

sealed class PillarOfShamash(BossModule module) : Components.SimpleAOEGroupsByTimewindow(module, [(uint)AID.PillarOfShamashCone1, (uint)AID.PillarOfShamashCone2,
(uint)AID.PillarOfShamashCone3], new AOEShapeCone(70f, 10f.Degrees()), expectedNumCasters: 9);
sealed class UltimatePseudoterror(BossModule module) : Components.SimpleAOEs(module, (uint)AID.UltimatePseudoterror, new AOEShapeDonut(15f, 70f));

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.TheDalriada, GroupID = 778, NameID = 10007, SortOrder = 6)]
public sealed class DAL4DiabloArmament(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, DefaultArena)
{
    public static readonly WPos ArenaCenter = new(-720f, -760f);
    public static readonly ArenaBoundsCustom DefaultArena = new([new Polygon(ArenaCenter, 29.5f, 48)]);
    public static readonly ArenaBoundsCircle SmallArena = new(17f); // this is a pulsing donut aoe
}
