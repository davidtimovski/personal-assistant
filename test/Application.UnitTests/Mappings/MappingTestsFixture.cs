using Accountant.Application.Mappings;
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
            cfg.AddProfile<Accountant.Application.Mappings.MappingProfile>();
            cfg.AddProfile<AccountantProfile>();
        });

        Mapper = ConfigurationProvider.CreateMapper();
    }

    public IConfigurationProvider ConfigurationProvider { get; }

    public IMapper Mapper { get; }
}
