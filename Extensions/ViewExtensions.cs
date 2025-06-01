using Microsoft.AspNetCore.Mvc.Rendering;

namespace MicrofinanceApp.Extensions
{
    public static class ViewExtensions
    {        
        public static string IsSelected(this IHtmlHelper html, string? controller = null, string? action = null)
        {
            string? currentAction = html.ViewContext.RouteData.Values["Action"]?.ToString();
            string? currentController = html.ViewContext.RouteData.Values["Controller"]?.ToString();

            if (string.IsNullOrEmpty(controller))
                controller = currentController ?? string.Empty;

            if (string.IsNullOrEmpty(action))
                action = currentAction ?? string.Empty;

            return controller == currentController ? "active" : "";
        }
    }
}
