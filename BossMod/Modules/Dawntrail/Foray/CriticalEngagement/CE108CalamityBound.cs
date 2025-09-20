namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE108CalamityBound;

public enum OID : uint
{
    Boss = 0x46C9, // R11.4
    Seal = 0x1EBCFD, // R0.5
    Tower = 0x1EBCFC, // R0.5
    BallOfFire = 0x47F0, // R2.3
    CloisterTorch = 0x46CB, // R3.6
    Helper = 0x233C
}

public enum AID : uint
{
    FellForces = 41363, // Boss->player, no cast, single-target

    SundersealRoarVisual = 41336, // Boss->self, 5.0s cast, ???
    SundersealRoar = 41337, // Helper->self, no cast, ???
    GigaflareVisual = 41361, // Boss->self, 5.0s cast, ???
    Gigaflare = 41362, // Helper->self, no cast, ???

    VoidThunderIII = 41358, // Helper->self, 5.0s cast, range 60 width 8 rect
    GreatBallOfFire = 41354, // Boss->self, 4.0s cast, single-target
    Explosion = 41357, // BallOfFire->self, 5.0s cast, range 22 circle

    TidalBreath = 41360, // Boss->self, 7.0s cast, range 40 180-degree cone
    KarmicDrain = 41359, // Helper->self, 5.0s cast, range 40 60-degree cone
    BlazingFlare = 41355, // Boss->self, 4.0s cast, single-target
    Flare = 41356, // Helper->self, 6.0s cast, range 10 circle
    SealAsunder1 = 41339, // Boss->self, 33.0s cast, single-target
    SealAsunder2 = 41340, // Boss->self, 40.0s cast, single-target
    SealAsunder3 = 41341, // Boss->self, 46.0s cast, single-target

    Visual = 41338, // Boss->self, no cast, single-target
    SelfDestructVisual = 41342, // CloisterTorch->self, no cast, ???
    SelfDestruct = 41313, // Helper->self, no cast, ???
    VoidDeathIV = 41351 // Boss->self, 3.0s cast, range 40 circle, enrage if seal got broken
}

sealed class SundersealRoar(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.SundersealRoarVisual, (uint)AID.SundersealRoar, 0.9d);
sealed class Gigaflare(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.GigaflareVisual, (uint)AID.Gigaflare, 0.9d, "Raidwide + bleed");
sealed class VoidThunderIII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.VoidThunderIII, new AOEShapeRect(60f, 4f), 4);
sealed class TidalBreath(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TidalBreath, new AOEShapeCone(40f, 90f.Degrees()));
sealed class KarmicDrain(BossModule module) : Components.SimpleAOEs(module, (uint)AID.KarmicDrain, new AOEShapeCone(60f, 30f.Degrees()), 3);
sealed class Explosion(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Explosion, 22f);
sealed class Flare(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Flare, 10f);
sealed class SealAsunder(BossModule module) : Components.CastHints(module, [(uint)AID.SealAsunder1, (uint)AID.SealAsunder2, (uint)AID.SealAsunder3], "Enrage!", true);

sealed class Seals(BossModule module) : Components.GenericTowersOpenWorld(module)
{
    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00040008u && Towers.Count != 0 && actor.OID == (uint)OID.Tower)
        {
            var count = Towers.Count;
            var pos = actor.Position;
            var towers = CollectionsMarshal.AsSpan(Towers);
            for (var i = 0; i < count; ++i)
            {
                if (towers[i].Position == pos)
                {
                    Towers.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Tower)
        {
            Towers.Add(new(actor.Position, 3f, 4, 8, activation: DateTime.MaxValue));
        }
    }
}

sealed class CE108CalamityBoundStates : StateMachineBuilder
{
    public CE108CalamityBoundStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<VoidThunderIII>()
            .ActivateOnEnter<SundersealRoar>()
            .ActivateOnEnter<Gigaflare>()
            .ActivateOnEnter<TidalBreath>()
            .ActivateOnEnter<KarmicDrain>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<Flare>()
            .ActivateOnEnter<SealAsunder>()
            .ActivateOnEnter<Seals>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CriticalEngagement, GroupID = 1018, NameID = 37)]
public sealed class CE108CalamityBound(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Polygon(new(-340f, 800f), 29.5f, 32)]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies((uint)OID.CloisterTorch));
        Arena.Actor(PrimaryActor);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.CloisterTorch => 1,
                _ => 0
            };
        }
    }

    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InCircle(Arena.Center, 25f);
}