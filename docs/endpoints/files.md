# files endpoints

| Method | Path | Client method | Test |
|---|---|---|---|
| GET | `/files/{id}` | `IFilesApi.GetAsync` | `FilesTests.Get_SendsGetFilesId` |
| GET | `/files/{id}/alerts` | `IFilesApi.ListAlertsAsync` | `FilesTests.ListAlerts_SendsGetFilesIdAlerts` |
| GET | `/files/{id}/machines` | `IFilesApi.ListMachinesAsync` | `FilesTests.ListMachines_SendsGetFilesIdMachines` |
| GET | `/files/{id}/stats` | `IFilesApi.GetStatisticsAsync` | `FilesTests.GetStatistics_SendsGetFilesIdStats_WithTheLookBackHours` |
