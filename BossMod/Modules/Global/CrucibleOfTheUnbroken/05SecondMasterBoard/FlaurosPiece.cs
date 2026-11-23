namespace BossMod.Global.CrucibleOfTheUnbroken.SecondMasterBoard.FlaurosPiece;

public enum OID : uint
{
    FlaurosPiece = 0x4CDB,
    Helper = 0x233C,
    LightningSprite = 0x4CDC, // R0.800-1.488, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 49680, // FlaurosPiece->player, no cast, single-target
    HeatLightningBoss = 49185, // FlaurosPiece->self, 4.0s cast, single-target
    HeatLightning = 49186, // Helper->location, 4.0s cast, range 6 circle
    ChargedLightningBoss = 49192, // FlaurosPiece->self, 5.0+0.5s cast, single-target
    ChargedLightning = 49193, // Helper->self, 5.8s cast, range 50 width 40 rect
    ElectricShockBoss = 49190, // FlaurosPiece->self, 5.5+0.5s cast, single-target
    ElectricShock = 49191, // Helper->self, 6.0s cast, range 16 circle
    AetherialOffering = 49187, // FlaurosPiece->self, 5.0s cast, single-target
    ErraticBlasterBoss = 49188, // FlaurosPiece->self, 6.0+1.0s cast, single-target
    ErraticBlaster = 49189, // Helper->player, 7.0s cast, single-target

    // LightningSprite
    LineVoltage = 49194, // 4CDC->self, 2.0s cast, range 100 width 2 rect
    LineVoltageBig = 49195, // 4CDC->self, 3.0s cast, range 100 width 6 rect
}

public enum SID : uint
{
    Paralysis = 5382, // Helper->player, extra=0x0
    SustainedDamage = 3795, // none->4CDC, extra=0x1
}

public enum IconID : uint
{
    ErraticBlasterTankBuster = 475, // player->self
}

public enum TetherID : uint
{
    LightningSpriteTether = 6, // 4CDC->FlaurosPiece
}

sealed class HeatLightning(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeatLightning, 6f);
sealed class LineVoltage(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LineVoltage, new AOEShapeRect(100f, 1f));
sealed class LineVoltageBig(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LineVoltageBig, new AOEShapeRect(100f, 3f));
sealed class ChargedLightning(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ChargedLightning, new AOEShapeRect(100f, 20f));
sealed class ElectricShock(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ElectricShock, 16f);
sealed class ErraticBlaster(BossModule module) : Components.SingleTargetCast(module, (uint)AID.ErraticBlaster, "TankBuster + applies Paralysis");

sealed class AddMovement(BossModule module) : Components.Adds(module, (uint)OID.LightningSprite, 2)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        var sprites = ActiveActors;
        var count = sprites.Count;

        if (count == 0)
        {
            return;
        }

        Actor? closest = null;
        var closestDistance = float.MaxValue;

        for (var i = 0; i < count; i++)
        {
            var sprite = sprites[i];
            var distance = (actor.Position - sprite.Position).LengthSq();
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = sprite;
            }
        }

        if (closest == null)
        {
            return;
        }

        hints.GoalZones.Add(AIHints.GoalSingleTarget(closest.Position, 6.0f, 5.0f));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) => Arena.Actors(Actors, Colors.Vulnerable);
}

sealed class FlaurosPieceStates : StateMachineBuilder
{
    public FlaurosPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HeatLightning>()
            .ActivateOnEnter<LineVoltage>()
            .ActivateOnEnter<LineVoltageBig>()
            .ActivateOnEnter<ChargedLightning>()
            .ActivateOnEnter<ElectricShock>()
            .ActivateOnEnter<ErraticBlaster>()
            .ActivateOnEnter<AddMovement>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.FlaurosPiece, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1092u, NameID = 14631u, SortOrder = 1)]
[PreferredKinship(BST.Kinship.Ash, "Cleanse Paralysis from Erratic Blaster")]
public sealed class FlaurosPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120f, -420f), new ArenaBoundsCircle(20f));
