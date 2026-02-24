// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.ConsoloniaControls;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Metadata;

public class BarPlot : Control
{
    public static readonly StyledProperty<double[]?> SourceProperty =
        AvaloniaProperty.Register<BarPlot, double[]?>(nameof(Source));

    public static readonly StyledProperty<double> ScaleProperty =
        AvaloniaProperty.Register<BarPlot, double>(nameof(Scale), 1);

    public static readonly StyledProperty<long> TickProperty =
        AvaloniaProperty.Register<BarPlot, long>(nameof(Tick), 1);

    public static readonly StyledProperty<IBrush?> ForegroundProperty =
        TextBlock.ForegroundProperty.AddOwner<BarPlot>();

    static BarPlot()
    {
        AffectsRender<BarPlot>(SourceProperty, ScaleProperty, TickProperty, ForegroundProperty);
        AffectsMeasure<BarPlot>(SourceProperty);
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
        if (this.Foreground is not null && this.Source is not null && this.Bounds.Width > 0 && this.Bounds.Height > 0)
        {
            double centerY = this.Bounds.Top + (this.Bounds.Height / 2) - 1;

            for (double i = 0; i < this.Bounds.Width; i++)
            {
                double xPercent = i / this.Bounds.Width;
                double originalVal = this.Source[(int)Math.Floor(xPercent * this.Source.Length)];
                double scaledVal = originalVal * this.Scale;
                double clampedVal = Math.Clamp(scaledVal, -1.0, 1.0);
                int height = (int)Math.Floor(clampedVal * this.Bounds.Height / 2);

                var rect = height >= 0
                    ? new Rect(new Point(this.Bounds.Left + i - 1, centerY - height), new Size(1, height))
                    : new Rect(new Point(this.Bounds.Left + i - 1, centerY), new Size(1, 0 - height));
                context.FillRectangle(this.Foreground, rect);
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
