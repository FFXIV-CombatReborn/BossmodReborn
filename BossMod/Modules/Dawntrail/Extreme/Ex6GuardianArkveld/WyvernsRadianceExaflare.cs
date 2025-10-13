namespace BossMod.Dawntrail.Extreme.Ex6GuardianArkveld;

sealed class WyvernsRadianceExaflare1(BossModule module) : Components.Exaflare(module, new AOEShapeRect(8f, 20f))
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var linesCount = Lines.Count;
        if (linesCount == 0)
        {
            return;
        }

        var imminentAOEs = ImminentAOEs(linesCount);

        // use only imminent aoes for hints
        var len = imminentAOEs.Length;
        for (var i = 0; i < len; ++i)
        {
            ref var aoe = ref imminentAOEs[i];
            hints.AddForbiddenZone(Shape, aoe.Item1, aoe.Item3, aoe.Item2);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WyvernsRadianceExaflareFirst)
        {
            Lines.Add(new(caster.Position, 8f * caster.Rotation.Round(1f).ToDirection(), Module.CastFinishAt(spell), 2.6d, 5, 2, spell.Rotation));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.WyvernsRadianceExaflareFirst or (uint)AID.WyvernsRadianceExaflareRest)
        {
            var count = Lines.Count;
            var pos = caster.Position;
            for (var i = 0; i < count; ++i)
            {
                var line = Lines[i];
                if (line.Next.AlmostEqual(pos, 1f))
                {
                    AdvanceLine(line, pos);
                    if (line.ExplosionsLeft == 0)
                    {
                        Lines.RemoveAt(i);
                    }
                    return;
                }
            }
        }
    }
}

sealed class WyvernsRadianceExaflare2(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = new(58);
    private readonly AOEShapeRect rect1 = new(40f, 4f), rect2 = new(40f, 2f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var max = NumCasts == 0 ? 6 : count > 8 ? 8 : count;
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        var maxC = Math.Min(max, count - NumCasts);
        return aoes.Slice(NumCasts, maxC);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WyvernsRadianceRect1)
        {
            var rot = (int)spell.Rotation.Deg;
            if (rot == 44)
            {
                var a45 = Angle.AnglesIntercardinals[1];
                var am45 = Angle.AnglesIntercardinals[0];
                (WPos pos, Angle rot)[] aoes =
                [
                    (new(85.832f, 85.832f), a45),
                    (new(114.122f, 85.832f), am45),

                    (new(109.88f, 81.59f), am45),
                    (new(90.074f, 81.59f), a45),
                    (new(81.590f, 90.074f), a45),
                    (new(118.364f, 90.074f), am45),
                    (new(121.202f, 92.912f), am45),
                    (new(107.042f, 78.782f), am45),
                    (new(78.782f, 92.912f), a45),
                    (new(92.912f, 78.782f), a45),
                    (new(95.75f, 75.944f), a45),
                    (new(124.041f, 95.750f), am45),
                    (new(104.234f, 75.944f), am45),
                    (new(75.944f, 95.75f), a45),
                    (new(126.848f, 98.558f), am45),
                    (new(101.396f, 73.106f), am45),
                    (new(98.558f, 73.106f), a45),
                    (new(73.106f, 98.558f), a45),

                    (new(126.848f, 98.558f), am45),
                    (new(101.396f, 73.106f), am45),
                    (new(98.558f, 73.106f), a45),
                    (new(73.106f, 98.558f), a45),

                    (new(104.234f, 75.944f), am45),
                    (new(124.041f, 95.75f), am45),
                    (new(95.75f, 75.944f), a45),
                    (new(75.944f, 95.75f), a45),
                    (new(78.782f, 92.912f), a45),
                    (new(107.042f, 78.782f), am45),
                    (new(121.202f, 92.912f), am45),
                    (new(92.912f, 78.782f), a45),
                    (new(109.88f, 81.59f), am45),
                    (new(81.59f, 90.074f), a45),
                    (new(118.364f, 90.074f), am45),
                    (new(90.074f, 81.590f), a45),
                    (new(112.718f, 84.428f), am45),
                    (new(115.526f, 87.266f), am45),
                    (new(87.266f, 84.428f), a45),
                    (new(84.428f, 87.266f), a45),
                    (new(84.428f, 87.266f), a45),
                    (new(115.526f, 87.266f), am45),
                    (new(112.718f, 84.428f), am45),
                    (new(87.266f, 84.428f), a45),
                    (new(109.88f, 81.59f), am45),
                    (new(118.364f, 90.074f), am45),
                    (new(90.074f, 81.59f), a45),
                    (new(81.59f, 90.074f), a45),
                    (new(121.202f, 92.912f), am45),
                    (new(78.782f, 92.912f), a45),
                    (new(107.042f, 78.782f), am45),
                    (new(92.912f, 78.782f), a45),
                    (new(104.234f, 75.944f), am45),
                    (new(124.041f, 95.750f), am45),
                    (new(95.750f, 75.944f), a45),
                    (new(75.944f, 95.750f), a45),
                    (new(73.106f, 98.558f), a45),
                    (new(126.848f, 98.558f), am45),
                    (new(101.396f, 73.106f), am45),
                    (new(98.558f, 73.106f), a45)
                ];
                AddAOEs(aoes);
            }
            else if (rot == -89)
            {
                var am180 = -179.984f.Degrees();
                var am90 = -89.982f.Degrees();
                (WPos pos, Angle rot)[] aoes =
                [
                    (new(99.992f, 119.982f), am180),
                    (new(119.982f, 99.992f), am90),

                    (new(106.004f, 119.982f), am180),
                    (new(93.980f, 119.982f), am180),
                    (new(119.982f, 105.974f), am90),
                    (new(119.982f, 93.98f), am90),
                    (new(89.982f, 119.982f), am180),
                    (new(110.002f, 119.982f), am180),
                    (new(119.982f, 109.972f), am90),
                    (new(119.982f, 89.982f), am90),
                    (new(85.985f, 119.982f), am180),
                    (new(119.982f, 85.985f), am90),
                    (new(114f, 119.982f), am180),
                    (new(119.982f, 113.97f), am90),
                    (new(119.982f, 81.987f), am90),
                    (new(81.987f, 119.982f), am180),
                    (new(117.998f, 119.982f), am180),
                    (new(119.982f, 117.968f), am90),

                    (new(81.987f, 119.982f), am180),
                    (new(117.998f, 119.982f), am180),
                    (new(119.982f, 117.968f), am90),
                    (new(119.982f, 81.987f), am90),

                    (new(85.985f, 119.982f), am180),
                    (new(114f, 119.982f), am180),
                    (new(119.982f, 85.985f), am90),
                    (new(119.982f, 113.970f), am90),
                    (new(110.002f, 119.982f), am180),
                    (new(119.982f, 89.982f), am90),
                    (new(89.982f, 119.982f), am180),
                    (new(119.982f, 109.972f), am90),
                    (new(106.004f, 119.982f), am180),
                    (new(119.982f, 105.974f), am90),
                    (new(93.98f, 119.982f), am180),
                    (new(119.982f, 93.980f), am90),
                    (new(102.007f, 119.982f), am180),
                    (new(97.978f, 119.982f), am180),
                    (new(119.982f, 97.978f), am90),
                    (new(119.982f, 101.976f), am90),
                    (new(119.982f, 101.976f), am90),
                    (new(102.007f, 119.982f), am180),
                    (new(97.978f, 119.982f), am180),
                    (new(119.982f, 97.978f), am90),
                    (new(93.980f, 119.982f), am180),
                    (new(119.982f, 93.980f), am90),
                    (new(106.004f, 119.982f), am180),
                    (new(119.982f, 105.974f), am90),
                    (new(89.982f, 119.982f), am180),
                    (new(119.982f, 109.972f), am90),
                    (new(119.982f, 89.982f), am90),
                    (new(110.002f, 119.982f), am180),
                    (new(85.985f, 119.982f), am180),
                    (new(114f, 119.982f), am180),
                    (new(119.982f, 113.97f), am90),
                    (new(119.982f, 85.985f), am90),
                    (new(81.987f, 119.982f), am180),
                    (new(119.982f, 117.968f), am90),
                    (new(117.998f, 119.982f), am180),
                    (new(119.982f, 81.987f), am90)
                ];
                AddAOEs(aoes);
            }
            void AddAOEs((WPos pos, Angle rot)[] aoes)
            {
                var act = Module.CastFinishAt(spell);
                var aoes_ = aoes;
                for (var i = 0; i < 58; ++i)
                {
                    ref var aoe = ref aoes_[i];
                    var delay = i switch
                    {
                        < 2 => 0d,
                        < 6 => 2.6d,
                        < 10 => 5.2d,
                        < 14 => 7.8d,
                        < 18 => 10.4d,
                        < 22 => 13d,
                        < 26 => 15.1d,
                        < 30 => 17.7d,
                        < 34 => 20.3d,
                        < 38 => 22.9d,
                        < 42 => 25.5d,
                        < 46 => 28.1d,
                        < 50 => 30.7d,
                        < 54 => 33.3d,
                        _ => 35.9d
                    };
                    var first = i < 2;
                    _aoes.Add(new(first ? rect1 : rect2, aoe.pos, aoe.rot, act.AddSeconds(delay), risky: first, color: first ? Colors.Danger : default));
                }
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.WyvernsRadianceRect1 or (uint)AID.WyvernsRadianceRect2 or (uint)AID.WyvernsRadianceRect3)
        {
            ++NumCasts;
            var count = _aoes.Count;
            if (count == 0 || count <= NumCasts)
            {
                return;
            }
            var max = NumCasts == 0 ? 6 : count > 8 ? 8 : count;
            var aoes = CollectionsMarshal.AsSpan(_aoes);
            var maxC = Math.Min(max, count - NumCasts);
            var maxI = NumCasts + maxC;
            var first = aoes[NumCasts].Activation;
            var last = aoes[maxI - 1].Activation;

            for (var i = NumCasts; i < maxI; ++i)
            {
                ref var aoe = ref aoes[i];
                if (aoe.Activation != last)
                {
                    aoe.Color = Colors.Danger;
                    aoe.Risky = true;
                }
                else if (last == first)
                {
                    aoe.Risky = true;
                }
            }
        }
    }
}
