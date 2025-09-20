namespace BossMod.Dawntrail.Trial.T05Necron;

sealed class Wipe(BossModule module) : BossComponent(module)
{
    public bool Wiped;
    public override bool KeepOnPhaseChange => true;

    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        if (updateID == 0x80000029)
        {
            Wiped = true;
        }
    }
}

sealed class DarknessOfEternity(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.DarknessOfEternityVisual, (uint)AID.DarknessOfEternity, 6.4d);
sealed class FearOfDeathAOE2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FearOfDeathAOE2, 3f);
sealed class FearOfDeathGrandCross(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.FearOfDeath, (uint)AID.GrandCross, (uint)AID.GrandCrossProximity]);
sealed class BlueShockwave(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(100f, 50f.Degrees()), (uint)IconID.BlueShockwave, (uint)AID.BlueShockwave, 7.2d)
{
    public override void Update()
    {
        if (CurrentBaits.Count != 0)
        {
            ref var b = ref CurrentBaits.Ref(0);
            var t = b.Target;
            if (t.IsDead || t.PosRot.Y < -100f) // target is dead or teleported into prison, not sure if new target gets selected, but it would probably be a random one anyway
            {
                CurrentBaits.Clear();
            }
        }
    }
}

public abstract class Necron(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, new ArenaBoundsRect(18f, 15f))
{
    public static readonly WPos ArenaCenter = new(100f, 100f);
    public static readonly ArenaBoundsCustom CircleArena = new([new Polygon(ArenaCenter, 9f, 32)]);
    public static readonly ArenaBoundsCustom SplitArena = new([new Rectangle(ArenaCenter, 18f, 15f)], [new Rectangle(ArenaCenter, 6f, 15f)]);
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", PrimaryActorOID = (uint)OID.Necron, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1061u, NameID = 14093u, Category = BossModuleInfo.Category.Trial, Expansion = BossModuleInfo.Expansion.Dawntrail, SortOrder = 1)]
public sealed class T05Necron(WorldState ws, Actor primary) : Necron(ws, primary)
{
    private static readonly uint[] hands = [(uint)OID.IcyHands2, (uint)OID.IcyHands3, (uint)OID.IcyHands4, (uint)OID.IcyHands5, (uint)OID.IcyHands6];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(this, hands);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.Necron => 0,
                _ => 1
            };
        }
    }
}
