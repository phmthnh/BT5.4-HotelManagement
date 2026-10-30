#nullable enable
namespace HotelManagement.GUI;
partial class CatalogForm
{
    private System.ComponentModel.IContainer? components=null;
    private ComboBox types=null!;private DataGridView grid=null!;private Panel fields=null!;private TextBox search=null!;
    protected override void Dispose(bool disposing){if(disposing)components?.Dispose();base.Dispose(disposing);}
    private void InitializeComponent()
    {
        types=new ComboBox();grid=new DataGridView();fields=new Panel();search=new TextBox();
        var find=new Button();var create=new Button();var save=new Button();var delete=new Button();
        ((System.ComponentModel.ISupportInitialize)grid).BeginInit();SuspendLayout();
        types.Location=new Point(16,16);types.Size=new Size(240,28);types.DropDownStyle=ComboBoxStyle.DropDownList;
        search.Location=new Point(275,16);search.Size=new Size(250,28);search.PlaceholderText="Tra cứu theo mã hoặc tên";
        find.Text="Tra cứu";find.Location=new Point(540,14);find.Size=new Size(100,30);find.Click+=SearchRows;
        grid.Location=new Point(16,60);grid.Size=new Size(640,420);grid.ReadOnly=true;grid.AllowUserToAddRows=false;grid.AllowUserToDeleteRows=false;grid.MultiSelect=false;grid.SelectionMode=DataGridViewSelectionMode.FullRowSelect;grid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill;grid.SelectionChanged+=ChooseRow;
        fields.Location=new Point(675,60);fields.Size=new Size(430,330);fields.AutoScroll=true;
        create.Text="Thêm mới";create.Location=new Point(685,410);create.Size=new Size(120,36);create.Click+=NewRow;
        save.Text="Lưu";save.Location=new Point(815,410);save.Size=new Size(120,36);save.Click+=SaveRow;
        delete.Text="Xóa";delete.Location=new Point(945,410);delete.Size=new Size(120,36);delete.Click+=DeleteRow;
        AutoScaleDimensions=new SizeF(7,15);AutoScaleMode=AutoScaleMode.Font;ClientSize=new Size(1120,500);Text="Quản lý danh mục";StartPosition=FormStartPosition.CenterParent;
        Controls.AddRange(new Control[]{types,search,find,grid,fields,create,save,delete});
        ((System.ComponentModel.ISupportInitialize)grid).EndInit();ResumeLayout(false);PerformLayout();
    }
}
