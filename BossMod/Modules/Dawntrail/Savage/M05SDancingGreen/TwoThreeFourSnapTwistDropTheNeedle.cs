namespace BossMod.Dawntrail.Savage.M05SDancingGreen;

sealed class TwoThreeFourSnapTwistDropTheNeedle(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(2);
    private readonly AOEShapeRect rect = new(20f, 20f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var max = count > 2 ? 2 : count;
        return CollectionsMarshal.AsSpan(_aoes)[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.TwoSnapTwistDropTheNeedleFirst1:
            case (uint)AID.TwoSnapTwistDropTheNeedleFirst2:
            case (uint)AID.TwoSnapTwistDropTheNeedleFirst3:
            case (uint)AID.TwoSnapTwistDropTheNeedleFirst4:
            case (uint)AID.TwoSnapTwistDropTheNeedleFirst5:
            case (uint)AID.TwoSnapTwistDropTheNeedleFirst6:
            case (uint)AID.TwoSnapTwistDropTheNeedleFirst7:
            case (uint)AID.TwoSnapTwistDropTheNeedleFirst8:
            case (uint)AID.ThreeSnapTwistDropTheNeedleFirst1:
            case (uint)AID.ThreeSnapTwistDropTheNeedleFirst2:
            case (uint)AID.ThreeSnapTwistDropTheNeedleFirst3:
            case (uint)AID.ThreeSnapTwistDropTheNeedleFirst4:
            case (uint)AID.ThreeSnapTwistDropTheNeedleFirst5:
            case (uint)AID.ThreeSnapTwistDropTheNeedleFirst6:
            case (uint)AID.ThreeSnapTwistDropTheNeedleFirst7:
            case (uint)AID.ThreeSnapTwistDropTheNeedleFirst8:
            case (uint)AID.FourSnapTwistDropTheNeedleFirst1:
            case (uint)AID.FourSnapTwistDropTheNeedleFirst2:
            case (uint)AID.FourSnapTwistDropTheNeedleFirst3:
            case (uint)AID.FourSnapTwistDropTheNeedleFirst4:
            case (uint)AID.FourSnapTwistDropTheNeedleFirst5:
            case (uint)AID.FourSnapTwistDropTheNeedleFirst6:
            case (uint)AID.FourSnapTwistDropTheNeedleFirst7:
            case (uint)AID.FourSnapTwistDropTheNeedleFirst8:
                AddAOE();
                AddAOE(180f.Degrees(), 3.5d);
                break;
        }
        void AddAOE(Angle offset = default, double delay = default)
        {
            var loc = spell.LocXZ;
            var rot = spell.Rotation;
            var pos = delay != default ? loc - 5f * rot.ToDirection() : loc;
            var rot2 = rot + offset;
            _aoes.Add(new(rect, pos, rot2, Module.CastFinishAt(spell, delay), shapeDistance: rect.Distance(pos, rot2)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.TwoSnapTwistDropTheNeedleFirst1:
            case (uint)AID.TwoSnapTwistDropTheNeedleFirst2:
            case (uint)AID.TwoSnapTwistDropTheNeedleFirst3:
            case (uint)AID.TwoSnapTwistDropTheNeedleFirst4:
            case (uint)AID.TwoSnapTwistDropTheNeedleFirst5:
            case (uint)AID.TwoSnapTwistDropTheNeedleFirst6:
            case (uint)AID.TwoSnapTwistDropTheNeedleFirst7:
            case (uint)AID.TwoSnapTwistDropTheNeedleFirst8:
            case (uint)AID.ThreeSnapTwistDropTheNeedleFirst1:
            case (uint)AID.ThreeSnapTwistDropTheNeedleFirst2:
            case (uint)AID.ThreeSnapTwistDropTheNeedleFirst3:
            case (uint)AID.ThreeSnapTwistDropTheNeedleFirst4:
            case (uint)AID.ThreeSnapTwistDropTheNeedleFirst5:
            case (uint)AID.ThreeSnapTwistDropTheNeedleFirst6:
            case (uint)AID.ThreeSnapTwistDropTheNeedleFirst7:
            case (uint)AID.ThreeSnapTwistDropTheNeedleFirst8:
            case (uint)AID.FourSnapTwistDropTheNeedleFirst1:
            case (uint)AID.FourSnapTwistDropTheNeedleFirst2:
            case (uint)AID.FourSnapTwistDropTheNeedleFirst3:
            case (uint)AID.FourSnapTwistDropTheNeedleFirst4:
            case (uint)AID.FourSnapTwistDropTheNeedleFirst5:
            case (uint)AID.FourSnapTwistDropTheNeedleFirst6:
            case (uint)AID.FourSnapTwistDropTheNeedleFirst7:
            case (uint)AID.FourSnapTwistDropTheNeedleFirst8:
                ++NumCasts;
                break;
            case (uint)AID.TwoSnapTwistDropTheNeedle2:
            case (uint)AID.TwoSnapTwistDropTheNeedle3:
            case (uint)AID.ThreeSnapTwistDropTheNeedle3:
            case (uint)AID.ThreeSnapTwistDropTheNeedle4:
            case (uint)AID.FourSnapTwistDropTheNeedle4:
            case (uint)AID.FourSnapTwistDropTheNeedle5:
                var count = _aoes.Count;
                if (count != 0)
                {
                    _aoes.RemoveAt(0);
                    if (count == 2)
                    {
                        ref var aoe2 = ref _aoes.Ref(0);
                        var rot = aoe2.Rotation;
                        aoe2.Origin -= 5f * rot.ToDirection();
                        aoe2.ShapeDistance = rect.Distance(aoe2.Origin, rot);
                    }
                }
                ++NumCasts;
                break;
        }
    }
}

sealed class FlipToABSide(BossModule module) : Components.GenericBaitStack(module)
{
    private readonly AOEShapeCone cone = new(60f, 22.5f.Degrees());
    private readonly AOEShapeRect rect = new(50f, 4f);
    public Actor? Source;
    private bool _lightparty;
    private bool active;
    private DateTime activation;

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Source != null && CurrentBaits.Count == 0)
        {
            hints.Add($"Stored: {(_lightparty ? "Light party" : "Role")} stack");
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.FlipToASide:
                Source = caster;
                ++NumCasts;
                _lightparty = false;
                break;
            case (uint)AID.FlipToBSide:
                Source = caster;
                _lightparty = true;
                ++NumCasts;
                break;
        }
    }

    public override void Update()
    {
        if (!active && Source != null && (activation - WorldState.CurrentTime).TotalSeconds < 5.1d)
        {
            var party = Raid.WithSlot(true, true, true);
            var len = party.Length;
            if (_lightparty)
            {
                for (var i = 0; i < len; ++i)
                {
                    ref var player = ref party[i];
                    var p = player.Item2;
                    if (p.Role == Role.Healer)
                    {
                        CurrentBaits.Add(new(Source, p, rect, activation));
                    }
                }
            }
            else
            {
                BitMask allowedTanks = default;
                BitMask allowedHealers = default;
                BitMask allowedDDs = default;
                for (var i = 0; i < len; ++i)
                {
                    ref var p = ref party[i];
                    switch (p.Item2.Role)
                    {
                        case Role.Tank:
                            allowedTanks.Set(p.Item1);
                            break;
                        case Role.Healer:
                            allowedHealers.Set(p.Item1);
                            break;
                        default:
                            allowedDDs.Set(p.Item1);
                            break;
                    }
                }
                var addedHealer = false;
                var addedTank = false;
                var addedDDs = false;
                for (var i = 0; i < len; ++i)
                {
                    ref var player = ref party[i];
                    var p = player.Item2;
                    if (p.IsDead) // dead players wont be targeted and it becomes random if all players of a role are dead
                    {
                        continue;
                    }
                    switch (p.Role)
                    {
                        case Role.Tank:
                            if (!addedTank)
                            {
                                AddBait(p, allowedTanks);
                                addedTank = true;
                            }
                            break;
                        case Role.Healer:
                            if (!addedHealer)
                            {
                                AddBait(p, allowedHealers);
                                addedHealer = true;
                            }
                            break;
                        default:
                            if (!addedDDs)
                            {
                                AddBait(p, allowedDDs);
                                addedDDs = true;
                            }
                            break;
                    }
                }
            }
            active = true;
            void AddBait(Actor player, BitMask allowed) => CurrentBaits.Add(new(Source!, player, cone, activation, ~allowed));
        }
    }

    public void SetActivation(double delay) => activation = WorldState.FutureTime(delay);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.PlayASide or (uint)AID.PlayBSide)
        {
            Source = null;
            CurrentBaits.Clear();
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        if (_lightparty || Source == null)
            return PlayerPriority.Irrelevant;
        return ClassRole.IsSameRole(pc, player) ? PlayerPriority.Interesting : PlayerPriority.Danger;
    }
}
