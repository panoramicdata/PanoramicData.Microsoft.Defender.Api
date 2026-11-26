# machines endpoints

| Method | Path | Client method | Test |
|---|---|---|---|
| GET | `/machines` | `IMachinesApi.ListAsync` | `MachinesTests.List_SendsGetMachines_WithTheODataOptions` |
| GET | `/machines/{id}` | `IMachinesApi.GetAsync` | `MachinesTests.Get_SendsGetMachinesId` |
| PATCH | `/machines/{machineId}` | `IMachinesApi.UpdateAsync` | `MachinesTests.Update_SendsPatchMachinesId_WithBothProperties` |
| GET | `/machines/{id}/logonusers` | `IMachinesApi.ListLogOnUsersAsync` | `MachinesTests.ListLogOnUsers_SendsGetMachinesIdLogonusers` |
| GET | `/machines/{id}/alerts` | `IMachinesApi.ListAlertsAsync` | `MachinesTests.ListAlerts_SendsGetMachinesIdAlerts` |
| GET | `/machines/find(timestamp={time},key={IP})` | `IMachinesApi.FindAsync` | `MachinesTests.Find_SendsTheParenthesisedPath_WithTheTimestampAndKey` |
| GET | `/machines/findbyip(ip='{IP}',timestamp={TimeStamp})` | `IMachinesApi.FindByIpAsync` | `MachinesTests.FindByIp_SendsTheQuotedIpAndTheTimestampInThePath` |
| GET | `/machines/findbytag` | `IMachinesApi.FindByTagAsync` | `MachinesTests.FindByTag_SendsTheTagAsAQueryParameter` |
| POST | `/machines/{id}/tags` | `IMachinesApi.AddOrRemoveTagAsync` | `MachinesTests.AddOrRemoveTag_SendsPostMachinesIdTags_WithCaseSensitiveProperties` |
| POST | `/machines/AddOrRemoveTagForMultipleMachines` | `IMachinesApi.AddOrRemoveTagForMultipleMachinesAsync` | `MachinesTests.AddOrRemoveTagForMultipleMachines_SendsPostWithTheMachineIds` |
| POST | `/machines/{machineId}/setDeviceValue` | `IMachinesApi.SetDeviceValueAsync` | `MachinesTests.SetDeviceValue_SendsPostMachinesIdSetDeviceValue` |
| GET | `/machines/{machineId}/vulnerabilities` | `IMachinesApi.ListVulnerabilitiesAsync` | `MachinesTests.ListVulnerabilities_SendsGetMachinesIdVulnerabilities` |
| GET | `/machines/{machineId}/software` | `IMachinesApi.ListSoftwareAsync` | `MachinesTests.ListSoftware_SendsGetMachinesIdSoftware` |
| GET | `/machines/{machineId}/getmissingkbs` | `IMachinesApi.ListMissingKbsAsync` | `MachinesTests.ListMissingKbs_SendsGetMachinesIdGetmissingkbs` |
| GET | `/machines/{machineId}/recommendations` | `IMachinesApi.ListRecommendationsAsync` | `MachinesTests.ListRecommendations_SendsGetMachinesIdRecommendations` |
