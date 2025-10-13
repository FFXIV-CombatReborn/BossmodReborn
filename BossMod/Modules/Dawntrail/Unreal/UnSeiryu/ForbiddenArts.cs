namespace BossMod.Dawntrail.Unreal.UnSeiryu;

sealed class ForbiddenArts(BossModule module) : Components.GenericBaitStack(module)
{
    private readonly AOEShapeRect rect = new(84.4f, 4f);
    private BitMask forbidden;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ForbiddenArtsFirst1:
            case (uint)AID.ForbiddenArtsSecond1:
                ++NumCasts;
                if (CurrentBaits.Count != 0)
                {
                    CurrentBaits.RemoveAt(0);
                }
                break;
            case (uint)AID.ForbiddenArtsFirst2:
            case (uint)AID.ForbiddenArtsSecond2:
                ++NumCasts;
                var count = CurrentBaits.Count;
                if (count != 0)
                {
                    CurrentBaits.RemoveAt(0);
                    if (count != 1)
                    {
                        var targets = CollectionsMarshal.AsSpan(spell.Targets);
                        var length = targets.Length;
                        for (var i = 0; i < length; ++i)
                        {
                            ref readonly var targ = ref targets[i];
                            if (Raid.FindSlot(targ.ID) is var slot && slot >= 0)
                            {
                                forbidden.Set(slot);
                            }
                        }
                        CurrentBaits.Ref(0).Forbidden = forbidden;
                    }
                }
                break;
            case (uint)AID.ForbiddenArtsMarker:
                var act = WorldState.FutureTime(5.3d);
                var party = Raid.WithSlot(true, true, true);
                var source = Module.PrimaryActor;
                var len = party.Length;
                var tid = spell.MainTargetID;
                var target = WorldState.Actors.Find(spell.MainTargetID);
                if (target is Actor t)
                {
                    CurrentBaits.Add(new(source, t, rect, act));
                }
                var slotT = Raid.FindSlot(tid);
                forbidden.Set(slotT);

                for (var i = 0; i < len; ++i) // unfortunately therew is no 2nd marker, so we need to find the 2nd healer. if there is no 2nd healer (alive) the target is random
                {
                    ref readonly var player = ref party[i];
                    var p = player.Item2;
                    if (p.Role == Role.Healer && player.Item1 != slotT)
                    {
                        CurrentBaits.Add(new(source, p, rect, act.AddSeconds(2d), forbidden: forbidden));
                        break;
                    }
                }
                break;
        }
    }
}
