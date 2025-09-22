namespace BossMod.Stormblood.Ultimate.UCOB;

class P2Cauterize(BossModule module) : Components.GenericAOEs(module)
{
    public int[] BaitOrder = new int[PartyState.MaxPartySize];
    public int NumBaitsAssigned;
    public List<AOEInstance> Casters = [];
    private readonly List<(Actor actor, int position)> _dragons = []; // position 0 is N, then CW

    private static readonly AOEShapeRect _shape = new(52f, 10f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(Casters);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (BaitOrder[slot] >= NextBaitOrder)
            hints.Add($"Bait {BaitOrder[slot]}", false);
        base.AddHints(slot, actor, hints);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (BaitOrder[pcSlot] >= NextBaitOrder)
        {
            var order = DragonsForOrder(BaitOrder[pcSlot]);
            var len = order.Length;
            for (var i = 0; i < len; ++i)
            {
                var d = order[i];
                Arena.Actor(d, Colors.Object, true);
                _shape.Outline(Arena, d.Position, Angle.FromDirection(pc.Position - d.Position));
            }
            // TODO: safe spots
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is (uint)OID.Firehorn or (uint)OID.Iceclaw or (uint)OID.Thunderwing or (uint)OID.TailOfDarkness or (uint)OID.FangOfLight)
        {
            var dir = 180f.Degrees() - Angle.FromDirection(actor.Position - Arena.Center);
            var pos = (int)MathF.Round(dir.Deg / 45f) & 7;
            _dragons.Add((actor, pos));
            if (_dragons.Count == 5)
            {
                // sort by direction
                _dragons.Sort(static (a, b) => a.position.CompareTo(b.position));
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.Cauterize1 or (uint)AID.Cauterize2 or (uint)AID.Cauterize3 or (uint)AID.Cauterize4 or (uint)AID.Cauterize5)
        {
            Casters.Add(new(_shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.Cauterize1 or (uint)AID.Cauterize2 or (uint)AID.Cauterize3 or (uint)AID.Cauterize4 or (uint)AID.Cauterize5)
        {
            Casters.RemoveAt(0);
            ++NumCasts;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID is (uint)IconID.Cauterize && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            BaitOrder[slot] = ++NumBaitsAssigned;
        }
    }

    private int NextBaitOrder => (Casters.Count + NumCasts) switch
    {
        0 => 1,
        1 or 2 => 2,
        3 => 3,
        _ => 4
    };

    private Actor[] DragonsForOrder(int order)
    {
        if (_dragons.Count != 5)
            return [];
        return order switch
        {
            1 => [
                    _dragons[0].actor,
                    _dragons[1].actor
                 ],
            2 => [_dragons[2].actor],
            3 => [
                    _dragons[3].actor,
                    _dragons[4].actor
                 ],
            _ => [],
        };
    }
}

class P2Hypernova(BossModule module) : Components.VoidzoneAtCastTarget(module, 5f, (uint)AID.Hypernova, GetVoidzones, 1.4f)
{
    private static Actor[] GetVoidzones(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.VoidzoneHypernova);
        var count = enemies.Count;
        if (count == 0)
            return [];

        var voidzones = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var z = enemies[i];
            if (z.EventState != 7)
                voidzones[index++] = z;
        }
        return voidzones[..index];
    }
}
