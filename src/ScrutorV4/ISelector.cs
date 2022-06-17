using Microsoft.Extensions.DependencyInjection;

namespace ScrutorOld
{
    public interface ISelector
    {
        void Populate(IServiceCollection services, RegistrationStrategy? options);
    }
}
