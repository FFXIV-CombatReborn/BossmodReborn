namespace BossMod.Dawntrail.Quest.Role.TheMightiestShield;

public enum OID : uint
{
    Boss = 0x43C7, // R5.4
    CrackedMettle1 = 0x43E1, // R2.0
    CrackedMettle2 = 0x43E4, // R4.86
    UnyieldingMettle1 = 0x43E2, // R1.0
    UnyieldingMettle2 = 0x43E0, // R2.0
    UnyieldingMettle3 = 0x43E3, // R4.86
    CravenFollower1 = 0x43CF, // R2.0
    CravenFollower2 = 0x43CB, // R2.0
    CravenFollower3 = 0x43C9, // R2.0
    CravenFollower4 = 0x43CD, // R2.0
    CravenFollower5 = 0x43D0, // R0.5
    CravenFollower6 = 0x43CE, // R0.5
    CravenFollower7 = 0x43CC, // R0.5
    CravenFollower8 = 0x43CA, // R0.5
    MagitekMissile = 0x43C8, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 39025, // Boss->Kaqool, no cast, single-target

    PhotonStream = 37001, // CravenFollower3/CravenFollower1/CravenFollower2/CravenFollower4->Kaqool, no cast, single-target
    HomingLaserMarker = 38622, // CravenFollower1/CravenFollower4/CravenFollower2->player/Kaqool, 5.0s cast, single-target
    HomingLaser = 38624, // CravenFollower1/CravenFollower4/CravenFollower2->player/Kaqool, no cast, range 42 width 8 rect
    MissileStormVisual = 38601, // Boss->self, no cast, single-target
    MissileStorm = 38602, // Helper->self, no cast, range 60 circle
    AreaBombardmentVisual = 38599, // Boss->self, 7.0s cast, single-target
    AreaBombardment = 38600, // Helper->self, 7.0s cast, range 4 circle
    MagitekCannon = 39006, // CravenFollower3->player, 5.0s cast, range 6 circle, spread

    GuidedMissileVisual1 = 38603, // Boss->self, no cast, single-target
    GuidedMissileVisual2 = 35763, // MagitekMissile->self, no cast, single-target
    GuidedMissileVisual3 = 35764, // MagitekMissile->self, no cast, single-target
    Burst = 38598, // MagitekMissile->self, no cast, range 3 circle

    SteelhogsMettle = 38621, // Boss->self, 3.0s cast, single-target
    SteelhogsRevenge = 38625, // UnyieldingMettle1->self, 7.0s cast, range 12 circle

    HeavySurfaceMissilesVisual = 38606, // Boss->self, 3.0s cast, single-target, damage fall off aoes
    HeavySurfaceMissiles1 = 38607, // Helper->self, 7.0s cast, range 60 circle
    HeavySurfaceMissiles2 = 38608, // Helper->self, 10.0s cast, range 60 circle
    HeavySurfaceMissiles3 = 38609, // Helper->self, 13.0s cast, range 60 circle
    HeavySurfaceMissiles4 = 38610, // Helper->self, 16.0s cast, range 60 circle

    GunsBlazing1 = 38614, // Boss->self, 5.0s cast, single-target
    GunsBlazing2 = 38616, // Boss->self, 5.0s cast, single-target
    NeedleGun1 = 38618, // Helper->self, 5.0s cast, range 40 90-degree cone
    NeedleGun2 = 38712, // Helper->self, 7.0s cast, range 40 90-degree cone
    OilShower1 = 38619, // Helper->self, 7.0s cast, range 40 270-degree cone
    OilShower2 = 38711, // Helper->self, 5.0s cast, range 40 270-degree cone

    RuthlessBombardmentVisual = 38611, // Boss->self, 7.0s cast, single-target
    RuthlessBombardment1 = 38613, // Helper->self, 7.0s cast, range 8 circle
    RuthlessBombardment2 = 38612, // Helper->self, 7.0s cast, range 4 circle

    Visual1 = 38615, // Boss->self, no cast, single-target
    Visual2 = 38617, // Boss->self, no cast, single-target

    SurfaceMissile1 = 38604, // Boss->self, no cast, single-target
    SurfaceMissile2 = 38605, // Helper->location, 5.0s cast, range 6 circle
    UnbreakableCermetDrill = 38620, // Boss->Kaqool, no cast, single-target

    RagingArtilleryVisual1 = 38741, // Boss->self, 5.0s cast, single-target
    RagingArtilleryVisual2 = 38743, // Boss->self, no cast, single-target
    RagingArtilleryFirst = 38742, // Helper->self, 5.0s cast, range 60 circle, multiple raidwides
    RagingArtilleryRest = 38744 // Helper->self, no cast, range 60 circle
}

public enum SID : uint
{
    RightwardFracture = 4036, // none->CrackedMettle1/CrackedMettle2, extra=0x0
    LeftwardFracture = 4037, // none->CrackedMettle1, extra=0x0
    BackwardFracture = 4039 // none->CrackedMettle1, extra=0x0
}

sealed class Fractures(BossModule module) : Components.DirectionalParry(module, [(uint)OID.CrackedMettle1, (uint)OID.CrackedMettle2])
{
    private const int sideR = (int)(Side.Front | Side.Back | Side.Left) << 4;
    private const int sideL = (int)(Side.Right | Side.Back | Side.Front) << 4;
    private static readonly Angle a180 = 180f.Degrees(), a90 = 90f.Degrees(), a20 = 20f.Degrees();

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (actor.OID is not (uint)OID.CrackedMettle1 and not (uint)OID.CrackedMettle2)
            return;
        var sides = status.ID switch
        {
            (uint)SID.RightwardFracture => Side.Front | Side.Back | Side.Left,
            (uint)SID.LeftwardFracture => Side.Right | Side.Back | Side.Front,
            (uint)SID.BackwardFracture => Side.Front | Side.Left | Side.Right,
            _ => Side.None
        };
        if (sides != Side.None)
            PredictParrySide(actor.InstanceID, sides);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID is (uint)SID.RightwardFracture or (uint)SID.LeftwardFracture or (uint)SID.BackwardFracture)
            UpdateState(actor.InstanceID, 0);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (ActorStates.Count != 0)
        {
            KeyValuePair<ulong, int> first = default;
            foreach (var pair in ActorStates)
            {
                first = pair;
                break;
            }
            var target = WorldState.Actors.Find(first.Key)!;
            var dir = first.Value == sideR ? target.Rotation - a90 : first.Value == sideL ? target.Rotation + a90 : target.Rotation + a180;
            hints.AddForbiddenZone(new SDInvertedDonutSector(target.Position, target.HitboxRadius, 20f, dir, a20));
        }
    }
}

sealed class SteelhogsRevenge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SteelhogsRevenge, 12f);
sealed class RuthlessBombardment1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RuthlessBombardment1, 8f);
sealed class Bombardment(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.RuthlessBombardment2, (uint)AID.AreaBombardment], 4f);

sealed class RagingArtillery(BossModule module) : Components.RaidwideCast(module, (uint)AID.RagingArtilleryFirst);
sealed class MagitekCannon(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.MagitekCannon, 6f);
sealed class MagitekMissile(BossModule module) : Components.Voidzone(module, 3f, GetVoidzones, 5f)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.MagitekMissile);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (!z.IsDead)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}

sealed class NeedleGunOilShower(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    private static readonly AOEShapeCone cone1 = new(40f, 135f.Degrees());
    private static readonly AOEShapeCone cone2 = new(40f, 45f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        aoes[0].Risky = true;
        return aoes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddAOE(AOEShape shape)
        {
            _aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), _aoes.Count == 0 ? Colors.Danger : default, false));
            if (_aoes.Count == 2)
            {
                var aoes = CollectionsMarshal.AsSpan(_aoes);
                ref var aoe1 = ref aoes[0];
                ref var aoe2 = ref aoes[1];
                if (aoe1.Activation > aoe2.Activation)
                {
                    (aoe1, aoe2) = (aoe2, aoe1);
                }
            }
        }
        switch (spell.Action.ID)
        {
            case (uint)AID.OilShower1:
            case (uint)AID.OilShower2:
                AddAOE(cone1);
                break;
            case (uint)AID.NeedleGun1:
            case (uint)AID.NeedleGun2:
                AddAOE(cone2);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0)
            switch (spell.Action.ID)
            {
                case (uint)AID.OilShower1:
                case (uint)AID.OilShower2:
                case (uint)AID.NeedleGun1:
                case (uint)AID.NeedleGun2:
                    _aoes.RemoveAt(0);
                    break;
            }
    }
}

sealed class HeavySurfaceMissiles(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.HeavySurfaceMissiles1, (uint)AID.HeavySurfaceMissiles2,
(uint)AID.HeavySurfaceMissiles3, (uint)AID.HeavySurfaceMissiles4], 14f, 2, 8);

sealed class HomingLaser(BossModule module) : Components.GenericBaitAway(module)
{
    private static readonly AOEShapeRect rect = new(42f, 4f);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HomingLaserMarker)
            CurrentBaits.Add(new(caster, WorldState.Actors.Find(spell.TargetID)!, rect, WorldState.FutureTime(5.7d)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.HomingLaser)
            CurrentBaits.Clear();
    }
}

sealed class TheMightiestShieldStates : StateMachineBuilder
{
    public TheMightiestShieldStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Fractures>()
            .ActivateOnEnter<RuthlessBombardment1>()
            .ActivateOnEnter<Bombardment>()
            .ActivateOnEnter<RagingArtillery>()
            .ActivateOnEnter<MagitekCannon>()
            .ActivateOnEnter<NeedleGunOilShower>()
            .ActivateOnEnter<HeavySurfaceMissiles>()
            .ActivateOnEnter<MagitekMissile>()
            .ActivateOnEnter<SteelhogsRevenge>()
            .ActivateOnEnter<HomingLaser>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70377, NameID = 12923)]
public sealed class TheMightiestShield(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly ArenaBoundsCustom arena = new([new Polygon(new(-191f, 72f), 14.5f, 20)]);
    private static readonly uint[] all = [(uint)OID.Boss, (uint)OID.CravenFollower1, (uint)OID.CravenFollower2, (uint)OID.CravenFollower3, (uint)OID.CravenFollower4,
    (uint)OID.CravenFollower5, (uint)OID.CravenFollower6, (uint)OID.CravenFollower7, (uint)OID.CravenFollower8, (uint)OID.MagitekMissile, (uint)OID.UnyieldingMettle2,
    (uint)OID.UnyieldingMettle3]; // except CrackedMettle1/2 since the Parry component is drawing them

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, all);
    }

    protected override bool CheckPull() => Raid.Player()!.InCombat;
}
