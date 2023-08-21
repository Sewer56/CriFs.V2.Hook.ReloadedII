using FileEmulationFramework.Lib.Utilities;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;

namespace CriFs.V2.Hook.Utilities;

/// <summary>
///     Utility class for querying sigscans to be done in parallel.
/// </summary>
public class SigScanHelper
{
    private readonly IStartupScanner? _startupScanner;
    private readonly Logger? _logger;

    public SigScanHelper(Logger? logger, IStartupScanner? startupScanner)
    {
        _logger = logger;
        _startupScanner = startupScanner;
    }

    public void FindPatternOffset(string? pattern, Action<uint> action, string? name = null)
    {
        _startupScanner?.AddMainModuleScan(pattern, res =>
        {
            if (res.Found)
            {
                if (!String.IsNullOrEmpty(name))
                    _logger?.Info("[CriFs.V2.Hook] {0} found at {1}", name, res.Offset.ToString("X"));

                action((uint)res.Offset);
            }
            else if (!String.IsNullOrEmpty(name))
            {
                _logger?.Error(
                    "[CriFs.V2.Hook] {0} not found. If you're using latest up to date Steam version, raise a GitHub issue.",
                    name);
            }
        });
    }

    public void FindPatternOffsetSilent(string? pattern, Action<uint> action)
    {
        _startupScanner?.AddMainModuleScan(pattern, res =>
        {
            if (res.Found)
            {
                action((uint)res.Offset);
            }
        });
    }
}