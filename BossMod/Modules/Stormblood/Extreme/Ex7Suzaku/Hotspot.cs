namespace BossMod.Stormblood.Extreme.Ex7Suzaku;

sealed class Hotspot(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone cone = new(21f, 45f.Degrees());
    public readonly List<AOEInstance> AOEs = new(16);
    private readonly PayThePiper _kb = module.FindComponent<PayThePiper>()!;
    private static readonly uint[] _songs = [(uint)OID.SongOfDurance, (uint)OID.SongOfOblivion, (uint)OID.SongOfSorrow, (uint)OID.SongOfFire];
    private Angle startrotation;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = AOEs.Count;
        if (count == 0)
            return [];
        var max = count > 2 ? 2 : count;
        var aoes = CollectionsMarshal.AsSpan(AOEs);
        var last = count - 1;
        var maxC = Math.Min(max, count - NumCasts);
        var maxI = NumCasts + maxC;

        for (var i = NumCasts; i < maxI; ++i)
        {
            ref var aoe = ref aoes[i];
            if (i == NumCasts)
            {
                if (i != last)
                    aoe.Color = Colors.Danger;
                aoe.Risky = true;
            }
            else
            {
                aoe.Risky = _kb != null && _kb.State.Count != 0;
            }
        }
        return aoes.Slice(NumCasts, maxC);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x1E43 && actor.OID == (uint)OID.Helper2)
        {
            startrotation = actor.Rotation + 180f.Degrees();
            GetAOES(startrotation, 6.7d);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Hotspot)
        {
            ++NumCasts;
        }
        else if (AOEs.Count != 16 && spell.Action.ID is (uint)AID.RuthlessRefrain or (uint)AID.MesmerizingMelody)
        {
            AOEs.Clear();
            NumCasts = 0;
            GetAOES(startrotation, -2.2d);
        }
    }

    private void GetAOES(Angle startrot, double delay)
    {
        var songs = Module.Enemies(_songs);
        var count = songs.Count;
        var pos = Ex7Suzaku.ArenaCenter.Quantized();

        for (var i = 0; i < count; ++i)
        {
            var song = songs[i];
            var relativeAngle = Angle.FromDirection(song.Position - Ex7Suzaku.ArenaCenter);
            var index = ((int)MathF.Round((startrot - relativeAngle).Deg / 12f) + 30) % 30;
            var rot = song.OID switch
            {
                (uint)OID.SongOfDurance => Angle.AnglesIntercardinals[3],
                (uint)OID.SongOfOblivion => Angle.AnglesIntercardinals[0],
                (uint)OID.SongOfSorrow => Angle.AnglesIntercardinals[1],
                (uint)OID.SongOfFire => Angle.AnglesIntercardinals[2],
                _ => default
            };

            AOEs.Add(new(cone, pos, rot, WorldState.FutureTime(delay + index * 1.25d)));
        }
        AOEs.Sort((x, y) => x.Activation.CompareTo(y.Activation));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        var count = AOEs.Count;
        if (count == 0 || count == 16 && NumCasts >= 15 || count == 8 && NumCasts >= 7 || _kb != null && _kb.State.Count != 0)
            return;
        // force ai to stay close to the borders of the 4 panels since there is usually just 1.2s between hits
        hints.AddForbiddenZone(new SDInvertedCross(Ex7Suzaku.ArenaCenter, default, 20f, 1f), DateTime.MaxValue);
    }
}
