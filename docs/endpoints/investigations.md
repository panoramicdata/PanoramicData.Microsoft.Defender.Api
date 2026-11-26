# investigations endpoints

| Method | Path | Client method | Test |
|---|---|---|---|
| GET | `/investigations` | `IInvestigationsApi.ListAsync` | `InvestigationsTests.List_SendsGetInvestigations_WithTheODataOptions` |
| GET | `/investigations/{id}` | `IInvestigationsApi.GetAsync` | `InvestigationsTests.Get_SendsGetInvestigationsId` |
| POST | `/machines/{id}/startInvestigation` | `IInvestigationsApi.StartAsync` | `InvestigationsTests.Start_SendsPostMachinesIdStartInvestigation_WithACapitalCommentProperty` |
