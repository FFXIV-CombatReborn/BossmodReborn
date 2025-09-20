namespace BossMod.Endwalker.VariantCriterion.V2MountRokkon.V25Enenra;

sealed class PipeCleaner(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeRect(60f, 5f), (uint)TetherID.PipeCleaner);
sealed class Uplift(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Uplift, 6f);
sealed class Snuff(BossModule module) : Components.BaitAwayCast(module, (uint)AID.Snuff, 6f, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);

sealed class Smoldering(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Smoldering, 8f, 8);
sealed class FlagrantCombustion(BossModule module) : Components.RaidwideCast(module, (uint)AID.FlagrantCombustion);
sealed class SmokeRings(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SmokeRings, 16f);
sealed class ClearingSmoke(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.ClearingSmoke, 16f, stopAfterWall: true)
{
    private readonly Smoldering _aoe = module.FindComponent<Smoldering>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0 && _aoe.Casters.Count != 0)
        {
            hints.AddForbiddenZone(new SDInvertedCircle(Arena.Center, 4f), Casters.Ref(0).Activation);
        }
    }
}

sealed class StringRock(BossModule module) : Components.ConcentricAOEs(module, _shapes)
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(6f), new AOEShapeDonut(6f, 12f), new AOEShapeDonut(12f, 18f), new AOEShapeDonut(18f, 24f)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.KiseruClamor)
        {
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count != 0)
        {
            var order = spell.Action.ID switch
            {
                (uint)AID.KiseruClamor => 0,
                (uint)AID.BedrockUplift1 => 1,
                (uint)AID.BedrockUplift2 => 2,
                (uint)AID.BedrockUplift3 => 3,
                _ => -1
            };
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2d));
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", PrimaryActorOID = (uint)OID.Enenra, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 945, NameID = 12393, SortOrder = 7, Category = BossModuleInfo.Category.VariantCriterion, Expansion = BossModuleInfo.Expansion.Endwalker)]
public sealed class V25Enenra(WorldState ws, Actor primary) : BossModule(ws, primary, new(900f, -900f), StartingBounds)
{
    public static readonly ArenaBoundsCircle StartingBounds = new(20.5f);
    public static readonly ArenaBoundsCircle DefaultBounds = new(20f);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.EnenraClone));
    }
}
