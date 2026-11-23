namespace BossMod.Endwalker.Trial.T07Zeromus;

public enum OID : uint
{
    Boss = 0x404E, // R20.000, x1
    Comet = 0x40AB, // R2.250, x22
    ToxicBubble = 0x40AC, // R1.700, spawn during fight
    FlowOfTheAbyss = 0x4110, // R1.000, x1
    Helper = 0x233C, // R0.500, x30, 523 type
    FlareTower = 0x1EB94E, // R0.500, EventObj type, spawn during fight
    FlareRay = 0x1EB94F, // R0.500, EventObj type, spawn during fight
    BlackHole = 0x1EB94C // R0.500, EventObj type, spawn during fight
}

public enum AID : uint
{
    AutoAttack = 35912, // Boss->player, no cast, single-target

    AbyssalNox = 35575, // Boss->self, 5.0s cast, range 60 circle, doom on everyone
    AbyssalEchoesVisualCardinal = 35576, // Helper->self, 9.0s cast, single-target, visual
    AbyssalEchoesVisualIntercardinal = 35577, // Helper->self, 9.0s cast, single-target, visual
    AbyssalEchoes = 35578, // Helper->self, 16.0s cast, range 12 circle

    SableThread = 35566, // Boss->self, 5.0s cast, single-target, visual (line stack)
    SableThreadTarget = 35567, // Helper->player, no cast, single-target, visual (target selection)
    SableThreadVisualHitIntermediate = 35568, // Boss->self, no cast, single-target, visual
    SableThreadVisualHitLast = 35569, // Boss->self, no cast, single-target, visual
    SableThreadAOE = 35570, // Helper->self, no cast, range 60 width 12 rect line stack

    DarkMatter = 35638, // Boss->self, 4.0s cast, single-target, visual (tankbuster)
    DarkMatterAOE = 35639, // Helper->player, no cast, range 8 circle tankbuster, 3 hits

    VisceralWhirlR = 35579, // Boss->self, 8.0s cast, single-target, visual (two lines with right safespot)
    VisceralWhirlRAOE1 = 35580, // Helper->self, 8.8s cast, range 29 width 28 rect
    VisceralWhirlRAOE2 = 35581, // Helper->self, 8.8s cast, range 60 width 28 rect
    VisceralWhirlL = 35582, // Boss->self, 8.0s cast, single-target, visual (two lines with left safespot)
    VisceralWhirlLAOE1 = 35583, // Helper->self, 8.8s cast, range 29 width 28 rect
    VisceralWhirlLAOE2 = 35584, // Helper->self, 8.8s cast, range 60 width 28 rect

    Flare = 35602, // Boss->self, 7.0s cast, single-target, visual (tower)
    FlareAOE = 35603, // Helper->self, 8.0s cast, range 5 circle tower (full party)
    FlareScald = 35765, // Helper->self, no cast, range 5 circle (tower aftereffect, damage + vuln)
    FlareKill = 35605, // Helper->self, 5.0s cast, range 5 circle (tower aftereffect, last hit)
    ProminenceSpine = 35606, // Helper->self, 5.0s cast, range 60 width 10 rect (tower aftereffect, ray)
    Nox = 36135, // Boss->self, 4.0s cast, single-target, visual (chasing aoe)
    NoxAOEFirst = 36137, // Helper->self, 5.0s cast, range 10 circle
    NoxAOERest = 36131, // Helper->self, no cast, range 10 circle

    VoidBio = 35607, // Boss->self, 5.0s cast, single-target, visual (spawn toxic bubbles)
    VoidBioBurst = 35608, // ToxicBubble->player, no cast, single-target (bubble touch)

    BigBang = 35587, // Boss->self, 10.0s cast, range 60 circle, raidwide with dot
    BigBangAOE = 35589, // Helper->location, 8.0s cast, range 5 circle puddle
    BigCrunch = 35588, // Boss->self, 10.0s cast, range 60 circle, raidwide with dot
    BigCrunchAOE = 36144, // Helper->location, 8.0s cast, range 5 circle puddle
    AccelerationBomb = 35590, // ?, no cast, single-target
    DarkDivides = 35593, // Helper->player, no cast, range 5 circle spread
    DarkBeckons = 35594, // Helper->player, no cast, range 6 circle stack

    MeteorImpact = 35595, // Boss->self, 11.0s cast, single-target, visual (tethered meteors)
    VoidMeteor = 35596, // Boss->self, 4.8s cast, single-target, visual (placed meteors)
    MeteorImpactChargeNormal = 35597, // Comet->location, no cast, width 4 rect charge
    MeteorImpactChargeClipping = 35598, // Comet->location, no cast, width 4 rect charge (clipped a placed meteor)
    MeteorImpactAppearClipping = 35599, // Helper->location, no cast, range 60 circle
    MeteorImpactMassiveExplosion = 35600, // Comet->self, no cast, range 60 circle
    MeteorImpactProximity = 35601, // Comet->self, 6.0s cast, range 40 circle with falloff
    MeteorImpactAppearNormal = 36024, // Helper->location, no cast, range 2 circle
    MeteorImpactExplosion = 36147, // Comet->self, 5.0s cast, range 10 circle

    BlackHole = 36133, // Boss->self, 5.0s cast, single-target, visual (black hole + laser)
    BlackHoleAOE = 36134, // Helper->location, 5.7s cast, range 4 circle (black hole drop spot)
    FracturedEventideEW = 35571, // Boss->self, 8.0s cast, single-target, visual (laser E to W)
    FracturedEventideWE = 35572, // Boss->self, 8.0s cast, single-target, visual (laser W to E)
    FracturedEventideAOEFirstEW = 35573, // Helper->self, 8.5s cast, range 60 width 8 rect
    FracturedEventideAOEFirstWE = 35910, // Helper->self, 8.5s cast, range 60 width 8 rect
    FracturedEventideAOERest = 35574, // Helper->self, no cast, range 60 width 8 rect

    RendTheRift = 35609, // Boss->self, 6.0s cast, range 60 circle, raidwide
    Nostalgia = 35610, // Boss->self, 5.0s cast, single-target, visual (multiple raidwides)
    NostalgiaBury1 = 35612, // Helper->self, 0.7s cast, range 60 circle
    NostalgiaBury2 = 35613, // Helper->self, 1.7s cast, range 60 circle
    NostalgiaBury3 = 35614, // Helper->self, 2.7s cast, range 60 circle
    NostalgiaBury4 = 35615, // Helper->self, 3.7s cast, range 60 circle
    NostalgiaRoar1 = 35616, // Helper->self, 5.7s cast, range 60 circle
    NostalgiaRoar2 = 35617, // Helper->self, 6.7s cast, range 60 circle
    NostalgiaPrimalRoar = 35618, // Helper->self, 9.7s cast, range 60 circle
    DimensionalSurgePuddle = 35633, // Helper->location, 4.0s cast, range 5 circle
    DimensionalSurgeStarCenter = 35634, // Helper->location, 4.0s cast, range 2 circle
    DimensionalSurgeStarLine = 35635, // Helper->self, 4.0s cast, range 60 width 2 rect (3 per star)

    FlowOfTheAbyss = 36089, // Boss->self, 7.0s cast, single-target, visual (spreads + unsafe line)
    FlowOfTheAbyssDimensionalSurge = 35637, // Helper->self, 9.0s cast, range 60 width 14 rect
    AkhRhaiFirst = 35619, // Helper->location, 3.0s cast, range 5 circle (at spread targets)
    AkhRhaiRest = 35620, // Helper->location, no cast, range 5 circle (10 repeats)
    ChasmicNails = 35622, // Boss->self, 7.0s cast, single-target, visual (pizzas)
    ChasmicNailsVisual1 = 35623, // Helper->self, 1.5s cast, range 60 40-degree cone, visual (telegraph)
    ChasmicNailsVisual2 = 35624, // Helper->self, 3.0s cast, range 60 40-degree cone, visual (telegraph)
    ChasmicNailsVisual3 = 35625, // Helper->self, 4.0s cast, range 60 40-degree cone, visual (telegraph)
    ChasmicNailsVisual4 = 35626, // Helper->self, 5.0s cast, range 60 40-degree cone, visual (telegraph)
    ChasmicNailsVisual5 = 35627, // Helper->self, 6.0s cast, range 60 40-degree cone, visual (telegraph)
    ChasmicNailsAOE1 = 35628, // Helper->self, 7.7s cast, range 60 40-degree cone
    ChasmicNailsAOE2 = 35629, // Helper->self, 8.4s cast, range 60 40-degree cone
    ChasmicNailsAOE3 = 35630, // Helper->self, 9.1s cast, range 60 40-degree cone
    ChasmicNailsAOE4 = 35631, // Helper->self, 9.8s cast, range 60 40-degree cone
    ChasmicNailsAOE5 = 35632 // Helper->self, 10.5s cast, range 60 40-degree cone
}

public enum SID : uint
{
    Doom = 1769, // Boss->player, extra=0x0
    AccelerationBomb = 2657, // none->player, extra=0x0
    DivisiveDark = 3762, // none->player, extra=0x0, spread when it expires
    BeckoningDark = 3763 // none->player, extra=0x0, stack when it expires
}

public enum IconID : uint
{
    DarkMatter = 364, // player
    DivisiveDark = 376, // player
    BeckoningDark = 100, // player
    AccelerationBomb = 267, // player
    Nox = 197, // player
    AkhRhai = 23, // player
    FracturedEventideEW = 501, // Boss
    FracturedEventideWE = 502 // Boss
}

public enum TetherID : uint
{
    VoidMeteorCloseClipping = 252, // Comet->player
    VoidMeteorCloseGood = 253, // Comet->player
    VoidMeteorStretchedClipping = 254, // Comet->player
    VoidMeteorStretchedGood = 255, // Comet->player
    FlowOfTheAbyss = 265 // FlowOfTheAbyss->Boss
}

sealed class Raidwides(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.AbyssalNox, (uint)AID.BigBang, (uint)AID.BigCrunch, (uint)AID.RendTheRift]);
sealed class NostalgiaHint(BossModule module) : Components.RaidwideCast(module, (uint)AID.Nostalgia, "Raidwide x7");
sealed class AbyssalEchoes(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AbyssalEchoes, 12f);
sealed class BigBangCrunchPuddles(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.BigBangAOE, (uint)AID.BigCrunchAOE], 5f);

sealed class AbyssalNoxDoom(BossModule module) : BossComponent(module)
{
    private BitMask _doomed;

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (_doomed.Any())
            hints.Add("Heal doomed players to full!");
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_doomed[slot] && actor.HPMP.CurHP < actor.HPMP.MaxHP)
            hints.Add("Get healed to full!");
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        => _doomed[playerSlot] && player.HPMP.CurHP < player.HPMP.MaxHP ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Doom)
            _doomed.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Doom)
            _doomed.Clear(Raid.FindSlot(actor.InstanceID));
    }
}

sealed class SableThread(BossModule module) : Components.GenericWildCharge(module, 6f, (uint)AID.SableThreadAOE, 60f)
{
    private int _hits;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == (uint)AID.SableThreadTarget)
        {
            Source = caster;
            _hits = 0;
            var party = Raid.WithSlot(true, true, true);
            for (var i = 0; i < party.Length; ++i)
            {
                var (slot, p) = party[i];
                PlayerRoles[slot] = p.InstanceID == spell.MainTargetID ? PlayerRole.Target : PlayerRole.Share;
            }
        }
        else if (spell.Action.ID == WatchedAction && ++_hits >= 5)
        {
            Source = null;
        }
    }
}

sealed class DarkMatter(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    private static readonly AOEShapeCircle _shape = new(8f);
    private int _remaining;

    public override void Update()
    {
        for (var i = CurrentBaits.Count - 1; i >= 0; --i)
        {
            var t = CurrentBaits[i].Target;
            if (t.IsDestroyed || t.IsDead)
                CurrentBaits.RemoveAt(i);
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.DarkMatter)
        {
            CurrentBaits.Add(new(Module.PrimaryActor, actor, _shape));
            _remaining = 3;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.DarkMatterAOE)
        {
            ++NumCasts;
            if (--_remaining <= 0)
                CurrentBaits.Clear();
        }
    }
}

sealed class VisceralWhirl(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeRect _shapeNormal = new(29f, 14f);
    private static readonly AOEShapeRect _shapeOffset = new(60f, 14f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.VisceralWhirlRAOE1:
            case (uint)AID.VisceralWhirlLAOE1:
                _aoes.Add(new(_shapeNormal, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
                break;
            case (uint)AID.VisceralWhirlRAOE2:
                _aoes.Add(new(_shapeOffset, caster.Position + _shapeOffset.HalfWidth * spell.Rotation.ToDirection().OrthoL(), spell.Rotation, Module.CastFinishAt(spell)));
                break;
            case (uint)AID.VisceralWhirlLAOE2:
                _aoes.Add(new(_shapeOffset, caster.Position + _shapeOffset.HalfWidth * spell.Rotation.ToDirection().OrthoR(), spell.Rotation, Module.CastFinishAt(spell)));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.VisceralWhirlRAOE1 or (uint)AID.VisceralWhirlRAOE2 or (uint)AID.VisceralWhirlLAOE1 or (uint)AID.VisceralWhirlLAOE2)
        {
            for (var i = _aoes.Count - 1; i >= 0; --i)
            {
                if (_aoes[i].Rotation.AlmostEqual(spell.Rotation, 0.05f))
                    _aoes.RemoveAt(i);
            }
        }
    }
}

sealed class VoidBio(BossModule module) : Components.Voidzone(module, 2.2f, GetBubbles)
{
    private static Actor[] GetBubbles(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.ToxicBubble);
        var count = enemies.Count;
        if (count == 0)
            return [];
        var bubbles = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            if (!enemies[i].IsDead)
                bubbles[index++] = enemies[i];
        }
        return bubbles[..index];
    }
}

sealed class FlareTower(BossModule module) : Components.CastTowers(module, (uint)AID.FlareAOE, 5f, 1, 8);

sealed class FlareScald(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle _shape = new(5f);
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.FlareAOE:
                _aoes.Add(new(_shape, caster.Position, default, WorldState.FutureTime(2d)));
                break;
            case (uint)AID.FlareScald:
                ++NumCasts;
                break;
            case (uint)AID.FlareKill:
                ++NumCasts;
                _aoes.Clear();
                break;
        }
    }
}

sealed class ProminenceSpine(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ProminenceSpine, new AOEShapeRect(60f, 5f));
sealed class Nox(BossModule module) : Components.StandardChasingAOEs(module, 10f, (uint)AID.NoxAOEFirst, (uint)AID.NoxAOERest, 2.5f, 1.55d, 3, icon: (uint)IconID.Nox);

sealed class NoxBait(BossModule module) : Components.UniformStackSpread(module, default, 10f)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Nox)
            AddSpread(actor, WorldState.FutureTime(10d));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.NoxAOEFirst)
            Spreads.Clear();
    }
}

sealed class AccelerationBomb(BossModule module) : Components.StayMove(module, 3d)
{
    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.AccelerationBomb)
        {
            var state = new PlayerState(Requirement.Stay, status.ExpireAt);
            SetState(Raid.FindSlot(actor.InstanceID), ref state);
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.AccelerationBomb)
            ClearState(Raid.FindSlot(actor.InstanceID));
    }
}

sealed class DarkBeckons(BossModule module) : Components.UniformStackSpread(module, 6f, default)
{
    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.BeckoningDark)
            AddStack(actor, status.ExpireAt);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.DarkBeckons)
            Stacks.Clear();
    }
}

sealed class DarkDivides(BossModule module) : Components.UniformStackSpread(module, default, 5f)
{
    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.DivisiveDark)
            AddSpread(actor, status.ExpireAt);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.DarkDivides)
            Spreads.Clear();
    }
}

sealed class MeteorImpactProximity(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MeteorImpactProximity, 10f); // falloff unverified
sealed class MeteorImpactExplosion(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MeteorImpactExplosion, 10f);

sealed class MeteorImpactCharge(BossModule module) : Components.GenericAOEs(module)
{
    private struct PlayerState
    {
        public Actor? TetherSource;
        public bool Stretched;
        public bool NonClipping;
    }

    private const float _radius = 2f;
    private const float _minStretch = 18f;
    private readonly List<WPos> _meteors = [];
    private readonly PlayerState[] _playerStates = new PlayerState[PartyState.MaxPartySize];
    private readonly List<PolygonCustom> _polygons = [];
    private AOEInstance[] _aoe = [];
    private DateTime _chargeDeadline;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _playerStates[slot].TetherSource != null ? _aoe : [];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        ref var state = ref _playerStates[slot];
        if (state.TetherSource != null)
        {
            if (!state.NonClipping)
                hints.Add("Avoid placed meteors!");
            if (!state.Stretched)
                hints.Add("Stretch the tether!");
        }
        if (IsClippedByOthers(actor))
            hints.Add("GTFO from charges!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_playerStates[slot].TetherSource is not Actor source)
            return;
        hints.AddForbiddenZone(new SDCircle(source.Position, _minStretch + 0.5f), _chargeDeadline);
        for (var i = 0; i < _meteors.Count; ++i)
        {
            var toMeteor = _meteors[i] - source.Position;
            hints.AddForbiddenZone(new SDCone(source.Position, 60f, Angle.FromDirection(toMeteor), Angle.Asin(_radius * 2f / toMeteor.Length())), _chargeDeadline);
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        => _playerStates[playerSlot].TetherSource != null ? PlayerPriority.Interesting : PlayerPriority.Normal;

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_aoe.Length == 0 && _meteors.Count != 0 && _playerStates[pcSlot].TetherSource is Actor source)
        {
            var center = Arena.Center;
            _polygons.Clear();
            for (var i = 0; i < _meteors.Count; ++i)
                _polygons.Add(new PolygonCustom(BuildShadowPolygon(source.Position - center, _meteors[i] - center, center)));
            var shape = new AOEShapeCustom(center, [.. _polygons]);
            _aoe = [new(shape, center, shapeDistance: shape.Distance(center, default))];
        }
        base.DrawArenaBackground(pcSlot, pc);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        for (var i = 0; i < _meteors.Count; ++i)
            Arena.ZoneCircleOutline(_meteors[i], _radius, Colors.Object);

        var party = Raid.WithSlot(true, true, true);
        for (var i = 0; i < party.Length; ++i)
        {
            var (slot, target) = party[i];
            ref var state = ref _playerStates[slot];
            if (state.TetherSource is not Actor source)
                continue;
            var thickness = slot == pcSlot ? 2f : 1f;
            Arena.AddLine(source.Position, target.Position, state.Stretched ? Colors.Safe : Colors.Danger, thickness);
            Arena.ZoneCircleOutline(target.Position, _radius, state.NonClipping ? Colors.Safe : Colors.Danger, thickness);
        }

        if (_playerStates[pcSlot].TetherSource is Actor pcSource)
            Arena.ZoneCircleOutline(pcSource.Position, _minStretch, Colors.Danger);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.VoidMeteor:
                _meteors.Clear();
                _aoe = [];
                break;
            case (uint)AID.MeteorImpact:
                Array.Fill(_playerStates, default);
                _aoe = [];
                _chargeDeadline = default;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.MeteorImpactProximity:
                _meteors.Add(caster.Position);
                _aoe = [];
                break;
            case (uint)AID.MeteorImpactChargeNormal:
            case (uint)AID.MeteorImpactChargeClipping:
                ++NumCasts;
                var party = Raid.WithSlot(true, true, true);
                var closest = -1;
                var best = float.MaxValue;
                for (var i = 0; i < party.Length; ++i)
                {
                    var d = (party[i].Item2.Position - spell.TargetXZ).LengthSq();
                    if (d < best)
                    {
                        best = d;
                        closest = party[i].Item1;
                    }
                }
                if (closest >= 0)
                    _playerStates[closest].TetherSource = null;
                break;
            case (uint)AID.MeteorImpactExplosion:
                _meteors.Clear();
                _aoe = [];
                break;
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID is (uint)TetherID.VoidMeteorCloseClipping or (uint)TetherID.VoidMeteorCloseGood or (uint)TetherID.VoidMeteorStretchedClipping or (uint)TetherID.VoidMeteorStretchedGood
            && Raid.FindSlot(tether.Target) is var slot && slot >= 0)
        {
            ref var state = ref _playerStates[slot];
            state.TetherSource = source;
            state.Stretched = tether.ID is (uint)TetherID.VoidMeteorStretchedClipping or (uint)TetherID.VoidMeteorStretchedGood;
            state.NonClipping = tether.ID is (uint)TetherID.VoidMeteorCloseGood or (uint)TetherID.VoidMeteorStretchedGood;
            if (_chargeDeadline == default)
                _chargeDeadline = WorldState.FutureTime(12d);
        }
    }

    private static WPos[] BuildShadowPolygon(WDir sourceOffset, WDir meteorOffset, WPos center)
    {
        var toMeteor = meteorOffset - sourceOffset;
        var dirToMeteor = Angle.FromDirection(toMeteor);
        var halfAngle = Angle.Asin(_radius * 2f / toMeteor.Length());
        var dirFromMeteor = dirToMeteor + 180f.Degrees();
        var halfAngleFromMeteor = 90f.Degrees() - halfAngle;
        var arc = CurveApprox.CircleArc(_radius * 2f, dirFromMeteor + halfAngleFromMeteor, dirFromMeteor - halfAngleFromMeteor, 0.01f);
        var count = arc.Length;
        var vertices = new WPos[count + 2];
        for (var i = 0; i < count; ++i)
            vertices[i] = meteorOffset + arc[i] + center;
        vertices[count] = sourceOffset + 100f * (dirToMeteor + halfAngle).ToDirection() + center;
        vertices[count + 1] = sourceOffset + 100f * (dirToMeteor - halfAngle).ToDirection() + center;
        return vertices;
    }

    private static bool IsClipped(WPos source, WPos target, WPos position) => position.InCircle(target, _radius) || position.InRect(source, target - source, _radius);

    private bool IsClippedByOthers(Actor player)
    {
        var party = Raid.WithSlot(true, true, true);
        for (var j = 0; j < party.Length; ++j)
        {
            var (i, p) = party[j];
            if (p != player && _playerStates[i].TetherSource is Actor src && IsClipped(src.Position, p.Position, player.Position))
                return true;
        }
        return false;
    }
}

sealed class BlackHoleDrop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BlackHoleAOE, 4f);

sealed class BlackHole(BossModule module) : BossComponent(module)
{
    public Actor? Voidzone;
    public WPos? SafeCorner { get; private set; }
    private DateTime _growthStart;
    private FracturedEventide? _laser;

    private static readonly WPos _cornerE = new(117f, 83.5f);
    private static readonly WPos _cornerW = new(83f, 83.5f);
    private const float _cornerRadius = 3f;
    private const float _startingRadius = 4f;
    private const float _maxRadius = 35f;
    private const float _growthPerSecond = 3.3f;

    private float CurrentRadius => _growthStart == default ? _startingRadius : Math.Min(_maxRadius, _startingRadius + _growthPerSecond * (float)(WorldState.CurrentTime - _growthStart).TotalSeconds);

    private WPos? SafeSpot => (_laser ??= Module.FindComponent<FracturedEventide>())?.Active == true ? null : SafeCorner;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Voidzone != null && actor.Position.InCircle(Voidzone.Position, CurrentRadius))
            hints.Add("GTFO from black hole!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Voidzone != null)
            hints.AddForbiddenZone(new SDCircle(Voidzone.Position, CurrentRadius + 1f));
        if (SafeSpot is WPos spot)
            hints.GoalZones.Add(AIHints.GoalSingleTarget(spot, _cornerRadius, 5f));
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (SafeSpot is WPos spot)
            Arena.ZoneCircle(spot, _cornerRadius, Colors.SafeFromAOE);
        if (Voidzone != null)
            Arena.ZoneCircle(Voidzone.Position, CurrentRadius, Colors.AOE);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (SafeSpot is WPos spot)
            Arena.ZoneCircleOutline(spot, _cornerRadius, Colors.Safe, 2f);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.FracturedEventideAOEFirstWE:
                SafeCorner = _cornerE;
                break;
            case (uint)AID.FracturedEventideAOEFirstEW:
                SafeCorner = _cornerW;
                break;
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.BlackHole)
            Voidzone = actor;
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor != Voidzone)
            return;
        switch (state)
        {
            case 0x00100008u:
                _growthStart = WorldState.CurrentTime;
                break;
            case 0x00040020u:
                Voidzone = null;
                _growthStart = default;
                SafeCorner = null;
                break;
        }
    }
}

sealed class FracturedEventide(BossModule module) : Components.GenericAOEs(module)
{
    private Actor? _source;
    private Angle _startingRotation;
    private Angle _increment;
    private DateTime _startingActivation;
    private BlackHole? _blackHole;
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shape = new(60f, 4f, 10f);
    private static readonly AOEShapeRect _shapeVisual = new(60f, 4f);
    private const int _maxCasts = 21;
    private const float _stepDuration = 0.45f;
    private const float _startRadius = 3f;
    private static readonly WDir _startOffset = new(-13.1f, 4.6f);

    public bool Active => _source != null && NumCasts < _maxCasts;

    private float Elapsed => (float)(WorldState.CurrentTime - _startingActivation).TotalSeconds;
    private Angle CurrentRotation => _startingRotation + Math.Clamp(Elapsed / _stepDuration, 0f, _maxCasts - 1) * _increment;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!Active)
            return [];
        _aoes.Clear();
        for (var i = NumCasts; i < _maxCasts; ++i)
            _aoes.Add(new(_shape, _source!.Position, _startingRotation + i * _increment, _startingActivation.AddSeconds(_stepDuration * i)));
        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (!Active)
            return;
        if (Elapsed < 0f)
        {
            var start = _source!.Position + new WDir(_increment.Rad > 0f ? _startOffset.X : -_startOffset.X, _startOffset.Z);
            hints.GoalZones.Add(AIHints.GoalSingleTarget(start, _startRadius, 5f));
        }
        else if ((_blackHole ??= Module.FindComponent<BlackHole>())?.SafeCorner is WPos corner)
        {
            hints.GoalZones.Add(AIHints.GoalProximity(corner, 30f, 1f));
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Active)
            _shapeVisual.Draw(Arena, _source!.Position, CurrentRotation, Colors.Danger);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.FracturedEventideAOEFirstWE or (uint)AID.FracturedEventideAOEFirstEW)
        {
            _source = caster;
            _startingRotation = spell.Rotation;
            _increment = _startingRotation.Rad > 0f ? -7f.Degrees() : 7f.Degrees();
            _startingActivation = Module.CastFinishAt(spell);
            NumCasts = 0;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.FracturedEventideAOEFirstWE or (uint)AID.FracturedEventideAOEFirstEW or (uint)AID.FracturedEventideAOERest)
            ++NumCasts;
    }
}

sealed class DimensionalSurgePuddle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DimensionalSurgePuddle, 5f);
sealed class DimensionalSurgeStarCenter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DimensionalSurgeStarCenter, 2f);
sealed class DimensionalSurgeStarLine(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DimensionalSurgeStarLine, new AOEShapeRect(60f, 1f));
sealed class FlowOfTheAbyssDimensionalSurge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FlowOfTheAbyssDimensionalSurge, new AOEShapeRect(60f, 7f));

sealed class FlowOfTheAbyssSpread(BossModule module) : Components.UniformStackSpread(module, default, 5f)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.AkhRhai)
            AddSpread(actor, WorldState.FutureTime(5d));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AkhRhaiFirst)
            Spreads.Clear();
    }
}

sealed class FlowOfTheAbyssAkhRhai(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCircle _shape = new(5f);
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AkhRhaiFirst)
        {
            if (_aoes.Count == 0)
                NumCasts = 0;
            _aoes.Add(new(_shape, spell.LocXZ, default, Module.CastFinishAt(spell)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.AkhRhaiRest && ++NumCasts >= _aoes.Count * 10)
            _aoes.Clear();
    }
}

sealed class ChasmicNails(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone _shape = new(60f, 20f.Degrees());
    private readonly List<AOEInstance> _all = [];
    private readonly AOEInstance[] _aoes = new AOEInstance[3];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = Math.Min(3, _all.Count);
        if (count == 0)
            return [];
        for (var i = 0; i < count; ++i)
        {
            var aoe = _all[i];
            if (i == 0 && _all.Count > 1)
                aoe.Color = Colors.Danger;
            _aoes[i] = aoe;
        }
        return _aoes.AsSpan(0, count);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ChasmicNailsAOE1 or (uint)AID.ChasmicNailsAOE2 or (uint)AID.ChasmicNailsAOE3 or (uint)AID.ChasmicNailsAOE4 or (uint)AID.ChasmicNailsAOE5)
        {
            _all.Add(new(_shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            _all.Sort(static (a, b) => a.Activation.CompareTo(b.Activation));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ChasmicNailsAOE1 or (uint)AID.ChasmicNailsAOE2 or (uint)AID.ChasmicNailsAOE3 or (uint)AID.ChasmicNailsAOE4 or (uint)AID.ChasmicNailsAOE5)
        {
            ++NumCasts;
            if (_all.Count != 0)
                _all.RemoveAt(0);
        }
    }
}

sealed class T07ZeromusStates : StateMachineBuilder
{
    public T07ZeromusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Raidwides>()
            .ActivateOnEnter<NostalgiaHint>()
            .ActivateOnEnter<AbyssalEchoes>()
            .ActivateOnEnter<BigBangCrunchPuddles>()
            .ActivateOnEnter<AbyssalNoxDoom>()
            .ActivateOnEnter<SableThread>()
            .ActivateOnEnter<DarkMatter>()
            .ActivateOnEnter<VisceralWhirl>()
            .ActivateOnEnter<VoidBio>()
            .ActivateOnEnter<FlareTower>()
            .ActivateOnEnter<FlareScald>()
            .ActivateOnEnter<ProminenceSpine>()
            .ActivateOnEnter<Nox>()
            .ActivateOnEnter<NoxBait>()
            .ActivateOnEnter<AccelerationBomb>()
            .ActivateOnEnter<DarkBeckons>()
            .ActivateOnEnter<DarkDivides>()
            .ActivateOnEnter<MeteorImpactProximity>()
            .ActivateOnEnter<MeteorImpactCharge>()
            .ActivateOnEnter<MeteorImpactExplosion>()
            .ActivateOnEnter<BlackHoleDrop>()
            .ActivateOnEnter<FracturedEventide>()
            .ActivateOnEnter<BlackHole>()
            .ActivateOnEnter<DimensionalSurgePuddle>()
            .ActivateOnEnter<DimensionalSurgeStarCenter>()
            .ActivateOnEnter<DimensionalSurgeStarLine>()
            .ActivateOnEnter<FlowOfTheAbyssDimensionalSurge>()
            .ActivateOnEnter<FlowOfTheAbyssSpread>()
            .ActivateOnEnter<FlowOfTheAbyssAkhRhai>()
            .ActivateOnEnter<ChasmicNails>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Nonu (ported from upstream with modifications by LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 964, NameID = 12586)]
public sealed class T07Zeromus(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsSquare(20f));
