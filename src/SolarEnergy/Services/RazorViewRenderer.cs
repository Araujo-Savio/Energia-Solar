using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace SolarEnergy.Services
{
    public interface IRazorViewRenderer
    {
        Task<string> RenderViewToStringAsync(string viewPath, object model);
    }

    public class RazorViewRenderer : IRazorViewRenderer
    {
        private readonly IRazorViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;

        public RazorViewRenderer(IRazorViewEngine viewEngine, ITempDataProvider tempDataProvider, IServiceProvider serviceProvider)
        {
            _viewEngine = viewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
        }

        public async Task<string> RenderViewToStringAsync(string viewPath, object model)
        {
            var httpContext = new DefaultHttpContext { RequestServices = _serviceProvider };
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            await using var sw = new StringWriter();
            var viewResult = _viewEngine.GetView(executingFilePath: null, viewPath: viewPath, isMainPage: true);
            if (!viewResult.Success)
            {
                viewResult = _viewEngine.FindView(actionContext, viewPath, isMainPage: true);
            }

            var view = viewResult.View ?? throw new InvalidOperationException($"View '{viewPath}' não encontrada.");

            var viewDataDictionary = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = model
            };

            var tempDataDictionary = new TempDataDictionary(actionContext.HttpContext, _tempDataProvider);

            var viewContext = new ViewContext(
                actionContext,
                view,
                viewDataDictionary,
                tempDataDictionary,
                sw,
                new HtmlHelperOptions());

            await view.RenderAsync(viewContext);
            return sw.ToString();
        }
    }
}
