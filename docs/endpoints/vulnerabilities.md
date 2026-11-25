# vulnerabilities endpoints

| Method | Path | Client method | Test |
|---|---|---|---|
| GET | `/vulnerabilities` | `IVulnerabilitiesApi.ListAsync` | `VulnerabilitiesTests.List_SendsGetVulnerabilities_WithNoQuery` |
| GET | `/vulnerabilities/{cveId}` | `IVulnerabilitiesApi.GetAsync` | `VulnerabilitiesTests.Get_SendsGetVulnerabilitiesCveId` |
| GET | `/vulnerabilities/{cveId}/machineReferences` | `IVulnerabilitiesApi.ListMachineReferencesAsync` | `VulnerabilitiesTests.ListMachineReferences_SendsGetVulnerabilitiesCveIdMachineReferences` |
| GET | `/vulnerabilities/machinesVulnerabilities` | `IVulnerabilitiesApi.ListMachineVulnerabilitiesAsync` | `VulnerabilitiesTests.ListMachineVulnerabilities_SendsGetMachinesVulnerabilities_WithTheODataOptions` |
