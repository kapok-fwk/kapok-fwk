﻿using System.Diagnostics;

namespace Kapok.View;

public class QuestionDialogPage : DialogPage
{
    private DialogButton? _dialogResultButton;
    private IList<DialogButton> _dialogButton = new List<DialogButton>();
        
    public QuestionDialogPage(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        DialogButtonAction = new UIAction<DialogButton>("DialogButton", DialogButtonExecute, CanDialogButtonExecute);
    }

    // TODO: implement that the icon of the window can be defined based on if that is an question or warning, or error

    #region ViewModel properties

    // ReSharper disable MemberCanBeProtected.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    // ReSharper disable UnusedMember.Global
    public string? Message { get; set; }

    public IList<DialogButton> DialogButtons
    {
        get => _dialogButton;
        protected set => _dialogButton = value;
    }

    public IAction<DialogButton> DialogButtonAction { get; }

    public bool HasEnabledCancelButton => DialogButtons.Any(db => db.IsCancel && db.IsEnabled);
    // ReSharper restore UnusedMember.Global
    // ReSharper restore UnusedAutoPropertyAccessor.Global
    // ReSharper restore MemberCanBeProtected.Global

    #endregion

    public DialogButton? DialogResultButton
    {
        get => _dialogResultButton;
        private set
        {
            _dialogResultButton = value;
            DialogResult = _dialogResultButton?.FallbackDialogResult;
        }
    }

    public class DialogButton : ICloneable
    {
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        public Caption? Label { get; set; }
        public Caption? Description { get; set; }

        public string? Image { get; set; }
        // ReSharper restore UnusedAutoPropertyAccessor.Global

        public bool IsDefault { get; set; }
        public bool IsCancel { get; set; }
        public bool IsEnabled { get; set; } = true;

        public bool? FallbackDialogResult
        {
            get
            {
                if (IsDefault)
                    return true;
                if (IsCancel)
                    return true;
                return null;
            }
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }

    private void DialogButtonExecute(DialogButton? dialogButton)
    {
        Debug.Assert(dialogButton != null);
        if (!dialogButton.IsEnabled)
            return;

        DialogResultButton = dialogButton;
        DialogResult = dialogButton.FallbackDialogResult;
        Close();
    }

    private bool CanDialogButtonExecute(DialogButton? dialogButton)
    {
        Debug.Assert(dialogButton != null);
        return dialogButton.IsEnabled;
    }

    protected override void OnClosed()
    {
        if (DialogResultButton == null)
            DialogResultButton = DialogButtons.First(db => db.IsCancel && db.IsEnabled);
        base.OnClosed();
    }

    protected override void Action()
    {
        DialogResultButton = DialogButtons.FirstOrDefault(db => db.IsDefault && db.IsEnabled);
        base.Action();
    }

    protected override bool CanAction()
    {
        if (!DialogButtons.Any(db => db.IsDefault && db.IsEnabled))
            return false;

        return base.CanAction();
    }

    protected override void Cancel()
    {
        DialogResultButton = DialogButtons.First(db => db.IsCancel && db.IsEnabled);
        base.Cancel();
    }
}