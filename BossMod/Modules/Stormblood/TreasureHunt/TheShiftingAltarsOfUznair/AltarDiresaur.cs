namespace BossMod.Stormblood.TreasureHunt.ShiftingAltarsOfUznair.AltarDiresaur;

public enum OID : uint
{
    Boss = 0x253A, //R=6.6
    AltarDragon = 0x256F, //R=4.0
    AltarMatanga = 0x2545, // R3.42
    GoldWhisker = 0x2544, // R0.54
    FireVoidzone = 0x1EA140,
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // Boss/GoldWhisker->player, no cast, single-target
    AutoAttack2 = 872, // AltarMatanga->player, no cast, single-target
    AutoAttack3 = 6497, // AltarDragon->player, no cast, single-target

    DeadlyHold = 13217, // Boss->player, 3.0s cast, single-target
    HeatBreath = 13218, // Boss->self, 3.0s cast, range 8+R 90-degree cone
    TailSmash = 13220, // Boss->self, 3.0s cast, range 20+R 90-degree cone
    RagingInferno = 13283, // Boss->self, 3.0s cast, range 60 circle
    Comet = 13835, // BossHelper->location, 3.0s cast, range 4 circle
    HardStomp = 13743, // 256F->self, 3.0s cast, range 6+R circle
    Fireball = 13219, // Boss->location, 3.0s cast, range 6 circle

    MatangaActivate = 9636, // AltarMatanga->self, no cast, single-target
    Spin = 8599, // AltarMatanga->self, no cast, range 6+R 120-degree cone
    RaucousScritch = 8598, // AltarMatanga->self, 2.5s cast, range 5+R 120-degree cone
    Hurl = 5352, // AltarMatanga->location, 3.0s cast, range 6 circle
    Telega = 9630 // AltarMatanga/GoldWhisker->self, no cast, single-target, bonus adds disappear
}

public enum IconID : uint
{
    Baitaway = 23 // player
}

class DeadlyHold(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.DeadlyHold);
class HeatBreath(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeatBreath, new AOEShapeCone(14.6f, 45f.Degrees()));
class TailSmash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TailSmash, new AOEShapeCone(26.6f, 45f.Degrees()));
class RagingInferno(BossModule module) : Components.RaidwideCast(module, (uint)AID.RagingInferno);
class Comet(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Comet, 4f);
class HardStomp(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HardStomp, 10f);
class Fireball(BossModule module) : Components.VoidzoneAtCastTarget(module, 6f, (uint)AID.Fireball, GetVoidzones, 0.7f)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.FireVoidzone);
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

class FireballBait(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private static readonly AOEShapeCircle circle = new(6);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Baitaway)
            CurrentBaits.Add(new(Module.PrimaryActor, actor, circle));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Fireball)
            ++NumCasts;
        if (NumCasts == 3)
        {
            CurrentBaits.Clear();
            NumCasts = 0;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (CurrentBaits.Count != 0 && CurrentBaits[0].Target == actor)
            hints.AddForbiddenZone(new SDCircle(Arena.Center, 17.5f));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (CurrentBaits.Count == 0)
            return;
        if (CurrentBaits[0].Target != actor)
            base.AddHints(slot, actor, hints);
        else
            hints.Add("Bait away! (3 times)");
    }
}

class RaucousScritch(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RaucousScritch, new AOEShapeCone(8.42f, 60f.Degrees()));
class Hurl(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Hurl, 6f);
class Spin(BossModule module) : Components.Cleave(module, (uint)AID.Spin, new AOEShapeCone(9.42f, 60f.Degrees()), [(uint)OID.AltarMatanga]);

class AltarDiresaurStates : StateMachineBuilder
{
    public AltarDiresaurStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DeadlyHold>()
            .ActivateOnEnter<HeatBreath>()
            .ActivateOnEnter<TailSmash>()
            .ActivateOnEnter<RagingInferno>()
            .ActivateOnEnter<Comet>()
            .ActivateOnEnter<HardStomp>()
            .ActivateOnEnter<Fireball>()
            .ActivateOnEnter<FireballBait>()
            .ActivateOnEnter<Hurl>()
            .ActivateOnEnter<RaucousScritch>()
            .ActivateOnEnter<Spin>()
            .Raw.Update = () => AllDeadOrDestroyed(AltarDiresaur.All);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 586, NameID = 7627)]
public class AltarDiresaur(WorldState ws, Actor primary) : THTemplate(ws, primary)
{
    private static readonly uint[] bonusAdds = [(uint)OID.GoldWhisker, (uint)OID.AltarMatanga];
    public static readonly uint[] All = [(uint)OID.Boss, (uint)OID.AltarDragon, .. bonusAdds];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.AltarDragon));
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
                (uint)OID.GoldWhisker => 3,
                (uint)OID.AltarMatanga => 2,
                (uint)OID.AltarDragon => 1,
                _ => 0
            };
        }
    }
}
