using AutoMapper;

namespace Application.UnitTests;

public static class MapperMocker
{
    public static IMapper GetMapper<TProfile>() where TProfile : Profile, new()
    {
        var configurationProvider = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ToDoAssistant.Application.Mappings.MappingProfile>();
            cfg.AddProfile<CookingAssistant.Application.Mappings.MappingProfile>();
            cfg.AddProfile<TProfile>();
        });
        return configurationProvider.CreateMapper();
    }
}
