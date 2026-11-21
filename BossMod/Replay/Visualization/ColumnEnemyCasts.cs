namespace BossMod.ReplayVisualization;

public sealed class ColumnEnemyCasts : ColumnGenericHistory, IToggleableColumn
{
    public bool Visible
    {
        get => Width > 0;
        set => Width = value ? DefaultWidth : 0;
    }

    public ColumnEnemyCasts(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant enemy)
        : base(timeline, tree, phaseBranches)
    {
        var moduleInfo = BossModuleRegistry.FindByOID(enc.OID);
        foreach (var c in enemy.Casts)
        {
            var name = $"{c.ID} ({moduleInfo?.ActionIDType?.GeneratedEnumName(c.ID.ID)}) {ReplayUtils.ParticipantString(enemy, c.Time.Start)} -> {ReplayUtils.ParticipantString(c.Target, c.Time.Start)}";
            this.AddHistoryEntryRange(enc.Time.Start, c.Time, name, c.Interruptible ? Colors.TextColor8 : Colors.TextColor7).AddCastTooltip(c);
        }
        foreach (var a in replay.EncounterActions(enc))
        {
            if (a.Source != enemy)
            {
                continue;
            }
            var name = $"{a.ID} ({moduleInfo?.ActionIDType?.GeneratedEnumName(a.ID.ID)}) {ReplayUtils.ParticipantString(a.Source, a.Timestamp)} -> {ReplayUtils.ParticipantString(a.MainTarget, a.Timestamp)} #{a.GlobalSequence}";
            var color = ColumnUtils.ActionHasDamageToPlayerEffects(a) ? Colors.TextColor1 : Colors.TextColor7;
            this.AddHistoryEntryDot(enc.Time.Start, a.Timestamp, name, color).AddActionTooltip(a);
        }
    }
}
