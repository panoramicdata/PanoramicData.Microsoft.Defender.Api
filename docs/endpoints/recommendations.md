# recommendations endpoints

| Method | Path | Client method | Test |
|---|---|---|---|
| GET | `/recommendations` | `IRecommendationsApi.ListAsync` | `RecommendationsTests.List_SendsGetRecommendations` |
| GET | `/recommendations/{id}` | `IRecommendationsApi.GetAsync` | `RecommendationsTests.Get_SendsGetRecommendationsId` |
| GET | `/recommendations/{id}/machineReferences` | `IRecommendationsApi.ListMachineReferencesAsync` | `RecommendationsTests.ListMachineReferences_SendsGetRecommendationsIdMachineReferences` |
| GET | `/recommendations/{id}/vulnerabilities` | `IRecommendationsApi.ListVulnerabilitiesAsync` | `RecommendationsTests.ListVulnerabilities_SendsGetRecommendationsIdVulnerabilities` |
| GET | `/recommendations/{id}/software` | `IRecommendationsApi.GetSoftwareAsync` | `RecommendationsTests.GetSoftware_SendsGetRecommendationsIdSoftware` |
