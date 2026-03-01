using MimeKit;

namespace PureTCOWebApp.Features.EmailModule;

/// <summary>
/// Builds MimeMessage objects for group-assignment email notifications.
/// </summary>
public static class GroupAssignmentEmailTemplate
{
    /// <summary>
    /// Builds an individual personalised email for a single participant.
    /// Used in production blast (one email per participant).
    /// </summary>
    public static MimeMessage Build(
        GroupAssignmentEmailData data,
        ParticipantEmailInfo recipient,
        string senderEmail,
        string senderName)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(senderName, senderEmail));

        // Production: send to real participant
        // To send to multiple participants at once (BCC blast), collect all
        // addresses and add them to message.Bcc instead of message.To.
        message.To.Add(new MailboxAddress(recipient.FullName, recipient.Email));

        message.Subject = $"[SBM Literation] New Assignment: {data.AssignmentTitle}";
        message.Body = new TextPart("html") { Text = BuildHtml(data, recipient.FullName) };

        return message;
    }

    /// <summary>
    /// Builds a single preview/redirect email used in PoC mode.
    /// The email shows all participant names so the tester can verify the data
    /// that would normally go to each of them, all in one inbox.
    /// </summary>
    public static MimeMessage BuildBlastPreview(
        GroupAssignmentEmailData data,
        IList<ParticipantEmailInfo> participants,
        string testEmail,
        string testName,
        string senderEmail,
        string senderName)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(senderName, senderEmail));

        // PoC: all emails go to the test inbox
        message.To.Add(new MailboxAddress(testName, testEmail));

        // Production recipients are listed as TO (commented out) – un-comment
        // the block below and remove the test recipient above to go live:
        //
        // foreach (var p in participants)
        //     message.To.Add(new MailboxAddress(p.FullName, p.Email));

        message.Subject = $"[PoC Preview] New Assignment: {data.AssignmentTitle}";
        message.Body = new TextPart("html")
        {
            Text = BuildBlastPreviewHtml(data, participants)
        };

        return message;
    }

    // -------------------------------------------------------------------------
    // HTML builders
    // -------------------------------------------------------------------------

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
                    <td style="background:#2b6cb0;padding:24px 32px;">
                      <h1 style="margin:0;color:#ffffff;font-size:22px;">SBM Literation</h1>
                      <p style="margin:4px 0 0;color:#bee3f8;font-size:13px;">Learning Platform Notification</p>
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
                             style="background:#ebf8ff;border-left:4px solid #2b6cb0;border-radius:4px;padding:0;">
                        <tr>
                          <td style="padding:20px 24px;">
                            <p style="margin:0 0 8px;font-size:18px;font-weight:bold;color:#2b6cb0;">
                              {data.AssignmentTitle}
                            </p>
                            {(string.IsNullOrWhiteSpace(data.AssignmentDescription)
                                ? ""
                                : $"<p style=\"margin:0 0 12px;font-size:14px;color:#4a5568;\">{data.AssignmentDescription}</p>")}
                            <p style="margin:0;font-size:13px;color:#e53e3e;">
                              <strong>Due:</strong>&nbsp;
                              {(data.DueDate.HasValue
                                  ? data.DueDate.Value.ToString("dddd, MMMM dd yyyy – HH:mm UTC")
                                  : "No deadline specified")}
                            </p>
                          </td>
                        </tr>
                      </table>

                      <p style="margin:24px 0 0;font-size:14px;color:#718096;">
                        Log in to the platform to view full details and submit your work.
                      </p>
                    </td>
                  </tr>

                  <!-- Footer -->
                  <tr>
                    <td style="background:#f7fafc;padding:16px 32px;border-top:1px solid #e2e8f0;">
                      <p style="margin:0;font-size:12px;color:#a0aec0;text-align:center;">
                        You received this email because you are a member of group <em>{data.GroupName}</em> on SBM Literation.
                        Please do not reply to this email.
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

    private static string BuildBlastPreviewHtml(GroupAssignmentEmailData data, IList<ParticipantEmailInfo> participants)
    {
        var participantRows = string.Join("", participants.Select((p, i) =>
            $"<tr style=\"background:{((i % 2 == 0) ? "#f7fafc" : "#ffffff")}\">" +
            $"<td style=\"padding:8px 12px;font-size:13px;color:#333;\">{p.FullName}</td>" +
            $"<td style=\"padding:8px 12px;font-size:13px;color:#555;\">{p.Email}</td>" +
            $"</tr>"));

        return $"""
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
                    <table width="640" cellpadding="0" cellspacing="0" style="background:#ffffff;border-radius:8px;overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,0.08);">

                      <!-- PoC Banner -->
                      <tr>
                        <td style="background:#ed8936;padding:10px 32px;">
                          <p style="margin:0;color:#fff;font-size:13px;font-weight:bold;">
                            ⚠ PoC Preview Mode – This email was redirected from {participants.Count} participant(s).
                            In production each participant receives their own personalised copy.
                          </p>
                        </td>
                      </tr>

                      <!-- Header -->
                      <tr>
                        <td style="background:#2b6cb0;padding:24px 32px;">
                          <h1 style="margin:0;color:#ffffff;font-size:22px;">SBM Literation</h1>
                          <p style="margin:4px 0 0;color:#bee3f8;font-size:13px;">Group Assignment Notification – Admin Preview</p>
                        </td>
                      </tr>

                      <!-- Body -->
                      <tr>
                        <td style="padding:32px;">
                          <h2 style="margin:0 0 4px;font-size:20px;color:#2b6cb0;">{data.AssignmentTitle}</h2>
                          <p style="margin:0 0 4px;font-size:13px;color:#718096;">Group: <strong>{data.GroupName}</strong></p>
                          {(string.IsNullOrWhiteSpace(data.AssignmentDescription)
                              ? ""
                              : $"<p style=\"margin:8px 0;font-size:14px;color:#4a5568;\">{data.AssignmentDescription}</p>")}
                          <p style="margin:8px 0 24px;font-size:13px;color:#e53e3e;">
                            <strong>Due:</strong>&nbsp;
                            {(data.DueDate.HasValue
                                ? data.DueDate.Value.ToString("dddd, MMMM dd yyyy – HH:mm UTC")
                                : "No deadline specified")}
                          </p>

                          <!-- Participants table -->
                          <p style="margin:0 0 8px;font-size:14px;font-weight:bold;color:#333;">
                            Recipients that would receive this email in production ({participants.Count}):
                          </p>
                          <table width="100%" cellpadding="0" cellspacing="0" style="border:1px solid #e2e8f0;border-radius:4px;overflow:hidden;">
                            <thead>
                              <tr style="background:#ebf8ff;">
                                <th style="padding:8px 12px;text-align:left;font-size:12px;color:#2b6cb0;">Name</th>
                                <th style="padding:8px 12px;text-align:left;font-size:12px;color:#2b6cb0;">Email</th>
                              </tr>
                            </thead>
                            <tbody>
                              {participantRows}
                            </tbody>
                          </table>
                        </td>
                      </tr>

                      <!-- Footer -->
                      <tr>
                        <td style="background:#f7fafc;padding:16px 32px;border-top:1px solid #e2e8f0;">
                          <p style="margin:0;font-size:12px;color:#a0aec0;text-align:center;">
                            SBM Literation – PoC Email Preview | Set <code>Email:TestRecipientEmail</code> to empty in production.
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
}
