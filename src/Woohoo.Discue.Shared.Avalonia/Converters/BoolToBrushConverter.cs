// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Shared.Avalonia.Converters;

using global::Avalonia.Data.Converters;
using global::Avalonia.Media;

public sealed class BoolToBrushConverter : IValueConverter
{
    public IBrush? TrueBrush { get; set; }

    public IBrush? FalseBrush { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? this.TrueBrush : this.FalseBrush;
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
