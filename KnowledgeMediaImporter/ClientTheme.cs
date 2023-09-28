using MudBlazor;

namespace KnowledgeMediaImporter;

public class ClientTheme: MudTheme
{
    public static ClientTheme CodeBlue = new ClientTheme()
    {
        Palette = new PaletteLight()
        {
            Primary = "#0082bb",
            AppbarBackground = "#0082bb",
            Secondary = "#ff8300",
            Background = Colors.Grey.Lighten5,
            DrawerBackground = "#FFF",
            DrawerText = "rgba(0,0,0, 0.7)",
            Success = "#128a00",
            Warning = "#ffdd00",
            Error = "#df1642"
        },
    };
}