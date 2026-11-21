namespace BossMod.Dawntrail.Trial.T06Arkveld;

sealed class Roar(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.Roar1, (uint)AID.Roar2]);
sealed class ForgedFury(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.ForgedFuryHit1, (uint)AID.ForgedFuryHit2, (uint)AID.ForgedFuryHit3], "Raidwide x3");

sealed class ChainbladeBlow(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ChainbladeBlow1, (uint)AID.ChainbladeBlow2, (uint)AID.ChainbladeBlow3,
(uint)AID.ChainbladeBlow4, (uint)AID.ChainbladeBlow5, (uint)AID.ChainbladeBlow6, (uint)AID.ChainbladeBlow7, (uint)AID.ChainbladeBlow8], new AOEShapeRect(40f, 2f));
sealed class ChainbladeSide(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ChainbladeSide1, (uint)AID.ChainbladeSide2, (uint)AID.ChainbladeSide3,
(uint)AID.ChainbladeSide4], new AOEShapeRect(80f, 14f));

sealed class SiegeflightBoss(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.SiegeflightBoss1, (uint)AID.SiegeflightBoss2, (uint)AID.SiegeflightBoss3,
(uint)AID.SiegeflightBoss4], new AOEShapeRect(40f, 2f));
sealed class SiegeflightCast(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.SiegeflightCast1, (uint)AID.SiegeflightCast2, (uint)AID.SiegeflightCast3,
(uint)AID.SiegeflightCast4], new AOEShapeRect(40f, 4f));

abstract class AfterSiegeflight(BossModule module, uint[] aids, AOEShape shape) : Components.SimpleAOEGroups(module, aids, shape)
{
    private bool _siegeflightComplete;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _siegeflightComplete ? base.ActiveAOEs(slot, actor) : [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.SiegeflightBoss1 or (uint)AID.SiegeflightBoss2 or (uint)AID.SiegeflightBoss3 or (uint)AID.SiegeflightBoss4)
        {
            _siegeflightComplete = false;
        }
        base.OnCastStarted(caster, spell);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.SiegeflightCast1 or (uint)AID.SiegeflightCast2 or (uint)AID.SiegeflightCast3 or (uint)AID.SiegeflightCast4)
        {
            _siegeflightComplete = true;
        }
        base.OnEventCast(caster, spell);
    }
}

sealed class SiegeflightFollowup(BossModule module) : AfterSiegeflight(module, [(uint)AID.SiegeflightFollowup1, (uint)AID.SiegeflightFollowup2], new AOEShapeRect(40f, 8f));
sealed class WyvernsRadiance(BossModule module) : AfterSiegeflight(module, [(uint)AID.WyvernsRadiance1, (uint)AID.WyvernsRadiance2, (uint)AID.WyvernsRadiance3,
(uint)AID.WyvernsRadiance4], new AOEShapeRect(40f, 9f));

sealed class CrackedCrystalSmall(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.WyvernsRadiancePuddleSmall, (uint)AID.WyvernsRadianceCrystalSmall], 6f);
sealed class CrackedCrystalLarge(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.WyvernsRadiancePuddleLarge, (uint)AID.WyvernsRadianceCrystalLarge], 12f);

sealed class WyvernsRadianceExawave(BossModule module) : Components.Exaflare(module, new AOEShapeRect(8f, 20f))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WyvernsRadianceExawaveFirst)
        {
            var rot = caster.Rotation;
            Lines.Add(new(spell.LocXZ, 8f * rot.ToDirection(), Module.CastFinishAt(spell), 2.5d, 5, 2, rot));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.WyvernsRadianceExawaveFirst or (uint)AID.WyvernsRadianceExawaveRest)
        {
            var count = Lines.Count;
            var pos = caster.Position;
            for (var i = 0; i < count; ++i)
            {
                var line = Lines[i];
                if (line.Next.AlmostEqual(pos, 1f))
                {
                    AdvanceLine(line, pos);
                    if (line.ExplosionsLeft <= 0)
                    {
                        Lines.RemoveAt(i);
                    }
                    return;
                }
            }
        }
    }
}

sealed class WyvernsRadianceQuake(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(8f), new AOEShapeDonut(8f, 14f), new AOEShapeDonut(14f, 20f), new AOEShapeDonut(20f, 26f)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WyvernsRadianceQuake1)
        {
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (Sequences.Count == 0)
        {
            return;
        }
        var order = spell.Action.ID switch
        {
            (uint)AID.WyvernsRadianceQuake1 => 0,
            (uint)AID.WyvernsRadianceQuake2 => 1,
            (uint)AID.WyvernsRadianceQuake3 => 2,
            (uint)AID.WyvernsRadianceQuake4 => 3,
            _ => -1
        };
        if (order >= 0)
        {
            AdvanceSequence(order, spell.LocXZ, WorldState.FutureTime(2d));
        }
    }
}

sealed class WyveCannonMiddle(BossModule module) : Components.Exaflare(module, new AOEShapeRect(40f, 2f, 40f))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WyveCannonMiddle)
        {
            var rot = caster.Rotation;
            var right = rot.ToDirection().OrthoR();
            var act = Module.CastFinishAt(spell);
            var pos = caster.Position;
            Lines.Add(new(pos + 2f * right, 4f * right, act, 2.6d, 5, 2, rot));
            Lines.Add(new(pos - 2f * right, -4f * right, act, 2.6d, 5, 2, rot));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var id = spell.Action.ID;
        if (id == (uint)AID.WyveCannonMiddle)
        {
            ++NumCasts;
            var pos = caster.Position;
            var rot = caster.Rotation;
            var count = Lines.Count;
            for (var i = 0; i < count; ++i)
            {
                var l = Lines[i];
                if (l.Next.AlmostEqual(pos, 4f) && l.Rotation.AlmostEqual(rot, 0.1f))
                {
                    AdvanceLine(l, pos + 0.5f * l.Advance);
                }
            }
            RemoveFinished();
        }
        else if (id == (uint)AID.WyveCannonRepeat)
        {
            var pos = caster.Position;
            var count = Lines.Count;
            for (var i = 0; i < count; ++i)
            {
                var l = Lines[i];
                if (l.Next.AlmostEqual(pos, 0.1f))
                {
                    ++NumCasts;
                    AdvanceLine(l, pos);
                    RemoveFinished();
                    return;
                }
            }
        }
    }

    private void RemoveFinished()
    {
        for (var i = Lines.Count - 1; i >= 0; --i)
        {
            if (Lines[i].ExplosionsLeft <= 0)
            {
                Lines.RemoveAt(i);
            }
        }
    }
}

sealed class WyveCannonEdge(BossModule module) : Components.Exaflare(module, new AOEShapeRect(40f, 2f, 40f))
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WyveCannonEdge)
        {
            var pos = caster.Position;
            Lines.Add(new(pos, 4f * (Arena.Center - pos).Normalized(), Module.CastFinishAt(spell), 2.6d, 10, 2, caster.Rotation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is not ((uint)AID.WyveCannonEdge or (uint)AID.WyveCannonRepeat))
        {
            return;
        }
        var count = Lines.Count;
        if (count == 0)
        {
            return;
        }
        var maxLeft = -1;
        for (var i = 0; i < count; ++i)
        {
            if (Lines[i].ExplosionsLeft > maxLeft)
            {
                maxLeft = Lines[i].ExplosionsLeft;
            }
        }
        var pos = caster.Position;
        for (var i = 0; i < count; ++i)
        {
            var l = Lines[i];
            if (l.ExplosionsLeft == maxLeft && l.Next.AlmostEqual(pos, 0.5f))
            {
                ++NumCasts;
                AdvanceLine(l, pos);
                if (l.ExplosionsLeft <= 0)
                {
                    Lines.RemoveAt(i);
                }
                return;
            }
        }
    }
}

sealed class Rush(BossModule module) : Components.GenericAOEs(module, (uint)AID.RushCast)
{
    private readonly List<AOEInstance> _aoes = [with(4)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count > 1)
        {
            _aoes.Ref(0).Color = Colors.Danger;
        }
        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RushTelegraph)
        {
            var from = caster.Position;
            var dir = spell.LocXZ - from;
            var rot = Angle.FromDirection(dir);
            var shape = new AOEShapeRect(dir.Length(), 6f);
            _aoes.Add(new(shape, from, rot, Module.CastFinishAt(spell), actorID: caster.InstanceID, shapeDistance: shape.Distance(from, rot)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.RushCast or (uint)AID.RushInstant)
        {
            ++NumCasts;
            var dest = spell.TargetXZ;
            var count = _aoes.Count;
            for (var i = 0; i < count; ++i)
            {
                ref var aoe = ref _aoes.Ref(i);
                if ((aoe.Origin + ((AOEShapeRect)aoe.Shape).LengthFront * aoe.Rotation.ToDirection()).AlmostEqual(dest, 1f))
                {
                    _aoes.RemoveAt(i);
                    return;
                }
            }
            if (count != 0)
            {
                _aoes.RemoveAt(0);
            }
        }
    }
}

sealed class ChainbladeCharge(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Share, (uint)AID.ChainbladeChargeStack, 6f, 8.3d, 5, PartyState.MaxPartySize);

sealed class WyvernsOuroblade(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.WyvernsOuroblade1, (uint)AID.WyvernsOuroblade2, (uint)AID.WyvernsOuroblade3,
(uint)AID.WyvernsOuroblade4], new AOEShapeCone(40f, 90f.Degrees()));

sealed class SteeltailThrust(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SteeltailThrust, new AOEShapeRect(60f, 3f));
sealed class WildEnergy(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.WildEnergy, 6f);

sealed class GuardianResonancePuddle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GuardianResonance1, 6f);
sealed class GuardianResonanceTowerSmall(BossModule module) : Components.CastTowers(module, (uint)AID.GuardianResonance2, 2f);

sealed class GuardianResonanceTowerLarge(BossModule module) : Components.CastTowers(module, (uint)AID.GuardianResonance3, 4f, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    public override void Update()
    {
        base.Update();
        var count = Towers.Count;
        if (count == 0)
        {
            return;
        }
        BitMask forbid = default;
        var party = Raid.WithSlot(false, true, true);
        var len = party.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var p = ref party[i];
            if (p.Item2.Role != Role.Tank)
            {
                forbid.Set(p.Item1);
            }
        }
        var towers = CollectionsMarshal.AsSpan(Towers);
        for (var i = 0; i < count; ++i)
        {
            towers[i].ForbiddenSoakers = forbid;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (actor.Role == Role.Tank)
        {
            return;
        }
        var towers = CollectionsMarshal.AsSpan(Towers);
        var len = towers.Length;
        for (var i = 0; i < len; ++i)
        {
            ref readonly var t = ref towers[i];
            hints.AddForbiddenZone(new SDCircle(t.Position, 4f), t.Activation);
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.GuardianArkveld, Contributors = "croizat, ported from upstream by LTS", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1043u, NameID = 14237u)]
public sealed class GuardianArkveld(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsCircle(20f));
