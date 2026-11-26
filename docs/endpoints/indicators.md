# indicators endpoints

| Method | Path | Client method | Test |
|---|---|---|---|
| GET | `/indicators` | `IIndicatorsApi.ListAsync` | `IndicatorsTests.List_SendsGetIndicators_WithNoQuery` |
| POST | `/indicators` | `IIndicatorsApi.CreateAsync` | `IndicatorsTests.Create_SendsPostIndicators_WithEveryProperty` |
| POST | `/indicators/import` | `IIndicatorsApi.ImportAsync` | `IndicatorsTests.Import_SendsPostIndicatorsImport_WithACapitalIndicatorsProperty` |
| DELETE | `/indicators/{id}` | `IIndicatorsApi.DeleteAsync` | `IndicatorsTests.Delete_SendsDeleteIndicatorsId` |
| POST | `/indicators/BatchDelete` | `IIndicatorsApi.BatchDeleteAsync` | `IndicatorsTests.BatchDelete_SendsPostIndicatorsBatchDelete_WithACapitalIndicatorIdsProperty` |
