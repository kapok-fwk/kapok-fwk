# Changelog

## 0.1.1 (2022-04-23)

### General

- add strong name to libraries

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

## 0.1.0 (2022-04-03)

- first public version
