using LegacyOrder.ModuleRegistrations;

namespace LegacyOrder.Tests.TestFixtures;

public class AutoMapperFixture
{
    public IMapper Mapper { get; }

    public AutoMapperFixture()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AutoMapperProfile>();
        });

        config.AssertConfigurationIsValid();
        Mapper = config.CreateMapper();
    }

    public static IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<AutoMapperProfile>();
        });

        return config.CreateMapper();
    }
}

