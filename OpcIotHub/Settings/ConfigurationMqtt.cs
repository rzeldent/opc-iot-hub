namespace OpcIotHub.Settings
{
    public class ConfigurationMqtt
    {
        public const string Section = "Amazon:IotHub";

        public string CaCertificate { get; set; }
        public string ClientCertificate { get; set; }
        public string ServiceUrl { get; set; }
        public string Topic { get; set; }
    }
}
