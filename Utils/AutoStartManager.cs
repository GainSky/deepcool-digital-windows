using Microsoft.Win32.TaskScheduler;

namespace DeepCool_Display.Utils;

public static class AutoStartManager
{
    private const string TaskName = "DeepCool Display";

    public static bool IsAutoStartEnabled()
    {
        using var ts = new TaskService();
        return ts.GetTask(TaskName) != null;
    }

    public static void SetAutoStart(bool enable)
    {
        using var ts = new TaskService();
        if (enable)
        {
            var td = ts.NewTask();
            td.Principal.RunLevel = TaskRunLevel.Highest;
            td.Triggers.Add(new LogonTrigger());
            td.Actions.Add(new ExecAction(Application.ExecutablePath, "--scheduled"));

            ts.RootFolder.RegisterTaskDefinition(TaskName, td);
        }
        else
        {
            ts.RootFolder.DeleteTask(TaskName, false);
        }
    }
}