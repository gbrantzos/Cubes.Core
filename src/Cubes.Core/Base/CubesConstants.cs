namespace Cubes.Core.Base
{
    public static class CubesConstants
    {
        public const string Files_DataAccess               = "Core.DataAccess.yaml";
        public const string Files_Scheduling               = "Core.Scheduling.yaml";
        public const string Files_StaticContent            = "Core.StaticContent.yaml";
        public const string Files_SmtpSettings             = "Core.SmtpSettings.yaml";
        public const string Files_AppSettings              = "appsettings.json";

        public const string Config_HostUseSSL              = "Host:UseSSL";
        public const string Config_HostEnableCompression   = "Host:EnableCompression";
        public const string Config_HostSwaggerTheme        = "Host:SwaggerTheme";
        public const string Config_HostWrapPath            = "Host:WrapPath";
        public const string Config_HostWrapPathExclude     = "Host:WrapPathExclude";
        public const string Config_HostCorsPolicies        = "Host:CorsPolicies";
        public const string Config_HostHealthCheckEndpoint = "Host:HealthCheckEndpoint";
        public const string Config_HostMetricsEndpoint     = "Host:MetricsEndpoint";
        public const string Config_ApiKey                  = "ApiKey";
        public const string Config_KeyLifetime             = "KeyLifetime";
        public const string Config_IpRestrictionsOptions   = "Host:IpRestrictionsOptions";

        public const string NLog_ConfigFile                = "NLog.config";
        public const string NLog_SampleFile                = "NLog.Sample.config";

        public const string LocalStorage_File              = "Core.LocalStorage.db";
        public const string Configuration_Section          = "CubesConfig";

        public const string Serializer_JSON                = "JSON";
        public const string Serializer_YAML                = "YAML";

        public const string Authentication_InternalAdmin   = "cubes";
        public const string Authentication_Persistence     = "Core.UsersStorage.db";
    }
}
