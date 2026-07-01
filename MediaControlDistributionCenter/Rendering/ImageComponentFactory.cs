using MediaControlDistributionCenter.ViewModels;

namespace MediaControlDistributionCenter.Rendering
{
    public class ImageComponentFactory : IComponentFactory
    {
        public string Type => "Image";

        public IRenderable Create(BaseComponentViewModel vm)
        {
            var filePath = ResolveFilePath(vm);
            return new ImageRenderable(vm, filePath);
        }

        private static string ResolveFilePath(BaseComponentViewModel vm)
        {
            if (!string.IsNullOrEmpty(vm.FileName))
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
                var path = Path.Combine(baseDir, "Assets", vm.FileName);
                if (File.Exists(path))
                    return path;
            }
            if (!string.IsNullOrEmpty(vm.Source) && File.Exists(vm.Source))
                return vm.Source;
            return string.Empty;
        }
    }
}
