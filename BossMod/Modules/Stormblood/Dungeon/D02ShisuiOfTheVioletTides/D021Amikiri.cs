namespace BossMod.Stormblood.Dungeon.D02ShisuiOfTheVioletTides.D021Amikiri;

public enum OID : uint
{
    Boss = 0x1B0C, // R4.5
    AmikiriLeg = 0x1B2A, // R4.5
    BindVoidzone = 0x1EA0D7, // R0.5
    WaterVoidzone = 0x1E950D, // R0.5
    Kamikiri = 0x1B0D, // R2.5
    Helper = 0x18D6
}

public enum AID : uint
{
    AutoAttack = 872, // Boss/Kamikiri->player, no cast, single-target
    SharpStrike = 8050, // Boss/Kamikiri->player, no cast, single-target
    MucalGlob = 8051, // Boss->self, no cast, single-target
    MucalGlob2 = 8052, // Helper->player, no cast, single-target
    Shuck = 8053, // Boss->player, 30.0s cast, single-target
    DigestiveFluid = 8056, // Kamikiri->self, no cast, single-target
    Digest = 8057, // Helper->player, no cast, range 5 circle
    TriumphantRise = 8055, // Boss->self, no cast, single-target
}

public enum IconID : uint
{
    Bind = 1, // player
    DigestiveFluid = 14, // player
}

class DigestiveFluid(BossModule module) : Components.VoidzoneAtCastTarget(module, 5, (uint)AID.Digest, m => m.Enemies((uint)OID.WaterVoidzone).Where(z => z.EventState != 7), 0.7f);
class BindVoidzone(BossModule module) : Components.Voidzone(module, 3, m => m.Enemies((uint)OID.BindVoidzone).Where(z => z.EventState != 7));

class DigestiveFluidBait(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private static readonly AOEShapeCircle circle = new(5f);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.DigestiveFluid)
            CurrentBaits.Add(new(Module.PrimaryActor, actor, circle, WorldState.FutureTime(7.2d)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Digest)
            CurrentBaits.RemoveAll(x => x.Target == WorldState.Actors.Find(spell.MainTargetID));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (ActiveBaitsOn(actor).Count != 0)
            hints.AddForbiddenZone(new SDCircle(Arena.Center, 17.5f));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (ActiveBaitsOn(actor).Count != 0)
            hints.Add("Bait away!");
    }

    public override void Update()
    {
        if (CurrentBaits.Count > 0 && Module.Enemies((uint)OID.Kamikiri).All(x => x.IsDead)) // if adds die baits get cancelled
            CurrentBaits.Clear();
    }
}

class BindBait(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private static readonly AOEShapeCircle circle = new(3f);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Bind)
            CurrentBaits.Add(new(Module.PrimaryActor, actor, circle));
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.BindVoidzone)
            CurrentBaits.Clear();
    }
}

class D021AmikiriStates : StateMachineBuilder
{
    public D021AmikiriStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BindVoidzone>()
            .ActivateOnEnter<BindBait>()
            .ActivateOnEnter<DigestiveFluid>()
            .ActivateOnEnter<DigestiveFluidBait>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 235, NameID = 6237)]
public class D021Amikiri(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Polygon(new(0, 69.221f), 19.5f * CosPI.Pi8th, 8, 22.5f.Degrees())]);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Kamikiri));
        Arena.Actors(Enemies((uint)OID.AmikiriLeg));
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.AmikiriLeg => 2,
                (uint)OID.Kamikiri => 1,
                _ => 0
            };
        }
    }
}
