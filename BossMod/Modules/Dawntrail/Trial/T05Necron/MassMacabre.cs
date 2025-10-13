namespace BossMod.Dawntrail.Trial.T05Necron;

sealed class MassMacabre(BossModule module) : Components.GenericTowers(module, prioritizeInsufficient: true)
{
    private int numAdded;

    public override void OnMapEffect(byte index, uint state)
    {
        if (state == 0x00020001u && numAdded < 4) // if players leave a tower state 20001 triggers again, so we need a counter
        {
            var pos = index switch
            {
                0x1A => new WPos(94f, 90f),
                0x1B => new(106f, 90f),
                0x1C => new(94f, 100f),
                0x1D => new(106f, 100f),
                _ => default
            };
            if (pos != default)
            {
                ++numAdded;
                Towers.Add(new(pos.Quantized(), 3f, 4, 4, activation: WorldState.FutureTime(15d), actorID: index));
            }
        }
        else if (state is 0x00080004u or 0x08000400u)
        {
            var count = Towers.Count;
            var towers = CollectionsMarshal.AsSpan(Towers);
            var id = (ulong)index;
            for (var i = 0; i < count; ++i)
            {
                if (towers[i].ActorID == id)
                {
                    Towers.RemoveAt(i);
                    if (Towers.Count == 0)
                    {
                        numAdded = 0;
                    }
                    return;
                }
            }
        }
    }
}

sealed class SpreadingFear(BossModule module) : Components.CastHint(module, (uint)AID.SpreadingFear, "Hand enraging!", true);
