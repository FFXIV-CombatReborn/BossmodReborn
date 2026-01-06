namespace BossMod.Dawntrail.Extreme.Ex7Doomtrain;

// Technically I've seen two survive aetherosote, but we want the highlighting for fewer than 3 in a stack
sealed class AetherocharAethersoteStackSpread(BossModule module) : Components.IconStackSpread(module, stackIcon=(uint)IconID.Aetherosote, spreadIcon=(uint)IconID.Aetherochar, stackAID=(uint)AID.Aetherosote, spreadAID=(uint)AID.Aetherochar, stackRadius=6f, spreadRadius=6f, minStackSize=3, maxStackSize=3, activationDelay=5.1d)
{
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID is stackIcon or spreadIcon)
        {
            raid = Raid.WithoutSlot(false, true, true);
            if (iconID == stackIcon)
            {
                // Stack always targets healers
                foreach (var actor in raid)
                {
                    if (actor.Role == Role.Healer)
                    {
                        AddStack(actor, WorldState.FutureTime(ActivationDelay));
                    }
                }
            } else {
                // Spread targets any 3, but we can't tell which
                foreach (var actor in raid)
                {
                    if (actor.Role != Role.Tank)
                    {
                        AddSpread(actor, WorldState.FutureTime(ActivationDelay));
                    }
                }
            }
        }
    }
}

// BossModule module, AOEShape shape, uint iconID, uint aid = default, double activationDelay = 5.1d, bool centerAtTarget = false, Actor? source = null, bool tankbuster = false, AIHints.PredictedDamageType damageType = AIHints.PredictedDamageType.Raidwide) : GenericBaitAway(module, aid, centerAtTarget: centerAtTarget, tankbuster: tankbuster, damageType: damageType
sealed class AetherialRay(BossModule module) : Components.BaitAwayIcon(module, shape=new AOEShapeCone(50f, 45f.Degrees()), iconID=(uint)IconID.AetherialRay, aid=AID.AetherialRay, activationDelay=7.6d, centerAtTarget=true, source=OID.GhostTrain, tankbuster=true, damageType=AIHints.PredictedDamageType.Tankbuster);

sealed class RunawayTrain(BossModule module) : Components.RaidwideAfterLogMessage(module, aid=(uint)AID.RunawayTrain, logMessageID=(uint)LogMessageID.RunawayTrain, delay=16d);
