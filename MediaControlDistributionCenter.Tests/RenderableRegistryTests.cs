using MediaControlDistributionCenter.Rendering;

namespace MediaControlDistributionCenter.Tests;

public class RenderableRegistryTests
{
    [Fact]
    public void Register_And_Create()
    {
        var registry = new RenderableRegistry();
        registry.Register(new TestComponentFactory());

        var result = registry.Create(new TestComponentViewModel { Type = "Test", ZIndex = 0 });

        Assert.NotNull(result);
        Assert.Equal("Test", result.Type);
    }

    [Fact]
    public void Create_Throws_For_Unknown_Type()
    {
        var registry = new RenderableRegistry();

        Assert.Throws<KeyNotFoundException>(() =>
            registry.Create(new TestComponentViewModel { Type = "Unknown" }));
    }

    [Fact]
    public void CanCreate_Returns_True_For_Registered()
    {
        var registry = new RenderableRegistry();
        registry.Register(new TestComponentFactory());

        Assert.True(registry.CanCreate("Test"));
        Assert.False(registry.CanCreate("NonExistent"));
    }

    private class TestComponentFactory : IComponentFactory
    {
        public string Type => "Test";

        public IRenderable Create(ViewModels.BaseComponentViewModel vm)
        {
            return new TestRenderable { ZIndex = vm.ZIndex };
        }
    }

    private class TestComponentViewModel : ViewModels.BaseComponentViewModel
    {
        public override string Type { get; set; } = "Test";
    }
}
