namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Extreme.FTME4Index;

sealed class SunderingSpellblade(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoesBuffer = [];
    // can start on left or right side?
    // exaflare but curves along edge of arena
    // no indicators of left/right; check using position
    // middle +- 3.25, -601
    // left/right is +- 21.758/25.008 , -638.685 / -644.315
    // moves 6f each cast, roughly 2s between eventcast
    // x4 straight, x1 45deg, x1 15deg, x1 60 deg, x2 straight
    // crappier version of exaflare; maybe write something fancier later

    private readonly AOEShapeCircle _circle = new(6f);
    private readonly float _distance = 6f;
    private readonly double _timeBetween = 2d;
    private readonly int _max = 3;
    private readonly List<List<AOEInstance>> _aoes = [];
    private readonly List<WPos> _spots = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count == 0)
        {
            return [];
        }

        var aoes = _aoesBuffer;
        aoes.Clear();

        var aoelist = CollectionsMarshal.AsSpan(_aoes);
        var listcount = aoelist.Length;
        for (var i = 0; i < listcount; ++i)
        {
            ref var sublistref = ref aoelist[i];
            var sublist = CollectionsMarshal.AsSpan(sublistref);
            var sublistcount = sublist.Length;
            var max = sublistcount > _max ? _max : sublistcount;
            for (var j = 0; j < max; ++j)
            {
                ref var sub = ref sublist[j];
                sub.Color = j == 0 ? Colors.Danger : sub.Color;
                aoes.Add(sub);
            }
        }

        return CollectionsMarshal.AsSpan(aoes);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SunderingSpellblade)
        {
            List<AOEInstance> aoes = [];
            //var initialPosition = caster.Position;
            //var initialRotation = caster.Rotation;
            var initialPosition = spell.LocXZ;
            var initialRotation = spell.Rotation;
            var initialTime = Module.CastFinishAt(spell);

            /*
            var isLeft = initialPosition.X switch
            {
                -25.008f => false,
                -21.758f => true,
                -3.25f => true,
                3.25f => false,
                21.758f => false,
                25.008f => true,
                _ => false
            };
            */

            var x = initialPosition.X;
            var isLeft = x is >= -22.25f and <= -21.25f or >= -3.75f and <= -2.75f or >= 24.5f and <= 25.5f;

            for (var i = 0; i < 9; ++i)
            {
                var pos = i == 0 ? initialPosition : aoes[i - 1].Origin;
                var rot = i == 0 ? initialRotation : aoes[i - 1].Rotation;
                var time = i == 0 ? initialTime : aoes[i - 1].Activation.AddSeconds(_timeBetween);

                if (i != 0)
                {
                    var angleMod = i switch
                    {
                        4 => 45f.Degrees(),
                        5 => -30f.Degrees(),
                        6 => 45f.Degrees(),
                        _ => 0f.Degrees()
                    };
                    angleMod *= isLeft ? 1 : -1;
                    rot += angleMod;
                    var direction = rot.ToDirection() * _distance;
                    pos += direction;
                }

                aoes.Add(new(_circle, pos, rot, time));
            }

            _aoes.Add(aoes);
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.SunderingSpellblade or (uint)AID.SunderingSpellblade1)
        {
            _spots.Add(caster.Position);
            ++NumCasts;

            var aoelist = CollectionsMarshal.AsSpan(_aoes);
            var listcount = aoelist.Length;
            for (var i = 0; i < listcount; ++i)
            {
                ref var sublistref = ref aoelist[i];
                var sublist = CollectionsMarshal.AsSpan(sublistref);
                var sublistcount = sublist.Length;
                if (sublistcount != 0)
                {
                    ref var aoe = ref sublist[0];
                    if (aoe.Origin.AlmostEqual(caster.Position, 1f))
                    {
                        _aoes[i].RemoveAt(0);
                    }
                }
            }
        }
    }
    /*
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        for (var i = 0; i < _spots.Count; ++i)
        {
            Arena.ZoneCircleOutline(_spots[i], 6f, 0xFFFFFFFF);
        }
    }

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        WPos pos1 = new(3.25f, -619f);
        WPos pos2 = new(7.794f, -623.5f);
        WPos pos3 = new(9.419f, -629.685f);
        WPos pos4 = new(14.615f, -632.685f);
        WPos pos5 = new(19.812f, -635.685f);
        hints.Add($"{(pos1 - pos2).ToAngle().Deg} | {(pos2 - pos3).ToAngle().Deg} | {(pos3 - pos4).ToAngle().Deg} | {(pos4 - pos5).ToAngle().Deg}");
        hints.Add($"Exacasts[{NumCasts}]");
    }
    */
}
