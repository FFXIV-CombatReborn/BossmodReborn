namespace BossMod;

// class that creates and manages instances of proper boss modules in response to world state changes
[SkipLocalsInit]
public sealed class BossModuleManager : IDisposable
{
    public readonly WorldState WorldState;
    public readonly RaidCooldowns RaidCooldowns;
    public static readonly BossModuleConfig Config = Service.Config.Get<BossModuleConfig>();
    private readonly EventSubscriptions _subsciptions;

    public readonly List<BossModule> PendingModules = [];
    public readonly List<BossModule> LoadedModules = [];
    public Event<BossModule> ModuleLoaded = new();
    public Event<BossModule> ModuleUnloaded = new();
    public Event<BossModule> ModuleActivated = new();
    public Event<BossModule> ModuleDeactivated = new();

    // drawn module among loaded modules; this can be changed explicitly if needed
    // usually we don't have multiple concurrently active modules, since this prevents meaningful cd planning, raid cooldown tracking, etc.
    // but it can theoretically happen e.g. around checkpoints and in typically trivial outdoor content
    private BossModule? _activeModule;
    private bool _activeModuleOverridden;
    public BossModule? ActiveModule
    {
        get => _activeModule;
        set
        {
            Service.Log($"[BMM] Active module override: from {_activeModule?.GetType().FullName ?? "<n/a>"} (manual-override={_activeModuleOverridden}) to {value?.GetType().FullName ?? "<n/a>"}");
            _activeModule = value;
            _activeModuleOverridden = true;
        }
    }

    public BossModuleManager(WorldState ws)
    {
        WorldState = ws;
        RaidCooldowns = new(ws);
        _subsciptions = new
        (
            WorldState.Actors.Added.Subscribe(ActorAdded),
            Config.Modified.ExecuteAndSubscribe(ConfigChanged)
        );

        foreach (var a in WorldState.Actors)
            ActorAdded(a);
    }

    public void Dispose()
    {
        _activeModule = null;
        foreach (var m in LoadedModules)
            m.Dispose();
        LoadedModules.Clear();

        _subsciptions.Dispose();
        RaidCooldowns.Dispose();
    }

    public void Update()
    {
        // update all loaded modules, handle activation/deactivation
        var bestPriority = 0;
        BossModule? bestModule = null;
        var anyModuleActivated = false;

        var maxDist = Config.MaxLoadDistance;
        if (WorldState.Party[0]?.PosRot.AsVector3() is Vector3 playerPos)
        {
            var countP = PendingModules.Count - 1;
            var maxSq = maxDist * maxDist;
            for (var i = countP; i >= 0; --i)
            {
                var m = PendingModules[i];
                var prim = m.PrimaryActor;
                if (prim.IsDeadOrDestroyed)
                {
                    PendingModules.RemoveAt(i);
                    m.Dispose();
                    continue;
                }
                if (m.OnlyLoadIfTargetable && !m.PrimaryActor.IsTargetable)
                {
                    continue;
                }
                if ((playerPos - prim.PosRot.AsVector3()).LengthSquared() <= maxSq)
                {
                    var countL = LoadedModules.Count;
                    var oid = prim.OID;

                    var exists = false;
                    for (var j = 0; j < countL; ++j)
                    {
                        if (oid == LoadedModules[j].PrimaryActor.OID)
                        {
                            exists = true;
                            break;
                        }
                    }
                    if (!exists)
                    {
                        LoadedModules.Add(m);
                        Service.Log($"[BMM] Boss module '{m.GetType()}' moved from pending to loaded for actor {prim}");
                        ModuleLoaded.Fire(m);
                        PendingModules.RemoveAt(i);
                    }
                }
            }

            for (var i = 0; i < LoadedModules.Count; ++i)
            {
                var m = LoadedModules[i];
                var wasActive = m.StateMachine.ActiveState != null;
                bool isActive;
                try
                {
                    m.Update();
                    isActive = m.StateMachine.ActiveState != null;
                }
                catch (Exception ex)
                {
                    Service.Log($"Boss module {m.GetType()} crashed: {ex}");
                    wasActive = true; // force unload if exception happened before activation
                    isActive = false;
                }

                // if module was activated or deactivated, notify listeners
                if (isActive != wasActive)
                    (isActive ? ModuleActivated : ModuleDeactivated).Fire(m);

                var actor = m.PrimaryActor;
                // unload module because it is not active and player is out of desired range
                if (!isActive && (playerPos - actor.PosRot.AsVector3()).LengthSquared() > maxSq && actor.SpawnIndex != -99)
                {
                    UnloadModule(i--);
                    if (!actor.IsDestroyed)
                    {
                        ActorAdded(actor);
                    }
                    continue;
                }

                // unload module either if it became deactivated or its primary actor disappeared without ever activating
                if (!isActive && (wasActive || m.PrimaryActor.IsDestroyed))
                {
                    UnloadModule(i--);
                    continue;
                }

                // if module is active and wants to be reset, oblige
                if (isActive && m.CheckReset())
                {
                    ModuleDeactivated.Fire(m);
                    UnloadModule(i--);
                    if (!actor.IsDestroyed)
                        ActorAdded(actor);
                    continue;
                }

                // module remains loaded
                var priority = ModuleDisplayPriority(m);
                if (priority > bestPriority)
                {
                    bestPriority = priority;
                    bestModule = m;
                }

                if (!wasActive && isActive)
                {
                    Service.Log($"[BMM] Boss module '{m.GetType()}' for actor {m.PrimaryActor.InstanceID:X} ({m.PrimaryActor.OID:X}) '{m.PrimaryActor.Name}' activated");
                    anyModuleActivated |= true;
                }
            }

            var curPriority = ModuleDisplayPriority(_activeModule);
            if (bestPriority > curPriority && (anyModuleActivated || !_activeModuleOverridden))
            {
                Service.Log($"[BMM] Active module change: from {_activeModule?.GetType().FullName ?? "<n/a>"} (prio {curPriority}, manual-override={_activeModuleOverridden}) to {bestModule?.GetType().FullName ?? "<n/a>"} (prio {bestPriority})");
                _activeModule = bestModule;
                _activeModuleOverridden = false;
            }
        }
    }

    private void LoadModule(BossModule m, bool oidExists = false)
    {
        var maxDist = Config.MaxLoadDistance;
        if (!oidExists && WorldState.Party[0]?.PosRot.AsVector3() is Vector3 playerPos && (playerPos - m.PrimaryActor.PosRot.AsVector3()).LengthSquared() <= maxDist * maxDist)
        {
            LoadedModules.Add(m);
            Service.Log($"[BMM] Boss module '{m.GetType()}' loaded for actor {m.PrimaryActor}");
            ModuleLoaded.Fire(m);
        }
        else
        {
            PendingModules.Add(m);
            Service.Log($"[BMM] Boss module '{m.GetType()}' loaded as pending for actor {m.PrimaryActor}");
        }
    }

    private void UnloadModule(int index)
    {
        var m = LoadedModules[index];
        Service.Log($"[BMM] Boss module '{m.GetType()}' unloaded for actor {m.PrimaryActor}");
        ModuleUnloaded.Fire(m);
        if (_activeModule == m)
        {
            _activeModule = null;
            _activeModuleOverridden = false;
        }
        m.Dispose();
        LoadedModules.RemoveAt(index);
    }

    private static int ModuleDisplayPriority(BossModule? m)
    {
        if (m == null)
            return 0;
        if (m.StateMachine.ActiveState != null)
            return 4;
        if (m.PrimaryActor.InstanceID == default)
            return 2; // demo module
        if (!m.PrimaryActor.IsDestroyed && !m.PrimaryActor.IsDead && m.PrimaryActor.IsTargetable)
            return 3;
        return 1;
    }

    private DemoModule CreateDemoModule() => new(WorldState, new(default, default, -99, default, default!, default, default, default, default, WorldState.Party[0]?.PosRot ?? default));

    private void ActorAdded(Actor actor)
    {
        var m = BossModuleRegistry.CreateModuleForActor(WorldState, actor, Config.MinMaturity);
        if (m != null)
        {
            var count = LoadedModules.Count;
            for (var i = 0; i < count; ++i)
            {
                var module = LoadedModules[i];
                var prim = module.PrimaryActor;
                if (prim.OID == actor.OID)
                {
                    if (prim.InstanceID == actor.InstanceID)  // module already exists, but actor reference was no longer valid (eg due to teleports at Necron)
                    {
                        module.PrimaryActor = actor;
                        return;
                    }
                    LoadModule(m, true); // module for oid already loaded, usually means its a trash mob
                    return;
                }
            }
            LoadModule(m, m.OnlyLoadIfTargetable);
        }
    }

    private void ConfigChanged()
    {
        var demoIndex = LoadedModules.FindIndex(m => m is DemoModule);
        if (Config.ShowDemo && demoIndex < 0)
            LoadModule(CreateDemoModule());
        else if (!Config.ShowDemo && demoIndex >= 0)
            UnloadModule(demoIndex);
    }
}
