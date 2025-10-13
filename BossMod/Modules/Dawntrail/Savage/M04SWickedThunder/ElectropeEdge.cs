namespace BossMod.Dawntrail.Savage.M04SWickedThunder;

sealed class ElectropeEdgeWitchgleam(BossModule module) : Components.GenericAOEs(module, (uint)AID.ElectropeEdgeWitchgleamAOE)
{
    private AOEInstance[] _aoe = [];

    private static readonly AOEShapeCross _shape = new(60f, 2.5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ElectropeEdgeWitchgleam)
        {
            _aoe = [new(_shape, spell.LocXZ, 45f.Degrees(), Module.CastFinishAt(spell, 1.2d))];
        }
    }
}

sealed class ElectropeEdgeSpark1(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ElectropeEdgeSpark1, new AOEShapeRect(10f, 5f));
sealed class ElectropeEdgeSpark2(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ElectropeEdgeSpark2, new AOEShapeRect(30f, 15f));

sealed class ElectropeEdgeSidewiseSpark(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ElectropeEdgeSidewiseSparkR, (uint)AID.ElectropeEdgeSidewiseSparkL], new AOEShapeCone(60f, 90f.Degrees()));

sealed class ElectropeEdgeStar(BossModule module) : Components.UniformStackSpread(module, 6f, 6f)
{
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.Marker)
        {
            switch (status.Extra)
            {
                case 0x2F0:
                    // TODO: can target any role, if not during cage?..
                    var cage = Module.FindComponent<LightningCage>();
                    var targets = Raid.WithSlot(true, true, true);
                    targets = cage != null ? [.. targets.WhereSlot(i => cage.Order[i] == 2)] : [.. targets.WhereActor(p => p.Class.IsSupport())];
                    AddStacks(targets.Actors(), status.ExpireAt.AddSeconds(1d));
                    break;
                case 0x2F1:
                    AddSpreads(Raid.WithoutSlot(true, true, true), status.ExpireAt.AddSeconds(1d));
                    break;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.FourStar or (uint)AID.EightStar)
        {
            Spreads.Clear();
            Stacks.Clear();
        }
    }
}

sealed class LightningCage(BossModule module) : Components.GenericAOEs(module, (uint)AID.LightningCageAOE)
{
    public int NumSparks;
    public int NumGleams;
    public readonly int[] Order = new int[PartyState.MaxPartySize];
    private readonly int[] _gleams = new int[PartyState.MaxPartySize];
    private BitMask _aoePattern;
    private DateTime _activation;

    public bool Active => _aoePattern.Any();

    private static readonly (BitMask pattern, int safe, int two1, int two2, int three1, int three2)[] _patterns =
    [
        (BitMask.Build(0, 1, 3,  4, 10, 18, 24, 26, 28, 33, 34, 35),  2,  8, 12, 32, 36),
        (BitMask.Build(1, 4, 8, 12, 16, 17, 18, 19, 24, 28, 33, 36), 20,  3, 35,  0, 32),
        (BitMask.Build(1, 2, 3,  8, 10, 12, 18, 26, 32, 33, 35, 36), 34, 24, 28,  0,  4),
        (BitMask.Build(0, 3, 8, 12, 17, 18, 19, 20, 24, 28, 32, 35), 16,  1, 33,  4, 36),
    ];

    private static readonly AOEShapeRect _cell = new(4f, 4f, 4f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var patternAOE = _aoePattern.SetBits();
        var safeCells = SafeCells(slot);
        var len = patternAOE.Length;
        var count = safeCells.Count;
        var aoes = new AOEInstance[len + count];
        for (var i = 0; i < len; ++i)
            aoes[i] = new(_cell, CellCenter(patternAOE[i]), default, _activation);
        for (var i = 0; i < count; ++i)
            aoes[i + len] = new(_cell, CellCenter(safeCells[i]), default, _activation, Colors.SafeFromAOE, false);
        return aoes;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (NumGleams >= 8)
        {
            var isActive = Active;
            var safeExists = false;

            if (isActive)
            {
                var safeCells = SafeCells(slot);
                var count = safeCells.Count;
                for (var i = 0; i < count; ++i)
                {
                    if (_cell.Check(actor.Position, CellCenter(safeCells[i]), default))
                    {
                        safeExists = true;
                        break;
                    }
                }
            }

            hints.Add($"Order: {Order[slot]}, position: {_gleams[slot] + Order[slot] - 1}", isActive && !safeExists);
        }
        base.AddHints(slot, actor, hints);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.ElectricalCondenser && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            Order[slot] = (status.ExpireAt - WorldState.CurrentTime).TotalSeconds < 30d ? 1 : 2;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            _aoePattern.Set(PositionToIndex(caster.Position));
            _activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            _aoePattern.Reset();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.LightningCageWitchgleamAOE:
                ++NumGleams;
                var targets = CollectionsMarshal.AsSpan(spell.Targets);
                var len = targets.Length;
                for (var i = 0; i < len; ++i)
                {
                    ref readonly var targ = ref targets[i];
                    if (Raid.FindSlot(targ.ID) is var slot && slot >= 0)
                    {
                        ++_gleams[slot];
                    }
                }
                break;
            case (uint)AID.LightningCageSpark2:
            case (uint)AID.LightningCageSpark3:
                ++NumSparks;
                break;
        }
    }

    private List<int> SafeCells(int slot)
    {
        var pattern = Array.FindIndex(_patterns, p => p.pattern == _aoePattern);
        if (pattern < 0)
            return [];
        var nextOrder = NumSparks switch
        {
            < 4 => 1,
            < 8 => 2,
            _ => 3
        };
        var cells = new List<int>(2);
        if (Order[slot] != nextOrder)
        {
            cells.Add(_patterns[pattern].safe);
        }
        else if (_gleams[slot] + nextOrder == 3)
        {
            cells.Add(_patterns[pattern].two1);
            cells.Add(_patterns[pattern].two2);
        }
        else if (_gleams[slot] + nextOrder == 4)
        {
            cells.Add(_patterns[pattern].three1);
            cells.Add(_patterns[pattern].three2);
        }
        return cells;
    }

    private static int CoordinateToIndex(float c) => c switch
    {
        < 88f => 0,
        < 96f => 1,
        < 104f => 2,
        < 112f => 3,
        _ => 4
    };
    private int PositionToIndex(WPos pos) => (CoordinateToIndex(pos.Z) << 3) | CoordinateToIndex(pos.X);
    private float CellCenterCoordinate(int index) => 84 + 8 * index;
    private WPos CellCenter(int index) => new(CellCenterCoordinate(index & 7), CellCenterCoordinate(index >> 3));
}

sealed class LightningCageWitchgleam(BossModule module) : Components.GenericBaitAway(module, (uint)AID.LightningCageWitchgleamAOE)
{
    private static readonly AOEShapeRect _shape = new(60f, 2.5f);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.LightningCageWitchgleam)
        {
            var party = Raid.WithoutSlot(true, true, true);
            var len = party.Length;
            for (var i = 0; i < len; ++i)
                CurrentBaits.Add(new(caster, party[i], _shape, Module.CastFinishAt(spell, 1.2d)));
        }
    }
}
