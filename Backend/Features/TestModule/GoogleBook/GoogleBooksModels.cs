namespace PureTCOWebApp.Features.TestModule.GoogleBook;

public record GoogleBooksResponse(string Kind, int TotalItems, List<BookItem>? Items);

public record BookItem(
    string Kind,
    string Id,
    string Etag,
    string SelfLink,
    VolumeInfo VolumeInfo,
    SaleInfo? SaleInfo,
    AccessInfo? AccessInfo,
    SearchInfo? SearchInfo
);

public record VolumeInfo(
    string Title,
    string? Subtitle,
    List<string>? Authors,
    string? Publisher,
    string? PublishedDate,
    string? Description,
    List<IndustryIdentifier>? IndustryIdentifiers,
    ReadingModes? ReadingModes,
    int? PageCount,
    string? PrintType,
    List<string>? Categories,
    string? MaturityRating,
    bool? AllowAnonLogging,
    string? ContentVersion,
    PanelizationSummary? PanelizationSummary,
    ImageLinks? ImageLinks,
    string? Language,
    string? PreviewLink,
    string? InfoLink,
    string? CanonicalVolumeLink
);

public record IndustryIdentifier(string Type, string Identifier);

public record ReadingModes(bool Text, bool Image);

public record PanelizationSummary(bool ContainsEpubBubbles, bool ContainsImageBubbles);

public record ImageLinks(string? SmallThumbnail, string? Thumbnail);

public record SaleInfo(
    string Country,
    string Saleability,
    bool IsEbook,
    ListPrice? ListPrice,
    RetailPrice? RetailPrice,
    string? BuyLink,
    List<Offer>? Offers
);

public record ListPrice(double Amount, string CurrencyCode);

public record RetailPrice(double Amount, string CurrencyCode);

public record Offer(
    int FinskyOfferType,
    OfferPrice? ListPrice,
    OfferPrice? RetailPrice,
    bool? Giftable
);

public record OfferPrice(long AmountInMicros, string CurrencyCode);

public record AccessInfo(
    string Country,
    string Viewability,
    bool Embeddable,
    bool PublicDomain,
    string TextToSpeechPermission,
    EpubInfo Epub,
    PdfInfo Pdf,
    string WebReaderLink,
    string AccessViewStatus,
    bool QuoteSharingAllowed
);

public record EpubInfo(bool IsAvailable);

public record PdfInfo(bool IsAvailable);

public record SearchInfo(string TextSnippet);
