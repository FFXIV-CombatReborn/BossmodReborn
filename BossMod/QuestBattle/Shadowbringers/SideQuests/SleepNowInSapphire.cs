namespace BossMod.QuestBattle.Shadowbringers.SideQuests;

class SapphireWeapon(WorldState ws) : UnmanagedRotation(ws, 40f)
{
    protected override void Exec(Actor? primaryTarget)
    {
        var havePyretic = Player.FindStatus((uint)Roleplay.SID.PyreticBooster) != null;
        var haveAegis = Player.FindStatus((uint)Roleplay.SID.AetherialAegis) != null;
        var pyreticLock = Player.FindStatus((uint)Roleplay.SID.SafetyLockPyreticBooster) != null;

        if (primaryTarget == null)
        {
            if (havePyretic)
                Hints.StatusesToCancel.Add(((uint)Roleplay.SID.PyreticBooster, Player.InstanceID));
            if (haveAegis)
                Hints.StatusesToCancel.Add(((uint)Roleplay.SID.AetherialAegis, Player.InstanceID));
            return;
        }

        if (!havePyretic && !pyreticLock)
            UseAction(Roleplay.AID.PyreticBooster, Player, -100f);

        var vuln = StatusDetails(primaryTarget, 444u, Player.InstanceID);
        if (vuln.Left < 5f && MP >= 800u)
            UseAction(Roleplay.AID.AetherMine, primaryTarget);

        if (MP >= 300)
        {
            switch (ComboAction)
            {
                case Roleplay.AID.Aethersaber:
                    Hints.GoalZones.Add(Hints.GoalSingleTarget(primaryTarget, 10f));
                    UseAction(Roleplay.AID.Aethercut, primaryTarget);
                    break;
                case Roleplay.AID.Aethercut:
                    Hints.GoalZones.Add(Hints.GoalSingleTarget(primaryTarget, 10f));
                    UseAction(Roleplay.AID.FinalFlourish, primaryTarget);
                    break;
            }

            if (!ActionDefinitions.IsDashDangerous(Player.Position, primaryTarget.Position, Hints))
            {
                UseAction(Roleplay.AID.Aethersaber, primaryTarget);
            }
        }

        UseAction(Roleplay.AID.AetherCannon, primaryTarget);

        if (Player.HPMP.CurMP <= 7000u)
            UseAction(Roleplay.AID.AutoRestoration, Player, -100f);
    }
}

[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, 740u)]
internal class SleepNowInSapphire(WorldState ws) : QuestBattle(ws)
{
    private readonly SapphireWeapon _weapon = new(ws);

    public override List<QuestObjective> DefineObjectives(WorldState ws) => [

        new QuestObjective(ws)
            .With(obj =>
            {
                var pyreticActivate = false;
                var shieldActivate = false;

                obj.OnStatusLose += (act, status) =>
                {
                    pyreticActivate |= status.ID == (uint)Roleplay.SID.SafetyLockPyreticBooster;
                    shieldActivate |= status.ID == (uint)Roleplay.SID.SafetyLockAetherialAegis;
                };

                obj.OnStatusGain += (act, status) => {
                    if (status.ID == (uint)Roleplay.SID.PyreticBooster)
                        pyreticActivate = false;
                    if (status.ID == (uint)Roleplay.SID.AetherialAegis)
                        shieldActivate = false;
                };

                obj.AddAIHints += (player, hints) => {
                    hints.PrioritizeAll();
                    if (pyreticActivate)
                        hints.ActionsToExecute.Push(ActionID.MakeSpell(Roleplay.AID.PyreticBooster), player, ActionQueue.Priority.High);
                    if (shieldActivate)
                        hints.ActionsToExecute.Push(ActionID.MakeSpell(Roleplay.AID.AetherialAegis), player, ActionQueue.Priority.High);
                };
            })
        ];

    public override void AddQuestAIHints(Actor player, AIHints hints)
    {
        _weapon.Execute(player, hints);
    }
}
