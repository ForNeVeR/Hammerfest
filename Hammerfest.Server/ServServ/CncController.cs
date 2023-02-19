using Microsoft.AspNetCore.Mvc;

namespace Hammerfest.Server.ServServ;

[Route("/servserv/cnc3_ep1/")]
public class CncController : Controller
{
    [HttpGet("cnc3ep1_english_1.2.patchinfo")]
    public FileResult GetPatchInfo() => new FileContentResult(Array.Empty<byte>(), "text/plain");
}
