using AutoMapper;

namespace Application.UnitTests;

internal static class MapperMocker
{
    internal static IMapper GetMapper<TProfile>() where TProfile : Profile, new()
    {
        var configurationProvider = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ToDoAssistant.Application.Mappings.MappingProfile>();
            cfg.AddProfile<Chef.Application.Mappings.MappingProfile>();
            cfg.AddProfile<TProfile>();
        });
        return configurationProvider.CreateMapper();
    }
}
