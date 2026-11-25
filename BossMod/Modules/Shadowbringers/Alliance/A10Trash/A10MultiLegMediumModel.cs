namespace BossMod.Shadowbringers.Alliance.A10MultiLegMediumModel;

public enum OID : uint
{
    Boss = 0x2C7D, // R3.480, x1
    SmallStubby = 0x2C7A, // R0.960, x9
    SmallFlyer = 0x2C7C, // R1.320, x4
    SmallBiped = 0x2C7B, // R0.960, x2
    SmallFlyerBomb = 0x2C79 // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 872, // SmallStubby/SmallBiped->player, no cast, single-target

    HighFrequencyLaser = 18718, // Boss->self, 4.0s cast, range 40 width 6 rect
    FrontalSomersault = 18715, // SmallBiped->location, 3.0s cast, range 6 circle
    LowPoweredOffensive = 18716, // SmallFlyer->self, 4.0s cast, single-target
    EnergyBomb = 18717 // SmallFlyerBomb->player/2P, no cast, single-target
}

public enum SID : uint
{
    VulnerabilityUp = 1789 // SmallFlyerBomb/SmallBiped->player, extra=0x1/0x2/0x3
}

sealed class HighFrequencyLaser(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HighFrequencyLaser, new AOEShapeRect(40f, 3f));
sealed class FrontalSomersault(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FrontalSomersault, 6f);

sealed class EnergyBomb(BossModule module) : Components.Voidzone(module, 3f, GetBombs, moveHintLength: 4f)
{
    private static Actor[] GetBombs(BossModule module)
    {
        var enemies = module.Enemies((uint)OID.SmallFlyerBomb);
        var count = enemies.Count;
        if (count == 0)
        {
            return [];
        }

        var bombs = new Actor[count];
        var index = 0;
        for (var i = 0; i < count; ++i)
        {
            var a = enemies[i];
            if (!a.IsDead && a.Renderflags == 0)
            {
                bombs[index++] = a;
            }
        }
        return bombs[..index];
    }
}

sealed class A10MultiLegMediumModelStates : StateMachineBuilder
{
    public A10MultiLegMediumModelStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HighFrequencyLaser>()
            .ActivateOnEnter<FrontalSomersault>()
            .ActivateOnEnter<EnergyBomb>()
            .Raw.Update = () => AllDestroyed(A10MultiLegMediumModel.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 700, NameID = 9137, SortOrder = 2)]
public sealed class A10MultiLegMediumModel(WorldState ws, Actor primary) : BossModule(ws, primary, new(-627f, -287f), new ArenaBoundsRect(19.5f, 24.5f))
{
    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.SmallFlyer, (uint)OID.SmallBiped];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, Trash);
    }
}
