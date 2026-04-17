using Microsoft.AspNetCore.Mvc;

class GameController : Controller         // klass som ärver från Controller, vilket gör att den kan hantera HTTP-förfrågningar
{
  [HttpGet("/api/gamesession")]            // en attribut som anger att denna metod ska hantera GET-förfrågningar till "/api/game"
  public IActionResult Get()       // en metod som returnerar en IActionResult, vilket är ett generellt sätt att representera HTTP-responsen
  {
    return Ok("Hello, World!");    // returnerar en HTTP 200 OK-respons med texten "Hello, World!" som innehåll.
  }
}