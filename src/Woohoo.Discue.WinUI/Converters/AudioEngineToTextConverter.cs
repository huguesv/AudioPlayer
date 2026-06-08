// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Converters;

using System;
using Microsoft.UI.Xaml.Data;
using Woohoo.Discue.ViewModels;

internal sealed partial class AudioEngineToTextConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is AudioEngineType theme)
        {
            switch (theme)
            {
                case AudioEngineType.WindowsMediaPlayer:
                    return "Windows Media Player";
                case AudioEngineType.Sdl3:
                    return "SDL3";
                default:
                    break;
            }
        }

        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
