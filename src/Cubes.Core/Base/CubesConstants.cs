namespace Cubes.Core.Base
{
    public static class CubesConstants
    {
        public static readonly string Files_DataAccess             = "Core.DataAccess.yaml";
        public static readonly string Files_Scheduling             = "Core.Scheduling.yaml";
        public static readonly string Files_StaticContent          = "Core.StaticContent.yaml";
        public static readonly string Files_SmtpSettings           = "Core.SmtpSettings.yaml";
        public static readonly string Files_Applications           = "Cubes.Applications.yaml";
        public static readonly string Files_Application            = "Application.yaml";

        public static readonly string Config_AppSettings           = "appsettings.json";
        public static readonly string Config_HostURLs              = "Host:URLs";
        public static readonly string Config_HostUseSSL            = "Host:UseSSL";
        public static readonly string Config_HostEnableCompression = "Host:EnableCompression";
        public static readonly string Config_HostSwaggerTheme      = "Host:SwaggerTheme";
        public static readonly string Config_HostWrapPath          = "Host:WrapPath";
        public static readonly string Config_HostWrapPathExclude   = "Host:WrapPathExclude";
        public static readonly string Config_HostCorsPolicies      = "Host:CorsPolicies";


        public static readonly string NLog_ConfigFile              = "NLog.config";
        public static readonly string NLog_SampleFile              = "NLog.Sample.config";

        public static readonly string Folders_Common               = "Common";
        public static readonly string LocalStorage_File            = "Core.LocalStorage.db";
        public static readonly string Configuration_Section        = "CubesConfig";

        public const string Serializer_JSON                        = "JSON";
        public const string Serializer_YAML                        = "YAML";
    }
}
