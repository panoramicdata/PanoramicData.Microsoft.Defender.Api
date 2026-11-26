# authenticated-scan endpoints

| Method | Path | Client method | Test |
|---|---|---|---|
| GET | `/DeviceAuthenticatedScanDefinitions` | `IAuthenticatedScanApi.ListDefinitionsAsync` | `AuthenticatedScanTests.ListDefinitions_SendsGetDeviceAuthenticatedScanDefinitions` |
| POST | `/DeviceAuthenticatedScanDefinitions` | `IAuthenticatedScanApi.CreateDefinitionAsync` | `AuthenticatedScanTests.CreateDefinition_SendsPostWithTheExactBody` |
| PATCH | `/DeviceAuthenticatedScanDefinitions/{id}` | `IAuthenticatedScanApi.UpdateDefinitionAsync` | `AuthenticatedScanTests.UpdateDefinition_SendsPatchWithOnlyTheGivenMembers` |
| POST | `/DeviceAuthenticatedScanDefinitions/BatchDelete` | `IAuthenticatedScanApi.DeleteDefinitionsAsync` | `AuthenticatedScanTests.DeleteDefinitions_SendsPostBatchDeleteWithTheIds` |
| GET | `/DeviceAuthenticatedScanAgents` | `IAuthenticatedScanApi.ListAgentsAsync` | `AuthenticatedScanTests.ListAgents_SendsGetDeviceAuthenticatedScanAgents` |
| GET | `/DeviceAuthenticatedScanAgents/{id}` | `IAuthenticatedScanApi.GetAgentAsync` | `AuthenticatedScanTests.GetAgent_SendsGetDeviceAuthenticatedScanAgentsId` |
| POST | `/DeviceAuthenticatedScanDefinitions/GetScanHistoryByScanDefinitionId` | `IAuthenticatedScanApi.GetScanHistoryByDefinitionAsync` | `AuthenticatedScanTests.GetScanHistoryByDefinition_SendsPostWithTheIdsBody_AndNoQuery` |
| POST | `/DeviceAuthenticatedScanDefinitions/GetScanHistoryBySessionId` | `IAuthenticatedScanApi.GetScanHistoryBySessionAsync` | `AuthenticatedScanTests.GetScanHistoryBySession_SendsPostWithTheIdsBody` |
