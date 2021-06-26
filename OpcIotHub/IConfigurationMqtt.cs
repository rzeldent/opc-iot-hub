namespace OpcIotHub
{
    public interface IConfigurationMqtt
    {
        string CaCertificate { get; }
        string ClientCertificate { get; }
        string ServiceUrl { get; }
        string Topic { get; }
    }
}
