using MediaControlDistributionCenter.ViewModels;
using System.IO;

namespace MediaControlDistributionCenter.Rendering
{
    public class ImageComponentFactory : IComponentFactory
    {
        private readonly BitmapCache? _cache;

        public string Type => "Image";

        public ImageComponentFactory() : this(null)
        {
        }

        public ImageComponentFactory(BitmapCache? cache)
        {
            _cache = cache;
        }

        public IRenderable Create(BaseComponentViewModel vm)
        {
            var filePath = ResolveFilePath(vm);
            return new ImageRenderable(vm, filePath, _cache);
        }

        private static string ResolveFilePath(BaseComponentViewModel vm)
        {
            if (!string.IsNullOrEmpty(vm.FileName))
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var assetsDir = Path.GetFullPath(Path.Combine(baseDir, "Assets"));
                var fullPath = Path.GetFullPath(Path.Combine(baseDir, "Assets", vm.FileName));
                if (!fullPath.StartsWith(assetsDir, StringComparison.OrdinalIgnoreCase))
                    return string.Empty;
                if (File.Exists(fullPath))
                    return fullPath;
            }
            if (!string.IsNullOrEmpty(vm.Source))
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var assetsDir = Path.GetFullPath(Path.Combine(baseDir, "Assets"));
                var fullPath = Path.GetFullPath(Path.Combine(baseDir, "Assets", vm.Source));
                if (fullPath.StartsWith(assetsDir, StringComparison.OrdinalIgnoreCase) && File.Exists(fullPath))
                    return fullPath;
            }
            return string.Empty;
        }
    }
}
