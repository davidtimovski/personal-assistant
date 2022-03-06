using AutoMapper;
using Application.Mappings;

namespace Application.UnitTests.Mappings;

public class MappingTestsFixture
{
    public MappingTestsFixture()
    {
        ConfigurationProvider = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
            cfg.AddProfile<ToDoAssistantProfile>();
            cfg.AddProfile<CookingAssistantProfile>();
            cfg.AddProfile<AccountantProfile>();
        });

        Mapper = ConfigurationProvider.CreateMapper();
    }

    public IConfigurationProvider ConfigurationProvider { get; }

    public IMapper Mapper { get; }
}
