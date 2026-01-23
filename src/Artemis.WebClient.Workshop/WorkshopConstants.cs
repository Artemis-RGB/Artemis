namespace Artemis.WebClient.Workshop;

public static class WorkshopConstants
{
    // This is so I can never accidentally release with localhost    
#if DEBUG
    // public const string AUTHORITY_URL = "https://localhost:5001";
    // public const string WORKSHOP_URL = "https://localhost:7281";
    public const string AUTHORITY_URL = "https://identity.artemis-rgb.com";
    public const string WORKSHOP_URL = "https://workshop.artemis-rgb.com";
#else
    public const string AUTHORITY_URL = "https://identity.artemis-rgb.com";
    public const string WORKSHOP_URL = "https://workshop.artemis-rgb.com";
#endif

    public const string IDENTITY_CLIENT_NAME = "IdentityApiClient";
    public const string WORKSHOP_CLIENT_NAME = "WorkshopApiClient";
}