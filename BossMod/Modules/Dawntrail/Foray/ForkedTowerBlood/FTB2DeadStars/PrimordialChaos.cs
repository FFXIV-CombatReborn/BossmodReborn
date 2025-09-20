namespace BossMod.Dawntrail.Foray.ForkedTowerBlood.FTB2DeadStars;

sealed class PrimordialChaos(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance>[] _aoesPerPlayer = new List<AOEInstance>[PartyState.MaxPartySize];
    private readonly int[] playerTemperatures = new int[PartyState.MaxPartySize];
    private readonly List<Polygon> circlesRed = new(3), circlesBlue = new(3);
    private static readonly AOEShapeCircle circle = new(22f);
    public int NumTelegraphCasts;
    private bool isInit;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => slot < PartyState.MaxPartySize ? CollectionsMarshal.AsSpan(_aoesPerPlayer[slot]) : [];

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var id = status.ID;
        if (NumTelegraphCasts == 0 && id is (uint)SID.NovaOoze or (uint)SID.IceOoze && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            playerTemperatures[slot] = (id == (uint)SID.NovaOoze ? 1 : -1) * status.Extra;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (actor.IsDead && status.ID is (uint)SID.NovaOoze or (uint)SID.IceOoze && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            playerTemperatures[slot] = default;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.FrozenFalloutTelegraphBlue:
                ++NumTelegraphCasts;
                circlesBlue.Add(new(spell.LocXZ, 22f, 64));
                InitIfReady();
                break;
            case (uint)AID.FrozenFalloutTelegraphRed:
                circlesRed.Add(new(spell.LocXZ, 22f, 64));
                InitIfReady();
                break;
        }
    }

    private void InitIfReady()
    {
        if (!isInit && circlesRed.Count != 0 && circlesBlue.Count != 0)
        {
            var act = WorldState.FutureTime(NumCasts == 0 ? 9d : 5.6d);
            var center = Arena.Center;
            var color = Colors.SafeFromAOE;
            var blue0 = circlesBlue[0];
            var red0 = circlesRed[0];
            List<AOEInstance> isFire = [new(new AOEShapeCustom([blue0], [red0], invertForbiddenZone: true), center, default, act, color)];
            List<AOEInstance> isIce = [new(new AOEShapeCustom([red0], [blue0], invertForbiddenZone: true), center, default, act, color)];
            List<AOEInstance> rest = [new(circle, blue0.Center, default, act), new(circle, red0.Center, default, act)];
            for (var i = 0; i < 8; ++i)
            {
                var temp = playerTemperatures[i];
                _aoesPerPlayer[i] = [];
                if (temp != 0)
                {
                    _aoesPerPlayer[i] = temp > 0 ? isFire : isIce;
                }
                else
                {
                    _aoesPerPlayer[i] = rest;
                }
            }
            circlesBlue.RemoveAt(0);
            circlesRed.RemoveAt(0);
            isInit = true;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var id = spell.Action.ID;
        if (id is (uint)AID.FrozenFalloutBlue or (uint)AID.FrozenFalloutRed)
        {
            var targets = CollectionsMarshal.AsSpan(spell.Targets);
            var len = targets.Length;
            var isBlue = id == (uint)AID.FrozenFalloutBlue;
            for (var i = 0; i < len; ++i)
            {
                ref readonly var targ = ref targets[i];
                if (Raid.FindSlot(targ.ID) is var slot && slot >= 0)
                {
                    playerTemperatures[slot] += isBlue ? -1 : 1;
                }
            }
            if ((++NumCasts & 3) == 0) // 2 casters for each AOE
            {
                Array.Clear(_aoesPerPlayer);
                isInit = false;
                InitIfReady();
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (slot < PartyState.MaxPartySize && playerTemperatures[slot] != 0 && _aoesPerPlayer[slot] != default)
        {
            hints.Add("Get hit by AOE of correct temperature!", !_aoesPerPlayer[slot][0].Check(actor.Position));
        }
        else
        {
            base.AddHints(slot, actor, hints);
        }
    }
}
