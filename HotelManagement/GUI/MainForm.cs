using System.Data;
using System.Text;
using Microsoft.Data.SqlClient;
using HotelManagement.BUS;
using HotelManagement.DTO;
namespace HotelManagement.GUI;
public partial class MainForm : Form
{
    private readonly HotelBUS service=new();private DataTable? roomsData;private int booking;private InvoiceDTO? lastInvoice;
    public MainForm(){InitializeComponent();Shown+=(_,_)=>Guard(Reload);}
    private void Guard(Action action){try{action();}catch(Exception ex) when(ex is SqlException or ArgumentException or InvalidOperationException or OverflowException){MessageBox.Show(this,ex is SqlException?"Không thể thao tác CSDL. Kiểm tra kết nối, dữ liệu và chạy Database/ScriptDB.sql.":ex.Message,"Thông báo",MessageBoxButtons.OK,MessageBoxIcon.Warning);}}
    private string Room=>rooms.CurrentRow?.Cells["SoPhong"].Value?.ToString()??"";
    private void Reload(){roomsData=service.Rooms();rooms.DataSource=roomsData.DefaultView;services.DisplayMember="TenDV";services.ValueMember="MaDV";services.DataSource=service.Services();report.DataSource=service.Report();ApplyFilter();LoadStay();}
    private void ApplyFilter(){if(roomsData is null)return;var filters=new List<string>();if(typeFilter.SelectedIndex>0)filters.Add("LoaiPhong='"+typeFilter.Text.Replace("'","''")+"'");if(stateFilter.SelectedIndex>0)filters.Add("TrangThai='"+stateFilter.Text.Replace("'","''")+"'");roomsData.DefaultView.RowFilter=string.Join(" AND ",filters);}
    private void FilterChanged(object? s,EventArgs e){ApplyFilter();LoadStay();}
    private void LoadStay(){Guard(()=>{booking=0;var active=service.Active(Room);if(active.Rows.Count>0){booking=(int)active.Rows[0]["MaPhieu"];stay.Text=$"Phòng {Room} | Phiếu {booking} | {active.Rows[0]["HoTen"]}";}else stay.Text="Phòng "+Room+" | Chưa có khách lưu trú";used.DataSource=service.Used(booking);});}
    private void RoomChanged(object? s,EventArgs e){if(IsHandleCreated)LoadStay();}
    private void Lookup(object? s,EventArgs e)=>Guard(()=>{var result=service.Guest(identity.Text);if(result.Rows.Count==0){MessageBox.Show(this,"Khách mới. Nhập họ tên và điện thoại để nhận phòng.");return;}name.Text=result.Rows[0]["HoTen"].ToString();phone.Text=result.Rows[0]["SoDienThoai"].ToString();});
    private void CheckIn(object? s,EventArgs e)=>Guard(()=>{var id=service.CheckIn(Room,new GuestDTO(name.Text,identity.Text,phone.Text));Reload();MessageBox.Show(this,"Nhận phòng thành công. Mã phiếu: "+id);});
    private void AddService(object? s,EventArgs e)=>Guard(()=>{if(services.SelectedValue is not int id)throw new ArgumentException("Chọn dịch vụ.");service.AddService(new ServiceDTO(booking,id,(int)quantity.Value));LoadStay();});
    private static string Receipt(InvoiceDTO i)=>$"HÓA ĐƠN KHÁCH SẠN\nPhiếu: {i.MaPhieu} | Phòng: {i.SoPhong}\nNhận: {i.NgayNhanPhong:dd/MM/yyyy HH:mm:ss}\nTrả: {i.NgayTraPhong:dd/MM/yyyy HH:mm:ss}\nTiền phòng: {i.TienPhong:N2} đ\nDịch vụ: {i.TienDichVu:N2} đ\nTỔNG: {i.TongTien:N2} đ";
    private void CheckOut(object? s,EventArgs e)=>Guard(()=>{if(booking<=0)throw new ArgumentException("Chọn phòng đang có khách.");if(MessageBox.Show(this,"Xác nhận thanh toán tiền phòng theo thời gian thực và dịch vụ đã dùng?","Check-out",MessageBoxButtons.YesNo,MessageBoxIcon.Question)!=DialogResult.Yes)return;lastInvoice=service.CheckOut(booking);Reload();MessageBox.Show(this,Receipt(lastInvoice),"Hóa đơn");});
    private void Export(object? s,EventArgs e)=>Guard(()=>{if(lastInvoice is null)throw new ArgumentException("Thực hiện check-out để xuất hóa đơn vừa thanh toán.");using var dialog=new SaveFileDialog{Filter="Hóa đơn văn bản (*.txt)|*.txt",FileName="HoaDon_"+lastInvoice.MaPhieu+".txt"};if(dialog.ShowDialog(this)==DialogResult.OK)File.WriteAllText(dialog.FileName,Receipt(lastInvoice),Encoding.UTF8);});
    private void Catalog(object? s,EventArgs e){using var form=new CatalogForm();form.ShowDialog(this);Guard(Reload);}
    private void RefreshAll(object? s,EventArgs e)=>Guard(Reload);
}
