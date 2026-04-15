namespace ProjectPulse.Infrastructure.Services;

public static class EmailTemplateHelper
{
    // Base layout wrapper
    public static string Wrap(string title, string bodyContent) => $"""
        <!DOCTYPE html>
        <html>
        <head>
          <meta charset="UTF-8"/>
          <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
          <title>{title}</title>
        </head>
        <body style="margin:0;padding:0;background:#f4f6f9;font-family:'Segoe UI',Arial,sans-serif;">
          <table width="100%" cellpadding="0" cellspacing="0" style="background:#f4f6f9;padding:40px 0;">
            <tr>
              <td align="center">
                <table width="600" cellpadding="0" cellspacing="0"
                  style="background:#ffffff;border-radius:12px;
                         box-shadow:0 2px 8px rgba(0,0,0,0.08);overflow:hidden;">

                  <!-- Header -->
                  <tr>
                    <td style="background:linear-gradient(135deg,#6366f1,#8b5cf6);
                               padding:32px 40px;text-align:center;">
                      <h1 style="margin:0;color:#ffffff;font-size:26px;font-weight:700;
                                 letter-spacing:-0.5px;">
                        ⚡ ProjectPulse
                      </h1>
                      <p style="margin:6px 0 0;color:rgba(255,255,255,0.8);font-size:14px;">
                        Team Productivity Platform
                      </p>
                    </td>
                  </tr>

                  <!-- Body -->
                  <tr>
                    <td style="padding:40px;">
                      {bodyContent}
                    </td>
                  </tr>

                  <!-- Footer -->
                  <tr>
                    <td style="background:#f8fafc;padding:24px 40px;
                               border-top:1px solid #e2e8f0;text-align:center;">
                      <p style="margin:0;color:#94a3b8;font-size:12px;">
                        © 2026 ProjectPulse. All rights reserved.<br/>
                        You received this email because you are a member of ProjectPulse.
                      </p>
                    </td>
                  </tr>

                </table>
              </td>
            </tr>
          </table>
        </body>
        </html>
    """;

    // Reusable button
    public static string Button(string url, string label, string color = "#6366f1") => $"""
        <div style="text-align:center;margin:28px 0;">
          <a href="{url}"
             style="background:{color};color:#ffffff;padding:14px 32px;
                    border-radius:8px;text-decoration:none;font-weight:600;
                    font-size:15px;display:inline-block;">
            {label}
          </a>
        </div>
    """;

    // Reusable info badge
    public static string Badge(string label, string value, string color = "#6366f1") => $"""
        <div style="display:inline-block;background:{color}15;border:1px solid {color}30;
                    border-radius:6px;padding:6px 14px;margin:4px;">
          <span style="color:#64748b;font-size:12px;">{label}: </span>
          <strong style="color:{color};font-size:13px;">{value}</strong>
        </div>
    """;

    // Priority color helper
    public static string PriorityColor(string priority) => priority switch
    {
        "Critical" => "#ef4444",
        "High" => "#f97316",
        "Medium" => "#eab308",
        "Low" => "#22c55e",
        _ => "#6366f1"
    };

    // Status color helper
    public static string StatusColor(string status) => status switch
    {
        "Done" => "#22c55e",
        "InProgress" => "#6366f1",
        "Review" => "#f97316",
        "Todo" => "#94a3b8",
        _ => "#6366f1"
    };
}