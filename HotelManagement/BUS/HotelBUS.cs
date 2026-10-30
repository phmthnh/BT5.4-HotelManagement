using System.Data;
using HotelManagement.DAL;
using HotelManagement.DTO;
namespace HotelManagement.BUS;
public sealed class HotelBUS
{
    private readonly HotelDAL dal=new();
    public DataTable Rooms()=>dal.Rooms();public DataTable Services()=>dal.Services();public DataTable Active(string room)=>dal.Active(room);public DataTable Used(int id)=>dal.Used(id);public DataTable Report()=>dal.Report();
    public DataTable Guest(string id){if(string.IsNullOrWhiteSpace(id)||id.Trim().Length>20)throw new ArgumentException("Nhập CCCD tối đa 20 ký tự.");return dal.Guest(id.Trim());}
    public int CheckIn(string room,GuestDTO guest)
    {
        guest=guest with{HoTen=guest.HoTen.Trim(),CCCD=guest.CCCD.Trim(),SoDienThoai=guest.SoDienThoai.Trim()};
        if(string.IsNullOrWhiteSpace(room)||room.Length>10||guest.HoTen.Length is 0 or >100||guest.CCCD.Length is 0 or >20||guest.SoDienThoai.Length>15)throw new ArgumentException("Chọn phòng, nhập họ tên và CCCD hợp lệ; điện thoại tối đa 15 ký tự.");
        return dal.CheckIn(room,guest);
    }
    public void AddService(ServiceDTO s){if(s.MaPhieu<=0||s.MaDV<=0||s.SoLuong<=0||s.SoLuong>1000)throw new ArgumentException("Chọn phiếu đang ở, dịch vụ và số lượng 1–1000.");dal.AddService(s);}
    public InvoiceDTO CheckOut(int id){if(id<=0)throw new ArgumentException("Chọn phòng đang có khách.");return dal.CheckOut(id);}
}
