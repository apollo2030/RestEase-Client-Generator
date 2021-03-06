using Microsoft.VisualStudio.Shell;

namespace RestEaseClientGenerator.VSIX.Options
{
    public abstract class OptionsBase<TOptionsInterface, TOptionsPage>
        where TOptionsInterface : class
        where TOptionsPage : DialogPage
    {
        protected TOptionsInterface GetFromDialogPage() => VsPackage.Instance.GetDialogPage(typeof(TOptionsPage)) as TOptionsInterface;
    }
}