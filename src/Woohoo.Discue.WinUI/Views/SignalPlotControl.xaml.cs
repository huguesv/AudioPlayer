// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Views;

using System;
using System.Numerics;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

public sealed partial class SignalPlotControl : UserControl
{
    public static readonly DependencyProperty SignalDataProperty =
        DependencyProperty.Register(
            nameof(SignalData),
            typeof(double[]),
            typeof(SignalPlotControl),
            new PropertyMetadata(new double[0]));

    private readonly SolidColorBrush? visualizationBrush;
    private readonly Color visualizationColor;

    public SignalPlotControl()
    {
        this.InitializeComponent();

        this.visualizationBrush = Application.Current.Resources["AccentFillColorDefaultBrush"] as SolidColorBrush;
        this.visualizationColor = this.visualizationBrush?.Color ?? Colors.Pink;
    }

    public double[] SignalData
    {
        get { return (double[])this.GetValue(SignalDataProperty); }
        set { this.SetValue(SignalDataProperty, value); }
    }

    public void Invalidate()
    {
        this.canvas?.Invalidate();
    }

    private void Canvas_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
    {
        var data = this.SignalData;
        if (data.Length == 0)
        {
            return;
        }

        float width = (float)sender.ActualWidth;

        float height = (float)sender.ActualHeight;
        float heightFactor = 1f;
        int count = Math.Min((int)sender.ActualWidth, data.Length);
        Vector2 pt1 = new Vector2(0, ((float)data[0] * height * heightFactor) + (height / 2));
        for (int i = 1; i < count; i++)
        {
            Vector2 pt2 = new Vector2(i * width / (float)count, ((float)data[i] * height * heightFactor) + (height / 2));
            args.DrawingSession.DrawLine(pt1, pt2, this.visualizationColor);
            pt1 = pt2;
        }
    }

    private void UserControl_Unloaded(object sender, RoutedEventArgs e)
    {
        this.canvas.RemoveFromVisualTree();
        this.canvas = null;
    }
}
