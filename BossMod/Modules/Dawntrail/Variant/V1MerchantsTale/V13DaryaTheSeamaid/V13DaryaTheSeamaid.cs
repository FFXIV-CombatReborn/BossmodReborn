namespace BossMod.Dawntrail.VariantCriterion.V1MerchantsTale.V13DaryaTheSeamaid;

sealed class PiercingPlunge(BossModule module) : Components.RaidwideCast(module, (uint)AID.PiercingPlunge);
sealed class SurgingCurrent(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SurgingCurrent, new AOEShapeCone(60f, 90f.Degrees()));
sealed class AquaBall(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.AquaBall, (uint)AID.AquaBall1], 5f);
sealed class Hydrocannon(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(70f, 3f), (uint)IconID.TankLaserLockon, (uint)AID.Hydrocannon);
//sealed class BigWave(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.BigWave, 36f, kind: Kind.DirForward);
sealed class BigWave(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.BigWave, 36f, kind: Kind.DirForward)
{
    private readonly List<SDCircle> _aquaballs = [];
    private readonly List<WPos> _aquapos = [];
    private DateTime _activation = default;
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if (spell.Action.ID == (uint)AID.AquaBall1)
        {
            _aquaballs.Add(new(caster.Position, 5f));
            _aquapos.Add(caster.Position);
            _activation = Module.CastFinishAt(spell);
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
        if (spell.Action.ID == (uint)AID.AquaBall1)
        {
            _aquaballs.Clear();
            _aquapos.Clear();
            _activation = default;
        }
    }
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (IsKnockbackIntoAOE(slot, actor))
        {
            hints.Add("About to be knocked into danger!");
        }
        else
        {
            base.AddHints(slot, actor, hints);
        }
    }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var kbs = ActiveKnockbacks(slot, actor);
        if (kbs.Length == 0)
            return;

        var kb = kbs[0];
        var dir = kb.Direction.ToDirection() * kb.Distance;

        var aquaballs = _aquapos.ToArray();
        var len = aquaballs.Length;
        hints.AddForbiddenZone(new SDKnockbackInAABBRectFixedDirectionPlusAOECircles(Arena.Center, dir, 20f, 20f, aquaballs, 5f, len), _activation);
    }
    private bool IsKnockbackIntoAOE(int slot, Actor actor)
    {
        var kbs = ActiveKnockbacks(slot, actor);
        if (kbs.Length == 0)
            return false;

        var aquaballs = CollectionsMarshal.AsSpan(_aquaballs);
        var len = aquaballs.Length;
        if (len == 0)
            return false;

        var kb = kbs[0];
        var dir = kb.Direction.ToDirection() * kb.Distance;
        var est = actor.Position + dir;

        for (var i = 0; i < len; ++i)
        {
            if (aquaballs[i].Contains(est))
                return true;
        }

        return false;
    }
}
sealed class AlluringOrder(BossModule module) : Components.StatusDrivenForcedMarch(module, 3f, (uint)SID.ForwardMarch, 0, 0, 0);
sealed class SirenSphere(BossModule module) : Components.Voidzone(module, 2f, GetSirenSpheres, 3f)
{
    public static Actor[] GetSirenSpheres(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.SirenSphere);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.Renderflags == 0 && z.EventState != 7 && !z.IsDeadOrDestroyed)
                voidzones[index++] = z;
        }
        return voidzones[..index];
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
sealed class AquaSpear(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AquaSpear)
        {
            _aoes.Add(new(new AOEShapeRect(4f, 4f, 4f), caster.Position));
        }
    }
    public override void OnActorDestroyed(Actor actor)
    {
        if (actor.OID == (uint)OID.AquaSpearVoidzone)
        {
            _aoes.Clear();
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
sealed class EchoedSerenade(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _upcomingBuffer = [];
    // serenade casts are same, order by VFX?
    private const uint _steedvfx = 2741;
    private const uint _stalwartvfx = 2742;
    private readonly AOEShapeRect _shape = new(40f, 4f);
    private DateTime _castFinished = default;

    private readonly List<Actor> _steeds = [];
    private readonly List<Actor> _stalwarts = [];
    private readonly List<AOEInstance> _aoes = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var len = aoes.Length;

        if (len == 0)
            return [];

        var upcoming = _upcomingBuffer;
        upcoming.Clear();
        var firstAct = aoes[0].Activation;

        for (var i = 0; i < len; ++i)
        {
            if (aoes[i].Activation == firstAct)
            {
                upcoming.Add(aoes[i]);
            }
        }

        return CollectionsMarshal.AsSpan(upcoming);
    }

    public override void OnEventVFX(Actor actor, uint vfxID, ulong targetID)
    {
        // 1st happens roughly 3.8s after serenade ends, 2s between sets
        // happens in sets of 2 or 3; group by actor or activation time?
        if (actor.OID == (uint)OID.DaryaTheSeaMaid && vfxID is _steedvfx or _stalwartvfx)
        {
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            var len = aoes.Length;
            var activation = _castFinished.AddSeconds(3.8d + len * 2d);

            var actors = CollectionsMarshal.AsSpan(vfxID == _steedvfx ? _steeds : _stalwarts);
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
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.EchoedSerenade or (uint)AID.EchoedSerenade1)
        {
            _castFinished = Module.CastFinishAt(spell);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.Watersong or (uint)AID.Watersong1)
        {
            if (_aoes.Count > 0)
            {
                _aoes.RemoveAt(0);
            }
        }
    }
}
sealed class SunkenTreasure(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _upcomingBuffer = [];
    // spawns 4 orbs
    // 2-3 explode at a time
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

        var upcoming = _upcomingBuffer;
        upcoming.Clear();
        var firstAct = aoes[0].Activation;

        for (var i = 0; i < len; ++i)
        {
            if (aoes[i].Activation == firstAct)
            {
                upcoming.Add(aoes[i]);
            }
        }

        return CollectionsMarshal.AsSpan(upcoming);
    }
    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == (uint)OID.SunkenTreasureOrb && state == 0x00100020)
        {
            _aoes.Add(new(new AOEShapeCircle(18f), actor.Position, activation: WorldState.CurrentTime.AddSeconds(7.7d)));
        }
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SphereShatter)
        {
            if (_aoes.Count > 0)
            {
                _aoes.RemoveAt(0);
            }
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

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
StatesType = typeof(V13DaryaTheSeamaidStates),
ConfigType = null,
ObjectIDType = typeof(OID),
ActionIDType = typeof(AID),
StatusIDType = typeof(SID),
TetherIDType = null,
IconIDType = typeof(IconID),
PrimaryActorOID = (uint)OID.DaryaTheSeaMaid,
Contributors = "gynorhino",
Expansion = BossModuleInfo.Expansion.Dawntrail,
Category = BossModuleInfo.Category.VariantCriterion,
GroupType = BossModuleInfo.GroupType.CFC,
GroupID = 1066u,
NameID = 14291u,
SortOrder = 4,
PlanLevel = 0)]
public class V13DaryaTheSeamaid(WorldState ws, Actor primary) : BossModule(ws, primary, new(375, 530), new ArenaBoundsSquare(20f));
