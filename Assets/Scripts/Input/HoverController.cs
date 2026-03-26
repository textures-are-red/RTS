using System.Collections.Generic;

public static class HoverController
{
    public static bool IsEnteredObject => _activatedDetectors.Count is not 0;

    private static LinkedList<HoverDetector> _detectors = new();
    private static LinkedList<HoverDetector> _activatedDetectors = new();

    public static void AddDetector(HoverDetector newDetector)
    {
        if (_detectors.Contains(newDetector) is false)
        {
            _detectors.AddLast(newDetector);
            newDetector.OnEnter += OnEnter;
            newDetector.OnExit += OnExit;
        }
    }

    public static void RemoveDetector(HoverDetector byeDetector)
    {
        _detectors.Remove(byeDetector);
        _activatedDetectors.Remove(byeDetector);
        byeDetector.OnEnter -= OnEnter;
        byeDetector.OnExit -= OnExit;
    }

    private static void OnEnter(HoverDetector detector)
    {
        if (_activatedDetectors.Contains(detector) is false)
            _activatedDetectors.AddLast(detector);
    }

    private static void OnExit(HoverDetector detector)
    {
        _activatedDetectors.Remove(detector);
    }
}
