namespace PetPassport.Services;

public interface IEmailService
{
    Task SendFeedbackAsync(string type, Dictionary<string, string> fields);
}
