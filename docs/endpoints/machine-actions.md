# machine-actions endpoints

| Method | Path | Client method | Test |
|---|---|---|---|
| POST | `/machines/{id}/isolate` | `IMachineActionsApi.IsolateAsync` | `MachineActionsTests.Isolate_SendsPostMachinesIdIsolate_WithTheCommentAndType` |
| POST | `/machines/{id}/unisolate` | `IMachineActionsApi.UnisolateAsync` | `MachineActionsTests.Unisolate_SendsPostMachinesIdUnisolate` |
| POST | `/machines/{id}/restrictCodeExecution` | `IMachineActionsApi.RestrictCodeExecutionAsync` | `MachineActionsTests.RestrictCodeExecution_SendsPostMachinesIdRestrictCodeExecution` |
| POST | `/machines/{id}/unrestrictCodeExecution` | `IMachineActionsApi.UnrestrictCodeExecutionAsync` | `MachineActionsTests.UnrestrictCodeExecution_SendsPostMachinesIdUnrestrictCodeExecution` |
| POST | `/machines/{id}/runAntiVirusScan` | `IMachineActionsApi.RunAntiVirusScanAsync` | `MachineActionsTests.RunAntiVirusScan_SendsPostMachinesIdRunAntiVirusScan_WithTheScanType` |
| POST | `/machines/{id}/StopAndQuarantineFile` | `IMachineActionsApi.StopAndQuarantineFileAsync` | `MachineActionsTests.StopAndQuarantineFile_SendsPostMachinesIdStopAndQuarantineFile` |
| POST | `/machines/{id}/collectInvestigationPackage` | `IMachineActionsApi.CollectInvestigationPackageAsync` | `MachineActionsTests.CollectInvestigationPackage_SendsPostMachinesIdCollectInvestigationPackage` |
| POST | `/machines/{id}/offboard` | `IMachineActionsApi.OffboardAsync` | `MachineActionsTests.Offboard_SendsPostMachinesIdOffboard` |
| POST | `/machines/{machine_id}/runliveresponse` | `IMachineActionsApi.RunLiveResponseAsync` | `MachineActionsTests.RunLiveResponse_SendsPostMachinesIdRunliveresponse_WithTheCommands` |
| GET | `/machineactions` | `IMachineActionsApi.ListAsync` | `MachineActionsTests.List_SendsGetMachineactions_WithTheODataOptions` |
| GET | `/machineactions/{id}` | `IMachineActionsApi.GetAsync` | `MachineActionsTests.Get_SendsGetMachineactionsId` |
| POST | `/machineactions/{id}/cancel` | `IMachineActionsApi.CancelAsync` | `MachineActionsTests.Cancel_SendsPostMachineactionsIdCancel_WithTheComment` |
| GET | `/machineactions/{id}/getPackageUri` | `IMachineActionsApi.GetPackageUriAsync` | `MachineActionsTests.GetPackageUri_SendsGetMachineactionsIdGetPackageUri` |
| GET | `/machineactions/{id}/GetLiveResponseResultDownloadLink(index={command-index})` | `IMachineActionsApi.GetLiveResponseResultDownloadLinkAsync` | `MachineActionsTests.GetLiveResponseResultDownloadLink_SendsTheIndexInTheParenthesisedPath` |
