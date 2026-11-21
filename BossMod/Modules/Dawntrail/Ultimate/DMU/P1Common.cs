namespace BossMod.Dawntrail.Ultimate.DMU;

class BlizzardIIIBlowout : Components.SimpleAOEGroups
{
    public BlizzardIIIBlowout(BossModule module) : base(module, [(uint)AID.BlizzardIIIBlowout, (uint)AID.BlizzardIIIBlowout1],
        new AOEShapeCone(40f, 45f.Degrees()))
    {
        Risky = false;
    }
}

class LightningSafeSpots : Components.SimpleAOEGroups
{
    public LightningSafeSpots(BossModule module) : base(module, [(uint)AID.ThrummingThunderIII, (uint)AID.ThrummingThunderIII1],
        new AOEShapeRect(40.0f, 5.0f))
    {
        Risky = false;
    }
}

sealed class LightOfJudgment(BossModule module) : Components.RaidwideCast(module, (uint)AID.P1LightOfJudgment);
