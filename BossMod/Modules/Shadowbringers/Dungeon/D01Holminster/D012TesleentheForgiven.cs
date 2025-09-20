namespace BossMod.Shadowbringers.Dungeon.D01Holminster.D012TesleentheForgiven;

public enum OID : uint
{
    Boss = 0x278B, // R1.8
    HolyWaterVoidzone = 0x1EABF9, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target

    TheTickler = 15823, // Boss->player, 4.0s cast, single-target, tankbuster
    ScoldsBridle = 15824, // Boss->self, 4.0s cast, range 40 circle, raidwide
    FeveredFlagellationVisual = 15829, // Boss->self, 8.0s cast, single-target
    FeveredFlagellation = 15830, // Boss->players, no cast, width 4 rect charge, limit cut mechanic
    ExorciseVisual = 15826, // Boss->none, 5.0s cast, single-target
    Exorcise = 15827, // Boss->location, no cast, range 6 circle
    HolyWaterVoidzones = 15825, // Boss->self, no cast, single-target
    HolyWater = 15828 // Helper->location, 7.0s cast, range 6 circle
}

public enum IconID : uint
{
    Exorcise = 62, // player->self
    Icon1 = 79, // player
    Icon2 = 80, // player
    Icon3 = 81, // player
    Icon4 = 82 // player
}

sealed class TheTickler(BossModule module) : Components.SingleTargetDelayableCast(module, (uint)AID.TheTickler);
sealed class ScoldsBridle(BossModule module) : Components.RaidwideCast(module, (uint)AID.ScoldsBridle);

sealed class FeveredFlagellation(BossModule module) : Components.GenericBaitAway(module)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (CurrentBaits.Count != 0 && spell.Action.ID == (uint)AID.FeveredFlagellation)
        {
            CurrentBaits.RemoveAt(0);
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID is >= (uint)IconID.Icon1 and <= (uint)IconID.Icon4)
        {
            var primary = Module.PrimaryActor;
            CurrentBaits.Add(new(primary, actor, new AOEShapeRect((actor.Position - primary.Position).Length(), 2f), WorldState.FutureTime(8.3d + CurrentBaits.Count * 0.3d)));
        }
    }

    public override void Update()
    {
        var count = CurrentBaits.Count;
        if (count == 0)
            return;

        var count2 = count - 1;
        for (var i = count2; i >= 0; --i)
        {
            if (CurrentBaits[i].Target.IsDead) // if a target dies we need to remove the bait since the cast event will no longer happen
            {
                CurrentBaits.RemoveAt(i);
            }
        }
        var baits = CollectionsMarshal.AsSpan(CurrentBaits);
        count = baits.Length;
        for (var i = 0; i < count; ++i)
        {
            ref var b = ref baits[i];
            var length = (b.Target.Position - b.Source.Position).Length();
            if (b.Shape is AOEShapeRect rect && rect.LengthFront != length)
            {
                b.Shape = new AOEShapeRect(length, 2f);
            }
        }
    }
}

sealed class Exorcise(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Exorcise, (uint)AID.Exorcise, 6f, 5.1d, 4, 4);
sealed class HolyWater(BossModule module) : Components.VoidzoneAtCastTarget(module, 6f, (uint)AID.HolyWater, GetVoidzones, 0.8d)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.HolyWaterVoidzone);
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

sealed class D012TesleentheForgivenStates : StateMachineBuilder
{
    public D012TesleentheForgivenStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TheTickler>()
            .ActivateOnEnter<ScoldsBridle>()
            .ActivateOnEnter<FeveredFlagellation>()
            .ActivateOnEnter<Exorcise>()
            .ActivateOnEnter<HolyWater>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "legendoficeman, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 676, NameID = 8300)]
public sealed class D012TesleentheForgiven(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Polygon(new(78f, -82f), 19.5f * CosPI.Pi64th, 64)], [new Rectangle(new(78f, -62.081f), 20f, 1.0562f),
    new Rectangle(new(78f, -102.023f), 20f, 1.0562f)]);
}
