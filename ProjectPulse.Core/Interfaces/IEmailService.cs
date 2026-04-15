namespace ProjectPulse.Core.Interfaces;

public interface IEmailService
{
    // Base
    Task SendEmailAsync(string toEmail, string subject, string htmlBody);

    // Auth emails
    Task SendEmailVerificationAsync(string toEmail, string userName, string verificationLink);
    Task SendWelcomeEmailAsync(string toEmail, string userName);
    Task SendPasswordChangedEmailAsync(string toEmail, string userName);

    // Project emails
    Task SendProjectInviteEmailAsync(string toEmail, string userName,
        string projectName, string inviterName);
    Task SendProjectRemovedEmailAsync(string toEmail, string userName, string projectName);

    // Task emails
    Task SendTaskAssignedEmailAsync(string toEmail, string userName,
        string taskTitle, string projectName, string priority, DateTime? dueDate);
    Task SendTaskStatusChangedEmailAsync(string toEmail, string userName,
        string taskTitle, string projectName, string oldStatus, string newStatus);
    Task SendTaskCommentEmailAsync(string toEmail, string userName,
        string taskTitle, string projectName, string commenterName, string comment);
}