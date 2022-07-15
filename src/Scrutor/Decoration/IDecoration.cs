using Microsoft.Extensions.DependencyInjection;

namespace Scrutor.Decoration
{
    public interface IDecoration
    {
        IServiceCollection Decorate(IServiceCollection services);
        bool TryDecorate(IServiceCollection services);
    }
}
