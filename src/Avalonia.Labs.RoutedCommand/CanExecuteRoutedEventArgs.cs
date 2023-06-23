﻿using System;
using Avalonia.Interactivity;
using System.Windows.Input;

namespace Avalonia.Labs.Input;

public sealed class CanExecuteRoutedEventArgs : RoutedEventArgs
{
    public ICommand Command { get; }

    public object? Parameter { get; }

    public bool CanExecute { get; set; }

    internal CanExecuteRoutedEventArgs(ICommand command, object? parameter)
    {
        Command = command ?? throw new ArgumentNullException(nameof(command));
        Parameter = parameter;
        RoutedEvent = RoutedCommandsManager.CanExecuteEvent;
    }
}
