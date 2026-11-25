# machine-actions endpoints

| Method | Path | Client method | Test |
|---|---|---|---|
| POST | `/machines/{id}/isolate` |  |  |
| POST | `/machines/{id}/unisolate` |  |  |
| POST | `/machines/{id}/restrictCodeExecution` |  |  |
| POST | `/machines/{id}/unrestrictCodeExecution` |  |  |
| POST | `/machines/{id}/runAntiVirusScan` |  |  |
| POST | `/machines/{id}/StopAndQuarantineFile` |  |  |
| POST | `/machines/{id}/collectInvestigationPackage` |  |  |
| POST | `/machines/{id}/offboard` |  |  |
| POST | `/machines/{machine_id}/runliveresponse` |  |  |
| GET | `/machineactions` |  |  |
| GET | `/machineactions/{id}` |  |  |
| POST | `/machineactions/{id}/cancel` |  |  |
| GET | `/machineactions/{id}/getPackageUri` |  |  |
| GET | `/machineactions/{id}/GetLiveResponseResultDownloadLink(index={command-index})` |  |  |
