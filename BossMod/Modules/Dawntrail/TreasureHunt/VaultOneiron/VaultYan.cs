namespace BossMod.Dawntrail.TreasureHunt.VaultOneiron.VaultYan;

public enum OID : uint
{
    VaultYan = 0x4902, // R1.0
    SilverSack = 0x1EBE48, // R0.5
    GoldSack = 0x1EBE47 // R0.5
}

public enum AID : uint
{
    Rush = 44269 // VaultYan->location, 4.0s cast, width 8 rect charge
}

sealed class Rush(BossModule module) : Components.ChargeAOEs(module, (uint)AID.Rush, 4f);

sealed class CollectSacks(BossModule module) : Components.GenericTowers(module)
{
    private readonly List<Actor> goldsacks = module.Enemies((uint)OID.GoldSack);
    private readonly List<Actor> silversacks = module.Enemies((uint)OID.SilverSack);
    private readonly Rush _aoe = module.FindComponent<Rush>()!;

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is (uint)OID.SilverSack or (uint)OID.GoldSack)
        {
            Towers.Add(new(actor.Position, 1f, 1, 1, default, WorldState.FutureTime(10d)));
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID is (uint)OID.SilverSack or (uint)OID.GoldSack)
        {
            var towers = CollectionsMarshal.AsSpan(Towers);
            var len = towers.Length;
            var pos = actor.Position;
            for (var i = 0; i < len; ++i)
            {
                ref var t = ref towers[i];
                if (t.Position == pos)
                {
                    Towers.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        Actor? closest = null;
        var minDistSq = float.MaxValue;

        var aoes = CollectionsMarshal.AsSpan(_aoe.Casters);

        bool InAOE(WPos pos, ReadOnlySpan<Components.GenericAOEs.AOEInstance> aoes)
        {
            var len = aoes.Length;
            for (var i = 0; i < len; ++i)
            {
                ref readonly var aoe = ref aoes[i];
                if (aoes[i].Check(pos))
                {
                    return true;
                }
            }
            return false;
        }

        void TryUpdateClosest(Actor sack, float weight, ReadOnlySpan<Components.GenericAOEs.AOEInstance> aoes)
        {
            var pos = sack.Position;
            var distSq = (actor.Position - pos).LengthSq() * weight;
            if (distSq < minDistSq && !InAOE(pos, aoes))
            {
                minDistSq = distSq;
                closest = sack;
            }
        }

        var countG = goldsacks.Count;
        for (var i = 0; i < countG; ++i)
        {
            TryUpdateClosest(goldsacks[i], 1f, aoes);
        }

        var countS = silversacks.Count;
        for (var i = 0; i < countS; ++i)
        {
            TryUpdateClosest(silversacks[i], 3f, aoes); // gold worth 3x silver
        }

        if (closest != null)
        {
            hints.GoalZones.Add(AIHints.GoalSingleTarget(closest, 0.5f, 10f));
        }
    }
}

sealed class VaultYanStates : StateMachineBuilder
{
    public VaultYanStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Rush>()
            .ActivateOnEnter<CollectSacks>()
            .Raw.Update = () => AllDestroyed(VaultYan.Sacks);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", PrimaryActorOID = (uint)OID.SilverSack, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1060u, NameID = 14046u, Category = BossModuleInfo.Category.TreasureHunt, Expansion = BossModuleInfo.Expansion.Dawntrail, SortOrder = 8)]
public sealed class VaultYan : SharedBoundsBoss
{
    public VaultYan(WorldState ws, Actor primary) : base(ws, primary)
    {
        goldsacks = Enemies((uint)OID.GoldSack);
    }

    public static readonly uint[] Sacks = [(uint)OID.SilverSack, (uint)OID.GoldSack];
    private readonly List<Actor> goldsacks;

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(goldsacks, Colors.Danger, true);
    }

    protected override bool CheckPull() => true;
}
