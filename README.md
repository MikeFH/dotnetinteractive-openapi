# OpenApi client generator for .NET Interactive

Quickly generate a C# client for an OpenApi compliant API for use in your .NET Interactive notebooks.

Powered by <a href="https://github.com/RicoSuter/NSwag">NSwag</a>

## Examples
```csharp
#r "nuget:MhfSoft.DotNet.Interactive.OpenApi"
#!openapi-client "https://petstore.swagger.io/v2/swagger.json"

var client = new OpenApiClient();
var response = await client.StoreInventoryAsync();
```

HTTP requests/responses can be traced to inspect them :
```csharp
#!openapi-client "https://petstore.swagger.io/v2/swagger.json" --enable-tracing
```
Provide your own ``System.Net.Http.HttpClient`` to the constructor if you need custom behavior
```csharp
using System.Net.Http;
using System.Net.Http.Headers;

var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("apikey", "xxxxx");

var client = new OpenApiClient(httpClient);
var r = await client.StoreInventoryAsync();
r
```

## Documentation
```
#!openapi-client
  Generate an api client based on its OpenAPI schema

Usage:
  [options] #!openapi-client <schema>

Arguments:
  <schema>

Options:
  -c, --class-name <class-name>          Name of the generated client class name [default: OpenApiClient]
  --method-name-type <OperationId|Path>  Defines how method names are generated (based on path or operation name) [default: Path]
  -t, --enable-tracing                   Enabled tracing of HTTP requests/responses [default: False]
  -?, -h, --help                         Show help and usage information
```