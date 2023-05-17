# Changelog

## 0.1.5.4 (2023-05-17)

### Kapok.DataPort

- fix deadlock in CsvDataPortTarget

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
