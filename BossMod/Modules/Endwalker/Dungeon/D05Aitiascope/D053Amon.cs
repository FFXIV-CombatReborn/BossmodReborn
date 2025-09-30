namespace BossMod.Endwalker.Dungeon.D05Aitiascope.D053Amon;

public enum OID : uint
{
    Boss = 0x346E, // R=16.98
    YsaylesSpirit = 0x346F, // R2.0
    Antistrophe = 0x1EB26D, // R0.5
    Ice = 0x3470, // R6.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 24712, // Boss->player, no cast, single-target

    Antistrophe = 25694, // Boss->self, 3.0s cast, single-target
    DarkForte = 25700, // Boss->player, 5.0s cast, single-targe, tankbuster
    Entracte = 25701, // Boss->self, 5.0s cast, range 40 circle, raidwide

    DreamsOfIce = 27756, // Helper->self, 14.7s cast, range 6 circle, knockback 13, dir forward, summons ice to hide behind
    CurtainCall = 25702, // Boss->self, 32.0s cast, range 40 circle, line of sight AOE

    Epode = 25695, // Helper->self, 8.0s cast, range 70 width 12 rect

    EruptionForteVisual = 24709, // Boss->self, 3.0s cast, single-target
    EruptionForte = 25704, // Helper->location, 4.0s cast, range 8 circle, baited AOE

    LeftFiragaForte = 25697, // Boss->self, 7.0s cast, range 40 width 20 rect
    RightFiragaForte = 25696, // Boss->self, 7.0s cast, range 40 width 20 rect

    Strophe = 25693, // Boss->self, 3.0s cast, single-target

    ThundagaForteProximity = 25690, // Boss->location, 5.0s cast, range 40 circle, damage fall off AoE
    ThundagaForteCone1 = 25691, // Helper->self, 5.0s cast, range 20 45-degree cone
    ThundagaForteCone2 = 25692, // Helper->self, 11.0s cast, range 20 45-degree cone

    Visual = 25703 // YsaylesSpirit->self, no cast, single-target
}

class CurtainCallArenaChange(BossModule module) : BossComponent(module)
{
    private static readonly Polygon[] circle = [new Polygon(new(11f, -490f), 6.5f, 20, 8.5f.Degrees())];
    public static readonly ArenaBoundsCustom CurtaincallArena = new(D053Amon.union, [.. D053Amon.difference, .. circle]);

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x05)
        {
            if (state == 0x00020001u)
                Arena.Bounds = CurtaincallArena;
            else if (state == 0x00080004u)
                Arena.Bounds = D053Amon.arena;
        }
    }
}

class Epode(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Epode, new AOEShapeRect(70f, 6f));
class EruptionForte(BossModule module) : Components.SimpleAOEs(module, (uint)AID.EruptionForte, 8f);

class FiragaForte(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.LeftFiragaForte, (uint)AID.RightFiragaForte], new AOEShapeRect(40f, 10f));
class ThundagaForteProximity(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ThundagaForteProximity, 15f);
class DarkForte(BossModule module) : Components.SingleTargetCast(module, (uint)AID.DarkForte);
class Entracte(BossModule module) : Components.RaidwideCast(module, (uint)AID.Entracte);
class DreamsOfIce(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DreamsOfIce, 6f);

class CurtainCall(BossModule module) : Components.CastLineOfSightAOE(module, (uint)AID.CurtainCall, 60f)
{
    public override ReadOnlySpan<Actor> BlockerActors() => CollectionsMarshal.AsSpan(Module.Enemies((uint)OID.Ice));

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return Arena.Bounds == CurtainCallArenaChange.CurtaincallArena ? CollectionsMarshal.AsSpan(Safezones) : [];
    }
}

class ThundagaForteCone(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ThundagaForteCone1, (uint)AID.ThundagaForteCone2], new AOEShapeCone(20f, 22.5f.Degrees()), 4, 8);

class D053AmonStates : StateMachineBuilder
{
    public D053AmonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CurtainCall>()
            .ActivateOnEnter<CurtainCallArenaChange>()
            .ActivateOnEnter<DreamsOfIce>()
            .ActivateOnEnter<Epode>()
            .ActivateOnEnter<EruptionForte>()
            .ActivateOnEnter<FiragaForte>()
            .ActivateOnEnter<ThundagaForteProximity>()
            .ActivateOnEnter<ThundagaForteCone>()
            .ActivateOnEnter<DarkForte>()
            .ActivateOnEnter<Entracte>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 786, NameID = 10293)]
public class D053Amon(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    public static readonly Polygon[] union = [new Polygon(new(11f, -490f), 19.5f, 48)];
    public static readonly Rectangle[] difference = [new(new(11f, -469.521f), 20, 1.25f)];
    public static readonly ArenaBoundsCustom arena = new(union, difference);
}
