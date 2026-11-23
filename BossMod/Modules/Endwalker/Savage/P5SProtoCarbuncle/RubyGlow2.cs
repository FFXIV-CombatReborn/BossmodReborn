namespace BossMod.Endwalker.Savage.P5SProtoCarbuncle;

// note: we start showing magic aoe only after double rush resolve
sealed class RubyGlow2(BossModule module) : RubyGlowCommon(module, (uint)AID.DoubleRush)
{
    private string _hint = "";

    private AOEInstance[] _aoesBuffer = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        // TODO: correct explosion time
        var stones = MagicStones;
        var countS = stones.Count;
        var stone = countS != 0 ? stones[0] : null;
        var magic = NumCasts > 0 ? stone : null;

        var poison = ActivePoisonAOEs();

        var countSadj = magic != null ? 1 : 0;
        var len = poison.Length;
        var aoesLen = countSadj + len;
        if (_aoesBuffer.Length < aoesLen)
            _aoesBuffer = new AOEInstance[aoesLen];
        var aoes = _aoesBuffer;
        var index = 0;
        if (magic != null)
            aoes[index++] = new(ShapeHalf, Arena.Center, Angle.FromDirection(QuadrantDir(QuadrantForPosition(magic.Position))));
        for (var i = 0; i < len; ++i)
        {
            aoes[index++] = poison[i];
        }
        return aoes.AsSpan(0, aoesLen);
    }

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        if (_hint.Length > 0)
            hints.Add(_hint);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            var stones = MagicStones;
            var magic = stones.Count != 0 ? MagicStones[0] : null;
            if (magic != null)
            {
                _hint = QuadrantDir(QuadrantForPosition(magic.Position)).Dot(spell.LocXZ - caster.Position) > 0 ? "Stay after charge" : "Swap sides after charge";
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == WatchedAction)
            _hint = "";
    }
}
