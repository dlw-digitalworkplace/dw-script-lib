namespace Core
{
    public class Globals
    {
        public class Tenant
        {
            public static readonly string Name = "Tenant:Name";
            public static readonly string Id = "Tenant:Id";
        }

        public class Client
        {
            public static readonly string Id = "Client:Id";
            public static readonly string Secret = "Client:Secret";
            public static readonly string CertificatePath = "Client:CertificatePath";
            public static readonly string CertificateThumbprint = "Client:CertificateThumbprint";
        }

        public class General
        {
            public static readonly string BatchSize = "General:BatchSize";

            public class DataSource
            {
                public static readonly string Option = "General:DataSource:Option";
                public static readonly string Value = "General:DataSource:Value";
            }
        }

        public class SP
        {
            public static readonly string VersionPolicyTrimMode = "SP:VersionPolicyTrimMode";
            public static readonly string SitePrefix = "SP:SitePrefix";
        }

        public class Task
        {
            public class IntelligentVersioning
            {
                public static readonly string ExpireVersionsAfterDays = "Task:IntelligentVersioning:ExpireVersionsAfterDays";
                public static readonly string KeepAmount = "Task:IntelligentVersioning:KeepAmount";
            }
        }

        public class AzureBlob
        {
            public static readonly string ConnectionString = "AzureBlob:ConnectionString";
            public static readonly string Container = "AzureBlob:Container";
            public static readonly string FilePathWithExtension = "AzureBlob:FilePathWithExtension";
        }
    }
}
