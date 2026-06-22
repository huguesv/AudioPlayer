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

    public static readonly StyledProperty<double> RangeMinProperty =
        AvaloniaProperty.Register<BarPlot, double>(nameof(RangeMin), 0);

    public static readonly StyledProperty<double> RangeMaxProperty =
        AvaloniaProperty.Register<BarPlot, double>(nameof(RangeMax), 1);

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

    public double RangeMin
    {
        get => this.GetValue(RangeMinProperty);
        set => this.SetValue(RangeMinProperty, value);
    }

    public double RangeMax
    {
        get => this.GetValue(RangeMaxProperty);
        set => this.SetValue(RangeMaxProperty, value);
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
        if (this.Foreground is not null && this.Source is not null && this.Bounds.Width > 0 && this.Bounds.Height > 0 && this.Source.Length > 0)
        {
            int barSpacing = 5;
            int barWidth = (int)Math.Floor((this.Bounds.Width / this.Source.Length) - barSpacing);
            int barAndSpacesWidth = (this.Source.Length * barWidth) + ((this.Source.Length - 1) * barSpacing);
            int leftMargin = (int)Math.Floor((this.Bounds.Width - barAndSpacesWidth) / 2);
            int bottom = (int)Math.Floor(this.Bounds.Bottom - 2);
            var yScale = (this.Bounds.Height - 2) / (this.RangeMax - this.RangeMin);
            var left = this.Bounds.Left + leftMargin;

            for (int col = 0; col < this.Source.Length; col++)
            {
                double scaledVal = this.Source[col] * yScale;
                int clampedVal = (int)Math.Floor(Math.Clamp(scaledVal, 0, this.Bounds.Height));
                var top = bottom - clampedVal;

                var rect = new Rect(new Point(left, top), new Size(barWidth, clampedVal));
                context.FillRectangle(this.Foreground, rect);

                left += barWidth + barSpacing;
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
