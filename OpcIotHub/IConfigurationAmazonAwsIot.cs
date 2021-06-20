namespace OpcIotHub
{
    public interface IConfigurationAmazionAwsIot
    {
        string PemCertificate { get; }
        string Endpoint { get; }
        string Topic { get; }
    }
}
