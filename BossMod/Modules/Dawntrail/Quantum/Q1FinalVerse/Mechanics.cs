namespace BossMod.Dawntrail.Quantum.Q1FinalVerse;

sealed class BladeOfFirstLight(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.BladeOfFirstLight1, (uint)AID.BladeOfFirstLight2], new AOEShapeRect(30f, 7.5f));
sealed class Eruption(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Eruption, 6f);
sealed class UnholyDarkness(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.UnholyDarkness1, (uint)AID.UnholyDarkness2]);
sealed class SelfDestruct(BossModule module) : Components.RaidwideCast(module, (uint)AID.SelfDestruct);
sealed class ManifoldLashingsTail(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ManifoldLashingsEnd, new AOEShapeRect(42f, 4.5f));
sealed class Vodoriga(BossModule module) : Components.Adds(module, (uint)OID.VodorigaMinion, forbidDots: true);

sealed class ChainsOfCondemnation(BossModule module) : Components.StayMove(module, 2d)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.ChainsOfCondemnation1 or (uint)AID.ChainsOfCondemnation2)
            Array.Fill(PlayerStates, new(Requirement.Stay2, Module.CastFinishAt(spell, 0.1d)));
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.ChainsOfCondemnation && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            PlayerStates[slot] = default;
            var party = Raid.WithSlot(true, false, false);
            for (var i = 0; i < party.Length; ++i)
            {
                if (party[i].Item2.IsDead)
                    PlayerStates[party[i].Item1] = default;
            }
        }
    }
}

sealed class BallOfFireBait(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private static readonly AOEShapeCircle _circle = new(6f);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.BallOfFireVisual1:
            case (uint)AID.BallOfFireVisual2:
                CurrentBaits.Clear();
                var party = Raid.WithoutSlot(false, true, true);
                var activation = Module.CastFinishAt(spell, 0.1d);
                for (var i = 0; i < party.Length; ++i)
                    CurrentBaits.Add(new(Module.PrimaryActor, party[i], _circle, activation));
                break;
            case (uint)AID.BallOfFire:
                CurrentBaits.Clear();
                break;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints) { }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { }
}

sealed class SearingChain(BossModule module) : Components.Chains(module, (uint)TetherID.SearingChains, chainLength: 20f);

sealed class SearingChainCross(BossModule module) : Components.GenericBaitAway(module, (uint)AID.SearingChains, centerAtTarget: true)
{
    private static readonly AOEShapeCross _cross = new(50f, 3f);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (CurrentBaits.Count == 0)
            return;
        var activation = CurrentBaits[0].Activation;
        var party = Raid.WithoutSlot();
        for (var i = 0; i < party.Length; ++i)
        {
            var p = party[i];
            if (p != actor)
                hints.AddForbiddenZone(new SDCross(p.Position, default, 50f, 3f), activation);
        }
        hints.AddPredictedDamage(Raid.WithSlot().Mask(), activation.AddSeconds(1.1d));
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.SearingChains)
            CurrentBaits.Add(new(Module.PrimaryActor, actor, _cross, WorldState.FutureTime(5.6d), customRotation: default(Angle)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            CurrentBaits.Clear();
        }
    }
}

sealed class Spinelash(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect _line = new(60f, 4f, invertForbiddenZone: true);
    private const float WindowZ = -284f;
    private Actor? _target;
    private DateTime _activation;
    private int _intactWindows = 0b111;

    private static float WindowCenterX(int window) => -614f + window * 14f;

    private bool AimedAtWindow(WPos pos)
    {
        for (var w = 0; w < 3; ++w)
        {
            if ((_intactWindows & (1 << w)) != 0 && MathF.Abs(pos.X - WindowCenterX(w)) < 7f)
                return true;
        }
        return false;
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_target == null || _target == actor)
            return [];
        var origin = new WPos(_target.Position.X, Module.PrimaryActor.Position.Z);
        return new AOEInstance[1] { new(_line, origin, default, _activation, Colors.SafeFromAOE) };
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_target == null)
            return;
        if (_target == actor)
            hints.Add("Aim for a window!", !AimedAtWindow(actor.Position));
        else
        {
            var aoes = ActiveAOEs(slot, actor);
            if (aoes.Length != 0)
                hints.Add("Share the charge!", !aoes[0].Check(actor.Position));
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_target != pc)
            return;
        var color = AimedAtWindow(pc.Position) ? Colors.Safe : Colors.Danger;
        for (var w = 0; w < 3; ++w)
        {
            if ((_intactWindows & (1 << w)) != 0)
                Arena.AddLine(new WPos(WindowCenterX(w) + 7f, WindowZ), new WPos(WindowCenterX(w) - 7f, WindowZ), color, 2f);
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Spinelash)
        {
            _target = actor;
            _activation = WorldState.FutureTime(10.5d);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Spinelash)
        {
            ++NumCasts;
            _target = null;
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        var window = index - 0x18;
        if (window is >= 0 and <= 2 && state == 0x00020001u)
            _intactWindows &= ~(1 << window);
    }
}

sealed class ShackleSpreadHint(BossModule module) : Components.GenericStackSpread(module, raidwideOnResolve: false)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID != (uint)AID.ShacklesOfGreaterSanctity)
            return;
        var activation = Module.CastFinishAt(spell);
        var party = Raid.WithoutSlot();
        for (var i = 0; i < party.Length; ++i)
        {
            var p = party[i];
            if (p.Role == Role.Healer)
                Spreads.Add(new(p, 21f, activation));
            else if (p.Role != Role.Tank)
                Spreads.Add(new(p, 8f, activation));
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.ShackledHealing)
            Spreads.Clear();
    }
}

sealed class ShackleHint(BossModule module) : BossComponent(module)
{
    private Actor? _healer;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.ShackledHealing)
            _healer = actor;
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.ShackledHealing && actor == _healer)
            _healer = null;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_healer != null)
            Arena.ZoneCircleOutline(_healer.Position, 21f, Colors.Danger);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor != _healer)
            return;
        var count = 0;
        var party = Raid.WithoutSlot();
        for (var i = 0; i < party.Length; ++i)
        {
            var p = party[i];
            if (p != actor && p.Position.InCircle(actor.Position, 21f))
                ++count;
        }
        hints.Add($"Allies in radius: {count}", false);
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        => player == _healer ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;
}

sealed class ArcaneFont(BossModule module) : Components.Adds(module, (uint)OID.ArcaneFont, forbidDots: true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var jailed = actor.FindStatus((uint)SID.HellishEarth) != null;
        var fonts = Module.Enemies((uint)OID.ArcaneFont);
        for (var i = 0; i < fonts.Count; ++i)
        {
            if (hints.FindEnemy(fonts[i]) is AIHints.Enemy e)
            {
                e.Priority = jailed ? AIHints.Enemy.PriorityInvincible : 0;
                e.ForbidDOTs = true;
            }
        }
    }
}

sealed class HellishEarthPull(BossModule module) : Components.GenericKnockback(module)
{
    private Actor? _caster;
    private Actor? _target;
    private readonly Knockback[] _kb = new Knockback[1];

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (_caster?.CastInfo is not ActorCastInfo cast)
            return [];
        var distance = actor == _target ? 60f : 10f;
        return _kb.Single(new(_caster.Position, distance, Module.CastFinishAt(cast), kind: Kind.TowardsOrigin, ignoreImmunes: true));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor == _target)
        {
            if (actor.Role != Role.Tank)
                hints.Add("Too far from boss!");
        }
        else if (_target != null && actor.Role == Role.Tank)
        {
            hints.Add("Go far to bait tether!");
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HellishEarth2)
            _caster = caster;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.HellishEarth1 or (uint)AID.HellishEarth2)
        {
            _target = null;
            _caster = null;
            ++NumCasts;
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.HellishEarth)
            _target = WorldState.Actors.Find(tether.Target);
    }
}

sealed class ManifoldLashingsTower(BossModule module) : Components.GenericTowers(module, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID != (uint)AID.ManifoldLashingsFirst)
            return;
        BitMask nonTanks = default;
        var party = Raid.WithSlot();
        for (var i = 0; i < party.Length; ++i)
        {
            var (slot, p) = party[i];
            if (p.Role != Role.Tank)
                nonTanks.Set(slot);
        }
        Towers.Add(new(caster.Position, 2f, forbiddenSoakers: nonTanks, activation: Module.CastFinishAt(spell)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ManifoldLashingsFirst:
            case (uint)AID.ManifoldLashingsRest:
                ++NumCasts;
                break;
            case (uint)AID.ManifoldLashingsEnd:
                Towers.Clear();
                break;
        }
    }
}

sealed class Flameborn(BossModule module) : Components.Adds(module, (uint)OID.Flameborn)
{
    private readonly Dictionary<Actor, Actor> _tethers = [];

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (source.OID == (uint)OID.Flameborn && WorldState.Actors.Find(tether.Target) is Actor target)
            _tethers[source] = target;
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether) => _tethers.Remove(source);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var adds = Module.Enemies((uint)OID.Flameborn);
        for (var i = 0; i < adds.Count; ++i)
        {
            var add = adds[i];
            if (hints.FindEnemy(add) is AIHints.Enemy e)
            {
                e.Priority = _tethers.ContainsKey(add) ? -1 : AIHints.Enemy.PriorityPointless;
                e.ForbidDOTs = true;
            }
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var (a, b) in _tethers)
            Arena.AddLine(a.Position, b.Position, Colors.Danger);
    }
}

sealed class FlamebornAura(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _adds = [];
    private readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        _aoes.Clear();
        for (var i = 0; i < _adds.Count; ++i)
        {
            var a = _adds[i];
            _aoes.Add(new(new AOEShapeCircle(4f * (a.HitboxRadius / 2.6f)), a.Position));
        }
        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void OnActorTargetable(Actor actor)
    {
        if (actor.OID == (uint)OID.Flameborn)
            _adds.Add(actor);
    }

    public override void OnActorUntargetable(Actor actor) => _adds.Remove(actor);
}

sealed class CrimeAndPunishmentHint(BossModule module) : Components.CastHint(module, (uint)AID.CrimeAndPunishmentVisual, "Get the dark debuff before the rot lands!");

sealed class SinBearer(BossModule module) : BossComponent(module)
{
    private Actor? _bearer;
    private int _stacks;
    private BitMask _immune;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.SinBearer:
                _bearer = actor;
                _stacks = status.Extra;
                break;
            case (uint)SID.WithoutSin:
                _immune.Set(Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.SinBearer && actor == _bearer)
            _bearer = null;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor == _bearer && _stacks >= 10)
            hints.Add("Pass the debuff to someone without it!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_bearer != null && !_immune[pcSlot])
            Arena.ZoneCircleOutline(_bearer.Position, 4f, _bearer == pc ? Colors.Safe : Colors.Danger);
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        => player == _bearer ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;
}

sealed class Doom(BossModule module) : BossComponent(module)
{
    private Actor? _victim;

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Doom)
            _victim = actor;
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Doom && _victim == actor)
            _victim = null;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_victim is Actor t && t.PendingDispels.Count == 0 && actor.Class.CanEsuna())
            hints.Add($"Cleanse {t.Name}!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_victim != null && Raid.FindSlot(_victim.InstanceID) is var v && v >= 0)
            hints.ShouldCleanse.Set(v);
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        => pc.Class.CanEsuna() && player == _victim ? PlayerPriority.Critical : PlayerPriority.Irrelevant;
}
