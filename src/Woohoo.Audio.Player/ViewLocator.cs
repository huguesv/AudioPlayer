// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Player;

using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Woohoo.Audio.Player.ViewModels;

public class ViewLocator : IDataTemplate
{
    private static readonly Dictionary<Type, Func<Control>> Registration = [];

    public static void Register<TViewModel, TView>()
        where TView : Control, new()
    {
        Registration.Add(typeof(TViewModel), () => new TView());
    }

    public static void Register<TViewModel, TView>(Func<TView> factory)
        where TView : Control
    {
        Registration.Add(typeof(TViewModel), factory);
    }

    public Control? Build(object? data)
    {
        if (data is null)
        {
            return null;
        }

        var type = data.GetType();

        if (Registration.TryGetValue(type, out var factory))
        {
            return factory();
        }
        else
        {
            return new TextBlock { Text = "Not Found: " + type };
        }
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
