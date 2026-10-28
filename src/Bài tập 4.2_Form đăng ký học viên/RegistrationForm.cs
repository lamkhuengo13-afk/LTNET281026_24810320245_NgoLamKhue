using Microsoft.Data.SqlClient;
using System.Data;
using System.Drawing;
using System.Text.RegularExpressions;
namespace BaiTap42_StudentRegistration;
public sealed class RegistrationForm : Form
{
    readonly TextBox name=new();
    readonly MaskedTextBox phone=new();
    readonly DateTimePicker birth=new();
    readonly ComboBox course=new();
    readonly RadioButton female=new(){Text="Nữ",Checked=true,AutoSize=true}, male=new(){Text="Nam",AutoSize=true};
    readonly CheckBox news=new(){Text="Đồng ý nhận thông tin khóa học",AutoSize=true};
    readonly Button save=new(){Text="Đăng ký"}, clear=new(){Text="Làm mới"};
    readonly DataGridView grid=new();
    readonly ErrorProvider errors=new();
    readonly Label status=new(){AutoSize=false};
    public RegistrationForm()
    {
        Text="Đăng ký học viên"; StartPosition=FormStartPosition.CenterScreen; ClientSize=new Size(960,650);
        MinimumSize=new Size(900,600); Font=new Font("Segoe UI",9.5f);
        Controls.Add(new Label{Text="ĐĂNG KÝ HỌC VIÊN",Font=new Font("Segoe UI",19,FontStyle.Bold),AutoSize=true,Location=new Point(24,16)});
        L("Họ và tên",25,65); name.SetBounds(25,88,390,30); name.MaxLength=100; name.PlaceholderText="Nhập họ tên"; Controls.Add(name);
        L("Số điện thoại",25,125); phone.SetBounds(25,148,390,30); phone.Mask="000 000 0000"; Controls.Add(phone);
        L("Ngày sinh",25,185); birth.SetBounds(25,208,390,30); birth.Format=DateTimePickerFormat.Short; birth.MaxDate=DateTime.Today; birth.Value=new DateTime(2005,1,1)>DateTime.Today?DateTime.Today:new DateTime(2005,1,1); Controls.Add(birth);
        L("Giới tính",25,245); female.SetBounds(25,270,70,25); male.SetBounds(110,270,80,25); Controls.AddRange([female,male]);
        L("Khóa học",25,305); course.SetBounds(25,328,390,30); course.DropDownStyle=ComboBoxStyle.DropDownList; Controls.Add(course);
        news.SetBounds(25,372,320,25); Controls.Add(news);
        save.SetBounds(25,415,185,38); save.Click+=async(_,_)=>await SaveStudent(); Controls.Add(save);
        clear.SetBounds(230,415,185,38); clear.Click+=(_,_)=>Clear(); Controls.Add(clear);
        status.SetBounds(25,460,410,60); Controls.Add(status);
        grid.SetBounds(450,65,485,490); grid.Anchor=AnchorStyles.Top|AnchorStyles.Bottom|AnchorStyles.Left|AnchorStyles.Right;
        grid.ReadOnly=true; grid.AllowUserToAddRows=false; grid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill; grid.SelectionMode=DataGridViewSelectionMode.FullRowSelect; Controls.Add(grid);
        errors.ContainerControl=this; Load+=async(_,_)=>await LoadData();
    }
    void L(string t,int x,int y)=>Controls.Add(new Label{Text=t,AutoSize=true,Location=new Point(x,y)});
    async Task LoadData()
    {
        try {
            using var c=Database.Connect(); await c.OpenAsync();
            using var cmd=new SqlCommand("SELECT CourseId,CourseName FROM dbo.Courses WHERE IsActive=1 ORDER BY CourseName",c);
            var dt=new DataTable(); using(var ad=new SqlDataAdapter(cmd)) ad.Fill(dt);
            course.DisplayMember="CourseName"; course.ValueMember="CourseId"; course.DataSource=dt;
            await RefreshGrid(); status.Text="Đã kết nối CSDL.";
        } catch(SqlException) { status.Text="Không kết nối được CSDL. Hãy chạy database/01_CreateDatabase.sql."; save.Enabled=false; }
    }
    bool Valid()
    {
        errors.Clear(); bool ok=true;
        if(string.IsNullOrWhiteSpace(name.Text)){errors.SetError(name,"Vui lòng nhập họ tên.");ok=false;}
        else if(name.Text.Trim().Length<2){errors.SetError(name,"Họ tên phải có ít nhất 2 ký tự.");ok=false;}
        var digits=Regex.Replace(phone.Text,@"\D","");
        if(digits.Length<9||digits.Length>10){errors.SetError(phone,"Nhập số điện thoại hợp lệ.");ok=false;}
        if(birth.Value.Date>=DateTime.Today){errors.SetError(birth,"Ngày sinh phải trước hôm nay.");ok=false;}
        if(course.SelectedValue is not int){errors.SetError(course,"Vui lòng chọn khóa học.");ok=false;}
        return ok;
    }
    async Task SaveStudent()
    {
        if(!Valid()) return; save.Enabled=false;
        try {
            using var c=Database.Connect(); await c.OpenAsync();
            using var cmd=new SqlCommand("INSERT dbo.Students(FullName,Phone,BirthDate,Gender,CourseId,ReceiveNews) VALUES(@n,@p,@b,@g,@c,@news)",c);
            cmd.Parameters.Add("@n",SqlDbType.NVarChar,100).Value=name.Text.Trim();
            cmd.Parameters.Add("@p",SqlDbType.NVarChar,20).Value=Regex.Replace(phone.Text,@"\D","");
            cmd.Parameters.Add("@b",SqlDbType.Date).Value=birth.Value.Date;
            cmd.Parameters.Add("@g",SqlDbType.NVarChar,20).Value=female.Checked?"Nữ":"Nam";
            cmd.Parameters.Add("@c",SqlDbType.Int).Value=(int)course.SelectedValue!;
            cmd.Parameters.Add("@news",SqlDbType.Bit).Value=news.Checked;
            await cmd.ExecuteNonQueryAsync(); status.Text="Đăng ký thành công, đã lưu vào SQL Server.";
            await RefreshGrid(); MessageBox.Show("Đã lưu thông tin học viên.","Thành công",MessageBoxButtons.OK,MessageBoxIcon.Information); Clear();
        } catch(SqlException ex) { status.Text="Lỗi CSDL: "+ex.Message; }
        finally { save.Enabled=true; }
    }
    async Task RefreshGrid()
    {
        using var c=Database.Connect(); await c.OpenAsync();
        using var cmd=new SqlCommand(@"SELECT s.StudentId AS [Mã],s.FullName AS [Họ tên],s.Phone AS [Điện thoại],s.BirthDate AS [Ngày sinh],s.Gender AS [Giới tính],c.CourseName AS [Khóa học],s.ReceiveNews AS [Nhận tin] FROM dbo.Students s JOIN dbo.Courses c ON c.CourseId=s.CourseId ORDER BY s.StudentId DESC",c);
        var dt=new DataTable(); using(var ad=new SqlDataAdapter(cmd)) ad.Fill(dt); grid.DataSource=dt;
        if(grid.Columns["Ngày sinh"]!=null) grid.Columns["Ngày sinh"].DefaultCellStyle.Format="dd/MM/yyyy";
    }
    void Clear(){name.Clear();phone.Clear();birth.Value=new DateTime(2005,1,1)>DateTime.Today?DateTime.Today:new DateTime(2005,1,1);female.Checked=true;news.Checked=false;if(course.Items.Count>0)course.SelectedIndex=0;errors.Clear();name.Focus();}
}
