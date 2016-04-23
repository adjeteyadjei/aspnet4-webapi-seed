namespace WebApiSeed.Models
{
    public class Privileges
    {
        //Permissions on the User Model
        public const string CanCreateUser = "CanCreateUser";
        public const string CanUpdateUser = "CanUpdateUser";
        public const string CanViewUser = "CanViewUser";
        public const string CanDeleteUser = "CanDeleteUser";

        //Permissions on the Role
        public const string CanCreateRole = "CanCreateRole";
        public const string CanUpdateRole = "CanUpdateRole";
        public const string CanViewRole = "CanViewRole";
        public const string CanDeleteRole = "CanDeleteRole";

        //permissions on systems configuration
        public const string MessageOutBox = "MessageOutBox";
        public const string CanViewDashboard = "CanViewDashboard";
        public const string CanViewReport = "CanViewReport";
        public const string CanViewSetting = "CanViewSetting";
        public const string CanViewAdministration = "CanViewAdministration";
    }

    public class GenericProperties
    {
        public const string CreatedBy = "CreatedBy";
        public const string CreatedAt = "CreatedAt";
        public const string ModifiedAt = "ModifiedAt";
        public const string ModifiedBy = "ModifiedBy";
        public const string Locked = "Locked";
    }

    public class ExceptionMessage
    {
        public const string RecordLocked = "Record is locked and can't be deleted.";
        public const string NotFound = "Record not found.";
    }

    public class DefaultKeys
    {
        public static string CurrencyFormat = "GHC##,###.00";
    }

    public class ConfigKeys
    {
        public static string EmailAccountName = "EmailAccountName";
        public static string EmailApiKey = "EmailApiKey";
        public static string EmailSender = "EmailSender";
        public static string SmsApiKey = "SmsApiKey";
        public static string SmsSender = "SmsSender";
        public static string AppTitle = "AppTitle";
        public static string Logo = "Logo";
        public static string ToolbarColour = "ToolbarColour";
    }

}