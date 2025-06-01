using Microsoft.AspNetCore.Mvc;

namespace MicrofinanceApp.ViewComponents
{
    public class MainNavigationViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
