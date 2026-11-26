# ips endpoints

| Method | Path | Client method | Test |
|---|---|---|---|
| GET | `/ips/{ip}/alerts` | `IIpsApi.ListAlertsAsync` | `IpsTests.ListAlerts_SendsGetIpsIpAlerts` |
| GET | `/ips/{ip}/stats` | `IIpsApi.GetStatisticsAsync` | `IpsTests.GetStatistics_SendsGetIpsIpStats_WithTheLookBackHours` |
