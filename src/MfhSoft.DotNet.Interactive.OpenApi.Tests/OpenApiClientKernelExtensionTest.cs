using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.CSharp;
using FluentAssertions;
using Xunit;
using Microsoft.DotNet.Interactive.Events;
using System.Threading.Tasks;
using System.IO;

namespace MfhSoft.DotNet.Interactive.OpenApi.Tests
{
    public class OpenApiClientKernelExtensionTest
    {
        private const string RemoteYamlSchemaUrl = @"https://petstore.swagger.io/v2/swagger.yaml";
        private const string RemoteJsonSchemaUrl = @"https://petstore.swagger.io/v2/swagger.json";

        private static readonly string LocalYamlSchemaPath = Path.Combine("schemas", "petstore.yaml");
        private static readonly string LocalJsonSchemaPath = Path.Combine("schemas", "petstore.json");

        private async Task<CSharpKernel> CreateKernel()
        {
            CSharpKernel kernel = new CSharpKernel().UseNugetDirective();
            //using the local Newtonsoft.Json because package restoration doesn't seem to work in this context
            //https://github.com/dotnet/interactive/issues/1055
            await kernel.SubmitCodeAsync(@"#r ""Newtonsoft.Json.dll""");

            OpenApiClientKernelExtension extension = new OpenApiClientKernelExtension();
            await extension.OnLoadAsync(kernel);

            return kernel;
        }

        [Fact]
        public async Task CanLoadLocalYamlSchema()
        {
            CSharpKernel kernel = await CreateKernel();

            KernelCommandResult result = await kernel.SubmitCodeAsync($@"#!openapi-client ""{LocalYamlSchemaPath}""");

            result.KernelEvents
                .ToSubscribedList()
                .Should()
                .NotContainErrors();
        }

        [Fact]
        public async Task CanLoadLocalJsonSchema()
        {
            CSharpKernel kernel = await CreateKernel();

            KernelCommandResult result = await kernel.SubmitCodeAsync($@"#!openapi-client ""{LocalJsonSchemaPath}""");

            result.KernelEvents
                .ToSubscribedList()
                .Should()
                .NotContainErrors();
        }

        [Fact]
        public async Task CanLoadRemoteYamlSchema()
        {
            CSharpKernel kernel = await CreateKernel();

            KernelCommandResult result = await kernel.SubmitCodeAsync($@"#!openapi-client ""{RemoteYamlSchemaUrl}""");

            result.KernelEvents
                .ToSubscribedList()
                .Should()
                .NotContainErrors();
        }

        [Fact]
        public async Task CanLoadRemoteJsonSchema()
        {
            CSharpKernel kernel = await CreateKernel();

            KernelCommandResult result = await kernel.SubmitCodeAsync($@"#!openapi-client ""{RemoteJsonSchemaUrl}""");

            result.KernelEvents
                .ToSubscribedList()
                .Should()
                .NotContainErrors();
        }

        [Fact]
        public async Task GeneratesAClassWithTwoConstructors()
        {
            CSharpKernel kernel = await CreateKernel();

            KernelCommandResult result = await kernel.SubmitCodeAsync($@"#!openapi-client ""{LocalJsonSchemaPath}""");

            result.KernelEvents
                .ToSubscribedList()
                .Should()
                .NotContainErrors();

            KernelCommandResult result2 = await kernel.SubmitCodeAsync("new OpenApiClient()");

            result2.KernelEvents
                .ToSubscribedList()
                .Should()
                .NotContainErrors();

            KernelCommandResult result3 = await kernel.SubmitCodeAsync("new OpenApiClient(new System.Net.Http.HttpClient())");

            result3.KernelEvents
                .ToSubscribedList()
                .Should()
                .NotContainErrors();
        }

        [Fact]
        public async Task CanCustomizeClassName()
        {
            CSharpKernel kernel = await CreateKernel();

            KernelCommandResult result = await kernel.SubmitCodeAsync($@"#!openapi-client ""{LocalJsonSchemaPath}"" -c ""CustomName""");

            result.KernelEvents
                .ToSubscribedList()
                .Should()
                .NotContainErrors();

            KernelCommandResult result2 = await kernel.SubmitCodeAsync("new CustomName()");

            result2.KernelEvents
                .ToSubscribedList()
                .Should()
                .NotContainErrors();
        }

        [Fact]
        public async Task HasMethodNameTypePath()
        {
            CSharpKernel kernel = await CreateKernel();

            KernelCommandResult result = await kernel.SubmitCodeAsync($@"#!openapi-client ""{LocalJsonSchemaPath}""");

            result.KernelEvents
                .ToSubscribedList()
                .Should()
                .NotContainErrors();

            KernelCommandResult result2 = await kernel.SubmitCodeAsync(@"typeof(OpenApiClient).GetMethods().Any(mi => mi.Name == ""PetGetAsync"")");

            result2.KernelEvents
                .ToSubscribedList()
                .Should()
                .NotContainErrors()
                .And
                .ContainSingle<ReturnValueProduced>()
                .Which
                .Value
                .As<bool>()
                .Should()
                .Be(true);
        }

        [Fact]
        public async Task HasMethodNameTypeOperationId()
        {
            CSharpKernel kernel = await CreateKernel();

            KernelCommandResult result = await kernel.SubmitCodeAsync($@"#!openapi-client ""{LocalJsonSchemaPath}"" --method-name-type operationid");

            result.KernelEvents
                .ToSubscribedList()
                .Should()
                .NotContainErrors();

            KernelCommandResult result2 = await kernel.SubmitCodeAsync(@"typeof(OpenApiClient).GetMethods().Any(mi => mi.Name == ""GetPetByIdAsync"")");

            result2.KernelEvents
                .ToSubscribedList()
                .Should()
                .NotContainErrors()
                .And
                .ContainSingle<ReturnValueProduced>()
                .Which
                .Value
                .As<bool>()
                .Should()
                .Be(true);
        }

        [Fact]
        public async Task ModelHasToJsonMethod()
        {
            CSharpKernel kernel = await CreateKernel();

            KernelCommandResult result = await kernel.SubmitCodeAsync($@"#!openapi-client ""{LocalJsonSchemaPath}""");

            result.KernelEvents
                .ToSubscribedList()
                .Should()
                .NotContainErrors();

            KernelCommandResult result2 = await kernel.SubmitCodeAsync(@"typeof(Pet).GetMethods().Any(mi => mi.Name == ""ToJson"")");

            result2.KernelEvents
                .ToSubscribedList()
                .Should()
                .NotContainErrors()
                .And
                .ContainSingle<ReturnValueProduced>()
                .Which
                .Value
                .As<bool>()
                .Should()
                .Be(true);
        }
    }
}
