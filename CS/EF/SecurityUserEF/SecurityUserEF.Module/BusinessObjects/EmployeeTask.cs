using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;

namespace SecurityUserEF.Module.BusinessObjects;

[DefaultClassOptions, ImageName("BO_Task")]
public class EmployeeTask : BaseObject {
    public virtual string Subject { get; set; }
    public virtual Employee Owner { get; set; }
}
