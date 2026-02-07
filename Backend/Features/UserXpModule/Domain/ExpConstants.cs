namespace PureTCOWebApp.Features.UserXpModule.Domain;

public static class ExpConstants
{
    public const decimal STREAK_7_DAYS_BONUS = 10m;
    public const decimal READING_PER_PAGE = 0.1m;
    public const decimal DAILY_READ_QUIZ_PASSED = 10m;
    public const decimal BOOK_COMPLETED = 3m;
    
    public const int STREAK_BONUS_DAYS = 7;
    public const int SNAPSHOT_EVENT_INTERVAL = 7;
    public const int BOOK_COMPLETION_COOLDOWN_DAYS = 7;
}
