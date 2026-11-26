# security-baselines endpoints

| Method | Path | Client method | Test |
|---|---|---|---|
| GET | `/baselineConfigurations` | `ISecurityBaselinesApi.ListConfigurationsAsync` | `SecurityBaselinesTests.ListConfigurations_SendsGetBaselineConfigurations` |
| GET | `/baselineProfiles` | `ISecurityBaselinesApi.ListProfilesAsync` | `SecurityBaselinesTests.ListProfiles_SendsGetBaselineProfiles` |
