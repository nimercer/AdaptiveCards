// -----------------------------------------------------------------------
// <copyright file="SimpleCommand.cs" company="Microsoft">
// Copyright (c) Microsoft Corporation. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace AdaptiveCardVisualizer
{
    using System;
    using System.Windows.Input;

    /// <summary>
    /// Simple command handler which invokes an Action when it is executed.
    /// </summary>
    public sealed class SimpleCommand : ICommand
    {
        private readonly Action commandAction;
        private readonly Func<bool> canExecute;

        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleCommand"/> class.
        /// Simple command that accepts a single parameter-less Action.
        /// </summary>
        /// <param name="commandAction">Action to execute when we're clicked on.</param>
        /// <param name="canExecute">Whether this command can execute or not function. If null, CanExecute will return true every time.</param>
        public SimpleCommand(Action commandAction, Func<bool> canExecute = null)
        {
            this.commandAction = commandAction ?? throw new ArgumentNullException(nameof(commandAction));
            this.canExecute = canExecute;
        }

        /// <inheritdoc />
        public bool CanExecute(object parameter) => this.CanExecute();

        /// <inheritdoc />
        public void Execute(object parameter) => this.Execute();

        /// <summary>
        /// Determines whether this instance can execute.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this instance can execute; otherwise, <c>false</c>.
        /// </returns>
        public bool CanExecute() => this.canExecute?.Invoke() ?? true;

        /// <summary>
        /// Executes this instance.
        /// </summary>
        public void Execute() => this.commandAction();
    }
}
