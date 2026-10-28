using Microsoft.Data.SqlClient;
namespace BaiTap42_StudentRegistration;
internal static class Database
{
    internal const string ConnectionString = @"Server=(localdb)\MSSQLLocalDB;Database=WinFormsCoursework;Integrated Security=True;TrustServerCertificate=True;";
    internal static SqlConnection Connect() => new(ConnectionString);
}
