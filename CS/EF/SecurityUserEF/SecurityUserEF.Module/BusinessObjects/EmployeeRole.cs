using System.Collections.ObjectModel;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;

namespace SecurityUserEF.Module.BusinessObjects;

[ImageName("BO_Role")]
public class EmployeeRole : PermissionPolicyRoleBase, IPermissionPolicyRoleWithUsers {
    public virtual IList<Employee> Employees { get; set; } = new ObservableCollection<Employee>();
    IEnumerable<IPermissionPolicyUser> IPermissionPolicyRoleWithUsers.Users {
        get { return Employees.OfType<IPermissionPolicyUser>(); }
    }
}

