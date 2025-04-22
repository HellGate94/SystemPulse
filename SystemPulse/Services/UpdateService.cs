using CommunityToolkit.Mvvm.DependencyInjection;
using Injectio.Attributes;
using System;
using System.Collections.Generic;
using System.Timers;

namespace SystemPulse.Services;

public interface IUpdatable {
    void Update();
}

[RegisterSingleton]
public sealed class UpdateService : IDisposable {
    public static UpdateService? Default => Ioc.Default.GetService<UpdateService>();

    private class UpdateGroup : IDisposable {
        public Timer Timer { get; }
        public List<IUpdatable> Updatables { get; } = [];

        public UpdateGroup(int intervalMilliseconds) {
            Timer = new Timer(intervalMilliseconds) {
                Enabled = true,
                AutoReset = true
            };
            Timer.Elapsed += OnTimerElapsed;
        }

        private void OnTimerElapsed(object? sender, ElapsedEventArgs e) {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => {
                foreach (var updatable in Updatables)
                    updatable.Update();
            });
        }

        public void Dispose() => Timer.Dispose();
    }

    private readonly Dictionary<int, UpdateGroup> _updateGroups = [];

    public void Register(IUpdatable updatable, int intervalMilliseconds) {
        if (!_updateGroups.TryGetValue(intervalMilliseconds, out var updateGroup)) {
            updateGroup = new UpdateGroup(intervalMilliseconds);
            _updateGroups[intervalMilliseconds] = updateGroup;
        }
        updateGroup.Updatables.Add(updatable);
    }

    private readonly List<int> _toRemove = [];
    public void Unregister(IUpdatable updatable) {
        foreach (var (interval, group) in _updateGroups) {
            if (group.Updatables.Remove(updatable) && group.Updatables.Count == 0) {
                _toRemove.Add(interval);
            }
        }
        foreach (var interval in _toRemove) {
            _updateGroups[interval].Dispose();
            _updateGroups.Remove(interval);
        }
        _toRemove.Clear();
    }

    public void Dispose() {
        foreach (var group in _updateGroups.Values)
            group.Dispose();
        _updateGroups.Clear();
    }
}