using System.Data;
using Microsoft.Data.SqlClient;
using HotelManagement.BUS;
namespace HotelManagement.GUI;
public partial class CatalogForm : Form
{
    private CatalogBUS? service;
    private object? selectedKey;
    private readonly Dictionary<string,TextBox> editors=new();
    public CatalogForm() { InitializeComponent(); Shown+=(_,_)=>InitializeCatalog(); }
    private void InitializeCatalog() { types.DisplayMember="Title";types.DataSource=CatalogBUS.Definitions();types.SelectedIndexChanged+=(_,_)=>LoadCatalog();LoadCatalog(); }
    private void LoadCatalog()
    {
        if(types.SelectedItem is not DTO.CatalogDTO item)return;
        service=new CatalogBUS(item.Table);selectedKey=null;fields.Controls.Clear();editors.Clear();int y=8;
        foreach(var field in item.Fields)
        {
            if(item.Identity && field.Column==item.Key)continue;
            fields.Controls.Add(new Label{Text=field.Caption,Left=10,Top=y+3,Width=140});
            var edit=new TextBox{Name="edit"+field.Column,Left=155,Top=y,Width=255,MaxLength=field.Kind is "money" or "quantity" ? 20:field.Length};
            fields.Controls.Add(edit);editors[field.Column]=edit;y+=36;
        }
        Reload();
    }
    private void Reload() { Guard(()=>{grid.DataSource=service?.List(search.Text);}); }
    private void Guard(Action action) { try { action(); } catch(Exception ex) when(ex is SqlException or ArgumentException or InvalidOperationException) { MessageBox.Show(this,ex is SqlException sql ? sql.Number==547?"Không thể xóa hoặc lưu: dữ liệu đang được sử dụng hoặc mã tham chiếu không hợp lệ.":sql.Number is 2601 or 2627?"Mã hoặc số điện thoại đã tồn tại.":"Không thể thao tác CSDL. Kiểm tra kết nối và dữ liệu.":ex.Message,"Thông báo",MessageBoxButtons.OK,MessageBoxIcon.Warning); } }
    private void ChooseRow(object? sender,EventArgs e)
    {
        if(service is null || grid.CurrentRow?.DataBoundItem is not DataRowView row)return;
        selectedKey=row[service.Catalog.Key];
        foreach(var field in service.Catalog.Fields)if(editors.TryGetValue(field.Column,out var edit)){edit.Text=Convert.ToString(row[field.Column]);edit.ReadOnly=field.Column==service.Catalog.Key;}
    }
    private void NewRow(object? sender,EventArgs e) { selectedKey=null;foreach(var edit in editors.Values){edit.ReadOnly=false;edit.Clear();} }
    private void SaveRow(object? sender,EventArgs e) { Guard(()=>{if(service is null)return;service.Save(editors.ToDictionary(p=>p.Key,p=>(object?)p.Value.Text),selectedKey);Reload();NewRow(null,EventArgs.Empty);}); }
    private void DeleteRow(object? sender,EventArgs e) { Guard(()=>{if(service is null || selectedKey is null)throw new ArgumentException("Chọn dòng cần xóa.");if(MessageBox.Show(this,"Xóa bản ghi đã chọn?","Xác nhận",MessageBoxButtons.YesNo,MessageBoxIcon.Warning)!=DialogResult.Yes)return;service.Delete(selectedKey);Reload();NewRow(null,EventArgs.Empty);}); }
    private void SearchRows(object? sender,EventArgs e)=>Reload();
}
