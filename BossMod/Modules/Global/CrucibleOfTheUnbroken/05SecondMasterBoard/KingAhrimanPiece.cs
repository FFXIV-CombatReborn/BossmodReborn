namespace BossMod.Global.CrucibleOfTheUnbroken.SecondMasterBoard.KingAhrimanPiece;

public enum OID : uint
{
    KingAhrimanPiece = 0x4D18, // R2.800, x1
    FinalHourglass = 0x4D19, // R1.000, x0 (spawn during fight)
    PointyFinger = 0x4D1A, // R1.000, x0 (spawn during fight)
    GrimReaperPiece = 0x4D1B, // R2.000, x0 (spawn during fight)
    HapalitPiece = 0x4D1C, // R2.080, x0 (spawn during fight)
    DirtyEyePiece = 0x4D1D, // R1.350, x0 (spawn during fight)
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 49682, // KingAhrimanPiece->player, no cast, single-target
    AutoAttackHapalit = 50398, // HapalitPiece->player, no cast, single-target
    Stone = 50747, // DirtyEyePiece->player, no cast, single-target

    Roulette = 49418, // KingAhrimanPiece->self, 5.0s cast, single-target
    RouletteRaidwide = 49419, // Helper->self, no cast, range 40 circle
    PointyFingerVisual = 49421, // PointyFinger->self, no cast, single-target
    Death = 49422, // GrimReaperPiece->self, no cast, range 23 90-degree cone
    Stare = 50940, // KingAhrimanPiece->players, no cast, range 50 width 8 rect
    Teleport = 49423, // KingAhrimanPiece->location, no cast, single-target
    MercurialBrand = 49427, // Helper->player, no cast, single-target
    DoubleVision = 49424, // KingAhrimanPiece->self, 5.0s cast, range 60 circle
    IrefulGaze = 49426, // Helper->self, 6.0s cast, range 30 180-degree cone
    SullenGaze = 49425, // Helper->self, 6.0s cast, range 30 180-degree cone
    CurtainsForRank5Visual = 49428, // KingAhrimanPiece->self, 5.0s cast, single-target
    CurtainsForRank5 = 49429, // Helper->self, 6.0s cast, range 50 circle
    Hourglass = 49420, // Helper->self, 25.0s cast, single-target
    StraightPunch = 49435, // HapalitPiece->player, no cast, single-target
    MortalGaze = 49434, // DirtyEyePiece->self, 4.0s cast, range 50 circle
    FlareVisual = 49431, // KingAhrimanPiece->self, 6.0s cast, single-target
    Flare = 49432 // Helper->players, no cast, range 43 circle
}

public enum SID : uint
{
    Bind = 3625, // none->player, extra=0x0
    Invincibility = 4539, // none->KingAhrimanPiece, extra=0x0
    BrandOfTheSullen = 5536, // Helper->player, extra=0x1
    BrandOfTheIreful = 5537 // Helper->player, extra=0x1
}

public enum IconID : uint
{
    Stare = 471, // player->self
    Icon327 = 327 // player->self
}

public enum TetherID : uint
{
    Invincibility = 1 // DirtyEyePiece/HapalitPiece->KingAhrimanPiece
}

sealed class FinalHourglass(BossModule module) : Components.Adds(module, (uint)OID.FinalHourglass, 1);

sealed class Stare(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(50f, 4f), (uint)IconID.Stare, (uint)AID.Stare)
{
    public override void Update()
    {
        for (var i = CurrentBaits.Count - 1; i >= 0; --i)
        {
            if (CurrentBaits[i].Target.IsDead)
                CurrentBaits.RemoveAt(i);
        }
    }
}

sealed class DoubleVision(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.IrefulGaze, (uint)AID.SullenGaze], new AOEShapeCone(30f, 90f.Degrees()))
{
    private readonly List<AOEInstance> _aoesBuffer = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoesBuffer.Clear();
        var ireful = actor.FindStatus((uint)SID.BrandOfTheIreful) != null;
        var sullen = actor.FindStatus((uint)SID.BrandOfTheSullen) != null;
        if (!ireful && !sullen)
            return [];
        var count = Casters.Count;
        for (var i = 0; i < count; ++i)
        {
            var c = Casters[i];
            var caster = WorldState.Actors.Find(c.ActorID);
            var id = caster?.CastInfo?.Action.ID;
            if (id == (uint)AID.IrefulGaze && ireful || id == (uint)AID.SullenGaze && sullen)
                _aoesBuffer.Add(c);
        }
        return CollectionsMarshal.AsSpan(_aoesBuffer);
    }
}

sealed class CurtainsForRank5(BossModule module) : Components.RaidwideCast(module, (uint)AID.CurtainsForRank5)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Casters.Count != 0 && WorldState.Actors.Find(WorldState.Client.ActivePet.InstanceID) is { IsDead: false })
            hints.Add("Dismiss pet!");
    }
}

sealed class Invincibility(BossModule module) : Components.InvincibleStatus(module, (uint)SID.Invincibility);
sealed class Adds(BossModule module) : Components.AddsMulti(module, [(uint)OID.HapalitPiece, (uint)OID.DirtyEyePiece]);
sealed class MortalGaze(BossModule module) : Components.CastGaze(module, (uint)AID.MortalGaze);
sealed class Flare(BossModule module) : Components.RaidwideCast(module, (uint)AID.FlareVisual);

sealed class KingAhrimanPieceStates : StateMachineBuilder
{
    public KingAhrimanPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Flare>()
            .ActivateOnEnter<FinalHourglass>()
            .ActivateOnEnter<Stare>()
            .ActivateOnEnter<DoubleVision>()
            .ActivateOnEnter<CurtainsForRank5>()
            .ActivateOnEnter<Invincibility>()
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<MortalGaze>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.KingAhrimanPiece, Contributors = "Xan (ported from upstream and modified by LTS)", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1092u, NameID = 14688u, SortOrder = 12)]
public sealed class KingAhrimanPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120f, -420f), new ArenaBoundsCircle(20f));
