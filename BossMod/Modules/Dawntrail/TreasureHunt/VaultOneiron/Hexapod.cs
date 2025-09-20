namespace BossMod.Dawntrail.TreasureHunt.VaultOneiron.Hexapod;

public enum OID : uint
{
    Hexapod = 0x48B3, // R4.5
    Mine = 0x48B4, // R1.0

    VaultOnion = 0x48B9, // R0.84, icon 1, needs to be killed in order from 1 to 5 for maximum rewards
    VaultEggplant = 0x48BA, // R0.84, icon 2, needs to be killed in order from 1 to 5 for maximum rewards
    VaultGarlic = 0x48BB, // R0.84, icon 3, needs to be killed in order from 1 to 5 for maximum rewards
    VaultTomato = 0x48BC, // R0.84, icon 4, needs to be killed in order from 1 to 5 for maximum rewards
    VaultQueen = 0x48BD, // R0.84, icon 5, needs to be killed in order from 1 to 5 for maximum rewards
    GoldyCat = 0x48B7, // R1.87
    Vaultkeeper = 0x48B8, // R2.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 43767, // Hexapod->player, no cast, single-target

    MagneticGenesisVisual = 43749, // Hexapod->self, 4.0s cast, single-target
    MagneticGenesis = 43750, // Helper->self, 4.0s cast, range 40 circle, apply positive/negative charges
    ApplyChargeIcons = 43601, // Hexapod->self, no cast, single-target

    PoleShiftMinusVisual = 43753, // Hexapod->self, 5.0s cast, single-target
    PoleShiftMinus = 43754, // Helper->self, 5.0s cast, range 50 circle, knockback 7, away from source or pull 7 between centers
    PoleShiftPlusVisual = 43751, // Hexapod->self, 5.0s cast, single-target
    PoleShiftPlus = 43752, // Helper->self, 5.0s cast, range 50 circle, knockback 7, away from source or pull 7 between centers

    ShockCircle = 43755, // Helper->self, 7.0s cast, range 10 circle
    ShockRect = 43756, // Helper->self, 7.0s cast, range 40 width 6 rect
    Electroblast = 43770, // Hexapod->self, 5.0s cast, range 45 circle, raidwide
    DiffusionRay = 43769, // Hexapod->self, 5.5s cast, range 30 120-degree cone
    AccelerationBombVisual = 43760, // Hexapod->self, 4.0s cast, single-target
    AccelerationBomb = 43761, // Helper->self, 4.0s cast, range 40 circle, apply acceleration bomb
    ElectricExcessVisual = 43758, // Hexapod->self, 4.0s cast, single-target
    ElectricExcess = 43759, // Helper->location, 4.0s cast, range 5 circle
    BrainjackVisual = 43763, // Hexapod->self, 4.0s cast, single-target
    Brainjack = 43764, // Helper->self, 4.0s cast, range 40 circle, apply forced march debuffs, 3s duration
    ElectromineVisual1 = 43765, // Hexapod->self, 4.0s cast, single-target
    ElectromineVisual2 = 43766, // Helper->self, 4.0s cast, range 40 circle, triggers all mines
    Electromine = 43771, // Mine->self, no cast, range 5 circle, stepped into mine
    AntiHeroArtillery = 43768, // Hexapod->player, 5.0s cast, range 5 circle, tankbuster

    AutoAttack2 = 871, // Vaultkeeper->player, no cast, single-target
    Thunderlance = 43727, // Vaultkeeper->self, 3.5s cast, range 20 width 3 rect
    LanceSwing = 43726, // Vaultkeeper->self, 4.0s cast, range 8 circle
    TearyTwirl = 32301, // VaultOnion->self, 3.5s cast, range 7 circle
    PluckAndPrune = 32302, // VaultEggplant->self, 3.5s cast, range 7 circle
    PungentPirouette = 32303, // VaultGarlic->self, 3.5s cast, range 7 circle
    HeirloomScream = 32304, // VaultTomato->self, 3.5s cast, range 7 circle
    Pollen = 32305, // VaultQueen->self, 3.5s cast, range 7 circle
    Telega = 9630 // BonusAdds->self, no cast, single-target, bonus adds disappear
}

public enum SID : uint
{
    PositiveCharge = 4551, // Helper->player, extra=0x0
    NegativeCharge = 4552, // Helper->player, extra=0x0
    AccelerationBomb = 3802, // Helper->player, extra=0x0
    ForwardMarch = 2161, // Helper->player, extra=0x0
    AboutFace = 2162, // Helper->player, extra=0x0
    LeftFace = 2163, // Helper->player, extra=0x0
    RightFace = 2164 // Helper->player, extra=0x0
}

sealed class ShockRect(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ShockRect, new AOEShapeRect(40f, 3f));
sealed class ShockCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ShockCircle, 10f);
sealed class DiffusionRay(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DiffusionRay, new AOEShapeCone(30f, 60f.Degrees()));
sealed class Electroblast(BossModule module) : Components.RaidwideCast(module, (uint)AID.Electroblast);
sealed class ElectricExcess(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ElectricExcess, 5f);
sealed class AntiHeroArtillery(BossModule module) : Components.BaitAwayCast(module, (uint)AID.AntiHeroArtillery, 5f, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);

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

sealed class Electromine(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = new(5);
    private readonly AOEShapeCircle circle = new(5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Mine)
        {
            AOEs.Add(new(circle, actor.Position.Quantized(), actorID: actor.InstanceID));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Electromine)
        {
            var count = AOEs.Count;
            var aoes = CollectionsMarshal.AsSpan(AOEs);
            var id = caster.InstanceID;
            for (var i = 0; i < count; ++i)
            {
                ref var aoe = ref aoes[i];
                if (aoe.ActorID == id)
                {
                    AOEs.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = AOEs.Count;
        if (count == 0)
        {
            return;
        }
        var aoes = CollectionsMarshal.AsSpan(AOEs);
        for (var i = 0; i < count; ++i)
        {
            ref var aoe = ref aoes[i];
            hints.TemporaryObstacles.Add(new SDCircle(aoe.Origin, 5f));
        }
    }
}

sealed class Brainjack(BossModule module) : Components.StatusDrivenForcedMarch(module, 3f, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace, stopAtWall: true)
{
    private readonly Electromine _mine = module.FindComponent<Electromine>()!;

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var count = _mine.AOEs.Count;
        var aoes = CollectionsMarshal.AsSpan(_mine.AOEs);
        var dir = (pos - actor.Position).Normalized();

        for (var i = 0; i < count; ++i)
        {
            if (Intersect.RayCircle(actor.Position - aoes[i].Origin, dir, 5f, 18f))
            {
                return true;
            }
        }
        return false;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var state = State.GetValueOrDefault(actor.InstanceID);
        if (state == null || state.PendingMoves.Count == 0)
        {
            return;
        }

        ref var move0 = ref state.PendingMoves.Ref(0);
        var act = move0.activation;
        var aoes = CollectionsMarshal.AsSpan(_mine.AOEs);
        var len = aoes.Length;
        var pos = actor.Position;
        var moveDir = move0.dir.ToDirection();
        for (var i = 0; i < len; ++i)
        {
            ref var aoe = ref aoes[i];
            var origin = aoe.Origin;
            var d = origin - pos;
            var dist = d.Length();

            if (dist is <= 5f or >= 24f) // inside mine or max distance 3s * 6 + 5 radius + 1 safety margin
            {
                continue; // inside mine or impossible to run into this mine from current position
            }

            var forward = d.Dot(moveDir);
            var sideways = d.Dot(moveDir.OrthoL());

            hints.ForbiddenDirections.Add(new(Angle.Atan2(sideways, forward), Angle.Asin(5f / dist), act));
        }
    }
}

sealed class PoleShift(BossModule module) : Components.GenericKnockback(module, stopAtWall: true)
{
    private readonly Knockback[][] _kbs = new Knockback[PartyState.MaxPartySize][];
    private readonly ShockRect _aoe1 = module.FindComponent<ShockRect>()!;
    private readonly ShockCircle _aoe2 = module.FindComponent<ShockCircle>()!;
    private BitMask negative;
    private BitMask positive;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (_kbs[slot] != null)
        {
            return _kbs[slot];
        }
        return [];
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        ref readonly var kb = ref _kbs[slot][0];
        if (kb.Kind == Kind.AwayFromOrigin)
        {
            var count = _aoe1.Casters.Count;
            var aoes = CollectionsMarshal.AsSpan(_aoe1.Casters);
            for (var i = 0; i < count; ++i)
            {
                if (aoes[i].Check(pos))
                {
                    return true;
                }
            }
        }
        else if (_aoe2.Casters.Count != 0)
        {
            ref var aoe = ref _aoe2.Casters.Ref(0);
            return aoe.Check(pos);
        }
        return false;
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.PositiveCharge:
                positive.Set(Raid.FindSlot(actor.InstanceID));
                break;
            case (uint)SID.NegativeCharge:
                negative.Set(Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.PositiveCharge:
                positive.Clear(Raid.FindSlot(actor.InstanceID));
                break;
            case (uint)SID.NegativeCharge:
                negative.Clear(Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        bool IsPull(int slot)
        => id == (uint)AID.PoleShiftMinus && positive[slot] || id == (uint)AID.PoleShiftPlus && negative[slot];

        bool IsKnockback(int slot)
        => id == (uint)AID.PoleShiftMinus && negative[slot] || id == (uint)AID.PoleShiftPlus && positive[slot];

        if (id is (uint)AID.PoleShiftMinus or (uint)AID.PoleShiftPlus)
        {
            var party = Raid.WithSlot(false, true, true);
            var len = party.Length;
            for (var i = 0; i < len; ++i)
            {
                ref readonly var p = ref party[i];
                var slot = p.Item1;
                var isPull = IsPull(slot);
                var isKnockback = IsKnockback(slot);
                if (isPull || isKnockback)
                {
                    _kbs[slot] = [new(spell.LocXZ, 7f, Module.CastFinishAt(spell), kind: isPull ? Kind.TowardsOrigin : Kind.AwayFromOrigin, minDistance: 1f, ignoreImmunes: true)];
                }
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.PoleShiftMinus or (uint)AID.PoleShiftPlus)
        {
            Array.Clear(_kbs);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_kbs[slot] != null)
        {
            ref readonly var kb = ref _kbs[slot][0];
            var pos = kb.Origin;
            var act = kb.Activation;
            if (kb.Kind == Kind.TowardsOrigin)
            {
                hints.AddForbiddenZone(new SDCircle(pos, 18f), act); // circle 10 + 7 pull + 1 safety margin
            }
            else
            {
                hints.AddForbiddenZone(new SDKnockbackInAABBSquareAwayFromOrigin(Arena.Center, pos, 7f, 13f), act); // arena square 20 - 6 rect width - 1 safety margin
            }
        }
    }
}

sealed class MandragoraAOEs(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PluckAndPrune, (uint)AID.TearyTwirl,
(uint)AID.HeirloomScream, (uint)AID.PungentPirouette, (uint)AID.Pollen], 7f);
sealed class Thunderlance(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Thunderlance, new AOEShapeRect(20f, 1.5f));
sealed class LanceSwing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LanceSwing, 8f);

sealed class HexapodStates : StateMachineBuilder
{
    public HexapodStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ShockCircle>()
            .ActivateOnEnter<ShockRect>()
            .ActivateOnEnter<DiffusionRay>()
            .ActivateOnEnter<Electroblast>()
            .ActivateOnEnter<ElectricExcess>()
            .ActivateOnEnter<AccelerationBomb>()
            .ActivateOnEnter<Electromine>()
            .ActivateOnEnter<Brainjack>()
            .ActivateOnEnter<PoleShift>()
            .ActivateOnEnter<AntiHeroArtillery>()
            .ActivateOnEnter<MandragoraAOEs>()
            .ActivateOnEnter<Thunderlance>()
            .ActivateOnEnter<LanceSwing>()
            .Raw.Update = () => AllDeadOrDestroyed(Hexapod.All);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", PrimaryActorOID = (uint)OID.Hexapod, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1060u, NameID = 14010u, Category = BossModuleInfo.Category.TreasureHunt, Expansion = BossModuleInfo.Expansion.Dawntrail, SortOrder = 14)]
public sealed class Hexapod(WorldState ws, Actor primary) : SharedBoundsBoss(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.VaultOnion, (uint)OID.VaultTomato, (uint)OID.VaultGarlic, (uint)OID.VaultEggplant, (uint)OID.VaultQueen, (uint)OID.Vaultkeeper, (uint)OID.GoldyCat];
    public static readonly uint[] All = [(uint)OID.Hexapod, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(this, bonusAdds, Colors.Vulnerable);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.VaultOnion => 6,
                (uint)OID.VaultEggplant => 5,
                (uint)OID.VaultGarlic => 4,
                (uint)OID.VaultTomato => 3,
                (uint)OID.VaultQueen or (uint)OID.GoldyCat => 2,
                (uint)OID.Vaultkeeper => 1,
                _ => 0
            };
        }
    }
}
