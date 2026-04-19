using System.Collections;
using Avalonia.Interactivity;

namespace Avalonia.Labs.Controls;

public class AddedItemEventArgs : RoutedEventArgs
{
    public AddedItemEventArgs(RoutedEvent routedEvent, object? addedItem, IList? targetList)
        : base(routedEvent)
    {
        AddedItem = addedItem;
        TargetList = targetList;
    }

    public object? AddedItem { get; }
    public IList? TargetList { get; }
}

public delegate void AddedItemEventArgsHandler(object? sender, AddedItemEventArgs args);
