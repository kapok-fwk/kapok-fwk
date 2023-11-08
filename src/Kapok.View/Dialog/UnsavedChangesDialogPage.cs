using Res = Kapok.View.Resources.Dialog.UnsavedChangesDialogPage;

namespace Kapok.View;

public class UnsavedChangesDialogPage : QuestionDialogPage
{
    public static UnsavedChangesDialogResult ShowDialog(IServiceProvider serviceProvider, bool allowCancel = true)
    {
        var questionWindow = new UnsavedChangesDialogPage(serviceProvider, allowCancel);
        ((QuestionDialogPage)questionWindow).ShowDialog();

        if (questionWindow.DialogResultButton == null)
            return UnsavedChangesDialogResult.Cancel;

        int index = questionWindow.DialogButtons.IndexOf(questionWindow.DialogResultButton);
        return (UnsavedChangesDialogResult) index;
    }

    public static UnsavedChangesDialogResult ShowDialog(IPage owner, bool allowCancel = true)
    {
        var questionWindow = new UnsavedChangesDialogPage(owner.ServiceProvider, allowCancel);
        ((QuestionDialogPage)questionWindow).ShowDialog(owner);

        if (questionWindow.DialogResultButton == null)
        {
            // TODO implement here a workaround so that it does not break the execution
            throw new NotSupportedException("Unknown dialog button was pressed");
        }

        int index = questionWindow.DialogButtons.IndexOf(questionWindow.DialogResultButton);
        if (index == -1)
        {
            // TODO implement here a workaround so that it does not break the execution
            throw new NotSupportedException("Unknown dialog button was pressed");
        }
        return (UnsavedChangesDialogResult) index;
    }

    private static List<DialogButton>? _staticDialogButtons;

    private static List<DialogButton> StaticDialogButtons =>
        _staticDialogButtons ??= new List<DialogButton>
        {
            // ReSharper disable PossiblyMissingIndexerInitializerComma
            new()
            {
                Image = "save",
                Label = Res.Save_Label,
                Description = Res.Save_Description,
                IsDefault = true
            },
            new()
            {
                Image = "trash",
                Label = Res.Discard_Label,
                Description = Res.Discard_Description
            },
            new()
            {
                Image = "command-undo",
                Label = Res.Cancel_Label,
                Description = Res.Cancel_Description,
                IsCancel = true
            }
            // ReSharper restore PossiblyMissingIndexerInitializerComma
        };

    private UnsavedChangesDialogPage(IServiceProvider serviceProvider, bool allowCancel = true)
        : base(serviceProvider)
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