# software endpoints

| Method | Path | Client method | Test |
|---|---|---|---|
| GET | `/Software` | `ISoftwareApi.ListAsync` | `SoftwareTests.List_SendsGetSoftware` |
| GET | `/Software/{Id}` | `ISoftwareApi.GetAsync` | `SoftwareTests.Get_SendsGetSoftwareId` |
| GET | `/Software/{Id}/distributions` | `ISoftwareApi.ListDistributionsAsync` | `SoftwareTests.ListDistributions_SendsGetSoftwareIdDistributions` |
| GET | `/Software/{Id}/machineReferences` | `ISoftwareApi.ListMachineReferencesAsync` | `SoftwareTests.ListMachineReferences_SendsGetSoftwareIdMachineReferences` |
| GET | `/Software/{Id}/vulnerabilities` | `ISoftwareApi.ListVulnerabilitiesAsync` | `SoftwareTests.ListVulnerabilities_SendsGetSoftwareIdVulnerabilities` |
| GET | `/Software/{Id}/getmissingkbs` | `ISoftwareApi.ListMissingKbsAsync` | `SoftwareTests.ListMissingKbs_SendsGetSoftwareIdGetMissingKbs` |
