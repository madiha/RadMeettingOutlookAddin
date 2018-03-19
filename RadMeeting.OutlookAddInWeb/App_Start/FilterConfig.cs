using System.Web;
using System.Web.Mvc;

namespace RedMeeting.OutlookAddInWeb
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
