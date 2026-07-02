using MediaControlDistributionCenter.Rendering;

namespace MediaControlDistributionCenter.Tests;

public class BitmapCacheTests
{
    [Fact]
    public void GetOrDecode_Null_ForMissingFile()
    {
        var cache = new BitmapCache();
        var result = cache.GetOrDecode("nonexistent.png");
        Assert.Null(result);
    }

    [Fact]
    public void Dispose_ClearsCache()
    {
        var cache = new BitmapCache();
        cache.Dispose();
        // Should not throw on double dispose
        cache.Dispose();
    }
}
