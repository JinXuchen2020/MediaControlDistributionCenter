using MediaControlDistributionCenter.ViewModels;
using System;

namespace MediaControlDistributionCenter.Rendering
{
    public interface IEditorSurface : IDisposable
    {
        double Width { get; set; }
        double Height { get; set; }
        double Ratio { get; set; }

        void LoadComponents(System.Collections.Generic.IEnumerable<BaseComponentViewModel> components);
        void AddComponent(BaseComponentViewModel component);
        void Clear();
        byte[]? CaptureSnapshot();

        BaseComponentViewModel? SelectedComponent { get; }
        event Action<BaseComponentViewModel?>? SelectedComponentChanged;

        void SetViewModel(MediaEditViewModel viewModel);
        void InvalidateVisual();
        void UpdateComponent(BaseComponentViewModel component);
        void SelectComponent(BaseComponentViewModel component);
    }
}
