namespace BossMod.Endwalker.VariantCriterion.C02AMR.C023Moko;

// the main complexity is that first status and cast-start happen at the same time, so we can receive them in arbitrary order
// we need cast to know proper rotation (we can't use actor's rotation, since it's interpolated)
sealed class TripleKasumiGiri(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<string> _hints = [];
    private readonly List<Angle> _directionOffsets = [];
    private BitMask _ins; // [i] == true if i'th aoe is in
    private readonly List<AOEInstance> _aoes = [];

    private readonly AOEShapeCone _shapeCone = new(60f, 135f.Degrees());
    private readonly AOEShapeCircle _shapeOut = new(6f);
    private readonly AOEShapeDonut _shapeIn = new(6f, 40f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
            return [];
        var max = count > 2 ? 2 : count;
        return CollectionsMarshal.AsSpan(_aoes)[..max];
    }

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (_hints.Count > 0)
            hints.Add($"Safespots: {string.Join(" > ", _hints)}");
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Giri)
        {
            var (dir, donut, hint) = status.Extra switch
            {
                0x24C => (default, false, "out/back"),
                0x24D => (-90f.Degrees(), false, "out/left"),
                0x24E => (180f.Degrees(), false, "out/front"),
                0x24F => (90f.Degrees(), false, "out/right"),
                0x250 => (default, true, "in/back"),
                0x251 => (-90f.Degrees(), true, "in/left"),
                0x252 => (180f.Degrees(), true, "in/front"),
                0x253 => (90f.Degrees(), true, "in/right"),
                _ => (default, false, "")
            };
            if (hint.Length == 0)
            {
                ReportError($"Unexpected extra {status.Extra:X}");
                return;
            }

            _ins[_directionOffsets.Count] = donut;
            _directionOffsets.Add(dir);
            _hints.Add(hint);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var (dir, donut, order) = ClassifyAction(spell.Action);
        if (order < 0)
            return; // irrelevant spell

        if (order == 0)
        {
            // first cast: create first aoe pair, first status may or may not be known yet, so verify consistency on cast-finish
            if (_aoes.Count > 0 || NumCasts > 0)
                ReportError("Unexpected state on first cast start");
        }
        else if (order > 0)
        {
            // subsequent casts: ensure predicted state is correct, then update aoes in case it was not
            if (NumCasts == 0 || NumCasts >= 3 || _aoes.Count != 2)
                ReportError("Unexpected state on subsequent cast");
            var mismatch = _aoes.FindIndex(aoe => !aoe.Rotation.AlmostEqual(spell.Rotation, 0.1f));
            if (mismatch >= 0)
                ReportError($"Mispredicted rotation: {spell.Rotation} vs predicted {_aoes[mismatch].Rotation}");
            _aoes.Clear();
        }
        _aoes.Add(new(donut ? _shapeIn : _shapeOut, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
        _aoes.Add(new(_shapeCone, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var (_, _, order) = ClassifyAction(spell.Action);
        if (order < 0)
            return; // irrelevant spell

        if (order == 0 != (NumCasts == 0))
            ReportError($"Unexpected cast order: spell {order} at num-casts {NumCasts}");
        if (_aoes.Count != 2)
            ReportError("No predicted casts");
        if (NumCasts >= _directionOffsets.Count)
            ReportError($"Unexpected cast #{NumCasts}");
        if (order == 0 && NumCasts == 0 && _directionOffsets.Count > 0 && !spell.Rotation.AlmostEqual(caster.Rotation + _directionOffsets[0], 0.1f))
            ReportError($"Mispredicted first rotation: expected {caster.Rotation}+{_directionOffsets[0]}, got {spell.Rotation}");

        // complete part of mechanic
        if (_hints.Count > 0)
            _hints.RemoveAt(0);
        ++NumCasts;
        _aoes.Clear();

        // predict next set of aoes
        if (NumCasts < _directionOffsets.Count)
        {
            var activation = WorldState.FutureTime(3.1d);
            var rotation = spell.Rotation + _directionOffsets[NumCasts];
            var pos = caster.Position.Quantized();
            _aoes.Add(new(_ins[NumCasts] ? _shapeIn : _shapeOut, pos, rotation, activation));
            _aoes.Add(new(_shapeCone, pos, rotation, activation));
        }
    }

    private static (Angle, bool, int) ClassifyAction(ActionID action) => action.ID switch
    {
        (uint)AID.NTripleKasumiGiriOutFrontFirst or (uint)AID.STripleKasumiGiriOutFrontFirst => (default, false, 0),
        (uint)AID.NTripleKasumiGiriOutRightFirst or (uint)AID.STripleKasumiGiriOutRightFirst => (-90f.Degrees(), false, 0),
        (uint)AID.NTripleKasumiGiriOutBackFirst or (uint)AID.STripleKasumiGiriOutBackFirst => (180f.Degrees(), false, 0),
        (uint)AID.NTripleKasumiGiriOutLeftFirst or (uint)AID.STripleKasumiGiriOutLeftFirst => (90f.Degrees(), false, 0),
        (uint)AID.NTripleKasumiGiriInFrontFirst or (uint)AID.STripleKasumiGiriInFrontFirst => (default, true, 0),
        (uint)AID.NTripleKasumiGiriInRightFirst or (uint)AID.STripleKasumiGiriInRightFirst => (-90f.Degrees(), true, 0),
        (uint)AID.NTripleKasumiGiriInBackFirst or (uint)AID.STripleKasumiGiriInBackFirst => (180f.Degrees(), true, 0),
        (uint)AID.NTripleKasumiGiriInLeftFirst or (uint)AID.STripleKasumiGiriInLeftFirst => (90f.Degrees(), true, 0),
        (uint)AID.NTripleKasumiGiriOutFrontRest or (uint)AID.STripleKasumiGiriOutFrontRest => (default, false, 1),
        (uint)AID.NTripleKasumiGiriOutRightRest or (uint)AID.STripleKasumiGiriOutRightRest => (-90f.Degrees(), false, 1),
        (uint)AID.NTripleKasumiGiriOutBackRest or (uint)AID.STripleKasumiGiriOutBackRest => (180f.Degrees(), false, 1),
        (uint)AID.NTripleKasumiGiriOutLeftRest or (uint)AID.STripleKasumiGiriOutLeftRest => (90f.Degrees(), false, 1),
        (uint)AID.NTripleKasumiGiriInFrontRest or (uint)AID.STripleKasumiGiriInFrontRest => (default, true, 1),
        (uint)AID.NTripleKasumiGiriInRightRest or (uint)AID.STripleKasumiGiriInRightRest => (-90f.Degrees(), true, 1),
        (uint)AID.NTripleKasumiGiriInBackRest or (uint)AID.STripleKasumiGiriInBackRest => (180f.Degrees(), true, 1),
        (uint)AID.NTripleKasumiGiriInLeftRest or (uint)AID.STripleKasumiGiriInLeftRest => (90f.Degrees(), true, 1),
        _ => (default, false, -1)
    };
}

class IaiGiriBait : Components.GenericBaitAway
{
    public sealed class Instance(Actor source)
    {
        public Actor Source = source;
        public Actor FakeSource = new(default, default, default, default, default!, default, default, default, default, default);
        public Actor? Target;
        public List<Angle> DirOffsets = [];
        public List<string> Hints = [];
    }

    public readonly float Distance;
    public List<Instance> Instances = [];
    private readonly float _jumpOffset;
    private bool _baitsDirty;

    public IaiGiriBait(BossModule module, float jumpOffset, float distance) : base(module)
    {
        Distance = distance;
        IgnoreOtherBaits = true; // this really makes things only worse...
        _jumpOffset = jumpOffset;
    }

    public override void Update()
    {
        var count = Instances.Count;
        for (var i = 0; i < count; ++i)
        {
            var inst = Instances[i];
            if (inst.Target != null)
            {
                inst.FakeSource.PosRot = inst.Target.PosRot - _jumpOffset * inst.Target.Rotation.ToDirection().ToVec4();
            }
        }

        // these typically are assigned over a single frame
        if (_baitsDirty)
        {
            _baitsDirty = false;
            CurrentBaits.Clear();
            for (var i = 0; i < count; ++i)
            {
                var inst = Instances[i];
                if (inst.Target != null && inst.DirOffsets.Count > 0)
                {
                    CurrentBaits.Add(new(inst.FakeSource, inst.Target, new AOEShapeCone(Distance - 3f, 135f.Degrees(), inst.DirOffsets[0]))); // distance-3 is a hack for better double kasumi-giri bait indicator
                }
            }
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.Giri)
        {
            var (dir, hint) = status.Extra switch
            {
                0x248 => (default, "back"),
                0x249 => (-90f.Degrees(), "left"),
                0x24A => (180f.Degrees(), "front"),
                0x24B => (90f.Degrees(), "right"),
                _ => (default, "")
            };
            if (hint.Length == 0)
            {
                ReportError($"Unexpected extra {status.Extra:X}");
                return;
            }

            var i = InstanceFor(actor);
            _baitsDirty |= i.DirOffsets.Count == 0;
            i.DirOffsets.Add(dir);
            i.Hints.Add(hint);
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID is (uint)TetherID.RatAndMouse)
        {
            InstanceFor(source).Target = WorldState.Actors.Find(tether.Target);
            _baitsDirty = true;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.FleetingIaiGiriJump or (uint)AID.ShadowKasumiGiriJump)
            CurrentBaits.Clear(); // TODO: this is a hack, if we ever mark baits as dirty again, they will be recreated - but we need instances for resolve
    }

    private Instance InstanceFor(Actor source)
    {
        var i = Instances.Find(i => i.Source == source);
        if (i == null)
        {
            i = new(source);
            Instances.Add(i);
        }
        return i;
    }
}

sealed class IaiGiriResolve(BossModule module) : Components.GenericAOEs(module)
{
    public sealed class Instance(Actor source)
    {
        public Actor Source = source;
        public List<AOEInstance> AOEs = [];
    }

    private readonly List<Instance> _instances = [];

    private readonly List<AOEInstance> _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _instances.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = _aoesBuffer;
        aoes.Clear();
        for (var i = 0; i < count; ++i)
        {
            var ins = _instances[i];
            if (ins.AOEs.Count != 0)
            {
                aoes.Add(ins.AOEs.Ref(0));
            }
        }
        return CollectionsMarshal.AsSpan(aoes);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.NFleetingIaiGiriFront:
            case (uint)AID.NFleetingIaiGiriRight:
            case (uint)AID.NFleetingIaiGiriLeft:
            case (uint)AID.SFleetingIaiGiriFront:
            case (uint)AID.SFleetingIaiGiriRight:
            case (uint)AID.SFleetingIaiGiriLeft:
            case (uint)AID.NShadowKasumiGiriFrontFirst:
            case (uint)AID.NShadowKasumiGiriRightFirst:
            case (uint)AID.NShadowKasumiGiriBackFirst:
            case (uint)AID.NShadowKasumiGiriLeftFirst:
            case (uint)AID.SShadowKasumiGiriFrontFirst:
            case (uint)AID.SShadowKasumiGiriRightFirst:
            case (uint)AID.SShadowKasumiGiriBackFirst:
            case (uint)AID.SShadowKasumiGiriLeftFirst:
            case (uint)AID.NShadowKasumiGiriFrontSecond:
            case (uint)AID.NShadowKasumiGiriRightSecond:
            case (uint)AID.NShadowKasumiGiriBackSecond:
            case (uint)AID.NShadowKasumiGiriLeftSecond:
            case (uint)AID.SShadowKasumiGiriFrontSecond:
            case (uint)AID.SShadowKasumiGiriRightSecond:
            case (uint)AID.SShadowKasumiGiriBackSecond:
            case (uint)AID.SShadowKasumiGiriLeftSecond:
                var inst = _instances.Find(i => i.Source == caster);
                if (inst == null)
                {
                    ReportError($"Did not predict cast for {caster.InstanceID:X}");
                    return;
                }

                // update all aoe positions, first rotation/activation
                var first = true;
                foreach (ref var aoe in inst.AOEs.AsSpan())
                {
                    aoe.Origin = spell.LocXZ;
                    if (first)
                    {
                        first = false;
                        aoe.Rotation = spell.Rotation;
                        aoe.Activation = Module.CastFinishAt(spell);
                    }
                }
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.FleetingIaiGiriJump:
            case (uint)AID.DoubleIaiGiri:
                var comp = Module.FindComponent<IaiGiriBait>();
                var bait = comp?.Instances.Find(i => i.Source == caster);
                if (bait?.Target == null)
                {
                    ReportError($"Failed to find bait for {caster.InstanceID:X}");
                    return;
                }

                var inst = new Instance(caster);
                var curRot = bait.Target.Rotation;
                var nextActivation = WorldState.FutureTime(2.7d);
                foreach (var off in bait.DirOffsets)
                {
                    curRot += off;
                    inst.AOEs.Add(new(new AOEShapeCone(comp!.Distance, 135f.Degrees()), bait.FakeSource.Position, curRot, nextActivation));
                    nextActivation = nextActivation.AddSeconds(3.1d);
                }
                _instances.Add(inst);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.NFleetingIaiGiriFront:
            case (uint)AID.NFleetingIaiGiriRight:
            case (uint)AID.NFleetingIaiGiriLeft:
            case (uint)AID.SFleetingIaiGiriFront:
            case (uint)AID.SFleetingIaiGiriRight:
            case (uint)AID.SFleetingIaiGiriLeft:
            case (uint)AID.NShadowKasumiGiriFrontFirst:
            case (uint)AID.NShadowKasumiGiriRightFirst:
            case (uint)AID.NShadowKasumiGiriBackFirst:
            case (uint)AID.NShadowKasumiGiriLeftFirst:
            case (uint)AID.SShadowKasumiGiriFrontFirst:
            case (uint)AID.SShadowKasumiGiriRightFirst:
            case (uint)AID.SShadowKasumiGiriBackFirst:
            case (uint)AID.SShadowKasumiGiriLeftFirst:
            case (uint)AID.NShadowKasumiGiriFrontSecond:
            case (uint)AID.NShadowKasumiGiriRightSecond:
            case (uint)AID.NShadowKasumiGiriBackSecond:
            case (uint)AID.NShadowKasumiGiriLeftSecond:
            case (uint)AID.SShadowKasumiGiriFrontSecond:
            case (uint)AID.SShadowKasumiGiriRightSecond:
            case (uint)AID.SShadowKasumiGiriBackSecond:
            case (uint)AID.SShadowKasumiGiriLeftSecond:
                ++NumCasts;
                var instance = _instances.Find(i => i.Source == caster);
                if (instance?.AOEs.Count > 0)
                    instance.AOEs.RemoveAt(0);
                break;
        }
    }
}

sealed class FleetingIaiGiriBait(BossModule module) : IaiGiriBait(module, 3f, 60f)
{
    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (Instances.Count == 1 && Instances[0].Hints.Count == 1)
            hints.Add($"Safespot: {Instances[0].Hints[0]}");
    }
}

sealed class DoubleIaiGiriBait(BossModule module) : IaiGiriBait(module, 1f, 23f)
{
    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        var count = Instances.Count;
        if (count == 0)
            return;
        List<string> safespotsList = [with(count)];

        for (var i = 0; i < count; ++i)
        {
            var instance = Instances[i];
            if (instance.Hints.Count == 2)
                safespotsList.Add(instance.Hints[1]);
        }

        var safespots = string.Join(", ", safespotsList);
        if (safespots.Length > 0)
            hints.Add($"Second safespots: {safespots}");
    }
}
