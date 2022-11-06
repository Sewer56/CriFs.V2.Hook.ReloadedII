using System.Diagnostics;
using CriFs.V2.Hook.Bind.Interfaces;

namespace CriFs.V2.Hook.Bind.Utilities;

/// <summary>
/// Provides the ID of the current process.
/// </summary>
public class CurrentProcessProvider : ICurrentProcessProvider
{
    private int _currentProcId;
    
    public CurrentProcessProvider() => _currentProcId = Process.GetCurrentProcess().Id;

    public CurrentProcessProvider(int currentProcId) => _currentProcId = currentProcId;

    public int GetProcessId() => _currentProcId;
}