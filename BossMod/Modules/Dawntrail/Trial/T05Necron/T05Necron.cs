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
// tank swap: two cone hits ~4.1s apart, the second one on whoever holds aggro after the first (matches upstream)
// previously the bait was cleared after the first hit, so the second hit was never shown
sealed class BlueShockwave(BossModule module) : Components.GenericBaitAway(module, (uint)AID.BlueShockwave, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    private static readonly AOEShapeCone cone = new(100f, 50f.Degrees());
    private DateTime _lastHit;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.BlueShockwave)
        {
            NumCasts = 0;
            _lastHit = default;
            CurrentBaits.Clear();
            CurrentBaits.Add(new(Module.PrimaryActor, actor, cone, WorldState.FutureTime(7.2d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID != WatchedAction || CurrentBaits.Count == 0)
        {
            return;
        }
        if (_lastHit != default && WorldState.CurrentTime < _lastHit.AddSeconds(0.5d))
        {
            return; // same hit reported more than once
        }
        _lastHit = WorldState.CurrentTime;
        if (++NumCasts >= 2)
        {
            CurrentBaits.Clear();
            return;
        }
        ref var b = ref CurrentBaits.Ref(0);
        b.Activation = WorldState.FutureTime(4.1d);
        b.Forbidden.Set(Raid.FindSlot(b.Target.InstanceID)); // the first target should not take the second hit
    }

    public override void Update()
    {
        if (CurrentBaits.Count == 0)
        {
            return;
        }
        ref var b = ref CurrentBaits.Ref(0);
        if (NumCasts == 1 && WorldState.Actors.Find(Module.PrimaryActor.TargetID) is Actor t)
        {
            b.Target = t; // second hit follows the boss target
        }
        var target = b.Target;
        if (target.IsDead || target.PosRot.Y < -100f) // target is dead or teleported into prison, not sure if new target gets selected, but it would probably be a random one anyway
        {
            CurrentBaits.Clear();
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
