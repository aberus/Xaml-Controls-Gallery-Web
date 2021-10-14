using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Mustache;
using System.Text.RegularExpressions;

namespace XamlControlsGalleryWeb
{
    public static class RedirectFunction
    {
        [FunctionName("Redirect")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Redirect/{category?}/{targetId?}")] HttpRequest req,
            ExecutionContext context,
            ILogger log)
        {
            try
            {
                string url = req.Path;
                string regexPattern = @"\/api\/redirect\/(?<category>\w*)\/(?<targetId>\w*)";

                var re = new Regex(regexPattern, RegexOptions.IgnoreCase);
                var match = re.Match(url);

                string category = match.Groups["category"].Value;
                string targetId = match.Groups["targetId"].Value;

                log.LogInformation($"Function called with {category}/{targetId}");

                string path = Path.Combine(context.FunctionAppDirectory, "index.html");
                string source = File.ReadAllText(path);

                var compiler = new FormatCompiler();
                var generator = compiler.Compile(source);

                string galleryUrl = null;
                if(string.IsNullOrWhiteSpace(category) && string.IsNullOrWhiteSpace(targetId))
                    galleryUrl = "xamlcontrolsgallery:";
                else
                    galleryUrl = $"xamlcontrolsgallery://{category}/{targetId}";

                var data = new
                {
                    url = galleryUrl
                };

                string content = generator.Render(data);

                return new ContentResult
                {
                     Content = req.PathBase + content,
                    ContentType = "text/html"
                };
            }
            catch (Exception)
            {
#if DEBUG
                throw;
#else
                return new BadRequestObjectResult("Please pass a name on the query string");
#endif
            }

        }
    }
}
