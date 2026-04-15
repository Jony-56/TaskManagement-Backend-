using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using ProjectPulse.Core.Interfaces;

namespace ProjectPulse.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config) => _config = config;

    // ─── Base Send ────────────────────────────────────────────
    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        var settings = _config.GetSection("EmailSettings");
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(settings["SenderName"], settings["SenderEmail"]));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(settings["Host"],
            int.Parse(settings["Port"]!), SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(settings["SenderEmail"], settings["Password"]);
        await smtp.SendAsync(message);
        await smtp.DisconnectAsync(true);
    }

    // ─── Auth Emails ──────────────────────────────────────────
    public async Task SendEmailVerificationAsync(
        string toEmail, string userName, string verificationLink)
    {
        var body = EmailTemplateHelper.Wrap("Verify Your Email", $"""
            <h2 style="color:#1e293b;margin:0 0 8px;">Welcome aboard, {userName}! 👋</h2>
            <p style="color:#64748b;font-size:15px;line-height:1.6;margin:0 0 24px;">
              Thanks for joining ProjectPulse. Please verify your email address
              to activate your account and start collaborating with your team.
            </p>
            {EmailTemplateHelper.Button(verificationLink, "✓ Verify Email Address")}
            <p style="color:#94a3b8;font-size:13px;text-align:center;margin:0;">
              This link expires in <strong>24 hours</strong>.
              If you didn't register, you can safely ignore this email.
            </p>
        """);
        await SendEmailAsync(toEmail, "Verify your ProjectPulse account", body);
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string userName)
    {
        var appUrl = _config["AppUrl"];
        var body = EmailTemplateHelper.Wrap("Welcome to ProjectPulse!", $"""
            <div style="text-align:center;margin-bottom:28px;">
              <div style="font-size:48px;">🎉</div>
              <h2 style="color:#1e293b;margin:12px 0 8px;">You're all set, {userName}!</h2>
              <p style="color:#64748b;font-size:15px;margin:0;">
                Your email has been verified. Welcome to ProjectPulse!
              </p>
            </div>
            <div style="background:#f8fafc;border-radius:8px;padding:20px;margin-bottom:24px;">
              <h3 style="color:#1e293b;margin:0 0 12px;font-size:15px;">
                🚀 Get started in 3 steps:
              </h3>
              <p style="color:#64748b;margin:6px 0;font-size:14px;">
                1️⃣ &nbsp; Create your first project
              </p>
              <p style="color:#64748b;margin:6px 0;font-size:14px;">
                2️⃣ &nbsp; Invite your team members
              </p>
              <p style="color:#64748b;margin:6px 0;font-size:14px;">
                3️⃣ &nbsp; Start creating and assigning tasks
              </p>
            </div>
            {EmailTemplateHelper.Button($"{appUrl}/login", "🚀 Go to ProjectPulse", "#22c55e")}
        """);
        await SendEmailAsync(toEmail, "🎉 Welcome to ProjectPulse!", body);
    }

    public async Task SendPasswordChangedEmailAsync(string toEmail, string userName)
    {
        var body = EmailTemplateHelper.Wrap("Password Changed", $"""
            <div style="text-align:center;margin-bottom:24px;">
              <div style="font-size:48px;">🔐</div>
              <h2 style="color:#1e293b;margin:12px 0 8px;">Password Changed</h2>
            </div>
            <p style="color:#64748b;font-size:15px;line-height:1.6;">
              Hi <strong>{userName}</strong>, your ProjectPulse password was
              successfully changed.
            </p>
            <div style="background:#fef3c7;border:1px solid #fcd34d;border-radius:8px;
                        padding:16px;margin:20px 0;">
              <p style="color:#92400e;margin:0;font-size:14px;">
                ⚠️ If you did not make this change, please contact support immediately
                and secure your account.
              </p>
            </div>
        """);
        await SendEmailAsync(toEmail, "🔐 Your password has been changed", body);
    }

    // ─── Project Emails ───────────────────────────────────────
    public async Task SendProjectInviteEmailAsync(
        string toEmail, string userName,
        string projectName, string inviterName)
    {
        var appUrl = _config["AppUrl"];
        var body = EmailTemplateHelper.Wrap("Project Invitation", $"""
            <h2 style="color:#1e293b;margin:0 0 8px;">You've been added to a project! 📁</h2>
            <p style="color:#64748b;font-size:15px;line-height:1.6;margin:0 0 20px;">
              Hi <strong>{userName}</strong>,
              <strong>{inviterName}</strong> has added you to the project:
            </p>
            <div style="background:#f0f9ff;border:1px solid #bae6fd;border-radius:8px;
                        padding:20px;text-align:center;margin-bottom:24px;">
              <h3 style="color:#0369a1;margin:0;font-size:20px;">📁 {projectName}</h3>
            </div>
            <p style="color:#64748b;font-size:14px;line-height:1.6;">
              You can now view project tasks, collaborate with your team,
              and chat in real-time with other members.
            </p>
            {EmailTemplateHelper.Button($"{appUrl}/projects", "View Project →")}
        """);
        await SendEmailAsync(toEmail, $"📁 You've been added to \"{projectName}\"", body);
    }

    public async Task SendProjectRemovedEmailAsync(
        string toEmail, string userName, string projectName)
    {
        var body = EmailTemplateHelper.Wrap("Removed from Project", $"""
            <h2 style="color:#1e293b;margin:0 0 8px;">Project Membership Update</h2>
            <p style="color:#64748b;font-size:15px;line-height:1.6;">
              Hi <strong>{userName}</strong>, you have been removed from the project:
            </p>
            <div style="background:#fef2f2;border:1px solid #fecaca;border-radius:8px;
                        padding:20px;text-align:center;margin:20px 0;">
              <h3 style="color:#dc2626;margin:0;font-size:20px;">📁 {projectName}</h3>
            </div>
            <p style="color:#94a3b8;font-size:13px;">
              If you think this was a mistake, please contact your project manager.
            </p>
        """);
        await SendEmailAsync(toEmail, $"Removed from project \"{projectName}\"", body);
    }

    // ─── Task Emails ──────────────────────────────────────────
    public async Task SendTaskAssignedEmailAsync(
        string toEmail, string userName, string taskTitle,
        string projectName, string priority, DateTime? dueDate)
    {
        var appUrl = _config["AppUrl"];
        var priorityColor = EmailTemplateHelper.PriorityColor(priority);
        var dueDateStr = dueDate.HasValue
            ? dueDate.Value.ToString("MMM dd, yyyy")
            : "No due date";

        var body = EmailTemplateHelper.Wrap("Task Assigned", $"""
            <h2 style="color:#1e293b;margin:0 0 8px;">New Task Assigned ✅</h2>
            <p style="color:#64748b;font-size:15px;line-height:1.6;margin:0 0 20px;">
              Hi <strong>{userName}</strong>, a new task has been assigned to you:
            </p>
            <div style="background:#f8fafc;border-left:4px solid {priorityColor};
                        border-radius:0 8px 8px 0;padding:20px;margin-bottom:20px;">
              <h3 style="color:#1e293b;margin:0 0 12px;font-size:17px;">
                {taskTitle}
              </h3>
              <div>
                {EmailTemplateHelper.Badge("Project", projectName)}
                {EmailTemplateHelper.Badge("Priority", priority, priorityColor)}
                {EmailTemplateHelper.Badge("Due Date", dueDateStr, "#6366f1")}
              </div>
            </div>
            {EmailTemplateHelper.Button($"{appUrl}/projects", "View Task →")}
        """);
        await SendEmailAsync(toEmail, $"✅ New task assigned: \"{taskTitle}\"", body);
    }

    public async Task SendTaskStatusChangedEmailAsync(
        string toEmail, string userName, string taskTitle,
        string projectName, string oldStatus, string newStatus)
    {
        var oldColor = EmailTemplateHelper.StatusColor(oldStatus);
        var newColor = EmailTemplateHelper.StatusColor(newStatus);
        var appUrl = _config["AppUrl"];

        var body = EmailTemplateHelper.Wrap("Task Status Updated", $"""
            <h2 style="color:#1e293b;margin:0 0 8px;">Task Status Updated 🔄</h2>
            <p style="color:#64748b;font-size:15px;line-height:1.6;margin:0 0 20px;">
              Hi <strong>{userName}</strong>, a task in
              <strong>{projectName}</strong> has been updated:
            </p>
            <div style="background:#f8fafc;border-radius:8px;padding:20px;margin-bottom:20px;">
              <h3 style="color:#1e293b;margin:0 0 16px;font-size:16px;">
                {taskTitle}
              </h3>
              <div style="display:flex;align-items:center;gap:12px;">
                {EmailTemplateHelper.Badge("From", oldStatus, oldColor)}
                <span style="color:#94a3b8;font-size:18px;">→</span>
                {EmailTemplateHelper.Badge("To", newStatus, newColor)}
              </div>
            </div>
            {EmailTemplateHelper.Button($"{appUrl}/projects", "View Task →")}
        """);
        await SendEmailAsync(toEmail, $"🔄 Task status updated: \"{taskTitle}\"", body);
    }

    public async Task SendTaskCommentEmailAsync(
        string toEmail, string userName, string taskTitle,
        string projectName, string commenterName, string comment)
    {
        var appUrl = _config["AppUrl"];
        var shortComment = comment.Length > 200
            ? comment[..200] + "..." : comment;

        var body = EmailTemplateHelper.Wrap("New Comment on Task", $"""
            <h2 style="color:#1e293b;margin:0 0 8px;">New Comment 💬</h2>
            <p style="color:#64748b;font-size:15px;line-height:1.6;margin:0 0 20px;">
              Hi <strong>{userName}</strong>,
              <strong>{commenterName}</strong> commented on a task
              in <strong>{projectName}</strong>:
            </p>
            <div style="background:#f8fafc;border-radius:8px;padding:20px;margin-bottom:16px;">
              <p style="color:#6366f1;font-size:13px;font-weight:600;
                        margin:0 0 8px;text-transform:uppercase;letter-spacing:0.5px;">
                {taskTitle}
              </p>
              <div style="background:#ffffff;border:1px solid #e2e8f0;border-radius:6px;
                          padding:16px;font-size:14px;color:#374151;line-height:1.6;">
                "{shortComment}"
              </div>
            </div>
            {EmailTemplateHelper.Button($"{appUrl}/projects", "Reply to Comment →")}
        """);
        await SendEmailAsync(toEmail,
            $"💬 New comment on \"{taskTitle}\" by {commenterName}", body);
    }
}