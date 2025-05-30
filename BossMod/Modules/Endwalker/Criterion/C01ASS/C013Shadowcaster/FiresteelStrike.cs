﻿namespace BossMod.Endwalker.VariantCriterion.C01ASS.C013Shadowcaster;

sealed class FiresteelStrike : Components.UniformStackSpread
{
    public int NumJumps;
    public int NumCleaves;
    private readonly List<Actor> _jumpTargets = [];
    private readonly List<Actor> _interceptors = [];

    private static readonly AOEShapeRect _cleaveShape = new(65f, 4f);

    public FiresteelStrike(BossModule module) : base(module, default, 10f, alwaysShowSpreads: true)
    {
        AddSpreads(Raid.WithoutSlot(true, true, true));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (NumJumps < 2)
        {
            base.AddHints(slot, actor, hints);
        }
        else if (NumCleaves < _jumpTargets.Count)
        {
            if (_jumpTargets[NumCleaves] == actor)
                hints.Add("Hide behind someone!", !TargetIntercepted());
            else if (_interceptors.Contains(actor))
                hints.Add("Intercept next cleave!", !TargetIntercepted());
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        if (NumJumps < 2)
            return base.CalcPriority(pcSlot, pc, playerSlot, player, ref customColor);
        else if (NumCleaves < _jumpTargets.Count)
            return player == _jumpTargets[NumCleaves] ? PlayerPriority.Danger : PlayerPriority.Normal;
        else
            return PlayerPriority.Irrelevant;
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (NumJumps >= 2 && NumCleaves < _jumpTargets.Count)
        {
            var target = _jumpTargets[NumCleaves];
            _cleaveShape.Draw(Arena, Module.PrimaryActor.Position, Angle.FromDirection(target.Position - Module.PrimaryActor.Position), target == pc || _interceptors.Contains(pc) ? Colors.SafeFromAOE : default);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.NFiresteelStrikeAOE1:
            case (uint)AID.NFiresteelStrikeAOE2:
            case (uint)AID.SFiresteelStrikeAOE1:
            case (uint)AID.SFiresteelStrikeAOE2:
                if ((spell.Targets.Count > 0 ? WorldState.Actors.Find(spell.Targets[0].ID) : null) is var target && target != null)
                {
                    _jumpTargets.Add(target);
                    Spreads.RemoveAll(s => s.Target == target);
                }
                if (++NumJumps == 2)
                {
                    // players that were not jumped on are now interceptors
                    _interceptors.AddRange(Spreads.Select(s => s.Target));
                    Spreads.Clear();
                }
                break;
            case (uint)AID.NBlessedBeaconAOE1:
            case (uint)AID.NBlessedBeaconAOE2:
            case (uint)AID.SBlessedBeaconAOE1:
            case (uint)AID.SBlessedBeaconAOE2:
                if (spell.Targets.Count > 0)
                {
                    _interceptors.RemoveAll(a => a.InstanceID == spell.Targets[0].ID);
                }
                ++NumCleaves;
                break;
        }
    }

    private bool TargetIntercepted()
    {
        var target = NumCleaves < _jumpTargets.Count ? _jumpTargets[NumCleaves] : null;
        if (target == null)
            return true;

        var toTarget = target.Position - Module.PrimaryActor.Position;
        var angle = Angle.FromDirection(toTarget);
        var distSq = toTarget.LengthSq();
        return _interceptors.InShape(_cleaveShape, Module.PrimaryActor.Position, angle).Any(a => (a.Position - Module.PrimaryActor.Position).LengthSq() < distSq);
    }
}
