using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;

namespace SecurityUser.Module.BusinessObjects {
    [DeferredDeletion(false)]
    [Persistent("PermissionPolicyUserLoginInfo")]
    public class EmployeeLoginInfo : BaseObject, ISecurityUserLoginInfo {
        private string loginProviderName;
        private Employee user;
        private string providerUserKey;
        public EmployeeLoginInfo(Session session) : base(session) { }

        [Indexed("ProviderUserKey", Unique = true)]
        [Appearance("PasswordProvider", Enabled = false, Criteria = "!(IsNewObject(this)) and LoginProviderName == '" + SecurityDefaults.PasswordAuthentication + "'", Context = "DetailView")]
        public string LoginProviderName {
            get { return loginProviderName; }
            set { SetPropertyValue(nameof(LoginProviderName), ref loginProviderName, value); }
        }

        [Appearance("PasswordProviderUserKey", Enabled = false, Criteria = "!(IsNewObject(this)) and LoginProviderName == '" + SecurityDefaults.PasswordAuthentication + "'", Context = "DetailView")]
        public string ProviderUserKey {
            get { return providerUserKey; }
            set { SetPropertyValue(nameof(ProviderUserKey), ref providerUserKey, value); }
        }

        [Association("User-LoginInfo")]
        public Employee User {
            get { return user; }
            set { SetPropertyValue(nameof(User), ref user, value); }
        }

        object ISecurityUserLoginInfo.User => User;
    }
}
