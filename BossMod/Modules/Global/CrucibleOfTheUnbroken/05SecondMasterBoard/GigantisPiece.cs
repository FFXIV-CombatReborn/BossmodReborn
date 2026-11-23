namespace BossMod.Global.CrucibleOfTheUnbroken.SecondMasterBoard.GigantisPiece;

public enum OID : uint
{
    GigantisPiece = 0x4D03,
    CyclopsPiece = 0x4D04, // R2.000, x0 (spawn during fight)
    CongealedLightning = 0x4D05, // R2.500-5.000, x0 (spawn during fight)
    CongealedLightningSmall = 0x4D06, // R0.500, x0 (spawn during fight)
    CongealedKindling = 0x4D07, // R2.500, x0 (spawn during fight)
    CongealedKindlingSmall = 0x4D08, // R0.500, x0 (spawn during fight)
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 50786, // GigantisPiece->player, no cast, single-target

    GiganticRageCircleBoss = 49358, // GigantisPiece->self, 5.0+1.0s cast, single-target
    GiganticRageCircle = 49359, // Helper->self, 6.0s cast, range 15 circle
    GiganticRageConeBoss = 49360, // GigantisPiece->self, 5.0+1.0s cast, single-target
    GiganticRageCone = 49361, // Helper->self, 6.0s cast, range 40 180.000-degree cone
    GiganticRageConeBoss1 = 49362, // GigantisPiece->self, 5.0+1.0s cast, single-target
    GiganticRageCone1 = 49363, // Helper->self, 6.0s cast, range 40 180.000-degree cone
    AftersparkTower = 49364, // Helper->location, 3.0s cast, range 2 circle
    Afterspark = 49365, // Helper->self, no cast, range 60 circle - happens if you miss the tower
    AfterburnAOE = 49366, // Helper->self, 3.0s cast, range 6 circle
    SmashingStampBoss = 49377, // GigantisPiece->self, 2.6+0.4s cast, single-target
    SmashingStamp = 49378, // Helper->self, 3.0s cast, range 40 width 8 rect
    SmashingStampBait = 49379, // Helper->self, 3.0s cast, range 7 width 4 rect

    // CyclopsPiece
    AutoAttackCyclopsPiece = 50398, // 4D04->player, no cast, single-target
    Glower = 49367, // 4D04->self, 4.0s cast, range 40 width 3 rect
    Camaraderie = 49368, // CyclopsPiece->GigantisPiece, 8.0s cast, single-target

    // Slimes
    RuptureFire = 49375, // 4D07->self, 7.0s cast, range 40 circle
    RuptureLightning = 49373, // 4D05->self, 7.0s cast, range 40 circle
    AutoAttackThunder = 48623, // 4D05/4D06->player, no cast, single-target
    AutoAttackFire = 48622, // 4D07/4D08->player, no cast, single-target
}

public enum SID : uint
{
    WeaponElement = 2056, // none->GigantisPiece, extra=0x499/0x49A - 0x499 - Lightning, 0x49A - Fire
    DamageUp = 2550, // none->4D05, extra=0x1
}

public enum IconID : uint
{
    SmashingStampIcon = 234, // player->self
}

sealed class Glower(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Glower, new AOEShapeRect(40f, 1.5f));
sealed class Camaraderie(BossModule module) : Components.CastInterruptHint(module, (uint)AID.Camaraderie, showNameInHint: true);

// Handles the bait and weapon's element

sealed class SmashingStampBait(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(7.0f, 2.0f), (uint)IconID.SmashingStampIcon,
    activationDelay: 6.1f)
{
    private readonly List<Actor> slimes = [];
    public bool weaponRecentlyChanged = false;

    public enum Element { NONE, LIGHTNING, FIRE }
    public Element weaponElement = Element.NONE;

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is (uint)OID.CongealedLightning or (uint)OID.CongealedKindling)
        {
            slimes.Add(actor);
            weaponRecentlyChanged = false;
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID is (uint)OID.CongealedLightning or (uint)OID.CongealedKindling)
        {
            slimes.Remove(actor);
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID is var id && id == (uint)SID.WeaponElement && status.Extra == 0x499)
        {
            weaponElement = Element.LIGHTNING;
            weaponRecentlyChanged = true;
        }
        else if (id == (uint)SID.WeaponElement && status.Extra == 0x49A)
        {
            weaponElement = Element.FIRE;
            weaponRecentlyChanged = true;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.SmashingStampIcon)
        {
            CurrentBaits.Add(new(Module.PrimaryActor, actor, Shape, WorldState.FutureTime(ActivationDelay)));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SmashingStampBait)
        {
            if (CurrentBaits.Count > 0)
            {
                CurrentBaits.RemoveAt(0);
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (CurrentBaits.Count == 0 || slimes.Count == 0)
        {
            return;
        }

        switch (weaponElement)
        {
            case Element.NONE:
                hints.Add("Hit any slime with the bait!", false);
                break;
            case Element.LIGHTNING:
                hints.Add("Hit the orange slime with the bait!", false);
                break;
            case Element.FIRE:
                hints.Add("Hit the purple slime with the bait!", false);
                break;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (CurrentBaits.Count == 0 || slimes.Count == 0)
        {
            return;
        }

        switch (weaponElement)
        {
            case Element.NONE:
                foreach (var slime in slimes)
                {
                    Arena.ZoneCircleOutline(slime.Position, 1f, Colors.Safe);
                }
                break;
            case Element.LIGHTNING:
                foreach (var slime in slimes)
                {
                    if (slime.OID == (uint)OID.CongealedKindling)
                    {
                        Arena.ZoneCircleOutline(slime.Position, 1f, Colors.Safe);
                    }
                }
                break;
            case Element.FIRE:
                foreach (var slime in slimes)
                {
                    if (slime.OID == (uint)OID.CongealedLightning)
                    {
                        Arena.ZoneCircleOutline(slime.Position, 1f, Colors.Safe);
                    }
                }
                break;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        var countSlime = slimes.Count;
        var countBaits = CurrentBaits.Count;
        if (countBaits == 0 || countSlime == 0 || !IsBaitTarget(actor))
        {
            return;
        }

        var currentBait = CurrentBaits[0];
        var activeSlimes = CollectionsMarshal.AsSpan(slimes);

        var targetSlime = weaponElement switch
        {
            Element.LIGHTNING => (uint)OID.CongealedKindling,
            Element.FIRE => (uint)OID.CongealedLightning,
            _ => (uint)0
        };

        List<ShapeDistance> circles = [];
        for (var i = 0; i < countSlime; i++)
        {
            ref var slime = ref activeSlimes[i];
            if (targetSlime == 0 || slime.OID == targetSlime)
            {
                circles.Add(new SDCircle(slime.Position, 1.0f));
            }
        }

        if (circles.Count == 0)
        {
            return;
        }

        hints.AddForbiddenZone(new SDInvertedUnion([.. circles]), currentBait.Activation);
    }
}

sealed class SmashingStamp(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SmashingStamp, new AOEShapeRect(40f, 4f));
sealed class SmashingStampBaitAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SmashingStampBait, new AOEShapeRect(7f, 2f)); // was keyed to the 40y SmashingStamp, so the bait rects never showed

sealed class GiganticRageCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GiganticRageCircle, 15f);
sealed class GiganticRageCone(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.GiganticRageCone, (uint)AID.GiganticRageCone1],
    new AOEShapeCone(40.0f, 90.0f.Degrees()));
sealed class AftersparkTower(BossModule module) : Components.CastTowers(module, (uint)AID.AftersparkTower, 2f);
sealed class AfterburnAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AfterburnAOE, 6f);
sealed class GiganticRageType(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true, onlyShowOutlines: true)
{
    private readonly SmashingStampBait? smashingStampBait = module.FindComponent<SmashingStampBait>();
    private readonly AOEShapeDonut donut = new(2f, 40f);
    private readonly AOEShapeCircle circle = new(6f);
    private bool active = false;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.GiganticRageCircle or (uint)AID.GiganticRageCone or (uint)AID.GiganticRageCone1)
        {
            active = true;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.GiganticRageCircle or (uint)AID.GiganticRageCone or (uint)AID.GiganticRageCone1)
        {
            active = false;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (smashingStampBait == null || smashingStampBait.weaponElement == SmashingStampBait.Element.NONE || !active)
        {
            return;
        }

        switch (smashingStampBait.weaponElement)
        {
            case SmashingStampBait.Element.LIGHTNING:
                hints.Add("You will drop a tower!", false);
                break;
            case SmashingStampBait.Element.FIRE:
                hints.Add("You will drop a puddle!", false);
                break;
        }
    }

    public override void Update()
    {
        base.Update();

        CurrentBaits.Clear();

        if (smashingStampBait == null || smashingStampBait.weaponElement == SmashingStampBait.Element.NONE || !active)
        {
            return;
        }

        if (Raid.Player() is not Actor player)
            return;

        switch (smashingStampBait.weaponElement)
        {
            case SmashingStampBait.Element.LIGHTNING:
                CurrentBaits.Add(new(Module.PrimaryActor, player, donut));
                break;
            case SmashingStampBait.Element.FIRE:
                CurrentBaits.Add(new(Module.PrimaryActor, player, circle));
                break;
        }
    }
}

sealed class SlimesInterrupt(BossModule module) : Components.CastInterruptHint(module, default, showNameInHint: true)
{
    private readonly SmashingStampBait smashingStampBait = module.FindComponent<SmashingStampBait>()!;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        // Case: When we have no weapon element & somehow we missed the slime
        var element = smashingStampBait.weaponElement;
        if (spell.Action.ID is var id && id is (uint)AID.RuptureLightning or (uint)AID.RuptureFire && element == SmashingStampBait.Element.NONE)
        {
            Casters.Add(caster);
            UpdateHint();
        }
        else if (id == (uint)AID.RuptureLightning && element == SmashingStampBait.Element.LIGHTNING)
        {
            Casters.Add(caster);
            UpdateHint();
        }
        else if (id == (uint)AID.RuptureFire && element == SmashingStampBait.Element.FIRE)
        {
            Casters.Add(caster);
            UpdateHint();
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.RuptureLightning or (uint)AID.RuptureFire)
        {
            if (smashingStampBait == null)
            {
                return;
            }

            Casters.Remove(caster);
            UpdateHint();
        }
    }

    private void UpdateHint()
    {
        var actionStr = "Interrupt";
        var nameStr = Casters.Count == 1 ? Casters[0].OID == (uint)OID.CongealedKindling ? " Orange slime" : " Purple slime" : "";
        Hint = $"{actionStr}{nameStr}!";
    }
}

sealed class GigantisPieceStates : StateMachineBuilder
{
    public GigantisPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Camaraderie>()
            .ActivateOnEnter<SmashingStampBait>()
            .ActivateOnEnter<SmashingStampBaitAOE>()
            .ActivateOnEnter<SmashingStamp>()
            .ActivateOnEnter<GiganticRageCircle>()
            .ActivateOnEnter<GiganticRageCone>()
            .ActivateOnEnter<AftersparkTower>()
            .ActivateOnEnter<AfterburnAOE>()
            .ActivateOnEnter<GiganticRageType>()
            .ActivateOnEnter<Glower>()
            .ActivateOnEnter<SlimesInterrupt>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.GigantisPiece, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1092u, NameID = 14670u, SortOrder = 10)]
[PreferredKinship(BST.Kinship.Soul, "Interrupt Cyclops Piece's Camaraderie and the slimes' Rupture")]
public sealed class GigantisPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120f, -420f), new ArenaBoundsCircle(20f))
{
    private SmashingStampBait? smashing;

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        smashing ??= FindComponent<SmashingStampBait>()!;
        var element = smashing.weaponElement;

        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.CyclopsPiece => 4,
                (uint)OID.CongealedLightning => SlimePriority(element, false),
                (uint)OID.CongealedKindling => SlimePriority(element, true),
                (uint)OID.CongealedLightningSmall => 2,
                (uint)OID.CongealedKindlingSmall => 2,
                (uint)OID.GigantisPiece => 1,
                _ => 0
            };
        }
    }

    private int SlimePriority(SmashingStampBait.Element element, bool isFire)
    {
        if (smashing!.weaponRecentlyChanged)
        {
            return 3;
        }

        return element switch
        {
            SmashingStampBait.Element.NONE => 3,
            SmashingStampBait.Element.FIRE => isFire ? 3 : AIHints.Enemy.PriorityForbidden,
            SmashingStampBait.Element.LIGHTNING => isFire ? AIHints.Enemy.PriorityForbidden : 3,
            _ => 2
        };
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.CyclopsPiece));
        Arena.Actors(Enemies((uint)OID.CongealedLightning));
        Arena.Actors(Enemies((uint)OID.CongealedKindling));
        Arena.Actors(Enemies((uint)OID.CongealedLightningSmall));
        Arena.Actors(Enemies((uint)OID.CongealedKindlingSmall));
    }

    private readonly string[] _prePullHints = [
        "Fight kill priority: CyclopsPiece -> Slimes (whichever one we are not killing with bait) -> small slimes / boss",
        "When the boss crushes a slime it will absorb its element and have additional effects after a Gigantic Rage cast ends. Lightning club will leave a tower under you (stay) after a Gigantic Rage, fire club will leave an aoe under you (move) after a Gigantic Rage",
        "Smashing Stamp: Line aoe in front, there is a smaller red rectangle in the aoe near the boss that will kill a slime in it and absorb its element. Avoid hitting a slime with the same element as the club, or the slime will grow large instead of dying",
        "Rapture: Interrupt - If there are 2 slimes, have the boss crush the one with the opposite element of the club, then interrupt the other one",
        "Camaraderie: Interrupt or it will heal the boss 5% max HP"
    ];

    public override string[] PrePullHints => _prePullHints;
}
