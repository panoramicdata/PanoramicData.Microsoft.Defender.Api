# domains endpoints

| Method | Path | Client method | Test |
|---|---|---|---|
| GET | `/domains/{domain}/alerts` | `IDomainsApi.ListAlertsAsync` | `DomainsTests.ListAlerts_SendsGetDomainsDomainAlerts` |
| GET | `/domains/{domain}/machines` | `IDomainsApi.ListMachinesAsync` | `DomainsTests.ListMachines_SendsGetDomainsDomainMachines` |
| GET | `/domains/{domain}/stats` | `IDomainsApi.GetStatisticsAsync` | `DomainsTests.GetStatistics_SendsGetDomainsDomainStats_WithTheLookBackHours` |
