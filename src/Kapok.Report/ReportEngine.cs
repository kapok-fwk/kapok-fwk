using System.Diagnostics;
using System.Runtime.CompilerServices;
using Kapok.BusinessLayer;
using Kapok.Data;
using Kapok.Data.InMemory;
using Kapok.Report.BusinessLayer;
using Kapok.Report.DataModel;
using Kapok.View;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kapok.Report;
// TODO: implement status reporting from the ReportEngine class e.g. to the UI

public sealed class ReportEngine
{
    #region Report processor registration

    private static readonly List<RegisteredReportProcessor> RegisteredReportProcessors = new();

    private struct RegisteredReportProcessor
    {
        public Type ReportProcessorType;
        public Type BaseModelType;

        public bool IsDesignable;
    }

    public static void RegisterProcessor(Type reportProcessor, Type baseModelType)
    {
        if (reportProcessor.IsAbstract)
            throw new ArgumentException("The report processor type can not be an abstract report processor.",
                nameof(reportProcessor));

        if (!typeof(IReportProcessor).IsAssignableFrom(reportProcessor))
            throw new ArgumentException(
                $"The report processor must implement the interface {typeof(IReportProcessor).FullName}.", nameof(reportProcessor));

        if (!typeof(IMimeTypeReportProcessor).IsAssignableFrom(reportProcessor))
            throw new ArgumentException(
                $"The report processor must implement the interface {typeof(IMimeTypeReportProcessor).FullName}.", nameof(reportProcessor));

        if (baseModelType == typeof(Model.Report) || !typeof(Model.Report).IsAssignableFrom(baseModelType))
            throw new ArgumentException(
                $"The base model type for an report processor must be an sub-type of {typeof(Model.Report).FullName}");

        bool isDesignable = reportProcessor.GetInterfaces().Contains(typeof(IDesignableReportProcessor));

        RegisteredReportProcessors.Add(new RegisteredReportProcessor
        {
            ReportProcessorType = reportProcessor,
            BaseModelType = baseModelType,
            IsDesignable = isDesignable
        });
    }

    #region Data source handling

    private readonly Dictionary<string, ReportDataSource> _dataSources = new();

    /// <summary>
    /// Registers a new data source to the report engine.
    /// </summary>
    /// <param name="dataSource"></param>
    /// <exception cref="ArgumentException">
    /// Is thrown when a data source with the same name is already registered to the report engine.
    /// </exception>
    public void RegisterDataSource(ReportDataSource dataSource)
    {
        if (string.IsNullOrEmpty(dataSource.Name))
            throw new ArgumentException($"The name of the data source can not be null or an empty string.", nameof(dataSource));

        if (_dataSources.ContainsKey(dataSource.Name))
            throw new ArgumentException($"A data source with name '{dataSource.Name}' is already registered.", nameof(dataSource));

        _dataSources.Add(dataSource.Name, dataSource);
    }

    /// <summary>
    /// Requests a data source from the report engine
    /// </summary>
    /// <param name="name">
    /// The name of the data source.
    /// </param>
    /// <returns>
    /// The report data source. Can not be null.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Is thrown when a data source with the given <paramref name="name"/> is unknown to the report engine.
    /// 
    /// You will have to call <see cref="RegisterDataSource"/> first to register a data source with the given name.
    /// </exception>
    public ReportDataSource GetDataSource(string name)
    {
        if (!_dataSources.ContainsKey(name))
            throw new ArgumentException($"Could not find data source by name: {name}", nameof(name));

        return _dataSources[name];
    }

    #endregion

    static ReportEngine()
    {
    }

    private RegisteredReportProcessor GetRegisteredReportProcessor(Model.Report model)
    {
        var modelType = model.GetType();

        foreach (var registeredReportProcessor in RegisteredReportProcessors)
        {
            if (registeredReportProcessor.BaseModelType.IsAssignableFrom(modelType))
                return registeredReportProcessor;
        }

        throw new NotSupportedException(
            $"No report has been registered which supports the report model {modelType.FullName}");
    }

    #endregion

    private readonly IDataDomain? _dataDomain;
    private IServiceProvider? _serviceProvider;

    public ReportEngine(IDataDomain? dataDomain = default)
    {
        _dataDomain = dataDomain;
        _serviceProvider = dataDomain?.ServiceProvider;
    }

    public ReportEngine(IDataDomain? dataDomain = default, IServiceProvider? serviceProvider = default)
        : this(dataDomain)
    {
        _serviceProvider = serviceProvider ?? dataDomain?.ServiceProvider;
    }

    /// <summary>
    /// The service provider to be used for page construction.
    /// </summary>
    public IServiceProvider ServiceProvider
    {
        get => _serviceProvider ??= CreateDefaultServiceProvider();
        set => _serviceProvider = value;
    }

    private IServiceProvider CreateDefaultServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton(this);
#pragma warning disable CS8603 // Possible null reference return.
        services.AddSingleton<IDataDomain, InMemoryDataDomain>();
        services.AddScoped<IDataDomainScope>(p => new InMemoryDataDomainScope(p.GetRequiredService<IDataDomain>(), p));
        services.TryAdd(ServiceDescriptor.Scoped(typeof(IRepository<>), typeof(InMemoryRepository<>)));

#pragma warning restore CS8603 // Possible null reference return.
        return services.BuildServiceProvider();
    }

    private void CheckDataDomainSet([CallerMemberName] string? memberName = null)
    {
        if (_dataDomain == null)
            throw new NotSupportedException($"You have to instantiate the ReportEngine with the dataDomain parameter to be able to call method {memberName}");
    }

    private ReportModel GetOrCreateReportModel(Model.Report model, IDataDomainScope dataDomainScope)
    {
        var reportModelType = model.GetType();
        return dataDomainScope.GetEntityService<ReportModel, IReportModelService>()
            .GetOrCreateFromType(reportModelType);
    }

    private ReportLayout GetOrCreateReportLayout(ReportLayout? layout, ReportModel reportModel, Model.Report model,
        IDataDomainScope dataDomainScope)
    {
        if (layout != null)
            return layout;

        if (reportModel.DefaultLayoutId != null)
        {
            layout = dataDomainScope.GetEntityService<ReportLayout, IReportLayoutService>()
                .FindByKey(reportModel.DefaultLayoutId);
            if (layout == null)
                throw new NotSupportedException("Referenced default layout does not exist.");
        }
        else
        {
            layout = new ReportLayout
            {
                ReportLayoutId = Guid.NewGuid(),
                Name = model.Caption,
                ReportModelId = reportModel.ReportModelId,
                ReportModel = reportModel
            };
            dataDomainScope.GetEntityService<ReportLayout, IReportLayoutService>()
                .Create(layout);

            dataDomainScope.Save(); // note a bit dirty, but needs to be done to do not create circulating references during insert

            reportModel.DefaultLayoutId = layout.ReportLayoutId;
            reportModel.DefaultLayout = layout;
            dataDomainScope.GetEntityService<ReportModel, IReportModelService>()
                .Update(reportModel);
        }

        return layout;
    }

    private ReportDesign GetOrCreateReportDesign(ReportDesign? design, ReportLayout layout,
        ReportProcessor defaultReportProcessor, IDataDomainScope dataDomainScope)
    {
        if (design != null)
            return design;

        var reportDesignRepo = dataDomainScope.GetEntityService<ReportDesign, IReportDesignService>();

        if (layout.ActiveDesignVersion.HasValue)
        {
            design = reportDesignRepo.FindByKey(layout.ReportLayoutId, layout.ActiveDesignVersion.Value);

            if (design == null)
                // TODO: implement an exception 'ReferencedEntityNotFound'
                throw new NotSupportedException($"The active design version number {layout.ActiveDesignVersion.Value} for layout {layout} could not be found.");
        }
        else if ((from d in reportDesignRepo.AsQueryable()
                     where d.ReportLayoutId == layout.ReportLayoutId
                     select d).Any())
        {
            throw new NotSupportedException($"No active design has been selected for layout {layout}.");
        }
        else
        {
            Debug.WriteLine($"{nameof(ReportEngine)}: No design exist for report layout {layout}, we will create one.");

            design = new ReportDesign
            {
                ReportLayoutId = layout.ReportLayoutId,
                ReportLayout = layout,
                ReportProcessorId = defaultReportProcessor.ReportProcessorId,
                ReportProcessor = defaultReportProcessor,
                VersionNum = 1
            };
            reportDesignRepo.Create(design);

            dataDomainScope.Save(); // note a bit dirty, but needs to be done to do not create circulating references during insert

            layout.ActiveDesignVersion = design.VersionNum;
            layout.ActiveDesign = design;
            dataDomainScope.GetEntityService<ReportLayout, IReportLayoutService>()
                .Update(layout);
        }

        return design;
    }

    private async Task<ReportDestination> GetReportDestination(ReportDestination? destination,
        ReportLayout layout,
        IDataDomainScope dataDomainScope)
    {
        if (destination != null)
            return destination;

        var destinationRepo = dataDomainScope.GetEntityService<ReportDestination, IReportDestinationService>();

        if (layout.DefaultDestinationId.HasValue)
        {
            destination =
                await destinationRepo.GetByKeyAsync(layout.ReportLayoutId, layout.DefaultDestinationId.Value);

            if (destination == null)
                // TODO: implement an exception 'ReferencedEntityNotFound'
                throw new NotSupportedException();
        }
        else if ((from d in destinationRepo.AsQueryable()
                     where d.ReportLayoutId == layout.ReportLayoutId
                     select d).Any())
        {
            throw new NotSupportedException($"No default destination has been selected for layout {layout}.");
        }
        else
        {
            throw new NotSupportedException($"No destination has been configured for layout {layout}.");
        }

        return destination;
    }

    private ReportDesign SaveNewDesign(IDesignableReportProcessor reportProcessor, ReportLayout reportLayout, IDataDomainScope dataDomainScope)
    {
        var processorRepo = dataDomainScope.GetEntityService<ReportProcessor, IReportProcessorService>();

        Type processorBasisType = reportProcessor.GetType();
        if (processorBasisType.IsGenericType && !processorBasisType.IsGenericTypeDefinition)
            processorBasisType = processorBasisType.GetGenericTypeDefinition();

        var processorEntity = processorRepo.GetOrCreateFromType(processorBasisType).Result;

        var designRepo = dataDomainScope.GetEntityService<ReportDesign, IReportDesignService>();
        var lastVersion = (
            from d in designRepo.AsQueryable()
            where d.ReportLayoutId == reportLayout.ReportLayoutId
            select (int?) d.VersionNum
        ).Max();

        var newDesign = new ReportDesign
        {
            ReportLayoutId = reportLayout.ReportLayoutId,
            ReportLayout = reportLayout,
            ReportProcessorId = processorEntity.ReportProcessorId,
            ReportProcessor = processorEntity,
            VersionNum = lastVersion + 1 ?? 1,
            DesignData = reportProcessor.CurrentDesign
        };
        designRepo.Create(newDesign);
        return newDesign;
    }

    private void DisposeProcessor(IMimeTypeReportProcessor? processor)
    {
        if (processor == null)
            return;

        // ReSharper disable once SuspiciousTypeConversion.Global
        if (processor is IDisposable disposable)
            disposable.Dispose();
    }

    public ReportLayout GetOrCreateReportLayout(Model.Report model, ReportLayout? layout)
    {
        CheckDataDomainSet();
#pragma warning disable CS8602
        using var dataDomainScope = _dataDomain.CreateScope();
#pragma warning restore CS8602
        var reportModel = GetOrCreateReportModel(model, dataDomainScope);
        // ReSharper disable once UnusedVariable
        var registeredProcessor = GetRegisteredReportProcessor(model);
        var reportLayout = GetOrCreateReportLayout(layout, reportModel, model, dataDomainScope);
        dataDomainScope.Save();
        return reportLayout;
    }

    public bool IsModelDesignable(Model.Report model)
    {
        var registeredProcessor = GetRegisteredReportProcessor(model);
        return registeredProcessor.IsDesignable;
    }

    public string[] GetSupportedMimeTypes(Model.Report model, ReportLayout? layout = null, ReportDesign? design = null)
    {
        CheckDataDomainSet();
#pragma warning disable CS8602
        using var dataDomainScope = _dataDomain.CreateScope();
#pragma warning restore CS8602
        var (_, _, reportProcessor) =
            InitiateReportProcessor(model, dataDomainScope, layout, design).Result;
        dataDomainScope.Save();
        return reportProcessor.SupportedMimeTypes;
    }

    private object CreateReportProcessorInstance(Type reportProcessorType)
    {
        return ActivatorUtilities.CreateInstance(ServiceProvider, reportProcessorType);
    }

    private Task<Tuple<ReportLayout, ReportDesign?, IMimeTypeReportProcessor>>
        InitiateReportProcessor(Model.Report model, IDataDomainScope dataDomainScope, ReportLayout? layout, ReportDesign? design)
    {
        var reportModel = GetOrCreateReportModel(model, dataDomainScope);
        var registeredProcessor = GetRegisteredReportProcessor(model);
        layout = GetOrCreateReportLayout(layout, reportModel, model, dataDomainScope);
            
        var reportProcessorService = dataDomainScope.GetEntityService<ReportProcessor, IReportProcessorService>();
        ReportProcessor? reportProcessor = reportProcessorService.GetOrCreateFromType(registeredProcessor.ReportProcessorType).Result;
        Type reportProcessorType = registeredProcessor.ReportProcessorType;
        if (registeredProcessor.IsDesignable)
        {
            design = GetOrCreateReportDesign(design, layout, reportProcessor, dataDomainScope);
            if (design.ReportProcessorId != reportProcessor.ReportProcessorId)
            {
                Debug.WriteLine($"{nameof(ReportEngine)}: Design report processor {reportProcessorType.FullName} is different as the registered report processor {registeredProcessor.ReportProcessorType.FullName}.");

                reportProcessor = reportProcessorService.FindByKey(design.ReportProcessorId);
                if (reportProcessor == null)
                    throw new NotSupportedException("Could not find report processor by id: " + design.ReportProcessorId);

                reportProcessorType = reportProcessorService.GetType(reportProcessor);

                // check; this should never happen!
                bool isDesignable = reportProcessorType.GetInterfaces().Contains(typeof(IDesignableReportProcessor));
                if (!isDesignable)
                {
                    throw new NotSupportedException(
                        "A non-designable report processor is configured for an report design!");
                }
            }
        }
        else if (design != null)
        {
            throw new NotSupportedException(
                "An design has been given for an report model which is connected to an registered report model which does not support an design.");
        }

        if (reportProcessorType.IsGenericTypeDefinition)
        {
            reportProcessorType = reportProcessorType.MakeGenericType(model.GetType());
        }

        var processor = (IMimeTypeReportProcessor)CreateReportProcessorInstance(reportProcessorType);
        processor.ReportModel = model;

        if (registeredProcessor.IsDesignable &&
            // ReSharper disable once SuspiciousTypeConversion.Global
            processor is IDesignableReportProcessor designableReportProcessor)
        {
            designableReportProcessor.CurrentDesign = design?.DesignData;
        }

        return Task.FromResult(new Tuple<ReportLayout, ReportDesign?, IMimeTypeReportProcessor>
        (
            layout,
            design,
            processor
        ));
    }

    private IMimeTypeReportProcessor InitiateReportProcessor(Model.Report model)
    {
        var registeredProcessor = GetRegisteredReportProcessor(model);

        Type reportProcessorType = registeredProcessor.ReportProcessorType;
        if (registeredProcessor.IsDesignable)
        {
            throw new NotSupportedException("The report processor is designable; it is not supported to use ReportEngine with designable reports without passing a DataDomain on construction");
        }

        if (reportProcessorType.IsGenericTypeDefinition)
        {
            reportProcessorType = reportProcessorType.MakeGenericType(model.GetType());
        }

        var processor = (IMimeTypeReportProcessor)CreateReportProcessorInstance(reportProcessorType);
        processor.ReportModel = model;

        return processor;
    }

    public void ExecuteReport(Model.Report model, Dictionary<string, object> parameterValues,
        string mimeType, Stream stream, ReportLayout? layout = null, ReportDesign? design = null)
    {
        IMimeTypeReportProcessor reportProcessor;

        if (_dataDomain == null)
        {
            if (layout != null)
                throw new ArgumentException($"When the report engine is initialized without DataDomain, you can not use parameter {nameof(layout)}", nameof(layout));
            if (design != null)
                throw new ArgumentException($"When the report engine is initialized without DataDomain, you can not use parameter {nameof(design)}", nameof(design));

            reportProcessor = InitiateReportProcessor(model);
        }
        else
        {
            using var dataDomainScope = _dataDomain.CreateScope();
            (_, _, reportProcessor) =
                InitiateReportProcessor(model, dataDomainScope, layout, design).Result;

            dataDomainScope.Save();
        }

        reportProcessor.ParameterValues = parameterValues;

        reportProcessor.ProcessToStream(mimeType, stream);

        DisposeProcessor(reportProcessor);
    }

    public void ExecuteReport(Model.Report model, Dictionary<string, object> parameterValues,
        ReportDestination destination, ReportLayout? layout = null, ReportDesign? design = null)
    {
        CheckDataDomainSet();
#pragma warning disable CS8602
        using var dataDomainScope = _dataDomain.CreateScope();
#pragma warning restore CS8602
        var (layoutCurrent, _, reportProcessor) =
            InitiateReportProcessor(model, dataDomainScope, layout, design).Result;
        destination = GetReportDestination(destination, layoutCurrent, dataDomainScope).Result;

        if (destination.MimeType == null)
        {
            throw new NotSupportedException("The destination has no MimeType specified");
        }

        reportProcessor.ParameterValues = parameterValues;

        var destinationRepo = dataDomainScope.GetEntityService<ReportDestination, IReportDestinationService>();

        using (var stream = destinationRepo.CreateStreamInstance(destination))
        {
            reportProcessor.ProcessToStream(destination.MimeType, stream);
        }

        DisposeProcessor(reportProcessor);

        dataDomainScope.Save();
    }

    /// <summary>
    /// Runs the report several times for all given report parameter combinations.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="parameterValuesSet">
    /// A set of parameter values the report engine shall iterate over.
    /// </param>
    /// <param name="layout"></param>
    /// <param name="destination"></param>
    /// <param name="design"></param>
    public void ExecuteIterativeReport(Model.Report model, Dictionary<string, object>[] parameterValuesSet,
        ReportDestination destination, ReportLayout? layout = null, ReportDesign? design = null)
    {
        // TODO: this here should be optimized, e.g. that the report processor is just initiated once and then executed with different values instead of initiate it for each iteration

        foreach (var parameterValues in parameterValuesSet)
        {
            ExecuteReport(model, parameterValues, destination, layout, design);
        }
    }

    public void OpenDesignDialog(Model.Report model, IViewDomain viewDomain, ReportLayout? layout = null, ReportDesign? design = null)
    {
        CheckDataDomainSet();
#pragma warning disable CS8602
        using var dataDomainScope = _dataDomain.CreateScope();
#pragma warning restore CS8602
        var task = InitiateReportProcessor(model, dataDomainScope, layout, design);
        var (layoutCurrent, designCurrent, reportProcessor) = task.Result;
        dataDomainScope.Save();
        reportProcessor.ReportLanguage = viewDomain.Culture;

        // ReSharper disable once SuspiciousTypeConversion.Global
        if (!(reportProcessor is IDesignableReportProcessor designableReportProcessor))
        {
            throw new NotSupportedException(
                $"The report processor for model {model.GetType().FullName} does not implement the interface {typeof(IDesignableReportProcessor).FullName}.");
        }

        if (designableReportProcessor.OpenDesignDialog(viewDomain))
        {
            // save new design version
            var designNew = SaveNewDesign(designableReportProcessor, layoutCurrent, dataDomainScope);

            // updates the layout version when the changed report is the current report (TODO this behavior should maybe be changed based on application configuration)
            if (layoutCurrent.ActiveDesignVersion != null &&
                layoutCurrent.ActiveDesignVersion.Value == designCurrent?.VersionNum)
            {
                layoutCurrent.ActiveDesignVersion = designNew.VersionNum;
                dataDomainScope.GetEntityService<ReportLayout, IReportLayoutService>()
                    .Update(layoutCurrent);
            }
        }

        DisposeProcessor(reportProcessor);

        dataDomainScope.Save();
    }
}