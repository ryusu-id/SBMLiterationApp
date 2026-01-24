namespace PureTCOWebApp.Features.TestModule.JournalDoi;

public record WorkMessage(
    DateInfo Indexed,
    int ReferenceCount,
    string Publisher,
    string? Issue,
    List<License>? License,
    ContentDomain ContentDomain,
    List<string> ShortContainerTitle,
    string? Abstract,
    string DOI,
    string Type,
    DateInfo Created,
    string? Page,
    string Source,
    int IsReferencedByCount,
    List<string> Title,
    string Prefix,
    string? Volume,
    List<Author> Author,
    string Member,
    PublicationDate PublishedOnline,
    List<string> ContainerTitle,
    List<string> OriginalTitle,
    List<Link>? Link,
    DateInfo Deposited,
    double Score,
    Resource Resource,
    List<string> Subtitle,
    List<string> ShortTitle,
    PublicationDate Issued,
    int ReferencesCount,
    JournalIssue JournalIssue,
    string URL,
    object Relation,
    List<string> ISSN,
    List<IssnType> IssnType,
    List<string> Subject,
    PublicationDate Published
);

public record DateInfo(List<List<int>> DateParts, string DateTime, long Timestamp, string? Version = null);

public record License(DateInfo Start, string ContentVersion, int DelayInDays, string URL);

public record ContentDomain(List<string> Domain, bool CrossmarkRestriction);

public record Author(string Given, string Family, string Sequence, List<string> Affiliation);

public record Link(string URL, string ContentType, string ContentVersion, string IntendedApplication);

public record Resource(Primary Primary);

public record Primary(string URL);

public record PublicationDate(List<List<int>> DateParts);

public record JournalIssue(string Issue, PublicationDate PublishedOnline);

public record IssnType(string Value, string Type);
