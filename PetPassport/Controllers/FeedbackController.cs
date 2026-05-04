using Microsoft.AspNetCore.Mvc;
using PetPassport.Services;

namespace PetPassport.Controllers;

public class FeedbackRequest
{
    public string Type { get; set; } = "";
    public Dictionary<string, string> Fields { get; set; } = new();
}

[ApiController]
[Route("api/feedback")]
public class FeedbackController : ControllerBase
{
    private readonly IEmailService _emailService;
    private readonly ILogger<FeedbackController> _logger;

    public FeedbackController(IEmailService emailService, ILogger<FeedbackController> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> SendFeedback([FromBody] FeedbackRequest request)
    {
        try
        {
            await _emailService.SendFeedbackAsync(request.Type, request.Fields);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send feedback email");
            return StatusCode(500, "Не удалось отправить сообщение");
        }
    }
}
