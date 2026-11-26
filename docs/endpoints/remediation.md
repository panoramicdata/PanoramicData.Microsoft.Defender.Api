# remediation endpoints

| Method | Path | Client method | Test |
|---|---|---|---|
| GET | `/remediationtasks` | `IRemediationApi.ListAsync` | `RemediationTests.List_SendsGetRemediationTasks` |
| GET | `/remediationtasks/{id}` | `IRemediationApi.GetAsync` | `RemediationTests.Get_SendsGetRemediationTasksId` |
| GET | `/remediationtasks/{id}/machineReferences` | `IRemediationApi.ListMachineReferencesAsync` | `RemediationTests.ListMachineReferences_SendsGetRemediationTasksIdMachineReferences` |
