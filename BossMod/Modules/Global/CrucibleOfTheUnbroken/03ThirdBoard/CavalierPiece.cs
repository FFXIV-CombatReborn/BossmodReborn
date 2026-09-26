namespace BossMod.Global.CrucibleOfTheUnbroken.ThirdBoard.CavalierPiece;

public enum OID : uint
{
    CavalierPiece = 0x4C8E,
    BoneBishop = 0x4C90, // R0.750, x0 (spawn during fight)
    BoneFragmentOrb = 0x4C92, // R1.000, x0 (spawn during fight)
    FeintedCavalierPiece = 0x4C8F, // R2.520, x5
    Helper = 0x233C,
}

public enum AID : uint
{
    AutoAttack = 49680, // CavalierPiece->player, no cast, single-target
    AutoAttackBlizzard = 48621, // 4C90->player, no cast, single-target
    SteelripperBoss = 48472, // CavalierPiece->self, 6.0+1.0s cast, single-target
    Steelripper = 48473, // Helper->self, 7.0s cast, range 60 130.000-degree cone
    Doubling = 48460, // CavalierPiece->self, 5.0s cast, single-target
    MenaceActor = 48463, // 4C8F->self, 5.4+0.6s cast, single-target
    Menace = 48464, // Helper->self, 6.0s cast, range 20 circle
    ValfodrActor = 48461, // 4C8F->self, 5.6+0.4s cast, single-target
    Valfodr = 48462, // Helper->self, 6.0s cast, range 60 width 8 rect
    CrushingBlade = 48471, // CavalierPiece->player, 5.0s cast, single-target
    FeintedCavalierTeleport = 50552, // 4C8F->CavalierPiece, no cast, single-target

    Unknown1 = 50551, // 4C92->CavalierPiece, no cast, single-target - most likely the orb teleport
}

public enum IconID : uint
{
    TankBusterKnockBack = 633, // CavalierPiece->player
}

public enum TetherID : uint
{
    DoublingTether = 398, // 4C92->CavalierPiece
}

sealed class Steelripper(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Steelripper, new AOEShapeCone(60.0f, 65.0f.Degrees()));

sealed class MenaceValfodr(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];
    private readonly AOEShapeCircle circle = new(20.0f);
    private readonly AOEShapeRect rectangle = new(60.0f, 4.0f);
    private const float riskyWindow = 7.0f;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.FeintedCavalierTeleport)
        {
            if (caster.Position.InRect(Arena.Center, 20f, 20f))
            {
                aoes.Add(new(circle, caster.Position, caster.Rotation, WorldState.FutureTime(12.3d)));
            }
            else
            {
                // Actors can spawn at angles, but will always look forward where ever they spawn, so we just correct it here
                var angleCorrection = (MathF.Round(caster.Rotation.Deg / 90f) * 90f).Degrees();
                if (angleCorrection.ToDirection().Dot(caster.Position - Arena.Center) > 0)
                {
                    angleCorrection += 180f.Degrees();
                }

                aoes.Add(new(rectangle, caster.Position, angleCorrection, WorldState.FutureTime(11.7d)));
            }
        }

        if (spell.Action.ID is (uint)AID.Menace or (uint)AID.Valfodr)
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

        var incomingAOEs = CollectionsMarshal.AsSpan(aoes);
        var time = WorldState.CurrentTime;

        for (var i = 0; i < count; ++i)
        {
            ref var aoe = ref incomingAOEs[i];
            aoe.Risky = aoe.Activation.AddSeconds(-riskyWindow) <= time;
        }

        return incomingAOEs;
    }
}

sealed class CrushingBlade(BossModule module) : Components.GenericKnockback(module)
{
    private const float KnockbackDistance = 15f;
    private BitMask affectedPlayers;
    private DateTime activation = default;
    private Actor? source = null;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.TankBusterKnockBack && Raid.FindSlot(targetID) is var slot && slot >= 0)
        {
            affectedPlayers[slot] = true;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.CrushingBlade)
        {
            activation = Module.CastFinishAt(spell);
            source = caster;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.CrushingBlade)
        {
            affectedPlayers.Reset();
        }
    }

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (affectedPlayers[slot] && activation != default && source != null)
        {
            return new Knockback[1] { new(source.Position, KnockbackDistance, activation) };
        }

        return [];
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!affectedPlayers[slot] || activation == default || source == null || IsImmune(slot, activation))
        {
            return;
        }

        hints.AddForbiddenZone(new SDKnockbackInAABBRectAwayFromOrigin(Arena.Center, source.Position, KnockbackDistance, 20.0f, 20.0f));
    }
}

sealed class CavalierPieceStates : StateMachineBuilder
{
    public CavalierPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Steelripper>()
            .ActivateOnEnter<MenaceValfodr>()
            .ActivateOnEnter<CrushingBlade>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.CavalierPiece, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1090u, NameID = 14564u, SortOrder = 1)]
public sealed class CavalierPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120f, 0f), new ArenaBoundsSquare(20f))
{
    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.BoneBishop => 2,
                (uint)OID.CavalierPiece => 1,
                _ => 0
            };
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.BoneBishop));
    }

    private readonly string[] _prePullHints = [
        "Bone bishops will periodically spawn, these will drop an orb when killed that the boss will absorb when it casts Doubling. Up to 5 orbs can be absorbed, avoid having 5+ orbs accumulate or else mechanics will be undodgable"
    ];

    public override string[] PrePullHints => _prePullHints;
}
