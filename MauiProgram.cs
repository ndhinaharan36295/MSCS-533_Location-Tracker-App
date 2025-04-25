using Microsoft.Maui.Controls.Maps;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<LocationTrackerApp.App>()
            .UseMauiMaps(); // Add this
        return builder.Build();
    }
}
