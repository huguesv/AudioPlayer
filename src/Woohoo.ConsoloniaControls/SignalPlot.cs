// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ConsoloniaControls;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Metadata;

public class SignalPlot : Control
{
    public static readonly StyledProperty<double[]?> SourceProperty =
        AvaloniaProperty.Register<SignalPlot, double[]?>(nameof(Source));

    public static readonly StyledProperty<double> ScaleProperty =
        AvaloniaProperty.Register<SignalPlot, double>(nameof(Scale), 1);

    public static readonly StyledProperty<long> TickProperty =
        AvaloniaProperty.Register<SignalPlot, long>(nameof(Tick), 1);

    public static readonly StyledProperty<char> DotProperty =
        AvaloniaProperty.Register<SignalPlot, char>(nameof(Dot), '*');

    public static readonly StyledProperty<IBrush?> ForegroundProperty =
        TextBlock.ForegroundProperty.AddOwner<SignalPlot>();

    static SignalPlot()
    {
        AffectsRender<SignalPlot>(SourceProperty, ScaleProperty, TickProperty, DotProperty, ForegroundProperty);
        AffectsMeasure<SignalPlot>(SourceProperty);
    }

    public double Scale
    {
        get => this.GetValue(ScaleProperty);
        set => this.SetValue(ScaleProperty, value);
    }

    public long Tick
    {
        get => this.GetValue(TickProperty);
        set => this.SetValue(TickProperty, value);
    }

    public char Dot
    {
        get => this.GetValue(DotProperty);
        set => this.SetValue(DotProperty, value);
    }

    /// <summary>
    /// Gets or sets a brush used to paint the plot.
    /// </summary>
    public IBrush? Foreground
    {
        get => this.GetValue(ForegroundProperty);
        set => this.SetValue(ForegroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the data that will be displayed.
    /// </summary>
    [Content]
    public double[]? Source
    {
        get => this.GetValue(SourceProperty);
        set => this.SetValue(SourceProperty, value);
    }

    public void Refresh()
    {
        this.InvalidateVisual();
    }

    public sealed override void Render(DrawingContext context)
    {
        if (this.Source is not null && this.Bounds.Width > 0 && this.Bounds.Height > 0)
        {
            var text = new FormattedText(this.Dot.ToString(), CultureInfo.InvariantCulture, FlowDirection.LeftToRight, Typeface.Default, 1, this.Foreground);

            for (double i = 0; i < this.Bounds.Width; i++)
            {
                double xPercent = i / this.Bounds.Width;
                double originalVal = this.Source[(int)Math.Floor(xPercent * this.Source.Length)];
                double scaledVal = originalVal * this.Scale;
                double clampedVal = Math.Clamp(scaledVal, -1.0, 1.0);

                int height = (int)Math.Floor(((clampedVal + 1) / 2.0) * this.Bounds.Height);
                context.DrawText(text, new Point(this.Bounds.Left + i - 1, this.Bounds.Bottom - height - 2));
            }
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return availableSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        return finalSize;
    }
}
