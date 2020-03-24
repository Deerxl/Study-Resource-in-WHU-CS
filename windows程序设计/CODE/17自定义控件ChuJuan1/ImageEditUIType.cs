using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;

/*创建日期 :2014.06.01
 *修改日期 :2014.06.01
 *类名称    :ImageEditUIType
 * 类说明   :由类UITypeEditor派生，用于指定自定义控件属性的编辑窗体对象
 */
namespace ChuJuan1
{
    public class ImageEditUIType : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            if (context != null && context.Instance != null)
            {
                return UITypeEditorEditStyle.Modal;
            }

            return base.GetEditStyle(context);
        }
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            IWindowsFormsEditorService editorService = null;

            if (context != null && context.Instance != null && provider != null)
            {
                editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
                if (editorService != null)
                {
                    UserControl1 control = (UserControl1)context.Instance;
                    FrmCtrlImgsEdit dlg = new FrmCtrlImgsEdit(control.ImageArray);
                    dlg.imgList.Clear();
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        value = dlg.imgList;
                        return value;
                    }
                }
            } 
            return value;
        } 
    }
}
