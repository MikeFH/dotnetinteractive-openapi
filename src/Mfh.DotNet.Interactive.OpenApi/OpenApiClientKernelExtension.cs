using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.CSharp;
using NSwag;
using NSwag.CodeGeneration.CSharp;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Microsoft.DotNet.Interactive.Formatting.PocketViewTags;

namespace Mfh.DotNet.Interactive.OpenApi
{
    public class OpenApiClientKernelExtension : IKernelExtension
    {
        public OpenApiClientKernelExtension()
        {

        }

        public Task OnLoadAsync(Kernel kernel)
        {
            Command command = new Command("#!openapi-client", "Generate an api client based on its OpenAPI schema");
            command.AddArgument(new Argument("schema"));

            command.Handler = CommandHandler.Create<string, KernelInvocationContext>(async (schema, invocationContext) => await GenerateClient(schema, invocationContext));

            kernel.AddDirective(command);
                       
            KernelInvocationContext.Current?.Display((object)
                details(
                    summary("Create strongly typed clients for OpenAPI compliant APIs"),
                    p("Use the ", code("#!openapi-client"), " command to create a c# API client"),
                    p(i("Powered by ", a[href: "https://github.com/RicoSuter/NSwag"]("NSwag")))
                )
            );

            return Task.CompletedTask;
        }

        private async Task GenerateClient(string schema, KernelInvocationContext invocationContext)
        {
            CSharpKernel csharpKernel = null;

            invocationContext.HandlingKernel.VisitSubkernelsAndSelf(k =>
            {
                if (k is CSharpKernel csk)
                {
                    csharpKernel = csk;
                }
            });

            if (csharpKernel is null)
            {
                return;
            }

            string clientClassName = "OpenApiClient";

            DisplayedValue dv = invocationContext.Display((object)i("Downloading schema..."), "text/html");

            OpenApiDocument document = await OpenApiDocument.FromUrlAsync(schema);

            dv.Update((object)i("Building client..."));

            CSharpClientGeneratorSettings settings = new CSharpClientGeneratorSettings()
            {
                ClassName = clientClassName,
                ExposeJsonSerializerSettings = true,
                ExceptionClass = clientClassName + "Exception",
                CSharpGeneratorSettings =
                {
                    Namespace = "DummyNamespace"                    
                }
            };

            CSharpClientGenerator generator = new CSharpClientGenerator(document, settings);
            string clientCode = generator.GenerateFile();

            //Remove #pragma diirectives useless in this context and the namespace to make the code usable in a script context
            clientCode = Regex.Replace(clientCode, "^(#pragma|//).*$", "", RegexOptions.Multiline);
            clientCode = clientCode
                .Replace("namespace DummyNamespace", "")
                .Trim()
                .Trim(new[] { '{', '}' });

            //Extend the client functionnality
            clientCode += Environment.NewLine;
            clientCode += $@"public partial class {clientClassName} {{
                private static readonly System.Net.Http.HttpClient DefaultHttpClient = new System.Net.Http.HttpClient();

                public {clientClassName}() 
                    :this(DefaultHttpClient) {{ }}

                partial void ProcessResponse(System.Net.Http.HttpClient client, System.Net.Http.HttpResponseMessage response) {{
                    display(response);
                }}
            }}";

            await csharpKernel.SubmitCodeAsync(@"#r ""System.ComponentModel.DataAnnotations""");
            await csharpKernel.SubmitCodeAsync(clientCode);

            dv.Update((object)
                details(
                    summary(
                        "Client generated for " + schema
                    ),
                    p(
                        "You can intialize the client as shown below",
                        br(),
                        code($"var client = new {clientClassName}();"),
                        br(),
                        "or using an already existing ",
                        code("HttpClient"),
                        br(),
                        code(
                            "var httpClient = new HttpClient();",
                            br(),
                            $"var client = new {clientClassName}(httpClient);"
                        )
                    )
                )
            );
        }
    }
}
