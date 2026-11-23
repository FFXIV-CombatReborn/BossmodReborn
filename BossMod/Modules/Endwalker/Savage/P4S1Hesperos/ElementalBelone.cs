namespace BossMod.Endwalker.Savage.P4S1Hesperos;

// state related to elemental belone mechanic (3 of 4 corners exploding)
sealed class ElementalBelone : BossComponent
{
    public bool Visible;
    private readonly SettingTheScene.Element _safeElement;
    private readonly List<WPos> _imminentExplodingCorners = [];

    public ElementalBelone(BossModule module) : base(module)
    {
        var assignments = module.FindComponent<SettingTheScene>()!;
        uint forbiddenCorners = 1; // 0 corresponds to 'unknown' corner
        foreach (var actor in WorldState.Actors.Where(a => a.OID == (uint)OID.Helper).Tethered(TetherID.Bloodrake))
            forbiddenCorners |= 1u << (int)assignments.FromPos(actor.Position);
        var safeCorner = (SettingTheScene.Corner)BitOperations.TrailingZeroCount(~forbiddenCorners);
        _safeElement = assignments.FindElement(safeCorner);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        for (var i = 0; i < _imminentExplodingCorners.Count; ++i)
        {
            if (actor.Position.InRect(_imminentExplodingCorners[i], new WDir(1f, default), 10f, 10f, 10f))
            {
                hints.Add("GTFO from exploding square");
                break;
            }
        }
    }

    public override void AddGlobalHints(Actor actor, GlobalHints hints)
    {
        hints.Add($"Safe square: {_safeElement}");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Visible)
        {
            var assignments = Module.FindComponent<SettingTheScene>()!;
            var safeCorner = assignments.Assignment(_safeElement);
            if (safeCorner != SettingTheScene.Corner.Unknown)
            {
                var p = Arena.Center + 10 * assignments.Direction(safeCorner);
                Arena.ZoneRect(p, new WDir(1, 0), 10, 10, 10, Colors.SafeFromAOE);
            }
        }
        foreach (var p in _imminentExplodingCorners)
        {
            Arena.ZoneRect(p, new WDir(1, 0), 10, 10, 10, Colors.AOE);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.PeriaktoiDangerAcid:
            case (uint)AID.PeriaktoiDangerLava:
            case (uint)AID.PeriaktoiDangerWell:
            case (uint)AID.PeriaktoiDangerLevinstrike:
                _imminentExplodingCorners.Add(caster.Position);
                break;
        }
    }
}
