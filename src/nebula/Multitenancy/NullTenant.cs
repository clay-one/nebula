using ComposerCore.Attributes;

namespace Nebula.Multitenancy
{
    [Component]
    public class NullTenant : ITenantProvider
    {
        public string Id { get; private set; }

        public void GetCurrentTenant()
        {
            Id = "None";
        }
    }
}