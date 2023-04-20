using AutoMapper;
using CookingAssistant.Application.Mappings;
using ToDoAssistant.Application.Mappings;

namespace Application.UnitTests.Mappings;

public class MappingTestsFixture
{
    public MappingTestsFixture()
    {
        ConfigurationProvider = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ToDoAssistant.Application.Mappings.MappingProfile>();
            cfg.AddProfile<ToDoAssistantProfile>();
            cfg.AddProfile<CookingAssistant.Application.Mappings.MappingProfile>();
            cfg.AddProfile<CookingAssistantProfile>();
        });

        Mapper = ConfigurationProvider.CreateMapper();
    }

    public IConfigurationProvider ConfigurationProvider { get; }

    public IMapper Mapper { get; }
}
