# library endpoints

| Method | Path | Client method | Test |
|---|---|---|---|
| GET | `/libraryfiles` | `ILibraryApi.ListAsync` | `LibraryTests.List_SendsGetLibraryFiles` |
| POST | `/libraryfiles` | `ILibraryApi.UploadAsync` | `LibraryTests.Upload_SendsPostLibraryFiles_AsMultipartFormData_WithEveryField` |
| DELETE | `/libraryfiles/{fileName}` | `ILibraryApi.DeleteAsync` | `LibraryTests.Delete_SendsDeleteLibraryFilesFileName` |
