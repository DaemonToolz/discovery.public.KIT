
namespace discovery.KIT.p2p.Models
{
    public struct IdentificationFrame
    {
        public string CipheredUsername;
        public string CipheredMachineName;
    }

    public struct IdentificationResponse
    {
        public bool Authenticated;
    }
}
