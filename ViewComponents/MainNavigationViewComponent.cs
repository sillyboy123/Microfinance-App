using Microsoft.AspNetCore.Mvc;

namespace FinPlus.ViewComponents
{
    public class MainNavigationViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
