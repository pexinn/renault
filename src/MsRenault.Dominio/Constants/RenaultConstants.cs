namespace MsRenault.Dominio.Constants;

public static class RenaultConstants
{
    public static class LeadProvider
    {
        public const string Dealer = "DEALER";
        public const string Facebook = "FACEBOOK";
        public const string Webmotors = "WEBMOTORS";
    }

    public static class TypeOfInterest
    {
        public const string VN = "VN";
        public const string VO = "VO";
        public const string APV = "APV";
        public const string Services = "SERVICES";
        public const string Mobility = "MOBILITY";
    }

    public static class SubTypeOfInterest
    {
        public const string BrochureRequest = "BROCHURE_REQUEST";
        public const string TestDriveRequest = "TESTDRIVE_REQUEST";
    }

    public static class Origin
    {
        public const string ShowRoom = "SHOW_ROOM";
        public const string OrganicSearch = "ORGANIC_SEARCH";
        public const string SocialMedia = "SOCIAL_MEDIA";
    }

    public static class Status
    {
        public const string Prospection = "PROSPECTION";
        public const string Visit = "VISIT";
        public const string TestDrive = "TEST_DRIVE";
        public const string Negotiation = "NEGOTIATION";
    }
}
