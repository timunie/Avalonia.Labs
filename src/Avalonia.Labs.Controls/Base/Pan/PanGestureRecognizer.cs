using System;

using Avalonia.Input;
using Avalonia.Input.GestureRecognizers;

namespace Avalonia.Labs.Controls.Base.Pan;

/// <summary>
/// The gesture recognizer for pan gesture 
/// </summary>
public class PanGestureRecognizer : GestureRecognizer
{
    private IInputElement? _inputElement;
    private IPointer? _tracking;
    private Point _startPosition;
    private Point _delta;
    private PanGestureStatus _state;
    private Visual? _visual;
    private Visual? _parent;
    private bool _isIgnored;

    public event EventHandler<PanUpdatedEventArgs>? OnPan;

    public PanDirection Direction { get; set; } =
        PanDirection.Left | PanDirection.Right | PanDirection.Up | PanDirection.Down;

    public float Threshold { get; set; } = 5;

    /// <inheritdoc />
    protected override void PointerPressed(PointerPressedEventArgs e)
    {
        _inputElement = Target;
        _tracking = e.Pointer;
        _visual = Target as Visual;
        _parent = _visual?.Parent as Visual;
        var container = _parent ?? _visual;
        _startPosition = e.GetPosition(container);
        _delta = default;
        _state = PanGestureStatus.Started;
        _isIgnored = false;
    }

    /// <inheritdoc />
    protected override void PointerMoved(PointerEventArgs e)
    {
        if (e.Pointer != _tracking || _isIgnored)
        {
            return;
        }

        if (Direction == PanDirection.None)
        {
            return;
        }

        var container = _parent ?? _visual;
        var currentPosition = e.GetPosition(container);
        _delta = currentPosition - _startPosition;

        var absX = Math.Abs(_delta.X);
        var absY = Math.Abs(_delta.Y);

        if (_state != PanGestureStatus.Running)
        {
            if (absX < Threshold && absY < Threshold)
            {
                return;
            }

            var dominantDirection = PanDirection.None;
            if (absX >= absY)
            {
                dominantDirection = _delta.X < 0 ? PanDirection.Left : PanDirection.Right;
            }
            else
            {
                dominantDirection = _delta.Y < 0 ? PanDirection.Up : PanDirection.Down;
            }

            if ((dominantDirection & Direction) == 0)
            {
                // The movement is in an orthogonal or disabled direction.
                // Do not capture pointer, do not handle event, and do not prevent ancestor recognizers (e.g. ScrollViewer).
                _isIgnored = true;
                return;
            }

            _state = PanGestureStatus.Running;
            Capture(e.Pointer);
            e.PreventGestureRecognition();
            e.Handled = true;

            OnPan?.Invoke(_inputElement, new PanUpdatedEventArgs(PanGestureStatus.Started, 0, 0));
            OnPan?.Invoke(_inputElement, new PanUpdatedEventArgs(PanGestureStatus.Running, _delta.X, _delta.Y));
        }
        else
        {
            OnPan?.Invoke(_inputElement, new PanUpdatedEventArgs(PanGestureStatus.Running, _delta.X, _delta.Y));
            e.Handled = true;
        }
    }

    /// <inheritdoc />
    protected override void PointerReleased(PointerReleasedEventArgs e)
    {
        if (e.Pointer != _tracking)
        {
            return;
        }

        var wasRunning = _state == PanGestureStatus.Running;
        var container = _parent ?? _visual;
        var currentPosition = e.GetPosition(container);
        var delta = currentPosition - _startPosition;

        _tracking = null;
        _isIgnored = false;
        _state = PanGestureStatus.Completed;

        if (wasRunning)
        {
            OnPan?.Invoke(_inputElement,
                new PanUpdatedEventArgs(PanGestureStatus.Completed, delta.X, delta.Y));

            e.Handled = true;
        }
    }

    /// <inheritdoc />
    protected override void PointerCaptureLost(IPointer pointer)
    {
        if (pointer != _tracking)
        {
            return;
        }

        var delta = _delta;
        var wasRunning = _state == PanGestureStatus.Running;

        _tracking = null;
        _isIgnored = false;
        _delta = default;
        _state = PanGestureStatus.Completed;

        if (wasRunning)
        {
            OnPan?.Invoke(_inputElement,
                new PanUpdatedEventArgs(
                    PanGestureStatus.Completed,
                    delta.X,
                    delta.Y));
        }
    }
}
