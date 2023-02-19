using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace Hammerfest.Server.ServServ;

[Route("/servserv/cnc3_ep1/")]
public class CncController : Controller
{
    [HttpGet("cnc3ep1_english_1.2.patchinfo")]
    public FileResult GetPatchInfo() => EmptyTextFile();

    [HttpGet("maps-65536.txt")]
    public FileResult GetMaps() => EmptyTextFile();

    [HttpGet("config.txt")] // TODO[#8]: What should we put into the QMMaps element?
    public FileResult GetConfig() => TextFileResult("""
    <QMMaps>
      Barstow Badlands
    </QMMaps>

    <Manglers>
      mangler1.generals.ea.com:4321
      mangler2.generals.ea.com:4321
      mangler3.generals.ea.com:4321
      mangler4.generals.ea.com:4321
    </Manglers>

    <PingServers>
      211.233.72.38
    </PingServers>

    <PingDuration>
      reps = 1
      timeout = 1000
      low = 100
      med = 400
    </PingDuration>

    <Ladders>
    </Ladders>

    <VIP>
      19576023
      21525197
    </VIP>

    <Custom>
      restricted = 1
    </Custom>
    """.ReplaceLineEndings("\r\n"));

    [HttpGet("drivers.txt")]
    public FileResult GetDrivers() => EmptyTextFile();

    [HttpGet("MOTD-english.txt")]
    public FileResult GetMotd() => TextFileResult("Welcome to Hammerfest!");

    private static FileResult EmptyTextFile() => TextFileResult("");

    private static FileResult TextFileResult(string content) =>
        new FileContentResult(Encoding.UTF8.GetBytes(content), MediaTypeNames.Text.Plain);
}
