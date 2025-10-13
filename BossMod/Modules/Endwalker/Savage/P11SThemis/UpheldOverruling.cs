namespace BossMod.Endwalker.Savage.P11SThemis;

class UpheldOverruling(BossModule module) : Components.UniformStackSpread(module, 6f, 13f, 7, 7)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.UpheldOverrulingLight:
            case (uint)AID.UpheldRulingLight:
                if (WorldState.Actors.Find(caster.Tether.Target) is var stackTarget && stackTarget != null)
                    AddStack(stackTarget, Module.CastFinishAt(spell, 0.3d));
                break;
            case (uint)AID.UpheldOverrulingDark:
            case (uint)AID.UpheldRulingDark:
                if (WorldState.Actors.Find(caster.Tether.Target) is var spreadTarget && spreadTarget != null)
                    AddSpread(spreadTarget, Module.CastFinishAt(spell, 0.3d));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.UpheldOverrulingAOELight:
            case (uint)AID.UpheldRulingAOELight:
                Stacks.Clear();
                break;
            case (uint)AID.UpheldOverrulingAOEDark:
            case (uint)AID.UpheldRulingAOEDark:
                Spreads.Clear();
                break;
        }
    }
}

abstract class Lightburst(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, 13f);
class LightburstBoss(BossModule module) : Lightburst(module, (uint)AID.LightburstBoss);
class LightburstClone(BossModule module) : Lightburst(module, (uint)AID.LightburstClone);

abstract class DarkPerimeter(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeDonut(8f, 50f));
class DarkPerimeterBoss(BossModule module) : DarkPerimeter(module, (uint)AID.DarkPerimeterBoss);
class DarkPerimeterClone(BossModule module) : DarkPerimeter(module, (uint)AID.DarkPerimeterClone);
