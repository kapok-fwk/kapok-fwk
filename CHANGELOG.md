# Changelog

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
