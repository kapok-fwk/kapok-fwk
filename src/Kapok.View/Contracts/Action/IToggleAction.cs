using System.ComponentModel;

namespace Kapok.View;

// there are some discussions going on in my head if we should have toggle actions or
// if we go for alternative UI alternatives
//[Obsolete("This action type will be removed in an future release")]
public interface IToggleAction : IAction, INotifyPropertyChanged
{
    bool IsChecked { get; set; }
}