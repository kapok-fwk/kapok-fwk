using System.Collections;

namespace Kapok.View;

public interface IDataSetSelectionAction : IAction<IList?>
{
}

public interface IDataSetSelectionAction<TEntry> : IAction<IList<TEntry?>?>
{
}