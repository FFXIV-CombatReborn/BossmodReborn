using System;

namespace BossMod;

[ConfigDisplay(Name = "RSR Integration", Order = 9)]
public sealed class RSRIntegrationConfig : ConfigNode
{
    [PropertyDisplay("Enable RSR integration", tooltip: "Allow BossMod to trigger Rotation Solver Reborn states via IPC.")]
    public bool Enable = false;

    [PropertyDisplay("Trigger Anti-knockback", tooltip: "Request RSR's AntiKnockback special when a knockback is imminent.")]
    public bool TriggerAntiKnockback = false;

    [PropertyDisplay("Trigger raidwide defense", tooltip: "Request RSR's DefenseArea special for imminent raidwides.")]
    public bool TriggerDefenseArea = false;

    [PropertyDisplay("Trigger tankbuster defense", tooltip: "Request RSR's DefenseSingle special when a tankbuster targets the player.")]
    public bool TriggerDefenseSingle = false;

    [PropertyDisplay("Trigger dispel/stance/positional", tooltip: "Request RSR's Dispel/Stance/Positional special when cleansing is needed. Restricted to BRD/WHM/SGE/SCH/AST.")]
    public bool TriggerDispelStancePositional = false;
}
