namespace BossMod.Components;

[SkipLocalsInit]
public abstract class CleansableDebuff(BossModule module, uint statusID, string noun = "Doom", string adjective = "doomed") : BossComponent(module)
{
    private readonly List<Actor> _affected = [];
    private readonly List<Actor> _pending = [];
    private readonly uint StatusID = statusID;
    private readonly string Noun = noun;
    private readonly string Adjective = adjective;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if (status.ID == StatusID)
        {
            var count = _affected.Count;
            for (var i = 0; i < count; ++i) // some status effects can be applied multiple times, filter to avoid duplicate hints
            {
                if (_affected[i] == actor)
                {
                    return; // already exists in list
                }
            }
            _affected.Add(actor);
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if (status.ID == StatusID)
        {
            _pending.Add(actor); // actor status list gets updated after OnStatusLose gets called, so we need to use Update method for final check
        }
    }

    public override void Update()
    {
        var count = _pending.Count - 1;
        for (var i = count; i >= 0; --i)
        {
            var pending = _pending[i];
            if (pending.FindStatus(StatusID) == null) // verify that all instances of the status effect are gone
            {
                _affected.Remove(pending);
            }
            _pending.Remove(pending); // second instance of debuff was found
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var count = _affected.Count;
        if (count != 0)
        {
            var contains = false;
            for (var i = 0; i < count; ++i)
            {
                if (_affected[i] == actor)
                {
                    contains = true;
                    break;
                }
            }
            if (contains)
            {
                if (!(actor.Role == Role.Healer || actor.Class == Class.BRD))
                {
                    hints.Add($"You were {Adjective}! Get cleansed fast.");
                }
                else
                {
                    hints.Add($"Cleanse yourself! ({Noun}).");
                }
            }
            else if (actor.Role == Role.Healer || actor.Class == Class.BRD)
            {
                for (var i = 0; i < count; ++i)
                {
                    hints.Add($"Cleanse {_affected[i].Name}! ({Noun})");
                }
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = _affected.Count;
        for (var i = 0; i < count; ++i)
        {
            var c = _affected[i];

            if (actor.Role == Role.Healer)
            {
                PressAction(ActionDefinitions.Esuna, c, 1f);
            }
            else if (actor.Class == Class.BRD)
            {
                PressAction(ActionDefinitions.WardensPaean, c);
            }
        }
        void PressAction(in ActionID action, Actor target, float castTime = default)
        => hints.ActionsToExecute.Push(action, target, ActionQueue.Priority.High, castTime);
    }
}
