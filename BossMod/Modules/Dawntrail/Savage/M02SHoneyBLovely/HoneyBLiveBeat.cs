namespace BossMod.Dawntrail.Savage.M02SHoneyBLovely;

sealed class HoneyBLiveBeat1(BossModule module) : Components.CastCounter(module, (uint)AID.HoneyBLiveBeat1AOE);
sealed class HoneyBLiveBeat2(BossModule module) : Components.CastCounter(module, (uint)AID.HoneyBLiveBeat2AOE);
sealed class HoneyBLiveBeat3(BossModule module) : Components.CastCounter(module, (uint)AID.HoneyBLiveBeat3AOE);

sealed class HoneyBLiveHearts(BossModule module) : BossComponent(module)
{
    public int[] Hearts = new int[PartyState.MaxPartySize];

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var hearts = NumHearts(status.ID);
        if (hearts >= 0 && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
            Hearts[slot] = hearts;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        var hearts = NumHearts(status.ID);
        if (hearts >= 0 && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0 && Hearts[slot] == hearts)
            Hearts[slot] = 0;
    }

    private static int NumHearts(uint sid) => sid switch
    {
        (uint)SID.Hearts0 => 0,
        (uint)SID.Hearts1 => 1,
        (uint)SID.Hearts2 => 2,
        (uint)SID.Hearts3 => 3,
        (uint)SID.Hearts4 => 4,
        _ => -1
    };
}

abstract class Fracture(BossModule module) : Components.CastTowers(module, (uint)AID.Fracture, 4f)
{
    protected abstract BitMask UpdateForbidden();

    public override void Update()
    {
        var forbidden = UpdateForbidden();
        foreach (ref var t in Towers.AsSpan())
            t.ForbiddenSoakers = forbidden;
    }
}

sealed class Fracture1(BossModule module) : Fracture(module)
{
    private readonly HoneyBLiveHearts? _hearts = module.FindComponent<HoneyBLiveHearts>();

    protected override BitMask UpdateForbidden()
    {
        var forbidden = new BitMask();
        if (_hearts != null)
            for (var i = 0; i < _hearts.Hearts.Length; ++i)
                if (_hearts.Hearts[i] == 3)
                    forbidden.Set(i);
        return forbidden;
    }
}

sealed class Fracture2(BossModule module) : Fracture(module)
{
    private BitMask _spreads;
    private readonly HoneyBLiveHearts? _hearts = module.FindComponent<HoneyBLiveHearts>();

    protected override BitMask UpdateForbidden()
    {
        var forbidden = _spreads;
        if (_hearts != null)
            for (var i = 0; i < _hearts.Hearts.Length; ++i)
                if (_hearts.Hearts[i] > 1)
                    forbidden.Set(i);
        return forbidden;
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        // spread targets should never take towers
        if (iconID == (uint)IconID.Heartsore)
            _spreads.Set(Raid.FindSlot(actor.InstanceID));
    }
}

sealed class Fracture3 : Fracture
{
    private BitMask _defamations;

    public Fracture3(BossModule module) : base(module)
    {
        var bigBurst = module.FindComponent<HoneyBLiveBeat3BigBurst>();
        if (bigBurst != null)
        {
            var order = bigBurst.NumCasts == 0 ? 1 : 2;
            _defamations = Raid.WithSlot(true, true, true).WhereSlot(i => bigBurst.Order[i] == order).Mask();
        }
    }

    protected override BitMask UpdateForbidden() => _defamations;
}

sealed class Loveseeker(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LoveseekerAOE, 10);
sealed class HeartStruck(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HeartStruck, 6);
sealed class Heartsore(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Heartsore, (uint)AID.Heartsore, 6, 7.1f);
sealed class SweetheartsS(BossModule module) : Raid.M02NHoneyBLovely.Sweethearts(module, (uint)OID.Sweetheart, (uint)AID.SweetheartTouch);

abstract class Heartsick(BossModule module, bool roles) : Components.StackWithIcon(module, (uint)IconID.Heartsick, (uint)AID.Heartsick, 6, 7, roles ? 2 : 4, roles ? 2 : 4)
{
    private readonly HoneyBLiveHearts? _hearts = module.FindComponent<HoneyBLiveHearts>();

    public override void Update()
    {
        if (_hearts != null)
        {
            foreach (ref var stack in Stacks.AsSpan())
            {
                for (var i = 0; i < _hearts.Hearts.Length; ++i)
                {
                    stack.ForbiddenPlayers[i] = roles
                        ? (_hearts.Hearts[i] > 0 || stack.Target.Class.IsSupport() != Raid[i]?.Class.IsSupport())
                        : _hearts.Hearts[i] == 3;
                }
            }
        }
    }
}
sealed class Heartsick1(BossModule module) : Heartsick(module, false);
sealed class Heartsick2(BossModule module) : Heartsick(module, true);

sealed class HoneyBLiveBeat3BigBurst(BossModule module) : Components.UniformStackSpread(module, default, 14f)
{
    public int NumCasts;
    public int[] Order = new int[PartyState.MaxPartySize];
    public readonly DateTime[] Activation = new DateTime[2];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Order[slot] != 0)
            hints.Add($"Order: {Order[slot]}", false);
        base.AddHints(slot, actor, hints);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.PoisonNPop)
        {
            var order = (status.ExpireAt - WorldState.CurrentTime).TotalSeconds > 30 ? 1 : 0;
            Activation[order] = status.ExpireAt;
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0)
                Order[slot] = order + 1;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Fracture && Spreads.Count == 0)
        {
            var order = NumCasts == 0 ? 1 : 2;
            AddSpreads(Raid.WithSlot(true, true, true).WhereSlot(i => Order[i] == order).Actors(), Activation[order - 1]);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.HoneyBLiveBeat3BigBurst)
        {
            ++NumCasts;
            Spreads.Clear();
        }
    }
}
