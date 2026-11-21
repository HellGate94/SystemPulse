
using Injectio.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.Win32.TaskScheduler;
using System;
using System.IO;
using System.Linq;
using System.Security.Principal;

namespace SystemPulse.Services;

public record StartupOptions(string TaskName, string Description, string ExecutablePath, string? Arguments = null) {
    public string WorkingDirectory => Path.GetDirectoryName(ExecutablePath)!;
}

[RegisterSingleton]
public sealed class StartupService {
    private readonly StartupOptions _options;
    private readonly ILogger<StartupService> _logger;

    public StartupService(StartupOptions options, ILogger<StartupService> logger) {
        _options = options;
        _logger = logger;
    }

    public bool IsEnabled {
        get {
            using var service = new TaskService();

            var task = service.GetTask(_options.TaskName);
            if (task is null)
                return false;

            var action = task.Definition.Actions
                .OfType<ExecAction>()
                .SingleOrDefault();

            return action is not null && PathsEqual(action.Path, _options.ExecutablePath);
        }
    }

    public void Enable() {
        using var service = new TaskService();

        var definition = service.NewTask();
        var user = WindowsIdentity.GetCurrent().Name;

        definition.RegistrationInfo.Description = _options.Description;

        definition.Principal.UserId = user;
        definition.Principal.LogonType = TaskLogonType.InteractiveToken;
        definition.Principal.RunLevel = TaskRunLevel.Highest;

        definition.Triggers.Add(new LogonTrigger {
            UserId = user
        });

        definition.Actions.Add(new ExecAction(
            _options.ExecutablePath,
            _options.Arguments,
            _options.WorkingDirectory));

        definition.Settings.MultipleInstances = TaskInstancesPolicy.IgnoreNew;

        definition.Settings.StartWhenAvailable = true;
        definition.Settings.DisallowStartIfOnBatteries = false;
        definition.Settings.StopIfGoingOnBatteries = false;
        definition.Settings.ExecutionTimeLimit = TimeSpan.Zero;

        service.RootFolder.RegisterTaskDefinition(
            _options.TaskName,
            definition,
            TaskCreation.CreateOrUpdate,
            user,
            null,
            TaskLogonType.InteractiveToken);

        _logger.LogInformation("Registered startup task with {Options}", _options);
    }

    public void Disable() {
        using var service = new TaskService();

        if (service.GetTask(_options.TaskName) is not null) {
            service.RootFolder.DeleteTask(_options.TaskName);

            _logger.LogInformation("Removed startup task with {Options}", _options.TaskName);
        }
    }

    private static bool PathsEqual(string first, string second) {
        return string.Equals(
            Path.GetFullPath(first),
            Path.GetFullPath(second),
            OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
    }
}
