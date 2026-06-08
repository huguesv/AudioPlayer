// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Player.Tui.Converters;

using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

public class PositionToLabelConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (targetType.IsAssignableTo(typeof(string)))
        {
            if (value is int position)
            {
                long length = position / 176400;

                var minutes = length / 60;
                var seconds = length % 60;
                return $"{minutes:D2}:{seconds:D2}";
            }
            else if (value is long position2)
            {
                long length = position2 / 176400;

                var minutes = length / 60;
                var seconds = length % 60;
                return $"{minutes:D2}:{seconds:D2}";
            }
        }

        return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
