namespace BossMod.Dawntrail.Extreme.Ex5Necron;

sealed class Shockwave(BossModule module) : Components.GenericBaitStack(module)
{
    private readonly AOEShapeCone cone = new(100f, 10f.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var id = spell.Action.ID;
        switch (id)
        {
            case (uint)AID.TwofoldBlight:
            case (uint)AID.TheSecondSeason:
                var act = GetActivation((uint)AID.TwofoldBlight);
                var party = Raid.WithoutSlot(true, true, true);
                var source = Module.PrimaryActor;
                var len = party.Length;

                for (var i = 0; i < len; ++i)
                {
                    var p = party[i];
                    if (p.Role == Role.Tank)
                    {
                        CurrentBaits.Add(new(source, p, cone, act));
                    }
                }
                break;
            case (uint)AID.FourfoldBlight:
            case (uint)AID.TheFourthSeason:
                var act2 = GetActivation((uint)AID.FourfoldBlight);
                var party2 = Raid.WithoutSlot(true, true, true);
                var source2 = Module.PrimaryActor;
                var len2 = party2.Length;

                for (var i = 0; i < len2; ++i)
                {
                    var p = party2[i];
                    if (p.Class.IsSupport())
                    {
                        CurrentBaits.Add(new(source2, p, cone, act2));
                    }
                }
                break;
        }
        DateTime GetActivation(uint p1AID) => Module.CastFinishAt(spell, id == p1AID ? 1.4d : 9.8d);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.ShockwaveLP or (uint)AID.ShockwavePS)
        {
            ++NumCasts;
        }
    }
}
