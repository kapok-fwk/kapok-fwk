using Res = Kapok.View.Resources.Dialog.UnsavedChangesDialogPage;

namespace Kapok.View;

public class UnsavedChangesDialogPage : QuestionDialogPage
{
    public static UnsavedChangesDialogResult ShowDialog(IViewDomain? viewDomain = null, bool allowCancel = true)
    {
        var questionWindow = new UnsavedChangesDialogPage(viewDomain, allowCancel);
        ((QuestionDialogPage)questionWindow).ShowDialog();

        int index = questionWindow.DialogButtons.IndexOf(questionWindow.DialogResultButton);
        return (UnsavedChangesDialogResult) index;
    }

    public static UnsavedChangesDialogResult ShowDialog(IPage owner, bool allowCancel = true)
    {
        var questionWindow = new UnsavedChangesDialogPage(owner.ViewDomain, allowCancel);
        ((QuestionDialogPage)questionWindow).ShowDialog(owner);

        if (questionWindow.DialogResultButton == null)
        {
            // TODO implement here a woraround so that it does not break the execution
            throw new NotSupportedException("Unknown dialog button was pressed");
        }

        int index = questionWindow.DialogButtons.IndexOf(questionWindow.DialogResultButton);
        if (index == -1)
        {
            // TODO implement here a woraround so that it does not break the execution
            throw new NotSupportedException("Unknown dialog button was pressed");
        }
        return (UnsavedChangesDialogResult) index;
    }

    private static List<DialogButton> _staticDialogButtons;

    private static List<DialogButton> StaticDialogButtons =>
        _staticDialogButtons ??= new List<DialogButton>
        {
            // ReSharper disable PossiblyMissingIndexerInitializerComma
            new DialogButton
            {
                Image = "save",
                Label = Res.Save_Label,
                Description = Res.Save_Description,
                IsDefault = true
            },
            new DialogButton
            {
                Image = "trash",
                Label = Res.Discard_Label,
                Description = Res.Discard_Description
            },
            new DialogButton
            {
                Image = "command-undo",
                Label = Res.Cancel_Label,
                Description = Res.Cancel_Description,
                IsCancel = true
            }
            // ReSharper restore PossiblyMissingIndexerInitializerComma
        };

    private UnsavedChangesDialogPage(IViewDomain? viewDomain, bool allowCancel = true)
        : base(viewDomain ?? View.ViewDomain.Default)
    {
        Title = Res.Title;
        Message = Res.Message;

        DialogButtons = StaticDialogButtons.Clone().ToList();

        if (!allowCancel)
        {
            var cancelButton = DialogButtons[(int) UnsavedChangesDialogResult.Cancel];
            cancelButton.Image = "command-undo-gray";
            cancelButton.IsEnabled = false;
        }
    }

    public enum UnsavedChangesDialogResult
    {
        /// <summary>
        /// The user choose to save the unsaved changes
        /// </summary>
        Save,

        /// <summary>
        /// The user choose to discard the unsaved changes
        /// </summary>
        Discard,

        /// <summary>
        /// The user choose to about the current process
        /// </summary>
        Cancel
    }
}