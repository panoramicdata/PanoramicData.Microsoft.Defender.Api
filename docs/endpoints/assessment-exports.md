# assessment-exports endpoints

| Method | Path | Client method | Test |
|---|---|---|---|
| GET | `/machines/SecureConfigurationsAssessmentByMachine` | `IAssessmentExportsApi.ListSecureConfigurationsByMachineAsync` | `AssessmentExportsTests.SecureConfigurationsByMachine_SendsGetSecureConfigurationsAssessmentByMachine` |
| GET | `/machines/SecureConfigurationsAssessmentExport` | `IAssessmentExportsApi.ExportSecureConfigurationsAsync` | `AssessmentExportsTests.SecureConfigurationsExport_SendsGetSecureConfigurationsAssessmentExport` |
| GET | `/machines/SoftwareInventoryByMachine` | `IAssessmentExportsApi.ListSoftwareInventoryByMachineAsync` | `AssessmentExportsTests.SoftwareInventoryByMachine_SendsGetSoftwareInventoryByMachine` |
| GET | `/machines/SoftwareInventoryExport` | `IAssessmentExportsApi.ExportSoftwareInventoryAsync` | `AssessmentExportsTests.SoftwareInventoryExport_SendsGetSoftwareInventoryExport` |
| GET | `/machines/SoftwareVulnerabilitiesByMachine` | `IAssessmentExportsApi.ListSoftwareVulnerabilitiesByMachineAsync` | `AssessmentExportsTests.SoftwareVulnerabilitiesByMachine_SendsGetSoftwareVulnerabilitiesByMachine` |
| GET | `/machines/SoftwareVulnerabilitiesExport` | `IAssessmentExportsApi.ExportSoftwareVulnerabilitiesAsync` | `AssessmentExportsTests.SoftwareVulnerabilitiesExport_SendsGetSoftwareVulnerabilitiesExport` |
| GET | `/machines/SoftwareVulnerabilityChangesByMachine` | `IAssessmentExportsApi.ListSoftwareVulnerabilityChangesByMachineAsync` | `AssessmentExportsTests.SoftwareVulnerabilityChangesByMachine_SendsTheRequiredSinceTime` |
| GET | `/machines/SoftwareInventoryNoProductCodeByMachine` | `IAssessmentExportsApi.ListNonCpeSoftwareInventoryByMachineAsync` | `AssessmentExportsTests.NonCpeSoftwareInventoryByMachine_SendsGetSoftwareInventoryNoProductCodeByMachine` |
| GET | `/machines/SoftwareInventoryNonCpeExport` | `IAssessmentExportsApi.ExportNonCpeSoftwareInventoryAsync` | `AssessmentExportsTests.NonCpeSoftwareInventoryExport_SendsGetSoftwareInventoryNonCpeExport` |
| GET | `/Machines/BrowserExtensionsInventoryByMachine` | `IAssessmentExportsApi.ListBrowserExtensionsByMachineAsync` | `AssessmentExportsTests.BrowserExtensionsByMachine_SendsGetBrowserExtensionsInventoryByMachine` |
| GET | `/machines/browserextensionsinventoryExport` | `IAssessmentExportsApi.ExportBrowserExtensionsAsync` | `AssessmentExportsTests.BrowserExtensionsExport_SendsGetBrowserExtensionsInventoryExport` |
| GET | `/Machines/InfoGatheringExport` | `IAssessmentExportsApi.ExportInfoGatheringAsync` | `AssessmentExportsTests.InfoGatheringExport_SendsGetInfoGatheringExport` |
| GET | `/machines/certificateAssessmentByMachine` | `IAssessmentExportsApi.ListCertificatesByMachineAsync` | `AssessmentExportsTests.CertificatesByMachine_SendsGetCertificateAssessmentByMachine` |
| GET | `/machines/certificateAssessmentExport` | `IAssessmentExportsApi.ExportCertificatesAsync` | `AssessmentExportsTests.CertificatesExport_SendsGetCertificateAssessmentExport` |
| GET | `/machines/baselineComplianceAssessmentByMachine` | `IAssessmentExportsApi.ListBaselineComplianceByMachineAsync` | `AssessmentExportsTests.BaselineComplianceByMachine_SendsGetBaselineComplianceAssessmentByMachine` |
| GET | `/machines/BaselineComplianceAssessmentExport` | `IAssessmentExportsApi.ExportBaselineComplianceAsync` | `AssessmentExportsTests.BaselineComplianceExport_SendsGetBaselineComplianceAssessmentExport` |
