# scores endpoints

| Method | Path | Client method | Test |
|---|---|---|---|
| GET | `/exposureScore` | `IScoresApi.GetExposureScoreAsync` | `ScoresTests.GetExposureScore_SendsGetExposureScore` |
| GET | `/exposureScore/ByMachineGroups` | `IScoresApi.ListExposureScoresByMachineGroupsAsync` | `ScoresTests.ListExposureScoresByMachineGroups_SendsGetExposureScoreByMachineGroups` |
| GET | `/configurationScore` | `IScoresApi.GetConfigurationScoreAsync` | `ScoresTests.GetConfigurationScore_SendsGetConfigurationScore` |
