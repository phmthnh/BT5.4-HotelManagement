using System.Data;
using Microsoft.Data.SqlClient;
using HotelManagement.DTO;
namespace HotelManagement.DAL;
public sealed class HotelDAL
{
    public DataTable Rooms()=>DatabaseHelper.Query("SELECT * FROM dbo.Phong ORDER BY SoPhong");
    public DataTable Services()=>DatabaseHelper.Query("SELECT * FROM dbo.DichVu ORDER BY MaDV");
    public DataTable Guest(string id)=>DatabaseHelper.Query("SELECT * FROM dbo.KhachHang WHERE CCCD=@id",DatabaseHelper.P("@id",SqlDbType.VarChar,id,20));
    public DataTable Active(string room)=>DatabaseHelper.Query("SELECT p.MaPhieu,p.SoPhong,k.HoTen,k.CCCD,p.NgayNhanPhong FROM dbo.PhieuDatPhong p JOIN dbo.KhachHang k ON k.MaKH=p.MaKH WHERE p.SoPhong=@room AND p.NgayTraPhong IS NULL",DatabaseHelper.P("@room",SqlDbType.VarChar,room,10));
    public DataTable Used(int id)=>DatabaseHelper.Query("SELECT d.TenDV,s.SoLuong,d.DonGia,s.ThoiGian,CAST(s.SoLuong*d.DonGia AS decimal(18,2)) AS ThanhTien FROM dbo.SuDungDichVu s JOIN dbo.DichVu d ON d.MaDV=s.MaDV WHERE s.MaPhieu=@id",DatabaseHelper.P("@id",SqlDbType.Int,id));
    private static SqlCommand Cmd(SqlConnection c,SqlTransaction t,string sql,params SqlParameter[] p){var q=new SqlCommand(sql,c,t);q.Parameters.AddRange(p);return q;}
    public int CheckIn(string room,GuestDTO guest)
    {
        using var c=DatabaseHelper.Open();using var t=c.BeginTransaction(IsolationLevel.Serializable);
        try
        {
            using(var q=Cmd(c,t,"SELECT TrangThai FROM dbo.Phong WITH(UPDLOCK,HOLDLOCK) WHERE SoPhong=@room",DatabaseHelper.P("@room",SqlDbType.VarChar,room,10)))if(q.ExecuteScalar() is not string state || state!="Trống")throw new InvalidOperationException("Phòng không còn trống.");
            int customer;
            using(var q=Cmd(c,t,"SELECT MaKH FROM dbo.KhachHang WITH(UPDLOCK,HOLDLOCK) WHERE CCCD=@id",DatabaseHelper.P("@id",SqlDbType.VarChar,guest.CCCD,20)))
            {
                var exists=q.ExecuteScalar();if(exists is int found)customer=found;
                else{using var insert=Cmd(c,t,"INSERT dbo.KhachHang(HoTen,CCCD,SoDienThoai) OUTPUT INSERTED.MaKH VALUES(@name,@id,@phone)",DatabaseHelper.P("@name",SqlDbType.NVarChar,guest.HoTen,100),DatabaseHelper.P("@id",SqlDbType.VarChar,guest.CCCD,20),DatabaseHelper.P("@phone",SqlDbType.VarChar,guest.SoDienThoai,15));customer=(int)insert.ExecuteScalar()!;}
            }
            int booking;
            using(var q=Cmd(c,t,"INSERT dbo.PhieuDatPhong(SoPhong,MaKH) OUTPUT INSERTED.MaPhieu VALUES(@room,@customer); UPDATE dbo.Phong SET TrangThai=N'Đang ở' WHERE SoPhong=@room;",DatabaseHelper.P("@room",SqlDbType.VarChar,room,10),DatabaseHelper.P("@customer",SqlDbType.Int,customer)))booking=(int)q.ExecuteScalar()!;
            t.Commit();return booking;
        }catch{t.Rollback();throw;}
    }
    public void AddService(ServiceDTO service)
    {
        using var c=DatabaseHelper.Open();using var t=c.BeginTransaction();
        try
        {
            using(var q=Cmd(c,t,"SELECT MaPhieu FROM dbo.PhieuDatPhong WITH(UPDLOCK,HOLDLOCK) WHERE MaPhieu=@id AND NgayTraPhong IS NULL",DatabaseHelper.P("@id",SqlDbType.Int,service.MaPhieu)))if(q.ExecuteScalar() is null)throw new InvalidOperationException("Phòng không có khách lưu trú.");
            using(var q=Cmd(c,t,"INSERT dbo.SuDungDichVu(MaPhieu,MaDV,SoLuong) VALUES(@id,@dv,@sl)",DatabaseHelper.P("@id",SqlDbType.Int,service.MaPhieu),DatabaseHelper.P("@dv",SqlDbType.Int,service.MaDV),DatabaseHelper.P("@sl",SqlDbType.Int,service.SoLuong)))q.ExecuteNonQuery();
            t.Commit();
        }catch{t.Rollback();throw;}
    }
    public InvoiceDTO CheckOut(int id)
    {
        using var c=DatabaseHelper.Open();using var t=c.BeginTransaction(IsolationLevel.Serializable);
        try
        {
            DateTime start,end;decimal rate;string room;
            using(var q=Cmd(c,t,"SELECT p.SoPhong,p.NgayNhanPhong,f.GiaTheoGio,GETDATE() FROM dbo.PhieuDatPhong p WITH(UPDLOCK,HOLDLOCK) JOIN dbo.Phong f ON f.SoPhong=p.SoPhong WHERE p.MaPhieu=@id AND p.NgayTraPhong IS NULL",DatabaseHelper.P("@id",SqlDbType.Int,id)))
            {using var r=q.ExecuteReader();if(!r.Read())throw new InvalidOperationException("Phiếu đã trả phòng hoặc không tồn tại.");room=r.GetString(0);start=r.GetDateTime(1);rate=r.GetDecimal(2);end=r.GetDateTime(3);}
            decimal services;
            using(var q=Cmd(c,t,"SELECT COALESCE(SUM(s.SoLuong*d.DonGia),0) FROM dbo.SuDungDichVu s JOIN dbo.DichVu d ON d.MaDV=s.MaDV WHERE s.MaPhieu=@id",DatabaseHelper.P("@id",SqlDbType.Int,id)))services=(decimal)q.ExecuteScalar()!;
            var invoice=new InvoiceDTO(id,room,start,end,Math.Round((decimal)(end-start).TotalMinutes/60m*rate,2,MidpointRounding.AwayFromZero),services);
            if(invoice.TienPhong<0)throw new InvalidOperationException("Thời điểm nhận phòng không hợp lệ.");
            using(var q=Cmd(c,t,"UPDATE dbo.PhieuDatPhong SET NgayTraPhong=@end,TongTienThanhToan=@total WHERE MaPhieu=@id; UPDATE dbo.Phong SET TrangThai=N'Đang dọn dẹp' WHERE SoPhong=@room;",DatabaseHelper.P("@end",SqlDbType.DateTime,end),DatabaseHelper.P("@total",SqlDbType.Decimal,invoice.TongTien),DatabaseHelper.P("@id",SqlDbType.Int,id),DatabaseHelper.P("@room",SqlDbType.VarChar,room,10)))q.ExecuteNonQuery();
            t.Commit();return invoice;
        }catch{t.Rollback();throw;}
    }
    public DataTable Report()=>DatabaseHelper.Query("SELECT p.MaPhieu,p.SoPhong,k.HoTen,p.NgayNhanPhong,p.NgayTraPhong,p.TongTienThanhToan FROM dbo.PhieuDatPhong p JOIN dbo.KhachHang k ON k.MaKH=p.MaKH WHERE NgayTraPhong IS NOT NULL ORDER BY MaPhieu DESC");
}
