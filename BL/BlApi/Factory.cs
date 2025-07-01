namespace BlApi;
public static class Factory
{
    private static readonly Lazy<IBl> lazyInstance = new(() => new BlImplementation.Bl());

    public static IBl Get() => lazyInstance.Value;
}
