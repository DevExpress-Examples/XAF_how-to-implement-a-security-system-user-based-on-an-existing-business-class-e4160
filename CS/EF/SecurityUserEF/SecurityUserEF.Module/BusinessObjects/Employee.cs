using System.Collections.ObjectModel;
using DevExpress.ExpressApp.DC;
using System.ComponentModel;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Base.Security;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.Validation;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SecurityUserEF.Module.BusinessObjects;


[DefaultClassOptions]

public class Employee : BaseObject, ISecurityUser, IAuthenticationStandardUser, IAuthenticationActiveDirectoryUser, ISecurityUserWithRoles, IPermissionPolicyUser, ICanInitialize, ISecurityUserWithLoginInfo {
    public virtual string FirstName { get; set; }
    public virtual IList<EmployeeTask> OwnTasks { get; set; } = new ObservableCollection<EmployeeTask>();

    #region ISecurityUser Members
    public virtual bool IsActive { get; set; } = true;
    [RuleRequiredField("EmployeeUserNameRequired", DefaultContexts.Save)]
    [RuleUniqueValue("EmployeeUserNameIsUnique", DefaultContexts.Save,
        "The login with the entered user name was already registered within the system.")]
    public virtual string UserName { get; set; }
    #endregion
    #region IAuthenticationStandardUser Members
    public virtual bool ChangePasswordOnFirstLogon { get; set; }
    [Browsable(false), FieldSize(FieldSizeAttribute.Unlimited), SecurityBrowsable]
    protected virtual string StoredPassword { get; set; }
    public bool ComparePassword(string password) {
        return PasswordCryptographer.VerifyHashedPasswordDelegate(this.StoredPassword, password);
    }
    public void SetPassword(string password) {
        this.StoredPassword = PasswordCryptographer.HashPasswordDelegate(password);
    }
    #endregion
    #region ISecurityUserWithRoles Members
    IList<ISecurityRole> ISecurityUserWithRoles.Roles {
        get {
            IList<ISecurityRole> result = new List<ISecurityRole>();
            foreach (EmployeeRole role in EmployeeRoles) {
                result.Add(role);
            }
            return result;
        }
    }
    [RuleRequiredField("EmployeeRoleIsRequired", DefaultContexts.Save,
        TargetCriteria = "IsActive",
        CustomMessageTemplate = "An active employee must have at least one role assigned")]
    public virtual IList<EmployeeRole> EmployeeRoles { get; set; } = new ObservableCollection<EmployeeRole>();
    #endregion
    #region IPermissionPolicyUser Members
    IEnumerable<IPermissionPolicyRole> IPermissionPolicyUser.Roles {
        get { return EmployeeRoles.OfType<IPermissionPolicyRole>(); }
    }
    #endregion
    #region ICanInitialize Members
    void ICanInitialize.Initialize(IObjectSpace objectSpace, SecurityStrategyComplex security) {
        EmployeeRole newUserRole = (EmployeeRole)objectSpace.FirstOrDefault<EmployeeRole>(role => role.Name == security.NewUserRoleName);
        if (newUserRole == null) {
            newUserRole = objectSpace.CreateObject<EmployeeRole>();
            newUserRole.Name = security.NewUserRoleName;
            newUserRole.IsAdministrative = true;
            newUserRole.Employees.Add(this);
        }
    }
    #endregion
    #region ISecurityUserWithLoginInfo Members
    [Browsable(false)]
    [DevExpress.ExpressApp.DC.Aggregated]
    public virtual IList<EmployeeLoginInfo> EmployeeLogins { get; set; } = new ObservableCollection<EmployeeLoginInfo>();

    IEnumerable<ISecurityUserLoginInfo> IOAuthSecurityUser.UserLogins => EmployeeLogins.OfType<ISecurityUserLoginInfo>();

    ISecurityUserLoginInfo ISecurityUserWithLoginInfo.CreateUserLoginInfo(string loginProviderName, string providerUserKey) {
        EmployeeLoginInfo result = ((IObjectSpaceLink)this).ObjectSpace.CreateObject<EmployeeLoginInfo>();
        result.LoginProviderName = loginProviderName;
        result.ProviderUserKey = providerUserKey;
        result.User = this;
        return result;
    }
    #endregion
}
public class EmployeeLoginInfo : ISecurityUserLoginInfo {

    public EmployeeLoginInfo() { }

    [Browsable(false)]
    public virtual Guid ID { get; protected set; }

    [Appearance("PasswordProvider", Enabled = false, Criteria = "!(IsNewObject(this)) and LoginProviderName == '" + SecurityDefaults.PasswordAuthentication + "'", Context = "DetailView")]
    public virtual string LoginProviderName { get; set; }

    [Appearance("PasswordProviderUserKey", Enabled = false, Criteria = "!(IsNewObject(this)) and LoginProviderName == '" + SecurityDefaults.PasswordAuthentication + "'", Context = "DetailView")]
    public virtual string ProviderUserKey { get; set; }

    [Browsable(false)]
    public virtual Guid UserForeignKey { get; set; }

    [Required]
    [ForeignKey(nameof(UserForeignKey))]
    public virtual Employee User { get; set; }

    object ISecurityUserLoginInfo.User => User;
}

