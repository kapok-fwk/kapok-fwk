# Changelog

## 0.1.14 (2023-11-05)

- :bug: *fix* apply filter from DataSetListView on ListPage
- :bug: *fix* MetadataEngine use IFilterJsonConverter

## 0.1.13 (2032-11-05)

- :tada: use when building models dependency injection
- :tada: add `Filter` to `DataSetListView` with serialization support

## 0.1.12 (2023-10-29)

### Kapok.View

- :tada: add ListViewName to UIOpenPageAction, add as property for  UIOpenReferencedPageAction
- :rocket: refactoring EntityDefaultPageType, allow non IDataPage pages to be a default page type

## 0.1.11 (2023-10-28)

### General

- :rocket: dependency upgrades

### Kapok.Core

- :tada: add support to generate automatically a new GUID for properties with attribute `AutoGenerateValue`, option `Identity`
- :rocket: smaller (micro) optimizations
- :bug: *fix* type check bug in `PropertyStaticFilter` class

### Kapok.Data.EntityFrameworkCore

- :rocket: EF Core: use no generic value comparer/converter for ef core Json convertion (preparing support for EF core precompiled models)
- :rocket: smaller (micro) optimizations

### Kapok.View

- :tada: add metadata manager for page list views and optional metadata
- :rocket: smaller (micro) optimizations
- :rocket: add page constructors for `IDataDomain` and instanciate scope on demand
- :rocket: add serialization support for DataSetListView and expressions (requires nuget package `Nuqleon.Linq.Expressions.Bonsai.Serialization`)

## 0.1.10.3 (2023-10-08)

### Kapok.View

- :bug: *fix* bug using PropertyView.Name instead of PropertyInfo.Name
- :bug: *fix* memomy leak by unregistering the PageContainer from the view domain in InteractivePage

## 0.1.10.2 (2023-10-03)

### Kapok.View
- :bug: *fix* set HostPage for referenced pages

## 0.1.10.1 (2023-10-03)

### Kapok.View

- :bug: *fix* patch menus before adding pages in DocumentPageCollectionPage

## 0.1.10 (2023-10-03)

### Kapok.Acl
- :tada: add ClaimType Partition

### Kapok.View

- :rocket: improve menu item creation in DocumentPageCollectionPage
- :rocket: allow saving always when DAO is not deferred and is EF Core
- :rocket: allow construction of UIMenuItem.SubMenuItems
- :rocket: *refactoring* DocumentPageCollectionPage integration

## 0.1.9.1 (2023-09-10)

### Kapok.View

- :bug: *fixing* menu issues with DocumentPageCollectionPage

## 0.1.9 (2023-09-03)

### Kapok.Acl

- :tada: add LoginProvider to database with new `Configuration` property

### Kapok.Core
- :rocket: *change* InMemoryDataDomain to hold data during livetime of DataDomain, not only scope repository livetime
- :rocket: set DataPartition when `DaoBase.New()` is called
- :bug: *fix* type casting issue when DataPartition property is a primitive type

### Kapok.Data.EntityFrameworkCore

- :tada: add support for Json serialization based on `System.Text.Json.Node` objects
- :bug: *fix* parallel SQLite unit test executioning

### Kapok.View

- :tada: add support relative WPF path image names in ImageManager (e.g. `/AssemblyName;component/Images/Untitled.png`)

## 0.1.8 (2023-08-13)

### Kapok.View

- :rocket: refactor DocumentPageCollectionPage, add routing for default page events

## 0.1.7 (2023-08-13)

### Kapok.View

- :rocket: add actions to page contracts
- :rocket: support PropertyView/ColumnPropertyView just based on property name with later binding to PropertyInfo

## 0.1.6

### Kapok.View (2023-08-13)

- :tada: add TextWrap property to ColumnPropertyView class
- :rocket: make it possible to add own UIMenu instances to InteractivePage
- :bug: fix crash with DocumentPageCollectionPage + fix memory leak bug

### Kapok.Data.EntityFrameworkCore (2023-07-24)

- add support for auto-generated current date/time sql function for sql provider SQLite and PostgreSQL

## 0.1.5.8 (2023-06-10)

### Kapok.Core

- simplify `ILookupDefinition`: return value of `EntriesFunc` changed from `IQueryable<T>` to `IEnumerable<T>` (#14)

### Kapok.View

- add constructor to `ProperyViewCollection` which uses `currentSelector` (usable in card pages) (#13)

## 0.1.5.7 (2023-06-08)

### Kapok.Core

- :bug: fix index was not unique when using AddUniqueIndex (#11)

## 0.1.5.6 (2023-05-17)

### Kapok.Report

- :bug: fixing issue with passing ReportParameterCollection to IDbReportDataSet.ExecuteQuery

## 0.1.5.5 (2023-05-17)

### Kapok.DataPort

- :bug: fix duplicated stream write, remove internal async. logic in CsvDataPortTarget

## 0.1.5.4 (2023-05-17)

### Kapok.DataPort

- :bug: fix deadlock in CsvDataPortTarget

## 0.1.5.3 (2023-05-17)

### Kapok.DataPort

- :bug: fix add CSV quotation support for `CsvDataPortTarget` (#10)

## 0.1.5.2 (2023-04-15)

### Kapok.Core

- :rocket: implement additional constructors for BusinessLayerException / BusinessLayerErrorException
- :rocket: *change* root namespace of Kapok.Core to Kapok
- :rocket: *change* fix null hint in IViewDomain.BusinessLayerMessageEventToSingleUIMessage
- :bug: fix a bug with AutoCalculate when base query is based on QueryTranslator<T>

## 0.1.5.1 (2023-03-05)

### Kapok.Report

- refactoring namespaces in Kapok.Report
- add DataTableReportProcessor back again

## 0.1.5 (2023-03-05)

### General

- refactor code; process suggestions from ReSharper
- add build artifacts (#7)
- add source link

### Kapok.Acl

- refactoring namespace naming

### Kapok.Core

- refactoring namespace naming

### Kapok.Data.EntityFrameworkCore

- see General

### Kapok.DataPort

- dependency update + see General

### Kapok.Report

- drop obsolete methods and classes

### Kapok.Report.Razor

- dependency update

### Kapok.Report.SqlServer

- dependency update

### Kapok.View

- dependency update + see General

## 0.1.4 (2023-02-21)

### Kapok.Core

- :rocket: *change* change like operators from %/_ to */? and implement to use automatically like when property type is string and */? is used

### Kapok.Report

- :tada: *feat* implement converting of some standard number formats to excel formats
- :rocket: *change* make DataTableReport.Fields optional
- :bug: *fix* excel correct table row and column size

### Kapok.View

- :tada: *feat* add calendar images
- :rocket: *change* expose DeferRefresh() method in IDataSetReadonlyView
- :bug: *fix* null value in DialogButtons
- :bug: *fix* missing PageContainer registration for InteractivePage.DetailPages

## 0.1.3 (2022-05-22)

### Kapok.Report

- :bug: *fix* `DataTableReportFormatter.Default` was null without a default value

### Kapok.View

- :tada: *feat* `DataSetListView` supports now to set the sort direction of a data set with property `SortDirection`
- :rocket: *change* `ListPage` is not anymore abstract

## 0.1.1-0.1.2 (2022-04-23)

### General

- add strong name to libraries

### Kapok.Core

- :tada: *feat* added a in-memory data domain for e.g. unit testing (see namespace Kapok.Data.InMemory)

### Kapok.DataPort

- :tada: *feat* added `NullValueString` option for `CsvDataPortSource/CsvDataPortTarget` data ports
- :rocket: :heavy_exclamation_mark: *Breaking change* several Kapok.DataPort.Entity classes have been renamed and moved to their own namespace:
  - `Kapok.DataPort.DataPortEntityCollectionTarget` --> `Kapok.DataPort.Entity.EntityCollectionDataPortTarget`
  - `Kapok.DataPort.DataPortEntityEnumeratorSource` --> `Kapok.DataPort.Entity.EntityEnumeratorDataPortSource`
  - `Kapok.DataPort.EntityDataPortTarget` --> `Kapok.DataPort.Entity.EntityDataPortTargetBase`
  - `Kapok.DataPort.EntityDataPortSource` --> `Kapok.DataPort.Entity.EntityDataPortSourceBase`
  - `Kapok.DataPort.EntityDataPortHelper` --> `Kapok.DataPort.Entity.EntityDataPortHelper`

### Kapok.View
- :rocket: *change* the `IViewDomain viewDomain` parameter is not anymore required in pages
- :bug: *fix* several members had a wrong nullable assigment
- :bug: *fix* bug with `ListPage<>.OpenCardPageAction` when using `UIOpenReferencedCardPageAction` and parameter `IDataSetView<T>` is used in the constructor of the new page

## 0.1.0 (2022-04-03)

- first public version
