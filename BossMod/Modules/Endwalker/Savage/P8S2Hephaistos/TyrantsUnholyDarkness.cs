namespace BossMod.Endwalker.Savage.P8S2;

sealed class TyrantsUnholyDarkness(BossModule module) : Components.CastCounter(module, (uint)AID.TyrantsUnholyDarknessAOE)
{
    private const float _radius = 6f;

    // TODO: we need some sort of a threat info in worldstate to determine targets properly...
    public bool IsTarget(Actor actor) => actor.Role == Role.Tank;

    private bool NearTarget(Actor actor)
    {
        var party = Raid.WithoutSlot(false, true, true);
        for (var i = 0; i < party.Length; ++i)
        {
            var p = party[i];
            if (p != actor && IsTarget(p) && p.Position.InCircle(actor.Position, _radius))
                return true;
        }
        return false;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (IsTarget(actor))
        {
            hints.Add("GTFO from raid!", Raid.WithoutSlot(false, true, true).AnyInRadiusExcluding(actor, _radius));
        }
        else if (NearTarget(actor))
        {
            hints.Add("GTFO from tanks!");
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => IsTarget(player) ? PlayerPriority.Danger : PlayerPriority.Irrelevant;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var target in Raid.WithoutSlot(false, true, true))
            if (IsTarget(target))
                Arena.ZoneCircleOutline(target.Position, _radius, Colors.Danger);
    }
}
