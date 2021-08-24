using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using System.Threading.Tasks;

namespace net5backendwithidentityandADandGroups.Controllers
.Controllers
{
    public class UserProfileController : Controller
    {
        private readonly GraphServiceClient graphServiceClient;

        public UserProfileController(GraphServiceClient graphServiceClient)
        {
            this.graphServiceClient = graphServiceClient;
        }

        [Authorize(Policy = "GroupAdmin")]
        [AuthorizeForScopes(Scopes = new[] { "User.Read" })]
        public async Task<IActionResult> Index()
        {
            User me = await graphServiceClient.Me.Request().GetAsync();
            ViewData["Me"] = me;

            return View();
        }
    }
}