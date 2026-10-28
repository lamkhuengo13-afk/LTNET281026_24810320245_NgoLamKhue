namespace BaiTap41_Login;
internal static class Program
{
    [STAThread] static void Main() { ApplicationConfiguration.Initialize(); Application.Run(new LoginForm()); }
}
