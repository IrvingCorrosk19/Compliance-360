using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Compliance360.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReconcileAlertOccurrenceTerminalStates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE compliance360.alert_occurrences AS occurrence
                SET "Status" = CASE
                        WHEN EXISTS (
                            SELECT 1
                            FROM compliance360.notification_messages AS message
                            WHERE message."TenantId" = occurrence."TenantId"
                              AND message."AlertOccurrenceId" = occurrence."Id"
                              AND message."Status" IN ('Failed', 'DeadLetter'))
                        THEN 'Failed'
                        ELSE 'Completed'
                    END,
                    "EvaluatedAtUtc" = COALESCE(occurrence."EvaluatedAtUtc", CURRENT_TIMESTAMP),
                    "FailureReason" = CASE
                        WHEN EXISTS (
                            SELECT 1
                            FROM compliance360.notification_messages AS message
                            WHERE message."TenantId" = occurrence."TenantId"
                              AND message."AlertOccurrenceId" = occurrence."Id"
                              AND message."Status" IN ('Failed', 'DeadLetter'))
                        THEN 'One or more notification messages failed terminally.'
                        ELSE occurrence."FailureReason"
                    END,
                    "UpdatedAtUtc" = CURRENT_TIMESTAMP
                WHERE occurrence."Status" = 'Queued'
                  AND EXISTS (
                      SELECT 1
                      FROM compliance360.notification_messages AS message
                      WHERE message."TenantId" = occurrence."TenantId"
                        AND message."AlertOccurrenceId" = occurrence."Id")
                  AND NOT EXISTS (
                      SELECT 1
                      FROM compliance360.notification_messages AS message
                      WHERE message."TenantId" = occurrence."TenantId"
                        AND message."AlertOccurrenceId" = occurrence."Id"
                        AND message."Status" IN ('Queued', 'Processing', 'Retried'));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Terminal delivery evidence is not downgraded.
        }
    }
}
