using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CustomerOrderAPI.Controllers.Base
{
    [Route(Routes.BaseUrl)]
    [ApiController]
    public class BaseController : ControllerBase
    {
        private ISender? _mediator;

        protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();
    }
}
