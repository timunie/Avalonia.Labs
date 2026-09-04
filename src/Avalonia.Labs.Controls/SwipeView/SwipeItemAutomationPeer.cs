using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;

namespace Avalonia.Labs.Controls.Automation.Peers;

/// <summary>
/// Automation peer for a swipe item direction.
/// </summary>
public class SwipeItemAutomationPeer : ControlAutomationPeer, ISelectionItemProvider
{
    private readonly SwipeAutomationPeer _container;
    private readonly SwipeState _openSwipeState;

    /// <summary>
    /// Initializes a new instance of the <see cref="SwipeItemAutomationPeer"/> class.
    /// </summary>
    /// <param name="container">The container swipe automation peer.</param>
    /// <param name="swipeItem">The swipe item direction.</param>
    public SwipeItemAutomationPeer(SwipeAutomationPeer container, OpenSwipeItem swipeItem)
        : base(container.Owner)
    {
        _container = container;
        SwipeItem = swipeItem;
        _openSwipeState = ToSwipeState(swipeItem);
    }

    /// <summary>
    /// Gets the owner <see cref="Swipe"/> control.
    /// </summary>
    public new Swipe Owner => _container.Owner;

    /// <inheritdoc />
    public bool IsSelected => Owner.SwipeState == _openSwipeState;

    /// <inheritdoc />
    public ISelectionProvider SelectionContainer => _container;

    /// <summary>
    /// Gets the swipe item direction.
    /// </summary>
    public OpenSwipeItem SwipeItem { get; }

    /// <inheritdoc />
    public void AddToSelection()
    {
        Select();
    }

    /// <inheritdoc />
    public void RemoveFromSelection()
    {
        if (IsSelected)
        {
            Owner.Close();
        }
    }

    /// <inheritdoc />
    public void Select()
    {
        Owner.Open(SwipeItem);
    }

    /// <inheritdoc />
    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.ListItem;
    }

    /// <inheritdoc />
    protected override string GetClassNameCore()
    {
        return nameof(SwipeItemAutomationPeer);
    }

    /// <inheritdoc />
    protected override string? GetNameCore()
    {
        return SwipeItem.ToString();
    }

    private static SwipeState ToSwipeState(OpenSwipeItem item) =>
        item switch
        {
            OpenSwipeItem.TopItems => SwipeState.TopVisible,
            OpenSwipeItem.RightItems => SwipeState.RightVisible,
            OpenSwipeItem.LeftItems => SwipeState.LeftVisible,
            OpenSwipeItem.BottomItems => SwipeState.BottomVisible,
            _ => SwipeState.Hidden
        };
}
