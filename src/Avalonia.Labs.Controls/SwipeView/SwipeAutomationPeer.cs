using System;
using System.Collections.Generic;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;

namespace Avalonia.Labs.Controls.Automation.Peers;

/// <summary>
/// Automation peer for the <see cref="Swipe"/> control.
/// </summary>
public class SwipeAutomationPeer : ControlAutomationPeer, ISelectionProvider
{
    private readonly Dictionary<OpenSwipeItem, SwipeItemAutomationPeer> _itemPeers = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SwipeAutomationPeer"/> class.
    /// </summary>
    /// <param name="owner">The owner swipe control.</param>
    public SwipeAutomationPeer(Swipe owner)
        : base(owner)
    {
        owner.PropertyChanged += Owner_PropertyChanged;
    }

    private void Owner_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == Swipe.SwipeStateProperty)
        {
            RaisePropertyChangedEvent(SelectionPatternIdentifiers.SelectionProperty, null, null);
        }
    }

    /// <summary>
    /// Gets the owner <see cref="Swipe"/> control.
    /// </summary>
    public new Swipe Owner => (Swipe)base.Owner;

    /// <inheritdoc />
    public bool CanSelectMultiple => false;

    /// <inheritdoc />
    public bool IsSelectionRequired => false;

    /// <inheritdoc />
    public IReadOnlyList<AutomationPeer> GetSelection()
    {
        var activeItem = Owner.SwipeState switch
        {
            SwipeState.LeftVisible => OpenSwipeItem.LeftItems,
            SwipeState.TopVisible => OpenSwipeItem.TopItems,
            SwipeState.RightVisible => OpenSwipeItem.RightItems,
            SwipeState.BottomVisible => OpenSwipeItem.BottomItems,
            _ => (OpenSwipeItem?)null
        };

        if (activeItem.HasValue)
        {
            return [GetOrCreateItemPeer(activeItem.Value)];
        }

        return Array.Empty<AutomationPeer>();
    }

    /// <inheritdoc />
    protected override AutomationControlType GetAutomationControlTypeCore()
    {
        return AutomationControlType.Pane;
    }

    /// <inheritdoc />
    protected override string GetClassNameCore()
    {
        return nameof(Swipe);
    }

    /// <inheritdoc />
    protected override IReadOnlyList<AutomationPeer> GetChildrenCore()
    {
        var baseChildren = base.GetChildrenCore();
        var children = baseChildren is not null
            ? new List<AutomationPeer>(baseChildren)
            : new List<AutomationPeer>();

        if (Owner.Left is not null)
        {
            children.Add(GetOrCreateItemPeer(OpenSwipeItem.LeftItems));
        }
        if (Owner.Top is not null)
        {
            children.Add(GetOrCreateItemPeer(OpenSwipeItem.TopItems));
        }
        if (Owner.Right is not null)
        {
            children.Add(GetOrCreateItemPeer(OpenSwipeItem.RightItems));
        }
        if (Owner.Bottom is not null)
        {
            children.Add(GetOrCreateItemPeer(OpenSwipeItem.BottomItems));
        }

        return children;
    }

    /// <summary>
    /// Gets or creates a <see cref="SwipeItemAutomationPeer"/> for the specified swipe item direction.
    /// </summary>
    /// <param name="item">The swipe item direction.</param>
    /// <returns>The automation peer for the swipe item direction.</returns>
    public SwipeItemAutomationPeer GetOrCreateItemPeer(OpenSwipeItem item)
    {
        if (!_itemPeers.TryGetValue(item, out var peer))
        {
            peer = new SwipeItemAutomationPeer(this, item);
            _itemPeers[item] = peer;
        }
        return peer;
    }
}
