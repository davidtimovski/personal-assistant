using AutoMapper;
using Chef.Application.Mappings;
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
            cfg.AddProfile<Chef.Application.Mappings.MappingProfile>();
            cfg.AddProfile<ChefProfile>();
        });

        Mapper = ConfigurationProvider.CreateMapper();
    }

    public IConfigurationProvider ConfigurationProvider { get; }

    public IMapper Mapper { get; }
}
