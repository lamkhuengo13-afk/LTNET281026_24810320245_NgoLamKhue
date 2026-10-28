using Microsoft.Data.SqlClient;
using System.Drawing;
namespace BaiTap41_Login;
public sealed class LoginForm : Form
{
    readonly TextBox user=new(), pass=new();
    readonly CheckBox show=new(){Text="Hiện mật khẩu",AutoSize=true};
    readonly Button login=new(){Text="Đăng nhập"}, exit=new(){Text="Thoát"};
    readonly ErrorProvider errors=new();
    readonly Label status=new(){AutoSize=false};
    public LoginForm()
    {
        Text="Đăng nhập"; StartPosition=FormStartPosition.CenterScreen;
        FormBorderStyle=FormBorderStyle.FixedDialog; MaximizeBox=false; ClientSize=new Size(440,350);
        Font=new Font("Segoe UI",10); BackColor=Color.FromArgb(246,248,252);
        Controls.Add(new Label{Text="ĐĂNG NHẬP",Font=new Font("Segoe UI",20,FontStyle.Bold),AutoSize=true,Location=new Point(28,20)});
        LabelAt("Tên đăng nhập",30,75); user.SetBounds(30,100,380,32); user.PlaceholderText="Nhập tên đăng nhập"; user.MaxLength=50; Controls.Add(user);
        LabelAt("Mật khẩu",30,145); pass.SetBounds(30,170,380,32); pass.UseSystemPasswordChar=true; pass.PlaceholderText="Nhập mật khẩu"; Controls.Add(pass);
        show.Location=new Point(30,210); show.CheckedChanged+=(_,_)=>pass.UseSystemPasswordChar=!show.Checked; Controls.Add(show);
        login.SetBounds(30,250,180,40); login.Click+=async(_,_)=>await DoLogin(); Controls.Add(login);
        exit.SetBounds(230,250,180,40); exit.Click+=(_,_)=>Close(); Controls.Add(exit);
        status.SetBounds(30,300,380,35); status.ForeColor=Color.Firebrick; Controls.Add(status);
        errors.ContainerControl=this; AcceptButton=login; CancelButton=exit;
    }
    void LabelAt(string t,int x,int y)=>Controls.Add(new Label{Text=t,AutoSize=true,Location=new Point(x,y)});
    async Task DoLogin()
    {
        errors.Clear(); status.Text="";
        if(string.IsNullOrWhiteSpace(user.Text)) errors.SetError(user,"Vui lòng nhập tên đăng nhập.");
        if(string.IsNullOrEmpty(pass.Text)) errors.SetError(pass,"Vui lòng nhập mật khẩu.");
        if(string.IsNullOrWhiteSpace(user.Text)||string.IsNullOrEmpty(pass.Text)) return;
        login.Enabled=false;
        try {
            using var c=Database.Connect(); await c.OpenAsync();
            using var cmd=new SqlCommand("SELECT PasswordHash,PasswordSalt,DisplayName FROM dbo.AppUsers WHERE Username=@u",c);
            cmd.Parameters.Add("@u",System.Data.SqlDbType.NVarChar,50).Value=user.Text.Trim();
            using var r=await cmd.ExecuteReaderAsync();
            if(!await r.ReadAsync()){status.Text="Tên đăng nhập hoặc mật khẩu không đúng.";return;}
            var hash=(byte[])r["PasswordHash"]; var salt=(byte[])r["PasswordSalt"]; var name=(string)r["DisplayName"];
            if(!PasswordSecurity.Verify(pass.Text,hash,salt)){status.Text="Tên đăng nhập hoặc mật khẩu không đúng.";return;}
            MessageBox.Show($"Xin chào {name}!","Đăng nhập thành công",MessageBoxButtons.OK,MessageBoxIcon.Information);
            status.ForeColor=Color.DarkGreen; status.Text="Đăng nhập thành công.";
        } catch(SqlException) { status.Text="Không kết nối được CSDL. Hãy chạy script tạo CSDL."; }
        finally { login.Enabled=true; }
    }
}
