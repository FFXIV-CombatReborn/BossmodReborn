namespace BossMod.Dawntrail.Advanced.Ad01TheMerchantsTale.Ad012DaryaTheSeamaid;

sealed class PiercingPlunge(BossModule module) : Components.RaidwideCast(module, (uint)AID.PiercingPlunge);
sealed class SurgingCurrent(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SurgingCurrent, new AOEShapeCone(60f, 45f.Degrees()));
sealed class AquaBall(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AquaBall, 5f);
sealed class Hydrocannon(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(70f, 3f), (uint)IconID.TankLaserLockon, (uint)AID.Hydrocannon);
sealed class HydrobulletSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.HydrobulletSpread, 15f);
sealed class AlluringOrder(BossModule module) : Components.StatusDrivenForcedMarch(module, 3f, default, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace)
{
    // draw expected AOE after forced march ends for other players?
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // TODO: add AI hints so it doesn't run out of arena
        // each player has icon hydrobullets that resolve after end of forced move
        // would need to calculate expected end position of each player and check that we don't get clipped by their AOE
    }
}
sealed class TidalSpout(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.Tidalspout, 6f, 1, 3)
{
    private BitMask _spreads = default;
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        if (id == StackAction && WorldState.Actors.Find(spell.TargetID) is Actor stackTarget)
        {
            AddStack(stackTarget, Module.CastFinishAt(spell), _spreads);
        }
        else if (id == (uint)AID.HydrobulletSpread)
        {
            var slot = Raid.FindSlot(spell.TargetID);
            _spreads.Set(slot);

            var stacks = CollectionsMarshal.AsSpan(Stacks);
            var len = stacks.Length;
            if (len == 0)
                return;

            for (var i = 0; i < Stacks.Count; ++i)
            {
                ref var s = ref stacks[i];
                s.ForbiddenPlayers = _spreads;
            }
        }
    }
}
sealed class Hydrobullet(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Hydrobullet, 15f)
{
    private BitMask _targets = default;
    private DateTime _activation = default;
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Hydrobullet)
        {
            var slot = Raid.FindSlot(spell.TargetID);
            if (slot == -1)
                return;

            var target = WorldState.Actors.Find(spell.TargetID);
            if (target == null)
                return;

            _targets.Set(slot);
            _activation = Module.CastFinishAt(spell);
            AddSpread(target, _activation);
        }
    }
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!_targets[slot])
            return;

        var spreads = CollectionsMarshal.AsSpan(ActiveSpreads);
        var spreadLen = spreads.Length;
        if (spreadLen == 0)
            return;

        var partyWS = Raid.WithSlot(false, true, true);
        var partylen = partyWS.Length;
        for (var i = 0; i < partylen; ++i)
        {
            var playerSlot = partyWS[i].Item1;
            if (!_targets[playerSlot] || slot == playerSlot)
                continue;

            var player = partyWS[i].Item2;
            hints.Add("Spread!", actor.DistanceToPoint(player.Position) <= SpreadRadius);
        }
    }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!_targets[slot])
            return;

        var spreads = CollectionsMarshal.AsSpan(ActiveSpreads);
        var spreadLen = spreads.Length;
        if (spreadLen == 0)
            return;

        var partyWS = Raid.WithSlot(false, true, true);
        var partylen = partyWS.Length;
        for (var i = 0; i < partylen; ++i)
        {
            var playerSlot = partyWS[i].Item1;
            if (!_targets[playerSlot] || slot == playerSlot)
                continue;

            var player = partyWS[i].Item2;
            hints.AddForbiddenZone(new AOEShapeCircle(SpreadRadius), player.Position, activation: _activation);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!_targets[pcSlot])
            return;
        base.DrawArenaForeground(pcSlot, pc);
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Hydrobullet)
        {
            _targets.Reset();
            _activation = default;
        }
        base.OnCastFinished(caster, spell);
    }
}
sealed class CeaselessCurrent(BossModule module) : Components.Exaflare(module, new AOEShapeRect(4f, 20f, 4f))
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var linesCount = Lines.Count;
        if (linesCount == 0)
        {
            return;
        }

        var imminentAOEs = ImminentAOEs(linesCount);

        // use only imminent aoes for hints
        var len = imminentAOEs.Length;
        for (var i = 0; i < len; ++i)
        {
            ref var aoe = ref imminentAOEs[i];
            hints.AddForbiddenZone(Shape, aoe.Item1, aoe.Item3, aoe.Item2);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.CeaselessCurrentFirst)
        {
            Lines.Add(new(caster.Position, 8f * caster.Rotation.Round(1f).ToDirection(), Module.CastFinishAt(spell), 2.1d, 5, 2, spell.Rotation));
        }
    }

    //public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.CeaselessCurrentFirst or (uint)AID.CeaselessCurrentRest)
        {
            var count = Lines.Count;
            var pos = caster.Position;
            for (var i = 0; i < count; ++i)
            {
                var line = Lines[i];
                if (line.Next.AlmostEqual(pos, 1f))
                {
                    AdvanceLine(line, pos);
                    if (line.ExplosionsLeft == 0)
                    {
                        Lines.RemoveAt(i);
                    }
                    return;
                }
            }
        }
    }
}
sealed class Hydrofall(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.HydrofallOrb)
        {
            _aoes.Add(new(new AOEShapeCircle(12f), actor.Position));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Hydrofall)
        {
            _aoes.Clear();
        }
    }
}
sealed class NearFarTide(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeCircle _inner = new(10f);
    private readonly AOEShapeDonut _outer = new(10f, 40f);
    private readonly List<AOEInstance> _aoes = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var len = aoes.Length;

        if (len == 0)
            return [];

        return aoes[..1];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.NearTide or (uint)AID.FarTide)
        {
            var activation = Module.CastFinishAt(spell);
            _aoes.Add(new(spell.Action.ID == (uint)AID.NearTide ? _inner : _outer, caster.Position, activation: activation));
            _aoes.Add(new(spell.Action.ID == (uint)AID.NearTide ? _outer : _inner, caster.Position, activation: activation.AddSeconds(3d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.NearTide or (uint)AID.NearTide1 or (uint)AID.FarTide or (uint)AID.FarTide1)
        {
            if (_aoes.Count > 0)
            {
                _aoes.RemoveAt(0);
            }
        }
    }
}
sealed class EchoedSerenade(BossModule module) : Components.GenericAOEs(module)
{
    // serenade casts are same, order by VFX?
    private const uint _steedvfx = 2741;
    private const uint _stalwartvfx = 2742;
    private const uint _stewardvfx = 2743;
    private const uint _soldiervfx = 2744;
    private readonly AOEShapeRect _shape = new(40f, 4f);
    private DateTime _castFinished = default;
    private int _counter = -1;

    private readonly List<Actor> _steeds = [];
    private readonly List<Actor> _stalwarts = [];
    private readonly List<Actor> _stewards = [];
    private readonly List<Actor> _soldiers = [];
    private readonly List<AOEInstance> _aoes = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var len = aoes.Length;

        if (len == 0)
            return [];

        var firstAct = aoes[0].Activation;
        var count = 1;
        while (count < len && aoes[count].Activation == firstAct)
        {
            ++count;
        }

        return aoes[..count];
    }

    public override void OnEventVFX(Actor actor, uint vfxID, ulong targetID)
    {
        // 1st happens roughly 3.8s after serenade ends, 2s between sets
        // happens in sets of 2 or 3; group by actor or activation time?
        if (actor.OID == (uint)OID.DaryaTheSeaMaid && vfxID is _steedvfx or _stalwartvfx or _stewardvfx or _soldiervfx)
        {
            _counter++;
            var activation = _castFinished.AddSeconds(3.8d + _counter * 2d);

            var group = vfxID switch
            {
                _steedvfx => _steeds,
                _stalwartvfx => _stalwarts,
                _stewardvfx => _stewards,
                _soldiervfx => _soldiers,
                _ => []
            };
            if (group.Count == 0)
                return;

            var actors = CollectionsMarshal.AsSpan(group);
            var actorlen = actors.Length;

            if (actorlen == 0)
                return;

            for (var i = 0; i < actorlen; ++i)
            {
                _aoes.Add(new(_shape, actors[i].Position, actors[i].Rotation, activation));
            }
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.SeabornSteed)
        {
            _steeds.Add(actor);
        }
        else if (actor.OID == (uint)OID.SeabornStalwart)
        {
            _stalwarts.Add(actor);
        }
        else if (actor.OID == (uint)OID.SeabornSteward)
        {
            _stewards.Add(actor);
        }
        else if (actor.OID == (uint)OID.SeabornSoldier)
        {
            _soldiers.Add(actor);
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.SeabornSteed)
        {
            _steeds.Clear();
        }
        else if (actor.OID == (uint)OID.SeabornStalwart)
        {
            _stalwarts.Clear();
        }
        else if (actor.OID == (uint)OID.SeabornSteward)
        {
            _stewards.Clear();
        }
        else if (actor.OID == (uint)OID.SeabornSoldier)
        {
            _soldiers.Clear();
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.EchoedSerenade)
        {
            _castFinished = Module.CastFinishAt(spell);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.Watersong1 or (uint)AID.Watersong2 or (uint)AID.Watersong3 or (uint)AID.Watersong4)
        {
            _counter = -1;
            if (_aoes.Count > 0)
            {
                _aoes.RemoveAt(0);
            }
        }
    }
}
sealed class SunkenTreasure(BossModule module) : Components.GenericAOEs(module)
{
    // spawns 2 orbs, 4 donuts
    // all except 1 orb explodes (2 safe spots inside donut) then remaining orb explodes
    // EObjAnim 00100020 -> shatter 1, 3s
    // EObjAnim 00400080 -> shatter 2, 3s, aoe imminent
    // Cast start 3s
    // EObjAnim always within same frame, or possible it can get staggered?
    private readonly List<AOEInstance> _aoes = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var len = aoes.Length;

        if (len == 0)
            return [];

        var firstAct = aoes[0].Activation;
        var count = 1;
        while (count < len && aoes[count].Activation == firstAct)
        {
            ++count;
        }

        return aoes[..count];
    }
    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00100020)
        {
            if (actor.OID == (uint)OID.SunkenTreasureOrb)
            {
                _aoes.Add(new(new AOEShapeCircle(18f), actor.Position, activation: WorldState.CurrentTime.AddSeconds(7.7d)));
            }
            else if (actor.OID == (uint)OID.SunkenTreasureDonut)
            {
                _aoes.Add(new(new AOEShapeDonut(4f, 20f), actor.Position, activation: WorldState.CurrentTime.AddSeconds(7.7d)));
            }
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.SphereShatter or (uint)AID.SphereShatterDonut)
        {
            if (_aoes.Count > 0)
            {
                _aoes.RemoveAt(0);
            }
        }
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, PrimaryActorOID = (uint)OID.DaryaTheSeaMaid, Contributors = "LTS, gynorhino", Category = BossModuleInfo.Category.VariantCriterion,
GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1084u, NameID = 14291u, SortOrder = 2)]
public sealed class Ad012DaryaTheSeamaid(WorldState ws, Actor primary) : BossModule(ws, primary, new(375f, 530f), new ArenaBoundsSquare(20f));
