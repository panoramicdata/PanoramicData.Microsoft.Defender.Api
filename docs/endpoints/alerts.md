# alerts endpoints

| Method | Path | Client method | Test |
|---|---|---|---|
| GET | `/alerts` | `IAlertsApi.ListAsync` | `AlertsTests.List_SendsGetAlerts_WithNoQuery` |
| GET | `/alerts/{id}` | `IAlertsApi.GetAsync` | `AlertsTests.Get_SendsGetAlertsId` |
| PATCH | `/alerts/{id}` | `IAlertsApi.UpdateAsync` | `AlertsTests.Update_SendsPatchAlertsId_WithEveryProperty` |
| POST | `/alerts/batchUpdate` | `IAlertsApi.BatchUpdateAsync` | `AlertsTests.BatchUpdate_SendsPostAlertsBatchUpdate` |
| POST | `/alerts/CreateAlertByReference` | `IAlertsApi.CreateByReferenceAsync` | `AlertsTests.CreateByReference_SendsPostAlertsCreateAlertByReference` |
| GET | `/alerts/{id}/domains` | `IAlertsApi.ListDomainsAsync` | `AlertsTests.ListDomains_SendsGetAlertsIdDomains` |
| GET | `/alerts/{id}/files` | `IAlertsApi.ListFilesAsync` | `AlertsTests.ListFiles_SendsGetAlertsIdFiles` |
| GET | `/alerts/{id}/ips` | `IAlertsApi.ListIpsAsync` | `AlertsTests.ListIps_SendsGetAlertsIdIps` |
| GET | `/alerts/{id}/machine` | `IAlertsApi.GetMachineAsync` | `AlertsTests.GetMachine_SendsGetAlertsIdMachine` |
| GET | `/alerts/{id}/user` | `IAlertsApi.GetUserAsync` | `AlertsTests.GetUser_SendsGetAlertsIdUser` |
