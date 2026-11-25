namespace BossMod.Shadowbringers.Trial.T07DiamondWeapon;

public enum OID : uint
{
    Boss = 0x32DC,
    Helper = 0x233C,
    ArticulatedBitA = 0x32DD,
    ArticulatedBitB = 0x32DE
}

public enum AID : uint
{
    AdamantPurgeRight = 24474,
    AdamantPurgeLeft = 24475,
    AdamantPurgeHomingLaser = 24476,
    AdamantPurgeDiamondFlash = 24477,
    ClawSwipeRightVisual = 24478,
    ClawSwipeLeftVisual = 24479,
    HomingLaserVisual = 24480,
    DiamondRainVisual = 24487,
    PhotonBurstVisual = 24488,
    ArticulatedBits = 24489,
    DiamondShrapnelVisual = 24492,
    CodeChiXiStigma = 24493,
    RestoringArmor = 24494,
    AuriArtsVisual1 = 24495,
    AuriArtsVisual2 = 24498,
    AuriArtsTelegraph = 24504,
    AuriCycloneVisual = 24505,
    DiamondFlash = 24532,
    AetherialBullet = 24533,
    VerticalCleaveVisual = 24534,
    Outrage = 24535,
    AuriDoomstead = 24536,
    ClawSwipeLeft = 24537,
    ClawSwipeRight = 24538,
    HomingLaser = 24539,
    Burst = 24540,
    DiamondRain = 24543,
    PhotonBurst = 24544,
    DiamondShrapnelFirst = 24545,
    DiamondShrapnelRest = 24546,
    AuriArtsDash = 24547,
    AuriArtsLine = 24548,
    VerticalCleave = 24549,
    AuriCyclone1 = 24550,
    AuriCyclone2 = 24551,
    AirshipsBane = 24552,
    AuriArtsVisual3 = 24568,
    AirshipsBaneVisual = 24574,
    AdamantSphere = 24900,
    AuriArtsVisual4 = 24913,
    AuriArtsCircle = 24914,
    AuriArtsDashFirst = 24922,
    AuriArtsLineTelegraph = 24992,
    DiamondFlashMarker = 22393
}

public enum IconID : uint
{
    DiamondShrapnel = 197
}

sealed class DiamondRain(BossModule module) : Components.RaidwideCast(module, (uint)AID.DiamondRainVisual);
sealed class Outrage(BossModule module) : Components.RaidwideCast(module, (uint)AID.Outrage);
sealed class AuriDoomstead(BossModule module) : Components.BaitAwayCast(module, (uint)AID.AuriDoomstead, new AOEShapeCircle(5f), true, tankbuster: true);
sealed class PhotonBurst(BossModule module) : Components.BaitAwayCast(module, (uint)AID.PhotonBurst, new AOEShapeCircle(15f), true, tankbuster: true);
sealed class HomingLaser(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.HomingLaser, 5f);
sealed class DiamondFlash(BossModule module) : Components.LineStack(module, aidMarker: (uint)AID.DiamondFlashMarker, (uint)AID.DiamondFlash, 8.4d, 40f, 3f, markerIsFinalTarget: false);
sealed class DiamondShrapnel(BossModule module) : Components.StandardChasingAOEs(module, 8f, (uint)AID.DiamondShrapnelFirst, (uint)AID.DiamondShrapnelRest, 6f, 1d, 5, true, (uint)IconID.DiamondShrapnel, 10.1d);
sealed class AetherialBullet(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AetherialBullet, new AOEShapeRect(40f, 2.5f));
sealed class AuriCyclone(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.AuriCyclone1, (uint)AID.AuriCyclone2], 5f);
sealed class AirshipsBane(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AirshipsBane, new AOEShapeRect(42f, 11f));
sealed class AuriArtsCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AuriArtsCircle, 16f);
sealed class VerticalCleave(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.VerticalCleave, 15f);
sealed class AdamantSphere(BossModule module) : Components.CastHint(module, (uint)AID.AdamantSphere, "Soak the towers!");

sealed class AdamantPurge(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect _rect = new(22f, 21f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (id == (uint)AID.AdamantPurgeLeft)
            _aoe = [new(_rect, new WPos(91f, 100f), -90f.Degrees(), Module.CastFinishAt(spell, 10.9d))];
        else if (id == (uint)AID.AdamantPurgeRight)
            _aoe = [new(_rect, new WPos(109f, 100f), 90f.Degrees(), Module.CastFinishAt(spell, 10.9d))];
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.ClawSwipeLeft or (uint)AID.ClawSwipeRight)
        {
            ++NumCasts;
            _aoe = [];
        }
    }
}

sealed class AuriArts(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect _line = new(50f, 5f);
    private readonly List<AOEInstance> _dashes = [];
    private AOEInstance? _finalLine;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_dashes.Count != 0)
            return CollectionsMarshal.AsSpan(_dashes);
        return _finalLine is AOEInstance l ? new[] { l } : [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (id == (uint)AID.AuriArtsTelegraph)
        {
            var dir = spell.LocXZ - caster.Position;
            _dashes.Add(new(new AOEShapeRect(dir.Length(), 5f), caster.Position, Angle.FromDirection(dir), Module.CastFinishAt(spell, 4.5d + 0.27d * _dashes.Count)));
        }
        else if (id == (uint)AID.AuriArtsLineTelegraph)
            _finalLine = new(_line, caster.Position, spell.Rotation, Module.CastFinishAt(spell, 8.8d));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var id = spell.Action.ID;
        if (id is (uint)AID.AuriArtsDash or (uint)AID.AuriArtsDashFirst)
        {
            ++NumCasts;
            if (_dashes.Count != 0)
                _dashes.RemoveAt(0);
        }
        else if (id == (uint)AID.AuriArtsLine)
        {
            ++NumCasts;
            _finalLine = null;
        }
    }
}

sealed class ArenaChanges(BossModule module) : BossComponent(module)
{
    private bool _leftRemains;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AirshipsBane)
            _leftRemains = spell.LocXZ.X > 100f;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var id = spell.Action.ID;
        if (id == (uint)AID.AirshipsBane)
        {
            var ship = _leftRemains ? T07DiamondWeapon.LeftShip : T07DiamondWeapon.RightShip;
            Arena.Bounds = ship;
            Arena.Center = ship.Center;
        }
        else if (id == (uint)AID.RestoringArmor)
        {
            Arena.Bounds = T07DiamondWeapon.BothShips;
            Arena.Center = T07DiamondWeapon.BothShips.Center;
        }
    }
}

sealed class T07DiamondWeaponStates : StateMachineBuilder
{
    public T07DiamondWeaponStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<DiamondRain>()
            .ActivateOnEnter<Outrage>()
            .ActivateOnEnter<AuriDoomstead>()
            .ActivateOnEnter<PhotonBurst>()
            .ActivateOnEnter<HomingLaser>()
            .ActivateOnEnter<DiamondFlash>()
            .ActivateOnEnter<DiamondShrapnel>()
            .ActivateOnEnter<AetherialBullet>()
            .ActivateOnEnter<AuriCyclone>()
            .ActivateOnEnter<AirshipsBane>()
            .ActivateOnEnter<AuriArtsCircle>()
            .ActivateOnEnter<VerticalCleave>()
            .ActivateOnEnter<AdamantSphere>()
            .ActivateOnEnter<AdamantPurge>()
            .ActivateOnEnter<AuriArts>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "LTS", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 781, NameID = 9953)]
public sealed class T07DiamondWeapon(WorldState ws, Actor primary) : BossModule(ws, primary, BothShips.Center, BothShips)
{
    public static readonly ArenaBoundsCustom LeftShip = new([new Rectangle(new(80f, 100f), 11f, 21f)]);
    public static readonly ArenaBoundsCustom RightShip = new([new Rectangle(new(120f, 100f), 11f, 21f)]);
    public static readonly ArenaBoundsCustom BothShips = new([new Rectangle(new(80f, 100f), 11f, 21f), new Rectangle(new(120f, 100f), 11f, 21f)]);

    private static readonly uint[] _bits = [(uint)OID.ArticulatedBitA, (uint)OID.ArticulatedBitB];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(this, _bits);
    }
}
