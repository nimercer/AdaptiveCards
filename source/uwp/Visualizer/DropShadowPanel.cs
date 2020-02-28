// -----------------------------------------------------------------------
// <copyright file="DropShadowPanel.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

// -----------------------------------------------------------------------
// File added on 28 Feb, 2020
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// -----------------------------------------------------------------------
namespace AdaptiveCardVisualizer.Styles
{
    using System;
    using System.Numerics;
    using Windows.UI;
    using Windows.UI.Composition;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using Windows.UI.Xaml.Hosting;
    using Windows.UI.Xaml.Shapes;

    /// <summary>
    /// The <see cref="DropShadowPanel"/> control allows the creation of a dropShadow for any Xaml FrameworkElement in markup
    /// making it easier to add shadows to Xaml without having to directly drop down to Windows.UI.Composition APIs.
    /// </summary>
    [TemplatePart(Name = PartShadow, Type = typeof(Border))]
    [TemplatePart(Name = RootGrid, Type = typeof(Grid))]
    public class DropShadowPanel : ContentControl
    {
        /// <summary>
        /// Identifies the <see cref="LightDiffusion"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LightDiffusionProperty = DependencyProperty.Register(nameof(LightDiffusion), typeof(double), typeof(DropShadowPanel), new PropertyMetadata(14.0, OnLightDiffusionChanged));

        /// <summary>
        /// Identifies the <see cref="Color"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(nameof(Color), typeof(Color), typeof(DropShadowPanel), new PropertyMetadata(Colors.Black, OnColorChanged));

        /// <summary>
        /// Identifies the <see cref="ShadowOpacity"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ShadowOpacityProperty = DependencyProperty.Register(nameof(ShadowOpacity), typeof(double), typeof(DropShadowPanel), new PropertyMetadata(0.2, OnShadowOpacityChanged));

        /// <summary>
        /// Identifies the <see cref="ObjectHeight"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ObjectHeightProperty = DependencyProperty.Register(nameof(ObjectHeight), typeof(double), typeof(DropShadowPanel), new PropertyMetadata(2.0, OnShadowHeightChanged));

        /// <summary>
        /// Identifies the <see cref="LightAngleX"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LightAngleXProperty = DependencyProperty.Register(nameof(LightAngleX), typeof(double), typeof(DropShadowPanel), new PropertyMetadata(90.0, OnLightAngleXChanged));

        /// <summary>
        /// Identifies the <see cref="LightAngleY"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LightAngleYProperty = DependencyProperty.Register(nameof(LightAngleY), typeof(double), typeof(DropShadowPanel), new PropertyMetadata(90.0, OnLightAngleYChanged));

        /// <summary>
        /// Identifies the <see cref="ObjectHeightAnimationDuration"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ObjectHeightAnimationDurationProperty = DependencyProperty.Register(nameof(ObjectHeightAnimationDuration), typeof(Duration), typeof(DropShadowPanel), new PropertyMetadata(new Duration(TimeSpan.Zero)));

        /// <summary>
        /// Identifies the <see cref="ShadowMaskingEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ShadowMaskingEnabledProperty = DependencyProperty.Register(nameof(ShadowMaskingEnabled), typeof(bool), typeof(DropShadowPanel), new PropertyMetadata(false, OnShadowMaskingChanged));

        private const string PartShadow = "PART_ShadowElement";
        private const string RootGrid = "PART_RootGrid";
        private readonly SpriteVisual shadowVisual;
        private readonly DropShadow dropShadow;
        private Border border;
        private Grid rootGrid;
        private Compositor compositor;
        private Rectangle shadowMaskElement;

        /// <summary>
        /// Initializes a new instance of the <see cref="DropShadowPanel"/> class.
        /// </summary>
        public DropShadowPanel()
        {
            this.compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

            if (this.compositor != null)
            {
                this.shadowVisual = this.compositor.CreateSpriteVisual();
                this.dropShadow = this.compositor.CreateDropShadow();
                this.shadowVisual.Shadow = this.dropShadow;
            }
        }

        /// <summary>
        /// Gets or sets the diffusion of the light source.
        /// </summary>
        public double LightDiffusion
        {
            get
            {
                return (double)this.GetValue(LightDiffusionProperty);
            }

            set
            {
                this.SetValue(LightDiffusionProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the height of the object casting the shadow above the plane.
        /// </summary>
        public double ObjectHeight
        {
            get
            {
                return (double)this.GetValue(ObjectHeightProperty);
            }

            set
            {
                this.SetValue(ObjectHeightProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the duration that the object should take when moving from one height to another.
        /// </summary>
        public Duration ObjectHeightAnimationDuration
        {
            get
            {
                return (Duration)this.GetValue(ObjectHeightAnimationDurationProperty);
            }

            set
            {
                this.SetValue(ObjectHeightAnimationDurationProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the X angle (in degrees) of the light source.
        /// </summary>
        public double LightAngleX
        {
            get
            {
                return (double)this.GetValue(LightAngleXProperty);
            }

            set
            {
                this.SetValue(LightAngleXProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the Y angle (in degrees) of the light source.
        /// </summary>
        public double LightAngleY
        {
            get
            {
                return (double)this.GetValue(LightAngleYProperty);
            }

            set
            {
                this.SetValue(LightAngleYProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the color of the shadow.
        /// </summary>
        public Color Color
        {
            get
            {
                return (Color)this.GetValue(ColorProperty);
            }

            set
            {
                this.SetValue(ColorProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the opacity of the shadow.
        /// </summary>
        public double ShadowOpacity
        {
            get
            {
                return (double)this.GetValue(ShadowOpacityProperty);
            }

            set
            {
                this.SetValue(ShadowOpacityProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether shadow masking is enabled.
        /// </summary>
        public bool ShadowMaskingEnabled
        {
            get
            {
                return (bool)this.GetValue(ShadowMaskingEnabledProperty);
            }

            set
            {
                this.SetValue(ShadowMaskingEnabledProperty, value);
            }
        }

        /// <summary>
        /// Update the visual state of the control when its template is changed.
        /// </summary>
        protected override void OnApplyTemplate()
        {
            this.border = (Border)this.GetTemplateChild(PartShadow);
            this.rootGrid = (Grid)this.GetTemplateChild(RootGrid);

            if (this.border == null || this.rootGrid == null)
            {
                throw new InvalidOperationException("DropShadowPanel template is missing required elements.");
            }

            ElementCompositionPreview.SetElementChildVisual(this.border, this.shadowVisual);
            this.ConfigureShadowVisualForCastingElement();

            base.OnApplyTemplate();
        }

        /// <inheritdoc/>
        protected override void OnContentChanged(object oldContent, object newContent)
        {
            if (oldContent != null)
            {
                if (oldContent is FrameworkElement oldElement)
                {
                    oldElement.SizeChanged -= this.OnSizeChanged;
                }
            }

            if (newContent != null)
            {
                if (newContent is FrameworkElement newElement)
                {
                    newElement.SizeChanged += this.OnSizeChanged;
                }
            }

            if (this.shadowMaskElement != null)
            {
                // Remove the old shadow mask
                this.rootGrid?.Children.Remove(this.shadowMaskElement);
                this.shadowMaskElement = null;
            }

            base.OnContentChanged(oldContent, newContent);
        }

        /// <summary>
        /// Invoked when the angle of the light source is updated on the X axis.
        /// </summary>
        /// <param name="d">The source dependency object.</param>
        /// <param name="e">The event args.</param>
        private static void OnLightAngleXChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dsp = (DropShadowPanel)d;
            if (dsp.dropShadow != null)
            {
                dsp.dropShadow.Offset = dsp.CalculateShadowOffsetsForLightAngle();
            }
        }

        /// <summary>
        /// Invoked when the angle of the light source is updated on the Y axis.
        /// </summary>
        /// <param name="d">The source dependency object.</param>
        /// <param name="e">The event args.</param>
        private static void OnLightAngleYChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dsp = (DropShadowPanel)d;
            if (dsp.dropShadow != null)
            {
                dsp.dropShadow.Offset = dsp.CalculateShadowOffsetsForLightAngle();
            }
        }

        /// <summary>
        /// Invoked when OnShadowMasking property is changed.
        /// </summary>
        /// <param name="d">The source dependency object.</param>
        /// <param name="e">The event args.</param>
        private static void OnShadowMaskingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dsp = (DropShadowPanel)d;
            dsp.UpdateShadowMask();
        }

        /// <summary>
        /// Invoked when the light diffusion is changed.
        /// </summary>
        /// <param name="d">The source dependency object.</param>
        /// <param name="e">The event args.</param>
        private static void OnLightDiffusionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var panel = (DropShadowPanel)d;
            if (panel.dropShadow != null)
            {
                panel.dropShadow.BlurRadius = (float)panel.LightDiffusion;
            }
        }

        /// <summary>
        /// Invoked when the colour of the shadow is changed.
        /// </summary>
        /// <param name="d">The source dependency object.</param>
        /// <param name="e">The event args.</param>
        private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var panel = (DropShadowPanel)d;
            if (panel.dropShadow != null)
            {
                panel.dropShadow.Color = panel.Color;
            }
        }

        /// <summary>
        /// Invoked when the opacity of the shadow is changed.
        /// </summary>
        /// <param name="d">The source dependency object.</param>
        /// <param name="e">The event args.</param>
        private static void OnShadowOpacityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var panel = (DropShadowPanel)d;
            if (panel.dropShadow != null)
            {
                panel.dropShadow.Opacity = (float)panel.ShadowOpacity;
            }
        }

        /// <summary>
        /// Invoked when the height of the object casting a shadow is changed.
        /// </summary>
        /// <param name="d">The source dependency object.</param>
        /// <param name="e">The event args.</param>
        private static void OnShadowHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dsp = (DropShadowPanel)d;
            if (dsp.dropShadow == null)
            {
                // Unable to create composition shadow, skip.
                return;
            }

            if (dsp.ObjectHeightAnimationDuration.TimeSpan.TotalSeconds == 0)
            {
                // If no animation is required, update the blur radius and offset
                dsp.dropShadow.BlurRadius = (float)(dsp.LightDiffusion * (double)e.NewValue);
                dsp.dropShadow.Offset = dsp.CalculateShadowOffsetsForLightAngle();
            }
            else
            {
                // If an animation to the new value is required, calculate the final values...
                var blurRadius = (float)(dsp.LightDiffusion * (double)e.NewValue);
                var newOffset = dsp.CalculateShadowOffsetsForLightAngle();

                // Then create composition animations to change the radius and offsets to their new values
                var blurAnimation = dsp.compositor.CreateScalarKeyFrameAnimation();
                blurAnimation.InsertKeyFrame(1.0f, (float)blurRadius);
                blurAnimation.Duration = dsp.ObjectHeightAnimationDuration.TimeSpan;
                dsp.dropShadow.StartAnimation("BlurRadius", blurAnimation);

                var offsetXAnimation = dsp.compositor.CreateScalarKeyFrameAnimation();
                offsetXAnimation.InsertKeyFrame(1.0f, newOffset.X);
                offsetXAnimation.Duration = dsp.ObjectHeightAnimationDuration.TimeSpan;
                dsp.dropShadow.StartAnimation("Offset.X", offsetXAnimation);

                var offsetYAnimation = dsp.compositor.CreateScalarKeyFrameAnimation();
                offsetYAnimation.InsertKeyFrame(1.0f, newOffset.Y);
                offsetYAnimation.Duration = dsp.ObjectHeightAnimationDuration.TimeSpan;
                dsp.dropShadow.StartAnimation("Offset.Y", offsetYAnimation);
            }
        }

        /// <summary>
        /// Invoked when the shadow size is changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.UpdateShadowSize();
        }

        /// <summary>
        /// Configures the shadow.
        /// </summary>
        private void ConfigureShadowVisualForCastingElement()
        {
            this.UpdateShadowMask();
            this.UpdateShadowSize();
        }

        /// <summary>
        /// Calculates the offset of the shadow, based on the elevation of the light source above the horizon and the height
        /// of the object casting the shadow above the plane.
        /// </summary>
        /// <returns>
        /// A vector of offsets.
        /// </returns>
        private Vector3 CalculateShadowOffsetsForLightAngle()
        {
            // Convert the angles from deg to grad
            var angleX = (90.0 - this.LightAngleX) * (Math.PI / 180);
            var angleY = (90.0 - this.LightAngleY) * (Math.PI / 180);

            // Calculate the shadow length
            var lengthX = this.ObjectHeight * Math.Tan(angleX);
            var lengthY = this.ObjectHeight * Math.Tan(angleY);

            return new Vector3(
                (float)lengthX * (float)this.LightDiffusion,
                (float)lengthY * (float)this.LightDiffusion,
                0); // Nobody cares about the Z offset.
        }

        /// <summary>
        /// Updates the shadow mask for the current content.
        /// </summary>
        private void UpdateShadowMask()
        {
            if (this.dropShadow != null)
            {
                var dropShadowUsed = false;
                if (this.Content != null)
                {
                    CompositionBrush mask = null;

                    if (this.Content is Image)
                    {
                        mask = ((Image)this.Content).GetAlphaMask();
                    }
                    else if (this.Content is Shape)
                    {
                        mask = ((Shape)this.Content).GetAlphaMask();
                    }
                    else if (this.Content is TextBlock)
                    {
                        mask = ((TextBlock)this.Content).GetAlphaMask();
                    }
                    else if (
                        this.ShadowMaskingEnabled
                        && this.rootGrid != null
                        && this.Content is Grid grid
                        && (grid.CornerRadius.TopLeft > 0 || grid.CornerRadius.BottomRight > 0))
                    {
                        if (grid.CornerRadius.TopLeft != grid.CornerRadius.BottomRight
                            || grid.CornerRadius.TopLeft != grid.CornerRadius.BottomLeft
                            || grid.CornerRadius.TopLeft != grid.CornerRadius.TopRight)
                        {
                            throw new Exception("The drop shadow panel does not support masking of asymmetrical corner radii");
                        }

                        if (this.shadowMaskElement != null)
                        {
                            // Remove any existing shadow masks
                            this.rootGrid.Children.Remove(this.shadowMaskElement);
                            this.shadowMaskElement = null;
                        }

                        // Grids do not support opacity masks, so if the content is a grid and it is non-rectangular
                        // generate a shape with the same properties, and use it as the shadow mask.
                        this.shadowMaskElement = new Rectangle
                        {
                            Width = grid.Width,
                            Height = grid.Height,
                            RadiusX = grid.CornerRadius.TopLeft,
                            RadiusY = grid.CornerRadius.TopLeft,
                            Fill = grid.Background,
                            Opacity = 1,
                        };

                        // We need to wait for XAML to load the element
                        dropShadowUsed = true;
                        this.shadowMaskElement.Loaded += this.ShadowMaskElement_Loaded;

                        // Add the shadow mask to the tree to force a render (this will be behind a grid with identical dimensions, so will be invisible)
                        this.rootGrid.Children.Insert(0, this.shadowMaskElement);
                    }

                    this.dropShadow.Mask = mask;
                }
                else
                {
                    this.dropShadow.Mask = null;
                }

                if (!dropShadowUsed && this.shadowMaskElement != null)
                {
                    // Remove any shadow masks that aren't being used anymore.
                    this.rootGrid.Children.Remove(this.shadowMaskElement);
                    this.shadowMaskElement = null;
                }
            }
        }

        /// <summary>
        /// Invoked when the shadow mask element is loaded into the visual tree.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void ShadowMaskElement_Loaded(object sender, RoutedEventArgs e)
        {
            // We're done with this element now, we can remove it
            if (this.shadowMaskElement != null)
            {
                if (this.dropShadow != null)
                {
                    // Steal its opacity mask and use it for the shadow.
                    this.dropShadow.Mask = this.shadowMaskElement.GetAlphaMask();
                }

                // Remove the old shadow mask
                this.shadowMaskElement.Loaded -= this.ShadowMaskElement_Loaded;
            }
        }

        /// <summary>
        /// Updates the size of the shadow.
        /// </summary>
        private void UpdateShadowSize()
        {
            if (this.shadowVisual != null)
            {
                var newSize = new Vector2(0, 0);

                if (this.Content is FrameworkElement contentFE)
                {
                    newSize = new Vector2((float)contentFE.ActualWidth, (float)contentFE.ActualHeight);
                }

                this.shadowVisual.Size = newSize;
            }
        }
    }
}
