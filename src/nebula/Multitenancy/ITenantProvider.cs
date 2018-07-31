using ComposerCore.Attributes;

namespace Nebula.Multitenancy
{
    [Contract]
    public interface ITenantProvider
    {
        string Id { get; }

        void GetCurrentTenant();
    }
}