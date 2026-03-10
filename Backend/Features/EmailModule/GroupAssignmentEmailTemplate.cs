using MimeKit;

namespace PureTCOWebApp.Features.EmailModule;

public static class GroupAssignmentEmailTemplate
{
    public static MimeMessage Build(
        GroupAssignmentEmailData data,
        ParticipantEmailInfo recipient,
        string senderEmail,
        string senderName)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(senderName, senderEmail));
        message.To.Add(new MailboxAddress(recipient.FullName, recipient.Email));

        message.Subject = $"[SIGMA] New Assignment: {data.AssignmentTitle}";
        message.Body = new TextPart("html") { Text = BuildHtml(data, recipient.FullName) };

        return message;
    }

    private static string BuildHtml(GroupAssignmentEmailData data, string recipientName) => $"""
        <!DOCTYPE html>
        <html>
        <head>
          <meta charset="UTF-8" />
          <meta name="viewport" content="width=device-width, initial-scale=1.0" />
        </head>
        <body style="margin:0;padding:0;background:#f4f6f8;font-family:Arial,sans-serif;">
          <table width="100%" cellpadding="0" cellspacing="0" style="background:#f4f6f8;padding:32px 0;">
            <tr>
              <td align="center">
                <table width="600" cellpadding="0" cellspacing="0" style="background:#ffffff;border-radius:8px;overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,0.08);">

                  <!-- Header -->
                  <tr>
                    <td style="background:#ffffff;border-top:4px solid hsl(38,92%,50%);border-bottom:1px solid hsl(48,70%,85%);padding:16px 32px;">
                      <img src="https://sigmasbm.pure-tco.com/2.png" alt="SIGMA" height="40" style="display:block;border:0;" />
                    </td>
                  </tr>

                  <!-- Body -->
                  <tr>
                    <td style="padding:32px;">
                      <p style="margin:0 0 16px;font-size:16px;color:#333;">Hi <strong>{recipientName}</strong>,</p>
                      <p style="margin:0 0 24px;font-size:15px;color:#555;">
                        A new assignment has been posted for your group <strong>{data.GroupName}</strong>.
                        Please review the details below and submit before the deadline.
                      </p>

                      <!-- Assignment card -->
                      <table width="100%" cellpadding="0" cellspacing="0"
                             style="background:#fffbeb;border-left:4px solid hsl(38,92%,50%);border-radius:4px;padding:0;">
                        <tr>
                          <td style="padding:20px 24px;">
                            <p style="margin:0 0 8px;font-size:18px;font-weight:bold;color:#3d3c39;">
                              {data.AssignmentTitle}
                            </p>
                            <p style="margin:0;font-size:13px;color:#e53e3e;">
                              <strong>Due:</strong>&nbsp;
                              {(data.DueDate.HasValue
                                  ? data.DueDate.Value.ToString("dddd, MMMM dd yyyy \u2013 HH:mm") + " WIB"
                                  : "No deadline specified")}
                            </p>
                          </td>
                        </tr>
                      </table>

                      <p style="margin:24px 0 0;font-size:14px;color:#718096;">
                        Log in to the platform to view full details and submit your work.
                      </p>
                      <table width="100%" cellpadding="0" cellspacing="0" style="margin-top:20px;">
                        <tr>
                          <td align="center">
                            <a href="https://sigmasbm.pure-tco.com"
                               style="display:inline-block;background:hsl(38,92%,50%);color:#252432;font-family:Arial,sans-serif;font-size:14px;font-weight:bold;text-decoration:none;padding:13px 32px;border-radius:8px;letter-spacing:0.02em;">
                              Open SIGMA &rarr;
                            </a>
                          </td>
                        </tr>
                      </table>
                    </td>
                  </tr>

                  <!-- Footer -->
                  <tr>
                    <td style="background:#f7fafc;padding:16px 32px;border-top:1px solid #e2e8f0;">
                      <p style="margin:0;font-size:12px;color:#a0aec0;text-align:center;">
                        You received this email because you are a member of group <em>{data.GroupName}</em> on SIGMA.
                        Please do not reply to this email.
                      </p>
                      <p style="margin:6px 0 0;text-align:center;">
                        <a href="https://sigmasbm.pure-tco.com" style="font-size:11px;color:hsl(38,72%,42%);text-decoration:none;">sigmasbm.pure-tco.com</a>
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
}