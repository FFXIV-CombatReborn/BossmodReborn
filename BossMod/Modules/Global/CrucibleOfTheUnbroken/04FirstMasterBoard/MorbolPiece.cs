namespace BossMod.Global.CrucibleOfTheUnbroken.FirstMasterBoard.MorbolPiece;

public enum OID : uint
{
    MorbolPiece = 0x4CB9,
    CarrionBroth = 0x4CBC, // R2.500, x3 (spawn during fight)
    SeedlingPiece = 0x4CBA, // R0.900, x0 (spawn during fight)
    OchuPiece = 0x4CBB, // R2.400, x0 (spawn during fight)
    GreenPuddle = 0x1E9F40, // R0.500, x0 (spawn during fight), EventObj type
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 50937, // MorbolPiece->player, no cast, single-target
    AutoAttackSeedling = 50750, // 4CBA->player, no cast, single-target
    AutoAttackOchu = 50396, // 4CBB->player, no cast, single-target
    AutoAttackCarrion = 50544, // 4CBC->player, no cast, single-target

    ExtremelyBadBreathBossRight = 48671, // MorbolPiece->self, 4.5+0.5s cast, single-target
    ExtremelyBadBreathBossLeft = 48672, // MorbolPiece->self, 4.5+0.5s cast, single-target
    ExtremelyBadBreathStart = 48673, // Helper->self, 5.0s cast, range 50 90-degree cone
    ExtremelyBadBreath = 48674, // MorbolPiece->self, no cast, single-target
    ExtremelyBadBreathRest = 48675, // Helper->self, 0.5s cast, range 50 90-degree cone

    TremblorBoss = 50757, // MorbolPiece->self, 4.5+0.5s cast, single-target
    Tremblor = 50758, // Helper->self, 5.0s cast, range 50 circle
    VineProbeBoss = 48681, // MorbolPiece->self, 4.0+1.0s cast, single-target
    VineProbe = 48682, // Helper->self, 5.0s cast, range 13 width 8 rect

    StickySpit = 48680, // 4CBC->player, 2.0s cast, single-target
    AcidMist = 48679, // 4CBB->self, 4.0s cast, range 6 circle
}

public enum SID : uint
{
    Heavy = 2551, // 4CBC->player, extra=0x1E
    Paralysis = 5382, // 4CBC->player, extra=0x0
}

public enum IconID : uint
{
    TurnRight = 684, // MorbolPiece->self
    TurnLeft = 685, // MorbolPiece->self
}

public enum TetherID : uint
{
    AddTargetTether = 17, // 4CBA->player
    Unknown = 44, // 4CBC->player
}

sealed class AcidMist(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AcidMist, 6f);
sealed class Tremblor(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Tremblor, 10f);
sealed class VineProbe(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VineProbe, new AOEShapeRect(13f, 4f));

sealed class GreenPuddle(BossModule module) : Components.Voidzone(module, 6f, GetVoidzones)
{
    private readonly List<AOEInstance> _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var aoes = _aoesBuffer;
        aoes.Clear();
        foreach (var source in Sources(Module))
        {
            if (ArenaProjectionLayerParticipantApplies(source, ArenaProjectionLayer, RestrictToArenaProjectionLayer))
                aoes.Add(new(Shape, source.Position, source.Rotation, color: Colors.Danger, arenaProjectionLayer: ArenaProjectionLayer, restrictToArenaProjectionLayer: RestrictToArenaProjectionLayer));
        }
        return CollectionsMarshal.AsSpan(aoes);
    }

    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.GreenPuddle);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}

// Icon given like 3 seconds after the cast has started, so I use the spell IDs instead to figure out if its self or right
sealed class ExtremelyBadBreathBoss(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];
    private readonly AOEShapeCone shape = new(50f, 45f.Degrees());
    private ActorCastInfo? spellInfo;
    private Angle increment = default;
    private const int numberOfAOEs = 5;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ExtremelyBadBreathStart)
        {
            spellInfo = spell;
            InitIfReady();
        }

        var direction = spell.Action.ID switch
        {
            (uint)AID.ExtremelyBadBreathBossRight => -50f.Degrees(),
            (uint)AID.ExtremelyBadBreathBossLeft => 50f.Degrees(),
            _ => default
        };

        if (direction == default)
        {
            return;
        }

        increment = direction;
        InitIfReady();
    }

    private void InitIfReady()
    {
        if (spellInfo != null && increment != default)
        {
            for (var i = 0; i < numberOfAOEs; ++i)
            {
                if (i == 0)
                {
                    aoes.Add(new(shape, spellInfo.LocXZ, spellInfo.Rotation, Module.CastFinishAt(spellInfo)));
                    continue;
                }

                var activation = 2.1d * i;
                var rotation = spellInfo.Rotation + increment * i;
                aoes.Add(new(shape, spellInfo.LocXZ, rotation, Module.CastFinishAt(spellInfo).AddSeconds(activation)));
            }

            increment = default;
            spellInfo = null;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.ExtremelyBadBreathStart or (uint)AID.ExtremelyBadBreathRest)
        {
            if (aoes.Count > 0)
            {
                aoes.RemoveAt(0);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = aoes.Count;
        if (count == 0)
        {
            return [];
        }

        var max = count > 2 ? 2 : count;
        var nextAOEs = CollectionsMarshal.AsSpan(aoes);

        for (var i = 0; i < max; ++i)
        {
            ref var aoe = ref nextAOEs[i];
            aoe.Color = i == 0 ? Colors.Danger : Colors.AOE;
        }

        return nextAOEs[..max];
    }
}

sealed class MorbolPieceStates : StateMachineBuilder
{
    public MorbolPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ExtremelyBadBreathBoss>()
            .ActivateOnEnter<AcidMist>()
            .ActivateOnEnter<Tremblor>()
            .ActivateOnEnter<VineProbe>()
            .ActivateOnEnter<GreenPuddle>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.MorbolPiece, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1091u, NameID = 14599u, SortOrder = 2)]
[PreferredKinship(BST.Kinship.Vile, "Ignore Tremblor's knockback")]
[PreferredKinship(BST.Kinship.Ash, "Cleanse Paralysis from Carrion Broth")]
public sealed class MorbolPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120f, -420f), new ArenaBoundsCircle(20f))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.CarrionBroth => 4,
                (uint)OID.SeedlingPiece => 3,
                (uint)OID.OchuPiece => 2,
                (uint)OID.MorbolPiece => 1,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.CarrionBroth));
        Arena.Actors(Enemies((uint)OID.SeedlingPiece));
        Arena.Actors(Enemies((uint)OID.OchuPiece));
    }
}
