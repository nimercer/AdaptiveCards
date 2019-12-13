// -----------------------------------------------------------------------
// <copyright file="PointerAndFocusStateManagerBehavior.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AdaptiveCardVisualizer.Behaviors
{
    using Microsoft.Xaml.Interactivity;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;

    /// <summary>
    /// Behavior that supports state changes between focus, pointer over, and the combination of focus and pointer over.
    /// </summary>
    public class PointerAndFocusStateManagerBehavior : Behavior<Control>
    {
        /// <summary>
        /// DependencyProperty for the state to enter when the control loses focus and pointer over.
        /// </summary>
        public static readonly DependencyProperty NormalStateNameProperty =
            DependencyProperty.Register(
                "NormalStateName",
                typeof(string),
                typeof(PointerAndFocusStateManagerBehavior),
                new PropertyMetadata(null));

        /// <summary>
        /// DependencyProperty for the state to enter when the control gets focus.
        /// </summary>
        public static readonly DependencyProperty FocusStateNameProperty =
            DependencyProperty.Register(
                "FocusStateName",
                typeof(string),
                typeof(PointerAndFocusStateManagerBehavior),
                new PropertyMetadata(null));

        /// <summary>
        /// DependencyProperty for the state to enter when the control is pointed over.
        /// </summary>
        public static readonly DependencyProperty PointerOverStateNameProperty =
            DependencyProperty.Register(
                "PointerOverStateName",
                typeof(string),
                typeof(PointerAndFocusStateManagerBehavior),
                new PropertyMetadata(null));

        /// <summary>
        /// DependencyProperty for the state to enter when the control gets focus and pointer over.
        /// </summary>
        public static readonly DependencyProperty FocusedPointerOverStateNameProperty =
            DependencyProperty.Register(
                "FocusedPointerOverStateName",
                typeof(string),
                typeof(PointerAndFocusStateManagerBehavior),
                new PropertyMetadata(null));

        /// <summary>
        /// DependencyProperty for the desired FocusState type.
        /// </summary>
        public static readonly DependencyProperty DesiredFocusStateProperty =
            DependencyProperty.Register(
                "DesiredFocusState",
                typeof(FocusState),
                typeof(PointerAndFocusStateManagerBehavior),
                new PropertyMetadata(FocusState.Keyboard));

        private bool isFocused = false;
        private bool isPointerOver = false;

        /// <summary>
        /// Gets or sets the Normal state name.
        /// </summary>
        public string NormalFocusStateName
        {
            get { return (string)this.GetValue(NormalStateNameProperty); }
            set { this.SetValue(NormalStateNameProperty, value); }
        }

        /// <summary>
        /// Gets or sets the Focus state name.
        /// </summary>
        public string FocusStateName
        {
            get { return (string)this.GetValue(FocusStateNameProperty); }
            set { this.SetValue(FocusStateNameProperty, value); }
        }

        /// <summary>
        /// Gets or sets the Pointer Over state name.
        /// </summary>
        public string PointerOverStateName
        {
            get { return (string)this.GetValue(PointerOverStateNameProperty); }
            set { this.SetValue(PointerOverStateNameProperty, value); }
        }

        /// <summary>
        /// Gets or sets the Focused and Pointer Over state name.
        /// </summary>
        public string FocusedPointerOverStateName
        {
            get { return (string)this.GetValue(FocusedPointerOverStateNameProperty); }
            set { this.SetValue(FocusedPointerOverStateNameProperty, value); }
        }

        /// <summary>
        /// Gets or sets the desired FocusState type.
        /// </summary>
        public FocusState DesiredFocusState
        {
            get { return (FocusState)this.GetValue(DesiredFocusStateProperty); }
            set { this.SetValue(DesiredFocusStateProperty, value); }
        }

        /// <summary>
        /// Called when the behaviour is attached to an element.
        /// </summary>
        protected override void OnAttached()
        {
            if (this.AssociatedObject != null)
            {
                this.AssociatedObject.GotFocus += this.GotFocus;
                this.AssociatedObject.LostFocus += this.LostFocus;
                this.AssociatedObject.PointerEntered += this.PointerEntered;
                this.AssociatedObject.PointerExited += this.PointerExited;
            }
        }

        /// <summary>
        /// Called when the behaviour is detached to an element.
        /// </summary>
        protected override void OnDetaching()
        {
            if (this.AssociatedObject != null)
            {
                this.AssociatedObject.GotFocus -= this.GotFocus;
                this.AssociatedObject.LostFocus -= this.LostFocus;
                this.AssociatedObject.PointerEntered -= this.PointerEntered;
                this.AssociatedObject.PointerExited -= this.PointerExited;
            }
        }

        /// <summary>
        /// Called when the associated object gets focus.
        /// Sets the object's visual state to the Focused state or FocusedPointerOver state if the mouse is also pointed over.
        /// </summary>
        /// <param name="sender">The associated object.</param>
        /// <param name="e">The event args.</param>
        private void GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is Control senderControl)
            {
                if (senderControl.FocusState == this.DesiredFocusState)
                {
                    this.isFocused = true;

                    if (this.isPointerOver)
                    {
                        VisualStateManager.GoToState(senderControl, this.FocusedPointerOverStateName, false);
                    }
                    else
                    {
                        VisualStateManager.GoToState(senderControl, this.FocusStateName, false);
                    }
                }
            }
        }

        /// <summary>
        /// Called when the associated object loses focus.
        /// Sets the object's visual state to the Normal state or the PointerOver state if the mouse is pointed over.
        /// </summary>
        /// <param name="sender">The associated object.</param>
        /// <param name="e">The event args.</param>
        private void LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is Control senderControl)
            {
                this.isFocused = false;

                if (this.isPointerOver)
                {
                    VisualStateManager.GoToState(senderControl, this.PointerOverStateName, false);
                }
                else
                {
                    VisualStateManager.GoToState(senderControl, this.NormalFocusStateName, false);
                }
            }
        }

        /// <summary>
        /// Called when the associated object is pointed over.
        /// Sets the object's visual state to the PointerOver state or FocusedPointerOver state if the associated object is also focused.
        /// </summary>
        /// <param name="sender">The associated object.</param>
        /// <param name="e">The event args.</param>
        private void PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (sender is Control senderControl)
            {
                this.isPointerOver = true;

                if (this.isFocused)
                {
                    VisualStateManager.GoToState(senderControl, this.FocusedPointerOverStateName, false);
                }
                else
                {
                    VisualStateManager.GoToState(senderControl, this.PointerOverStateName, false);
                }
            }
        }

        /// <summary>
        /// Called when the associated object loses pointer over.
        /// Sets the object's visual state to the Normal state or Focused state if the associated object is focused.
        /// </summary>
        /// <param name="sender">The associated object.</param>
        /// <param name="e">The event args.</param>
        private void PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (sender is Control senderControl)
            {
                this.isPointerOver = false;

                if (this.isFocused)
                {
                    VisualStateManager.GoToState(senderControl, this.FocusStateName, false);
                }
                else
                {
                    VisualStateManager.GoToState(senderControl, this.NormalFocusStateName, false);
                }
            }
        }
    }
}
