namespace BossMod.Global.CrucibleOfTheUnbroken.ThirdBoard.YmirPiece;

public enum OID : uint {
    YmirPiece = 0x4C93,
    SahaginPiece = 0x4C95, // R2.000, x1
    YmirShell = 0x4C94, // R2.000, x1, Part type
    Helper = 0x233C
}

public enum AID : uint {
    // Sahagin
    AutoAttackWater = 48626, // 4C95->player, no cast, single-target
    SahaginTeleport = 48484, // SahaginPiece->location, no cast, single-target
    WaterIIBoss = 48482, // 4C95->self, 3.0s cast, single-target
    WaterII = 48483, // Helper->location, 3.0s cast, range 6 circle
    TsunamiBoss = 48480, // 4C95->self, 8.0s cast, single-target
    Tsunami = 48481, // Helper->self, 8.0s cast, range 60 width 60 rect
    ParalyzingSpikes = 50532, // 4C95->self, 3.0s cast, single-target
    Dreadwash = 48485, // SahaginPiece->self, 8.0s cast, range 30 circle

    // Ymir
    AutoAttackHeadSnatch = 48477, // YmirPiece->self, no cast, range 7 ?-degree cone
    YmirTeleport = 48476, // YmirPiece->location, no cast, single-target
    BlanketThunder = 48479, // YmirPiece->self, 5.0s cast, range 40 circle
}

public enum SID : uint {
    VulnerabilityDown = 2198,
    ParalyzingSpikes = 5434, // none->4C95, extra=0x64
}

public enum TetherID : uint {
    ParalyzingSpikesTether = 6, // 4C95->YmirPiece
}

sealed class WaterII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WaterII, 6.0f);
sealed class BlanketThunder(BossModule module) : Components.RaidwideCast(module, (uint)AID.BlanketThunder);
sealed class Dreadwash(BossModule module) : Components.CastInterruptHint(module, (uint)AID.Dreadwash);

sealed class ParalyzingSpikes(BossModule module) : Components.Adds(module, (uint)OID.SahaginPiece, 3) {
    private readonly List<Actor> avoidBosses = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.ParalyzingSpikes) {
            avoidBosses.Add(caster);
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status) {
        if (status.ID == (uint)SID.ParalyzingSpikes) {
            avoidBosses.Remove(actor);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints) {
        var count = avoidBosses.Count;
        if (count == 0) {
            return;
        }

        for (var i = 0; i < count; i++) {
            if (avoidBosses[i].InstanceID == actor.TargetID) {
                hints.Add("Attacking boss with spikes debuff!");
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        base.AddAIHints(slot, actor, assignment, hints);
        var count = avoidBosses.Count;
        if (count == 0) {
            return;
        }

        for (var i = 0; i < count; i++) {
            hints.SetPriority(avoidBosses[i], AIHints.Enemy.PriorityForbidden);
        }
    }
}

sealed class VulnDown(BossModule module) : Components.Adds(module, (uint)OID.YmirPiece, 1) {
    private readonly List<Actor> avoidBosses = [];

    public override void OnStatusGain(Actor actor, ref ActorStatus status) {
        if (status.ID == (uint)SID.VulnerabilityDown) {
            avoidBosses.Add(actor);
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status) {
        if (status.ID == (uint)SID.VulnerabilityDown) {
            avoidBosses.Remove(actor);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints) {
        var count = avoidBosses.Count;
        if (count == 0) {
            return;
        }

        for (var i = 0; i < count; i++) {
            if (avoidBosses[i].InstanceID == actor.TargetID) {
                hints.Add("Attacking invincible target!");
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        base.AddAIHints(slot, actor, assignment, hints);
        var count = avoidBosses.Count;
        if (count == 0) {
            return;
        }

        for (var i = 0; i < count; i++) {
            hints.SetPriority(avoidBosses[i], AIHints.Enemy.PriorityForbidden);
        }
    }
}

sealed class YmirShell(BossModule module) : Components.Adds(module, (uint)OID.YmirShell, 2);

sealed class Tsunami(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Tsunami, 35f, kind: Kind.DirForward) {
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        var count = Casters.Count;
        if (count == 0) {
            return;
        }

        var knockback = Casters[0];

        if (!IsImmune(slot, knockback.Activation)) {
            hints.AddForbiddenZone(new SDKnockbackInAABBRectFixedDirection(Arena.Center, Distance * knockback.Direction.ToDirection(), 20f, 20f),
                knockback.Activation);
        }
    }
}

sealed class YmirPieceStates : StateMachineBuilder {
    public YmirPieceStates(BossModule module) : base(module) {
        TrivialPhase()
            .ActivateOnEnter<WaterII>()
            .ActivateOnEnter<Tsunami>()
            .ActivateOnEnter<BlanketThunder>()
            .ActivateOnEnter<ParalyzingSpikes>()
            .ActivateOnEnter<VulnDown>()
            .ActivateOnEnter<YmirShell>()
            .ActivateOnEnter<Dreadwash>()
            .Raw.Update = () => AllDeadOrDestroyed(YmirPiece.Bosses);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.YmirPiece, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1090u, NameID = 14569u, SortOrder = 2)]
public sealed class YmirPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120f, 0f), new ArenaBoundsSquare(20f)) {
    public static readonly uint[] Bosses = [(uint)OID.YmirPiece, (uint)OID.SahaginPiece];
    protected override void DrawEnemies(int pcSlot, Actor pc) {
        Arena.Actors(this, Bosses);
    }

    private readonly string[] _prePullHints = [
        "The ymir will take reduced damage until its shell is broken. When its shell is broken it will move to the closest shell and enter it. Stun and bind can help delay the ymir from reaching a shell.",
        "Paralyzing Spikes: Buff on sahagin that inflicts paralysis if hit. Casted at the start of the fight and whenever ymir gets a new shell",
        "Interrupt the Dreadwash spell or use your pet to take the damage down."
    ];

    public override string[] PrePullHints => _prePullHints;
}
