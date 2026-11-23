namespace BossMod.Global.CrucibleOfTheUnbroken.SecondBoard.YoungerTablitaurPiece;

public enum OID : uint
{
    YoungerTablitaurPiece = 0x4C60,
    ElderTablitaurPiece = 0x4C5F, // R3.600, x1
    Gen = 0x4E00, // R1.000, x2
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 50216, // ElderTablitaurPiece/YoungerTablitaurPiece->player, no cast, single-target
    Teleport = 48188, // YoungerTablitaurPiece/ElderTablitaurPiece->location, no cast, single-target

    TonzeSwipe1000Visual = 48189, // ElderTablitaurPiece->self, 6.5+0.5s cast, single-target
    TonzeSwipe1000 = 48190, // Helper->self, 7.0s cast, range 60 width 60 rect
    TonzeSwipe1000VisualLong = 48191, // YoungerTablitaurPiece->self, 9.5+0.5s cast, single-target
    TonzeSwipe1000Long = 48192, // Helper->self, 10.0s cast, range 60 width 60 rect
    TonzeSwing1111Visual = 48193, // ElderTablitaurPiece->self, 7.0+1.0s cast, single-target
    TonzeSwing1111 = 48194, // Helper->self, 8.0s cast, range 23 circle
    TonzeSwing1111VisualLong = 48195, // YoungerTablitaurPiece->self, 13.0+1.0s cast, single-target
    TonzeSwing1111Long = 48196, // Helper->self, 14.0s cast, range 23 circle

    TonzeStomp10Teleport = 48197, // YoungerTablitaurPiece->location, 6.3+0.7s cast, single-target
    Shockwave = 48199, // Helper->self, 7.0s cast, range ?-60 donut
    TonzeStomp10 = 48198, // Helper->self, 7.0s cast, range 5 circle

    TonzeStomp10TeleportLong = 48200, // ElderTablitaurPiece->location, 9.8+0.7s cast, single-target
    ShockwaveLong = 48202, // Helper->self, 10.5s cast, range ?-60 donut
    TonzeStomp10Long = 48201, // Helper->self, 10.5s cast, range 5 circle

    EndlessSwingVisual = 48205, // YoungerTablitaurPiece/ElderTablitaurPiece->self, 5.0+1.0s cast, single-target
    EndlessSwingFirst = 48206, // Helper->YoungerTablitaurPiece/ElderTablitaurPiece, 6.0s cast, range 8 circle
    EndlessSwingRest = 48207, // Helper->YoungerTablitaurPiece/ElderTablitaurPiece, no cast, range 8 circle

    EndlessSwipesYounger = 48208, // YoungerTablitaurPiece->self, 5.0s cast, single-target
    EndlessSwipesElder = 48209, // ElderTablitaurPiece->self, 5.0s cast, single-target
    EndlessSwipes1 = 48210, // Helper->self, 5.2s cast, range 40 60.000-degree cone
    EndlessSwipes2 = 48211, // YoungerTablitaurPiece->self, no cast, single-target
    EndlessSwipes3 = 48212, // Helper->self, 0.5s cast, range 40 60-degree cone

    RallyingCheer = 48203, // YoungerTablitaurPiece->ElderTablitaurPiece, 7.0s cast, single-target
    TonzeSlash100 = 48204, // ElderTablitaurPiece->self/player, 9.0s cast, range 65 width 8 rect

    // Enrage - if one dies before the other too early
    DisorientingGroan = 48213, // ElderTablitaurPiece/YoungerTablitaurPiece->self, no cast, single-target
    EndlessSlashesVisual1 = 48214, // ElderTablitaurPiece/YoungerTablitaurPiece->self, 9.0s cast, single-target
    EndlessSlashesVisual2 = 48215, // ElderTablitaurPiece/YoungerTablitaurPiece->self, no cast, single-target
    EndlessSlashes = 48216 // Helper->self, no cast, range 80 width 70 rect
}

public enum IconID : uint
{
    TankBuster = 412, // player->self
    TurnRight = 167, // YoungerTablitaurPiece->self
    TurnLeft = 168, // ElderTablitaurPiece->self
}

public enum TetherID : uint
{
    KnockbackTether = 54, // YoungerTablitaurPiece/4C5F->4E00
    TankBusterTether = 260, // YoungerTablitaurPiece->ElderTablitaurPiece
    UnknownTether = 17, // 4C5F->player - most likely the target tether for the spin attack that follows the player around
    _Gen_Tether_chn_dark001f = 1, // ElderTablitaurPiece->player
}

public enum SID : uint
{
    DamageUpEnrage1 = 1225, // YoungerTablitaurPiece->ElderTablitaurPiece, extra=0x0
    DamageUpEnrage2 = 2550 // YoungerTablitaurPiece->YoungerTablitaurPiece, extra=0x1/0x2
}

sealed class TonzeSlash100(YoungerTablitaurPiece module) : Components.BaitAwayIcon(module, new AOEShapeRect(65.0f, 4.0f), (uint)IconID.TankBuster,
    (uint)AID.TonzeSlash100, 9.1d, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    private readonly YoungerTablitaurPiece bossModule = module;

    public override Actor? BaitSource(Actor target) => bossModule.ElderTablitaur;
}

sealed class RallyingCheer(BossModule module) : Components.CastInterruptHint(module, (uint)AID.RallyingCheer, showNameInHint: true);
sealed class EndlessSlashes(BossModule module) : Components.RaidwideCast(module, (uint)AID.EndlessSlashesVisual1, "Enrage: 8x raidwide");

sealed class TonzeSwipe1000 : Components.SimpleAOEGroups
{
    public TonzeSwipe1000(BossModule module) : base(module, [(uint)AID.TonzeSwipe1000, (uint)AID.TonzeSwipe1000Long], new AOEShapeRect(60f, 30f),
        expectedNumCasters: 2)
    {
        MaxDangerColor = 1;
        MaxRisky = 1;
    }
}

sealed class TonzeSwing1111 : Components.SimpleAOEGroups
{
    public TonzeSwing1111(BossModule module) : base(module, [(uint)AID.TonzeSwing1111, (uint)AID.TonzeSwing1111Long], 23f, expectedNumCasters: 2)
    {
        MaxDangerColor = 1;
        MaxRisky = 1;
    }
}

sealed class Shockwave(BossModule module) : Components.SimpleKnockbackGroups(module, [(uint)AID.Shockwave, (uint)AID.ShockwaveLong], 20f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        SortHelpers.SortKnockbacksByActivation(Casters);
    }
}

sealed class TonzeStomp10 : Components.SimpleAOEGroups
{
    public TonzeStomp10(BossModule module) : base(module, [(uint)AID.TonzeStomp10, (uint)AID.TonzeStomp10Long], 5f, expectedNumCasters: 2)
    {
        MaxDangerColor = 1;
        MaxRisky = 1;
    }
}

sealed class EndlessSwing(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeCapsule shape = new(8f, 5f);
    private DateTime activation;
    private Actor? source;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (source is Actor caster)
        {
            var pos = caster.Position;
            var angle = Angle.FromDirection(actor.Position - pos);
            AOEInstance[] aoe = [new(shape, pos.Quantized(), angle, activation)];
            return aoe;
        }
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.EndlessSwingVisual)
        {
            source = caster;
            activation = Module.CastFinishAt(spell, 2.1d);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.EndlessSwipes1 or (uint)AID.EndlessSwipes3)
        {
            if (++NumCasts == 13)
            {
                source = null;
                NumCasts = 0;
            }
        }
    }

    public override void OnActorDeath(Actor actor)
    {
        if (actor.OID is (uint)OID.YoungerTablitaurPiece or (uint)OID.ElderTablitaurPiece)
        {
            source = null;
        }
    }
}

sealed class EndlessSwipes(BossModule module) : Components.GenericRotatingAOE(module)
{
    private ActorCastInfo? spellInfo;
    private Actor? source;
    private Angle increment;
    private readonly AOEShapeCone shape = new(40f, 30f.Degrees());

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID is (uint)IconID.TurnLeft or (uint)IconID.TurnRight)
        {
            increment = iconID == (uint)IconID.TurnLeft ? 30f.Degrees() : -30f.Degrees();
            InitIfReady();
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        // The spell for the rotation and starting locXZ
        if (spell.Action.ID is var id && id == (uint)AID.EndlessSwipes1)
        {
            spellInfo = spell;
            InitIfReady();
        }
        // The spell for which boss is actually performing the spell - it can be either one
        else if (id is (uint)AID.EndlessSwipesYounger or (uint)AID.EndlessSwipesElder)
        {
            source = caster;
            InitIfReady();
        }
    }

    private void InitIfReady()
    {
        if (spellInfo != null && increment != default && source != null)
        {
            Sequences.Add(new(shape, spellInfo.LocXZ, spellInfo.Rotation, increment, Module.CastFinishAt(spellInfo), 1.5d, 13, 5, actorID: source.InstanceID));
            spellInfo = null;
            increment = default;
            source = null;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.EndlessSwipes1 or (uint)AID.EndlessSwipes3)
        {
            if (Sequences.Count > 0)
            {
                AdvanceSequence(0, WorldState.CurrentTime);
            }
        }
    }

    // If the caster of swipes dies, then we have to clear any left over aoes otherwise they will not get removed - it can be either boss
    public override void OnActorDeath(Actor actor)
    {
        if (actor.OID is (uint)OID.YoungerTablitaurPiece or (uint)OID.ElderTablitaurPiece)
        {
            if (Sequences.Count > 0 && actor == WorldState.Actors.Find(Sequences[0].ActorID))
            {
                Sequences.Clear();
            }
        }
    }
}

// Consider changing to % ratio instead
sealed class HpChecker(YoungerTablitaurPiece module) : BossComponent(module)
{
    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        var elder = module.ElderTablitaur;
        if (elder == null || elder.IsDead)
        {
            return;
        }

        var younger = Module.PrimaryActor;
        var difference = (int)(younger.HPMP.CurHP - elder.HPMP.CurHP) * 100f / younger.HPMP.MaxHP;
        hints.Add($"Younger Tablitaur HP difference: {difference:f1}%");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var elder = module.ElderTablitaur;
        if (elder == null || elder.IsDead)
        {
            return;
        }

        var younger = Module.PrimaryActor;
        var difference = (int)(younger.HPMP.CurHP - elder.HPMP.CurHP) * 100f / younger.HPMP.MaxHP;

        for (var i = 0; i < hints.PotentialTargets.Count; i++)
        {
            var target = hints.PotentialTargets[i];

            // Younger is always set to priority 1
            if (target.Actor == younger)
            {
                target.Priority = 1;
            }

            // Elder is set to 0 when younger is ahead otherwise if Elder is ahead it will set to 2 to prioritise elder instead
            // if both are within the hp range then we don't care and treat both as the same
            if (target.Actor == elder)
            {
                target.Priority = difference > 30.0f ? 0 : difference < -30.0f ? 2 : 1;
            }
        }
    }
}

sealed class YoungerTablitaurPieceStates : StateMachineBuilder
{
    private readonly YoungerTablitaurPiece _module;

    public YoungerTablitaurPieceStates(YoungerTablitaurPiece module) : base(module)
    {
        _module = module;
        TrivialPhase()
            .ActivateOnEnter<RallyingCheer>()
            .ActivateOnEnter<EndlessSlashes>()
            .ActivateOnEnter<TonzeSwipe1000>()
            .ActivateOnEnter<TonzeSwing1111>()
            .ActivateOnEnter<Shockwave>()
            .ActivateOnEnter<TonzeStomp10>()
            .ActivateOnEnter<EndlessSwipes>()
            .ActivateOnEnter<EndlessSwing>()
            .ActivateOnEnter<TonzeSlash100>()
            .ActivateOnEnter<HpChecker>()
            .Raw.Update = () => _module.PrimaryActor.IsDeadOrDestroyed && _module.ElderTablitaur?.IsDeadOrDestroyed == true;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.YoungerTablitaurPiece, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1089u, NameID = 14556u, SortOrder = 5)]
[PreferredKinship(BST.Kinship.Soul, "Interrupt Rallying Cheer")]
[PreferredKinship(BST.Kinship.Wave, "Remove Elder Tablitaur's Damage Up if Rallying Cheer goes off")]
[PreferredKinship(BST.Kinship.Vile, "Ignore Shockwave's knockback")]
public sealed class YoungerTablitaurPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120f, 0f), new ArenaBoundsSquare(20f))
{
    public Actor? ElderTablitaur;

    protected override void UpdatePreModuleActivation()
    {
        ElderTablitaur ??= GetActor((uint)OID.ElderTablitaurPiece);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actor(ElderTablitaur);
    }

    private readonly string[] _prePullHints =
    [
        "Kill both at the same time!",
    ];

    public override string[] PrePullHints => _prePullHints;
}
