# users endpoints

| Method | Path | Client method | Test |
|---|---|---|---|
| GET | `/users/{id}/alerts` | `IUsersApi.ListAlertsAsync` | `UsersTests.ListAlerts_SendsGetUsersNameAlerts` |
| GET | `/users/{id}/machines` | `IUsersApi.ListMachinesAsync` | `UsersTests.ListMachines_SendsGetUsersNameMachines` |
