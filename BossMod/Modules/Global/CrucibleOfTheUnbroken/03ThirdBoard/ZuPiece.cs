namespace BossMod.Global.CrucibleOfTheUnbroken.ThirdBoard.ZuPiece;

public enum OID : uint {
    ZuPiece = 0x4C96,
    PulletPieceEgg = 0x4C9A, // R0.500, x8
    CockerelPieceEgg = 0x4C99, // R0.500, x8
    PulletPiece = 0x4C98, // R0.400, x0 (spawn during fight)
    CockerelPiece = 0x4C97, // R0.400, x0 (spawn during fight)
    Helper = 0x233C
}

public enum AID : uint {
    AutoAttack = 49680, // ZuPiece->player, no cast, single-target
    AutoAttackBreakbeak = 48488, // 4C97->player, no cast, single-target
    CausticVomit = 48489, // 4C98->player, 13.0s cast, single-target
    Hatch = 48487, // 4C9A/4C99->self, 10.0s cast, single-target
    CrossbreezeIcon = 48491, // ZuPiece->self, 10.0s cast, single-target
    Crossbreeze = 48492, // Helper->self, 3.0s cast, range 50 width 8 cross
    FlyingFrenzy = 50465, // ZuPiece->self, 8.0s cast, single-target
    FlyingFrenzyAOE = 48490, // ZuPiece->players, no cast, range 6 circle

    AiryPursuit = 48500, // ZuPiece->self, 8.0s cast, single-target
    AiryPursuitAOE = 48501, // Helper->location, 3.0s cast, range 6 circle
    AiryPursuitTeleport = 48502, // Helper->location, no cast, range 6 circle

    Featherglide = 48495, // ZuPiece->player, no cast, width 3 rect charge
    FerociousForeCarve = 48493, // ZuPiece->self, 6.0s cast, single-target
    ForeCarveVisual = 48496, // ZuPiece->self, 0.5+0.7s cast, single-target
    ForeCarve = 48497, // Helper->self, 1.0s cast, range 15 180.000-degree cone
    RampagingRearCarve = 48494, // ZuPiece->self, 6.0s cast, single-target
    RearCarveVisual = 48498, // ZuPiece->self, 0.5+0.7s cast, single-target
    RearCarve = 48499, // Helper->self, 1.0s cast, range 15 180.000-degree cone
}

public enum SID : uint {
    BroodRage = 5433, // none->ZuPiece, extra=0x1/0x2/0x3/0x4/0x5/0x6/0x7/0x8/0x9/0xA/0xB/0xD/0xE/0xF/0x10
}

public enum IconID : uint {
    EggExclamation = 569, // 4C9A/4C99->self
    Crossbreeze = 686, // player->self
    TankBuster = 465, // player->self
    AiryPursuit = 197, // player->self
}

public enum TetherID : uint {
    Carve = 57, // ZuPiece->player
    CarveStretched = 1, // ZuPiece->player
}

sealed class Crossbreeze(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Crossbreeze, new AOEShapeCross(50f, 4f));
sealed class AiryPursuit(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AiryPursuitAOE, 6f);
sealed class CarveTether(BossModule module) : Components.StretchTetherDuo(module, 16f, 5d);

sealed class CrossbreezeBait(ZuPiece module) : Components.BaitAwayIcon(module, new AOEShapeCross(50f, 4f), (uint)IconID.Crossbreeze,
    (uint)AID.CrossbreezeIcon, 8.1d, centerAtTarget: true, customRotation: Angle.AnglesCardinals[1]) {
    const float hitboxradius = 0.5f;
    private readonly AOEShapeCross crossPlusEggHitbox = new(50f, 4f + hitboxradius);
    private readonly List<Actor> pulletPieceEggs = module.PulletPieceEgg;
    private readonly List<Actor> cockarelPieceEggs = module.CockarelPieceEgg;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.Crossbreeze) {
            if (CurrentBaits.Count > 0) {
                CurrentBaits.Clear();
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        base.DrawArenaForeground(pcSlot, pc);

        if (!IsBaitTarget(pc) || CurrentBaits.Count == 0) {
            return;
        }

        ref var bait = ref CurrentBaits.Ref(0);

        var countP = pulletPieceEggs.Count;
        var countC = cockarelPieceEggs.Count;
        var baittargetPos = bait.Target.Position;

        for (var i = 0; i < countP; ++i) {
            var eggPos = pulletPieceEggs[i].Position;
            var onHitbox = crossPlusEggHitbox.Check(eggPos, baittargetPos, Angle.AnglesCardinals[1]);
            Arena.ZoneCircleOutline(eggPos, hitboxradius, onHitbox ? Colors.Danger : Colors.Border);
        }

        for (var i = 0; i < countC; ++i) {
            var eggPos = cockarelPieceEggs[i].Position;
            var onHitbox = crossPlusEggHitbox.Check(eggPos, baittargetPos, Angle.AnglesCardinals[1]);
            Arena.ZoneCircleOutline(eggPos, hitboxradius, onHitbox ? Colors.Danger : Colors.Border);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints) {
        if (!IsBaitTarget(actor) || CurrentBaits.Count == 0) {
            return;
        }

        hints.Add("Avoid intersecting egg hitboxes!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        AllowPetTargets = false;
        base.AddAIHints(slot, actor, assignment, hints);

        if (!IsBaitTarget(actor) || CurrentBaits.Count == 0) {
            return;
        }

        var countP = pulletPieceEggs.Count;
        var countC = cockarelPieceEggs.Count;
        for (var i = 0; i < countP; ++i) {
            hints.TemporaryObstacles.Add(new SDCross(pulletPieceEggs[i].Position, default, 50f, 4f + 0.6f));
        }

        for (var i = 0; i < countC; ++i) {
            hints.TemporaryObstacles.Add(new SDCross(cockarelPieceEggs[i].Position, default, 50f, 4f + 0.6f));
        }
    }
}

sealed class FlyingFrenzy(ZuPiece module) : Components.BaitAwayIcon(module, baitRadius, (uint)IconID.TankBuster, (uint)AID.FlyingFrenzyAOE,
    activationDelay: 8.1d, tankbuster: true) {
    private const float baitRadius = 6f;
    const float hitboxradius = 0.5f;
    const float reach = baitRadius + hitboxradius;
    private readonly List<Actor> pulletPieceEggs = module.PulletPieceEgg;
    private readonly List<Actor> cockarelPieceEggs = module.CockarelPieceEgg;

    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        base.DrawArenaForeground(pcSlot, pc);

        if (!IsBaitTarget(pc) || CurrentBaits.Count == 0) {
            return;
        }

        ref var bait = ref CurrentBaits.Ref(0);

        var countP = pulletPieceEggs.Count;
        var countC = cockarelPieceEggs.Count;
        var baittargetPos = bait.Target.Position;

        const float reachSq = reach * reach;
        for (var i = 0; i < countP; ++i) {
            var eggPos = pulletPieceEggs[i].Position;
            var onHitbox = (eggPos - baittargetPos).LengthSq() <= reachSq;
            Arena.ZoneCircleOutline(eggPos, hitboxradius, onHitbox ? Colors.Danger : Colors.Border);
        }

        for (var i = 0; i < countC; ++i) {
            var eggPos = cockarelPieceEggs[i].Position;
            var onHitbox = (eggPos - baittargetPos).LengthSq() <= reachSq;
            Arena.ZoneCircleOutline(eggPos, hitboxradius, onHitbox ? Colors.Danger : Colors.Border);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints) {
        if (!IsBaitTarget(actor) || CurrentBaits.Count == 0) {
            return;
        }

        hints.Add("Avoid intersecting egg hitboxes!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        AllowPetTargets = true;
        base.AddAIHints(slot, actor, assignment, hints);

        if (!IsBaitTarget(actor) || CurrentBaits.Count == 0) {
            return;
        }

        var countP = pulletPieceEggs.Count;
        var countC = cockarelPieceEggs.Count;
        for (var i = 0; i < countP; ++i) {
            hints.TemporaryObstacles.Add(new SDCircle(pulletPieceEggs[i].Position, reach));
        }

        for (var i = 0; i < countC; ++i) {
            hints.TemporaryObstacles.Add(new SDCircle(cockarelPieceEggs[i].Position, reach));
        }
    }
}

sealed class AiryPursuitPuddles(ZuPiece module) : Components.StandardChasingAOEs(module, baitRadius, (uint)AID.AiryPursuitAOE, (uint)AID.AiryPursuitTeleport,
    3.5f, 1d, 6, icon: (uint)IconID.AiryPursuit) {
    private const float baitRadius = 6f;
    const float hitboxradius = 0.5f;
    const float reach = baitRadius + hitboxradius;
    private readonly List<Actor> pulletPieceEggs = module.PulletPieceEgg;
    private readonly List<Actor> cockarelPieceEggs = module.CockarelPieceEgg;

    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        base.DrawArenaForeground(pcSlot, pc);

        if (!IsChaserTarget(pc) || Chasers.Count == 0) {
            return;
        }

        var bait = Chasers[0];

        var countP = pulletPieceEggs.Count;
        var countC = cockarelPieceEggs.Count;
        var baittargetPos = bait.PredictedPosition();

        const float reachSq = reach * reach;
        for (var i = 0; i < countP; ++i) {
            var eggPos = pulletPieceEggs[i].Position;
            var onHitbox = (eggPos - baittargetPos).LengthSq() <= reachSq;
            Arena.ZoneCircleOutline(eggPos, hitboxradius, onHitbox ? Colors.Danger : Colors.Border);
        }

        for (var i = 0; i < countC; ++i) {
            var eggPos = cockarelPieceEggs[i].Position;
            var onHitbox = (eggPos - baittargetPos).LengthSq() <= reachSq;
            Arena.ZoneCircleOutline(eggPos, hitboxradius, onHitbox ? Colors.Danger : Colors.Border);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints) {
        if (!IsChaserTarget(actor) || Chasers.Count == 0) {
            return;
        }

        hints.Add("Avoid intersecting egg hitboxes!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        base.AddAIHints(slot, actor, assignment, hints);

        if (!IsChaserTarget(actor) || Chasers.Count == 0) {
            return;
        }

        var countP = pulletPieceEggs.Count;
        var countC = cockarelPieceEggs.Count;
        for (var i = 0; i < countP; ++i) {
            hints.TemporaryObstacles.Add(new SDCircle(pulletPieceEggs[i].Position, reach));
        }

        for (var i = 0; i < countC; ++i) {
            hints.TemporaryObstacles.Add(new SDCircle(cockarelPieceEggs[i].Position, reach));
        }
    }
}

sealed class AiryPursuitBait(ZuPiece module) : Components.BaitAwayIcon(module, baitRadius, (uint)IconID.AiryPursuit) {
    private const float baitRadius = 6f;
    const float hitboxradius = 0.5f;
    const float reach = baitRadius + hitboxradius;
    private readonly List<Actor> pulletPieceEggs = module.PulletPieceEgg;
    private readonly List<Actor> cockarelPieceEggs = module.CockarelPieceEgg;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID is (uint)AID.AiryPursuitAOE or (uint)AID.AiryPursuitTeleport) {
            CurrentBaits.Clear();
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        base.DrawArenaForeground(pcSlot, pc);

        if (!IsBaitTarget(pc) || CurrentBaits.Count == 0) {
            return;
        }

        ref var bait = ref CurrentBaits.Ref(0);

        var countP = pulletPieceEggs.Count;
        var countC = cockarelPieceEggs.Count;
        var baittargetPos = bait.Target.Position;

        const float reachSq = reach * reach;
        for (var i = 0; i < countP; ++i) {
            var eggPos = pulletPieceEggs[i].Position;
            var onHitbox = (eggPos - baittargetPos).LengthSq() <= reachSq;
            Arena.ZoneCircleOutline(eggPos, hitboxradius, onHitbox ? Colors.Danger : Colors.Border);
        }

        for (var i = 0; i < countC; ++i) {
            var eggPos = cockarelPieceEggs[i].Position;
            var onHitbox = (eggPos - baittargetPos).LengthSq() <= reachSq;
            Arena.ZoneCircleOutline(eggPos, hitboxradius, onHitbox ? Colors.Danger : Colors.Border);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints) {
        if (!IsBaitTarget(actor) || CurrentBaits.Count == 0) {
            return;
        }

        hints.Add("Avoid intersecting egg hitboxes!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        AllowPetTargets = false;
        base.AddAIHints(slot, actor, assignment, hints);

        if (!IsBaitTarget(actor) || CurrentBaits.Count == 0) {
            return;
        }

        var countP = pulletPieceEggs.Count;
        var countC = cockarelPieceEggs.Count;
        for (var i = 0; i < countP; ++i) {
            hints.TemporaryObstacles.Add(new SDCircle(pulletPieceEggs[i].Position, reach));
        }

        for (var i = 0; i < countC; ++i) {
            hints.TemporaryObstacles.Add(new SDCircle(cockarelPieceEggs[i].Position, reach));
        }
    }
}

sealed class Carve(BossModule module) : Components.GenericAOEs(module) {
    private readonly AOEShapeCone shape = new(15f, 90f.Degrees());
    private readonly List<AOEInstance> aoes = [];
    private Angle? rotation = default;
    private bool aoeLocked = true;
    private WPos startPosition = default;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID is (uint)AID.FerociousForeCarve or (uint)AID.RampagingRearCarve) {
            rotation = spell.Action.ID switch {
                (uint)AID.FerociousForeCarve => default,
                (uint)AID.RampagingRearCarve => 180f.Degrees(),
                _ => default
            };

            startPosition = caster.Position;
        }

        if (spell.Action.ID is (uint)AID.ForeCarve or (uint)AID.RearCarve) {
            aoeLocked = true;

            // Case: there are no aoes stored, so we just display it normally
            if (aoes.Count == 0) {
                aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            }

            if (aoes.Count > 0) {
                ref var aoe = ref aoes.Ref(0);
                aoe.Origin = spell.LocXZ;
                aoe.Rotation = spell.Rotation;
                aoe.Activation = Module.CastFinishAt(spell);
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID is (uint)AID.ForeCarve or (uint)AID.RearCarve) {
            if (aoes.Count > 0) {
                rotation = default;
                aoeLocked = false;
                aoes.RemoveAt(0);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.Featherglide) {
            aoeLocked = false;
        }
    }

    public override void Update() {
        if (aoeLocked || rotation == null) {
            return;
        }

        if ((Module.PrimaryActor.Position - startPosition).LengthSq() > 0.5f && Module.PrimaryActor.LastFrameMovementVec4 == default) {
            aoes.Add(new(shape, Module.PrimaryActor.Position, Module.PrimaryActor.Rotation + rotation.Value));
            aoeLocked = true;
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(aoes);
}

sealed class ZuPieceStates : StateMachineBuilder {
    public ZuPieceStates(BossModule module) : base(module) {
        TrivialPhase()
            .ActivateOnEnter<CrossbreezeBait>()
            .ActivateOnEnter<Crossbreeze>()
            .ActivateOnEnter<FlyingFrenzy>()
            .ActivateOnEnter<AiryPursuitPuddles>()
            .ActivateOnEnter<AiryPursuit>()
            .ActivateOnEnter<AiryPursuitBait>()
            .ActivateOnEnter<Carve>()
            .ActivateOnEnter<CarveTether>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.ZuPiece, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1090u, NameID = 14572u, SortOrder = 4)]
public sealed class ZuPiece : BossModule {
    public ZuPiece(WorldState ws, Actor primary) : base(ws, primary, new(120f, -420f), new ArenaBoundsCircle(20f)) {
        PulletPieceEgg = Enemies((uint)OID.PulletPieceEgg);
        CockarelPieceEgg = Enemies((uint)OID.CockerelPieceEgg);
        pulletPiece = Enemies((uint)OID.PulletPiece);
        cockerelPiece = Enemies((uint)OID.CockerelPiece);
    }

    public readonly List<Actor> PulletPieceEgg;
    public readonly List<Actor> CockarelPieceEgg;
    private readonly List<Actor> pulletPiece;
    private readonly List<Actor> cockerelPiece;

    protected override void DrawEnemies(int pcSlot, Actor pc) {
        Arena.Actor(PrimaryActor);
        Arena.Actors(pulletPiece, Colors.Vulnerable);
        Arena.Actors(cockerelPiece);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i) {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch {
                (uint)OID.PulletPiece => 3,
                (uint)OID.CockerelPiece => 2,
                (uint)OID.ZuPiece => 1,
                (uint)OID.PulletPieceEgg => AIHints.Enemy.PriorityForbidden,
                (uint)OID.CockerelPieceEgg => AIHints.Enemy.PriorityForbidden,
                _ => 0
            };
        }
    }

    private readonly string[] _prePullHints = [
        "Avoid breaking the eggs!",
        "When adds spawn kill order is the following: PulletPiece (purple) -> CockerelPiece -> Boss"
    ];

    public override string[] PrePullHints => _prePullHints;
}
