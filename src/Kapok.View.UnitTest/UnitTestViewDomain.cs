﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Kapok.Core;
using Kapok.Entity;

namespace Kapok.View.UnitTest;

public class UnitTestViewDomain : ViewDomain
{
    public override Type GetPageControlType(Type pageType)
    {
        throw new NotImplementedException();
    }

    public override IQueryableView<TEntity> CreateQueryableView<TEntity>(IQueryable<TEntity> queryable)
    {
        throw new NotImplementedException();
    }

    public override IPropertyLookupView CreatePropertyLookupView(ILookupDefinition lookupDefinition, IDataDomain dataDomain,
        IDataSetView? dataSet)
    {
        throw new NotImplementedException();
    }

    public override IDataSetView<TEntry> CreateDataSetView<TEntry>(IDataDomainScope dataDomainScope, IDao<TEntry>? repository = null)
    {
        return new DataSetView<TEntry>(this, dataDomainScope, repository);
    }

    public override IHierarchyDataSetView<TEntry> CreateHierarchyDataSetView<TEntry>(IDataDomainScope dataDomainScope, IDao<TEntry>? repository = null)
    {
        throw new NotImplementedException();
    }

    public override void RegisterPageContainer(IPage owningPage, IEnumerable<IPage> pageContainer)
    {
        throw new NotImplementedException();
    }

    public override void UnregisterPageContainer(IPage owningPage)
    {
        throw new NotImplementedException();
    }

    public override void ShowInfoMessage(string message, string? title = null, IPage? ownerPage = null)
    {
        if (title != null)
        {
            Debug.Write($"[{title}] {message}");
        }
        else
        {
            Debug.Write(message);
        }
    }

    public bool HasErrors { get; set; } = false;

    public override void ShowErrorMessage(string message, string? title = null, IPage? ownerPage = null, Exception? exception = null)
    {
        HasErrors = true;

        if (title != null)
        {
            Debug.Write($"[ERROR] [{title}] {message}");
        }
        else
        {
            Debug.Write($"[ERROR] {message}");
        }
    }

    public override bool ShowYesNoQuestionMessage(string message, string? title = null, IPage? ownerPage = null)
    {
        throw new NotSupportedException("The Unit Test view domain does not support Yes/No question messages");
    }

    public override bool ShowConfirmMessage(string message, string? title = null, IPage? ownerPage = null)
    {
        throw new NotSupportedException("The Unit Test view domain does not support confirm messages");
    }

    public override void ShowPage(IPage page)
    {
        // simulate to show a page

        if (page is Page pageInstance)
        {
            pageInstance.OnLoadingAction?.Execute();
            pageInstance.OnLoadedAction.Execute();
        }
    }

    /// <summary>
    /// The value which the method <c>ShowDialogPage</c> will return.
    /// </summary>
    public bool SimulateDialogResult { get; set; } = true;

    public override bool? ShowDialogPage(IPage page, IPage? ownerPage = null)
    {
        // simulate to open and close a page

        ShowPage(page);

        ClosePage(page);

        // this is a simulation value
        return SimulateDialogResult;
    }

    public override void ClosePage(IPage page)
    {
        // simulate to close a page

        if (page is Page pageInstance)
        {
            pageInstance.RaiseClosed();
        }
    }

    public override void PageEndEdit(IPage page)
    {
        throw new NotImplementedException();
    }

    public override void StartEditingDefaultDataGridCurrentEntity(IDataPage page, bool enforceFirstEditableRow)
    {
        throw new NotImplementedException();
    }

    public override string? OpenOpenFileDialog(string title, string fileMask, IPage? ownerPage = null)
    {
        throw new NotImplementedException();
    }

    public override string? OpenSaveFileDialog(string title, string fileMask, IPage? ownerPage = null)
    {
        throw new NotImplementedException();
    }

    public override bool OpenReportDialog(object model, IDataDomain dataDomain, object? reportLayout = null, IPage? ownerPage = null)
    {
        throw new NotImplementedException();
    }

    public override void OpenFile(string filename)
    {
        throw new NotImplementedException();
    }

    public override void BusinessLayerMessageEventToSingleUIMessage(object? businessLayerObject,
        ReportBusinessLayerMessageEventArgs eventArgs)
    {
        throw new NotImplementedException();
    }
}