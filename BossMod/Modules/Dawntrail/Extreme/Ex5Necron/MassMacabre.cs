namespace BossMod.Dawntrail.Extreme.Ex5Necron;

sealed class MassMacabre(BossModule module) : Components.GenericTowers(module)
{
    private BitMask forbidden;
    private int numAdded;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.MassMacabre)
        {
            AddTower(new(100f, 94f), 4, 0x1Cul, 6.1d);
            AddTower(new(100f, 106f), 4, 0x21ul, 6.1d);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.MementoMori1 or (uint)AID.MementoMori2)
        {
            var count = Towers.Count - 1;
            var max = count > 2 ? 2 : count;
            for (var i = max; i >= 0; --i)
            {
                ref var tower = ref Towers.Ref(i);
                if (tower.ActorID is 0x1Cul or 0x21ul)
                {
                    Towers.RemoveAt(i);
                }
            }
        }
    }

    private void AddTower(WPos pos, int soakers, ulong index, double delay) => Towers.Add(new(pos.Quantized(), 3f, soakers, soakers, activation: WorldState.FutureTime(delay), actorID: index));

    public override void OnMapEffect(byte index, uint state)
    {
        if (state == 0x00020001u && numAdded < 8) // if players leave a tower state 20001 triggers again, so we need a counter
        {
            (var pos, var minSoakers) = index switch
            {
                0x1A => (new WPos(91f, 88f), 3),
                0x1B => (new(109f, 88f), 3),
                0x1D => (new(85f, 97f), 2),
                0x1E => (new(115f, 97f), 2),
                0x1F => (new(85f, 103f), 2),
                0x20 => (new(115f, 103f), 2),
                0x22 => (new(91f, 112f), 3),
                0x23 => (new(109f, 112f), 3),
                _ => default
            };
            if (minSoakers != 0)
            {
                ++numAdded;
                AddTower(pos, minSoakers, index, 23d);
            }
        }
        else if (state == 0x00080004u)
        {
            var count = Towers.Count;
            var towers = CollectionsMarshal.AsSpan(Towers);
            var id = (ulong)index;
            for (var i = 0; i < count; ++i)
            {
                if (towers[i].ActorID == id)
                {
                    Towers.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.MacabreMark)
        {
            var targets = CollectionsMarshal.AsSpan(spell.Targets);
            var len = targets.Length;

            for (var i = 0; i < len; ++i)
            {
                ref readonly var t = ref targets[i];
                forbidden.Set(Raid.FindSlot(t.ID));
            }

            UpdateTowers();

            var count = Towers.Count;
            var towers = CollectionsMarshal.AsSpan(Towers);
            var pos = caster.Position;
            for (var i = 0; i < count; ++i)
            {
                if (towers[i].Position.AlmostEqual(pos, 1f))
                {
                    Towers.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == (uint)SID.MagicVulnerabilityUp && Raid.FindSlot(actor.InstanceID) is var slot)
        {
            forbidden.Clear(slot);
            UpdateTowers();
        }
    }

    private void UpdateTowers()
    {
        var count = Towers.Count;
        var towers = CollectionsMarshal.AsSpan(Towers);

        for (var i = 0; i < count; ++i)
        {
            ref var tower = ref towers[i];
            tower.ForbiddenSoakers = forbidden;
        }
    }
}
