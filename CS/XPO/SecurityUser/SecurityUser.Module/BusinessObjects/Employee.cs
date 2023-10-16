using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Base.Security;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SecurityUser.Module.BusinessObjects {
    [DefaultClassOptions]
    public class Employee : BaseObject , ISecurityUser , IAuthenticationStandardUser, IAuthenticationActiveDirectoryUser, ISecurityUserWithRoles, IPermissionPolicyUser, ICanInitialize, ISecurityUserWithLoginInfo {
        public Employee(Session session)
            : base(session) { }
        [Association("Employee-Task")]
        public XPCollection<EmployeeTask> OwnTasks {
            get { return GetCollection<EmployeeTask>(nameof(OwnTasks)); }
        }
        string _firstName;
        public string FirstName {
            get {
                return _firstName;
            }
            set {
                SetPropertyValue(nameof(FirstName), ref _firstName, value);
            }
        }

        #region ISecurityUser Members
        private bool isActive = true;
        public bool IsActive {
            get { return isActive; }
            set { SetPropertyValue(nameof(IsActive), ref isActive, value); }
        }
        private string userName = String.Empty;
        [RuleRequiredField("EmployeeUserNameRequired", DefaultContexts.Save)]
        [RuleUniqueValue("EmployeeUserNameIsUnique", DefaultContexts.Save,
            "The login with the entered user name was already registered within the system.")]
        public string UserName {
            get { return userName; }
            set { SetPropertyValue(nameof(UserName), ref userName, value); }
        }
        #endregion
        #region IAuthenticationStandardUser Members
        private bool changePasswordOnFirstLogon;
        public bool ChangePasswordOnFirstLogon {
            get { return changePasswordOnFirstLogon; }
            set {
                SetPropertyValue(nameof(ChangePasswordOnFirstLogon), ref changePasswordOnFirstLogon, value);
            }
        }
        private string storedPassword;
        [Browsable(false), Size(SizeAttribute.Unlimited), Persistent, SecurityBrowsable]
        protected string StoredPassword {
            get { return storedPassword; }
            set { storedPassword = value; }
        }
        public bool ComparePassword(string password) {
            return PasswordCryptographer.VerifyHashedPasswordDelegate(this.storedPassword, password);
        }
        public void SetPassword(string password) {
            this.storedPassword = PasswordCryptographer.HashPasswordDelegate(password);
            OnChanged(nameof(StoredPassword));
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
      
        [Association("Employees-EmployeeRoles")]
        [RuleRequiredField("EmployeeRoleIsRequired", DefaultContexts.Save,
            TargetCriteria = "IsActive",
            CustomMessageTemplate = "An active employee must have at least one role assigned")]
        public XPCollection<EmployeeRole> EmployeeRoles {
            get {
                return GetCollection<EmployeeRole>(nameof(EmployeeRoles));
            }
        }
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
        [Aggregated, Association("User-LoginInfo")]
        public XPCollection<EmployeeLoginInfo> LoginInfo {
            get { return GetCollection<EmployeeLoginInfo>(nameof(LoginInfo)); }
        }

        IEnumerable<ISecurityUserLoginInfo> IOAuthSecurityUser.UserLogins => LoginInfo.OfType<ISecurityUserLoginInfo>();

        ISecurityUserLoginInfo ISecurityUserWithLoginInfo.CreateUserLoginInfo(string loginProviderName, string providerUserKey) {
            EmployeeLoginInfo result = new EmployeeLoginInfo(Session);
            result.LoginProviderName = loginProviderName;
            result.ProviderUserKey = providerUserKey;
            result.User = this;
            return result;
        }
        #endregion
    }
}
