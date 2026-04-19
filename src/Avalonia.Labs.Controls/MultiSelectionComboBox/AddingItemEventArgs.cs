using System;
using System.Collections;
using System.Globalization;
using Avalonia.Interactivity;

namespace Avalonia.Labs.Controls;

public class AddingItemEventArgs : RoutedEventArgs
{
    public AddingItemEventArgs(RoutedEvent routedEvent, string? input, object? parsedObject, bool accepted,
        IList? targetList, Type? targetType, string? stringFormat, CultureInfo? culture, IParseStringToObject? parser)
        : base(routedEvent)
    {
        Input = input;
        ParsedObject = parsedObject;
        Accepted = accepted;
        TargetList = targetList;
        TargetType = targetType;
        StringFormat = stringFormat;
        Culture = culture;
        Parser = parser;
    }

    public string? Input { get; }
    public object? ParsedObject { get; set; }
    public string? StringFormat { get; }
    public CultureInfo? Culture { get; }
    public IParseStringToObject? Parser { get; }
    public Type? TargetType { get; }
    public IList? TargetList { get; }
    public bool Accepted { get; set; }
}

public delegate void AddingItemEventArgsHandler(object? sender, AddingItemEventArgs args);
