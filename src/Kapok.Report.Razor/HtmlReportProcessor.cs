﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Reflection;
using System.Threading;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace Kapok.Report.Razor
{
    public class HtmlReportProcessor : ReportProcessor<Model.HtmlReport>, IHtmlReportProcessor
    {
        #region Static members

        static HtmlReportProcessor()
        {
            ReportEngine.RegisterProcessor(typeof(HtmlReportProcessor), typeof(Model.HtmlReport));
        }

        public static void Register()
        {
            // this function can be called to make sure that the static constructor is called.
        }

        #endregion

        private const string TemplateAssemblyName = "Kapok.Report.Razor.HtmlReportProcessor.TemplateAssembly";
        private const string TemplateNamespace = "Kapok.Report.Razor";

        public const string TextMimeType = "text/plain";
        public const string HtmlMimeType = "text/html";

        private readonly ReportEngine? _reportEngine;

        public HtmlReportProcessor(ReportEngine? reportEngine = default)
        {
            _reportEngine = reportEngine;
        }

        public override void ValidateReportModel()
        {
            base.ValidateReportModel();

            if (ReportModel.Template == null && ReportModel.TemplateResourceName == null)
                throw new NotSupportedException($"The report model does not have an html template (property {nameof(Model.HtmlReport.Template)} or {nameof(Model.HtmlReport.TemplateResourceName)}).");
        }

        public override string[] SupportedMimeTypes => new[] { HtmlMimeType };

        public override void ProcessToStream(string mimeType, Stream stream)
        {
            TestMimeType(mimeType);

            switch (mimeType)
            {
                case HtmlMimeType:
                {
                    var writer = new StreamWriter(stream);
                    writer.Write(Process<HtmlTemplate>());
                    writer.Flush();
                    stream.Position = 0;
                }
                    break;
                case TextMimeType:
                {
                    var writer = new StreamWriter(stream);
                    writer.Write(Process<TextTemplate>());
                    writer.Flush();
                    stream.Position = 0;
                }
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Additional metadata references you want to use in your template.
        /// </summary>
        public MetadataReference[]? AdditionalMetadataReferences { get; set; }

        /// <summary>
        /// Let you use the razor template engine with a custom razor template class.
        /// </summary>
        /// <typeparam name="TRazorTemplate">
        /// The base razor template class.
        /// </typeparam>
        /// <returns>
        /// Returns the finalized string from the model with the given template.
        /// </returns>
        public string Process<TRazorTemplate>()
            where TRazorTemplate : RazorTemplateBase
        {
            var templateString = GetTemplateString();

            if (templateString == null)
                throw new NotSupportedException("Template is null");

            // add inherit for base model
            templateString = $"@inherits {typeof(TRazorTemplate).FullName}\n{templateString}";

            // execute all DataSets
            if (_reportEngine != null)
            {
                foreach (var dataSet in ReportModel.DataSets.Values)
                {
                    if (dataSet is IDbReportDataSet dbDataSet && dbDataSet.DataSourceName != null)
                    {
                        var dataSource = (DbReportDataSource)_reportEngine.GetDataSource(dbDataSet.DataSourceName);
                        using var connection = dataSource.CreateNewConnection();

                        dbDataSet.ExecuteQuery(connection,
                            parameters: ReportModel.Parameters,
                            resourceProvider: ReportModel.Resources);
                    }
                }
            }

            var oldCulture = Thread.CurrentThread.CurrentCulture;
            var oldUiCulture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentCulture = ReportLanguage;
            Thread.CurrentThread.CurrentUICulture = ReportLanguage;

            string result = RunCompile(templateString, ReportModel, AdditionalMetadataReferences);

            Thread.CurrentThread.CurrentCulture = oldCulture;
            Thread.CurrentThread.CurrentUICulture = oldUiCulture;

            return result;
        }

        [Obsolete("Use method ProcessToStream instead")]
        public string ProcessToHtml()
        {
            return Process<HtmlTemplate>();
        }

        #region Private members

        private string? GetTemplateString()
        {
            string? templateString;

            if (ReportModel.Template == null)
            {
                if (ReportModel.TemplateResourceName == null)
                    throw new NotSupportedException($"You have to set property {nameof(ReportModel.Template)} or {nameof(ReportModel.TemplateResourceName)} in the model to be able to use the processor {typeof(HtmlReportProcessor).FullName}");

                // try to get the html template from the resource provider
                string? resourceName = ReportModel.TemplateResourceName?.LanguageOrDefault(ReportLanguage.Name);

                if (resourceName == null)
                    throw new NotSupportedException($"Could not find resource for culture {ReportLanguage.Name} in the property {nameof(ReportModel.TemplateResourceName)}.");

                templateString = System.Text.Encoding.Default.GetString(
                    ReportModel.Resources[resourceName].Data
                );
            }
            else
            {
                templateString = ReportModel.Template.LanguageOrDefault(ReportLanguage);
            }

            return templateString;
        }

        private static string GenerateCodeFromTemplate(string template)
        {
            RazorProjectEngine engine = RazorProjectEngine.Create(
                RazorConfiguration.Default,
                RazorProjectFileSystem.Create(@"."),
                (builder) =>
                {
                    builder.SetNamespace(TemplateNamespace);
                });

            string fileName = Path.GetRandomFileName();

            var document = RazorSourceDocument.Create(template, fileName);

            var codeDocument = engine.Process(document, null, new List<RazorSourceDocument>(), new List<TagHelperDescriptor>());

            RazorCSharpDocument razorCSharpDocument = codeDocument.GetCSharpDocument();

            return razorCSharpDocument.GeneratedCode;
        }

        private static MemoryStream Compile(string assemblyName, string code, MetadataReference[]? additionalReferences = null)
        {
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);

            List<MetadataReference> metadataReferences = new List<MetadataReference>(new[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(RazorTemplateBase).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(DynamicObject).Assembly.Location),
                MetadataReference.CreateFromFile(
                    Assembly.Load(new AssemblyName("Microsoft.CSharp")).Location),
                MetadataReference.CreateFromFile(
                    Assembly.Load(new AssemblyName("netstandard")).Location),
                MetadataReference.CreateFromFile(
                    Assembly.Load(new AssemblyName("System.Runtime")).Location),
            });
            if (additionalReferences != null)
                metadataReferences.AddRange(additionalReferences);

            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName,
                new[]
                {
                    syntaxTree
                },
                metadataReferences.ToArray(),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            MemoryStream memoryStream = new MemoryStream();

            EmitResult emitResult = compilation.Emit(memoryStream);

            if (!emitResult.Success)
            {
                throw new RazorTemplateCompilationException(emitResult.Diagnostics);
            }

            memoryStream.Position = 0;

            return memoryStream;
        }

        private static string RunCompile(string template, object model, MetadataReference[]? additionalReferences = null)
        {
            var code = GenerateCodeFromTemplate(template);
            var assemblyMemoryStream = Compile(TemplateAssemblyName, code, additionalReferences);

            Assembly assembly = Assembly.Load(assemblyMemoryStream.ToArray());
            Type templateType = assembly.GetType($"{TemplateNamespace}.Template");

            HtmlTemplate instance = (HtmlTemplate)Activator.CreateInstance(templateType);

            instance.Model = new AnonymousTypeWrapper(model);
            instance.ExecuteAsync().Wait();

            return instance.Result();
        }

        #endregion
    }
}
