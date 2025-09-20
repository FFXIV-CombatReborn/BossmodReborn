namespace BossMod.Shadowbringers.Alliance.A33RedGirl;

sealed class CrueltyP1(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.CrueltyVisualP1, (uint)AID.Cruelty, 0.1d);
sealed class CrueltyP2(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.CrueltyVisualP2, (uint)AID.Cruelty, 0.1d);
sealed class SublimeTranscendence(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.SublimeTranscendenceVisual, (uint)AID.SublimeTranscendence, 0.1d);
sealed class ManipulateEnergy(BossModule module) : Components.BaitAwayIcon(module, 3f, (uint)IconID.ManipulateEnergy, (uint)AID.ManipulateEnergy, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);

sealed class GenerateBarrier1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GenerateBarrierVisual1, new AOEShapeRect(18f, 1.5f));
sealed class GenerateBarrier2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GenerateBarrierVisual2, new AOEShapeRect(24f, 1.5f));
sealed class GenerateBarrier3(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GenerateBarrierVisual3, new AOEShapeRect(12f, 1.5f));
sealed class GenerateBarrier4(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GenerateBarrierVisual4, new AOEShapeRect(6f, 1.5f));

sealed class Explosion(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Explosion, 9f, riskyWithSecondsLeft: 5d);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 779, NameID = 9920, SortOrder = 4)]
public sealed class A33RedGirl(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, StartingArena)
{
    public static readonly WPos ArenaCenter = new(845f, -851f);
    public static readonly ArenaBoundsSquare StartingArena = new(24.5f);
    public static readonly PolygonCustom InnerSquare = new([new(847.5f, -848.5f), new(847.5f, -853.5f), new(842.5f, -853.5f), new(842.49945f, -848.50018f)]); // one vertice of the inner square is slightly misplaced. since it kills instantly we prefer perfection.
    public static readonly Square[] BigSquare = [new(ArenaCenter, 24.5f)];
    public static readonly Square[] DefaultSquare = [new(ArenaCenter, 20f)];
    public static readonly AOEShapeCustom ArenaTransition = new(BigSquare, DefaultSquare, [InnerSquare]);
    public static readonly ArenaBoundsCustom DefaultArena = new(DefaultSquare, [InnerSquare]);
    public static readonly PolygonCustom[] VirusArena1 = [new([new(6f, 856f), new(-6f, 856f), new(-6f, 868f), new(-1.5f, 868f), new(-1.5f, 880f),
    new(-8f, 880f), new(-8f, 882f), new(-12f, 882f), new(-12f, 884f), new(-14f, 884f),
    new(-14f, 886f), new(-16f, 886f), new(-16f, 888f), new(-18f, 888f), new(-18f, 892f),
    new(-20f, 892f), new(-20f, 908f), new(-18f, 908f), new(-18f, 912f), new(-16f, 912f),
    new(-16f, 914f), new(-14f, 914f), new(-14f, 916f), new(-12f, 916f), new(-12f, 918f),
    new(-8f, 918f), new(-8f, 920f), new(-1.5f, 920f), new(-1.5f, 932f), new(-6f, 932f),
    new(-6f, 944f), new(6f, 944f), new(6f, 932f), new(1.5f, 932f), new(1.5f, 920f),
    new(8f, 920f), new(8f, 918f), new(12f, 918f), new(12f, 916f), new(14f, 916f),
    new(14f, 914f), new(16f, 914f), new(16f, 912f), new(18f, 912f), new(18f, 908f),
    new(20f, 908f), new(20f, 892f), new(18f, 892f), new(18f, 888f), new(16f, 888f),
    new(16f, 886f), new(14f, 886f), new(14f, 884f), new(12f, 884f), new(12f, 882f),
    new(8f, 882f), new(8f, 880f), new(1.5f, 880f), new(1.5f, 868f), new(6f, 868f)])];
    public static readonly PolygonCustom[] VirusArena2 = GenerateVirusArena(new(default, -500f));
    public static readonly PolygonCustom[] VirusArena3 = GenerateVirusArena(new(default, -1000f));
    private static PolygonCustom[] GenerateVirusArena(WDir offset)
    {
        var vertices = new WPos[60];
        var o = offset;
        var vertices1 = VirusArena1[0].Vertices;
        for (var i = 0; i < 60; ++i)
        {
            vertices[i] = vertices1[i] + o;
        }
        return [new(vertices)];
    }

    public Actor? BossP2;
    public Actor? RedSphere;

    protected override void UpdateModule()
    {
        RedSphere ??= GetActor((uint)OID.RedSphere);
        if (StateMachine.ActivePhaseIndex >= 1)
        {
            BossP2 ??= GetActor((uint)OID.BossP2);
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        switch (StateMachine.ActivePhaseIndex)
        {
            case -1:
            case 0:
                Arena.Actor(PrimaryActor);
                break;
            case 1:
                Arena.Actor(RedSphere);
                Arena.Actors(Enemies((uint)OID.BlackPylon));
                Arena.Actors(Enemies((uint)OID.WhitePylon));
                Arena.Actors(Enemies((uint)OID.BlackWall), Colors.Object);
                Arena.Actors(Enemies((uint)OID.WhiteWall), Colors.Object);
                break;
            case 2:
                Arena.Actor(BossP2);
                break;
        }
    }
}
