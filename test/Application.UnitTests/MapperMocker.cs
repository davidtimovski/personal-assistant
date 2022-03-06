using AutoMapper;
using Application.Mappings;

namespace Application.UnitTests;

public static class MapperMocker
{
    public static IMapper GetMapper<TProfile>() where TProfile : Profile, new()
    {
        var configurationProvider = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
            cfg.AddProfile<TProfile>();
        });
        return configurationProvider.CreateMapper();
    }
}
