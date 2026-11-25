# machines endpoints

| Method | Path | Client method | Test |
|---|---|---|---|
| GET | `/machines` |  |  |
| GET | `/machines/{id}` |  |  |
| PATCH | `/machines/{machineId}` |  |  |
| GET | `/machines/{id}/logonusers` |  |  |
| GET | `/machines/{id}/alerts` |  |  |
| GET | `/machines/find(timestamp={time},key={IP})` |  |  |
| GET | `/machines/findbyip(ip='{IP}',timestamp={TimeStamp})` |  |  |
| GET | `/machines/findbytag` |  |  |
| POST | `/machines/{id}/tags` |  |  |
| POST | `/machines/AddOrRemoveTagForMultipleMachines` |  |  |
| POST | `/machines/{machineId}/setDeviceValue` |  |  |
| GET | `/machines/{machineId}/vulnerabilities` |  |  |
| GET | `/machines/{machineId}/software` |  |  |
| GET | `/machines/{machineId}/getmissingkbs` |  |  |
| GET | `/machines/{machineId}/recommendations` |  |  |
