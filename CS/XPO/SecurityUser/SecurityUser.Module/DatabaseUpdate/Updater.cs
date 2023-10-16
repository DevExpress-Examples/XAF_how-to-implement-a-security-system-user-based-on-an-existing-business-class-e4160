using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.Updating;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Security.Strategy;
using DevExpress.Xpo;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using SecurityUser.Module.BusinessObjects;

namespace SecurityUser.Module.DatabaseUpdate;

// For more typical usage scenarios, be sure to check out https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.Updating.ModuleUpdater
public class Updater : ModuleUpdater {
    public Updater(IObjectSpace objectSpace, Version currentDBVersion) :
        base(objectSpace, currentDBVersion) {
    }
    public override void UpdateDatabaseAfterUpdateSchema() {
        base.UpdateDatabaseAfterUpdateSchema();
        //string name = "MyName";
        //DomainObject1 theObject = ObjectSpace.FirstOrDefault<DomainObject1>(u => u.Name == name);
        //if(theObject == null) {
        //    theObject = ObjectSpace.CreateObject<DomainObject1>();
        //    theObject.Name = name;
        //}
#if !RELEASE
        Employee sampleUser = ObjectSpace.FirstOrDefault<Employee>(u => u.UserName == "User");
        if(sampleUser == null) {
            sampleUser = ObjectSpace.CreateObject<Employee>();
            sampleUser.UserName = "User";
            // Set a password if the standard authentication type is used
            sampleUser.SetPassword("");

            // The UserLoginInfo object requires a user object Id (Oid).
            // Commit the user object to the database before you create a UserLoginInfo object. This will correctly initialize the user key property.
            ObjectSpace.CommitChanges(); //This line persists created object(s).
            ((ISecurityUserWithLoginInfo)sampleUser).CreateUserLoginInfo(SecurityDefaults.PasswordAuthentication, ObjectSpace.GetKeyValueAsString(sampleUser));
        }
        EmployeeRole defaultRole = CreateDefaultRole();
        sampleUser.EmployeeRoles.Add(defaultRole);

        Employee userAdmin = ObjectSpace.FirstOrDefault<Employee>(u => u.UserName == "Admin");
        if(userAdmin == null) {
            userAdmin = ObjectSpace.CreateObject<Employee>();
            userAdmin.UserName = "Admin";
            // Set a password if the standard authentication type is used
            userAdmin.SetPassword("");

            // The UserLoginInfo object requires a user object Id (Oid).
            // Commit the user object to the database before you create a UserLoginInfo object. This will correctly initialize the user key property.
            ObjectSpace.CommitChanges(); //This line persists created object(s).
            ((ISecurityUserWithLoginInfo)userAdmin).CreateUserLoginInfo(SecurityDefaults.PasswordAuthentication, ObjectSpace.GetKeyValueAsString(userAdmin));
        }
		// If a role with the Administrators name doesn't exist in the database, create this role
        EmployeeRole adminRole = ObjectSpace.FirstOrDefault<EmployeeRole>(r => r.Name == "Administrators");
        if(adminRole == null) {
            adminRole = ObjectSpace.CreateObject<EmployeeRole>();
            adminRole.Name = "Administrators";
        }
        adminRole.IsAdministrative = true;
		userAdmin.EmployeeRoles.Add(adminRole);
        ObjectSpace.CommitChanges(); //This line persists created object(s).
#endif
    }
    public override void UpdateDatabaseBeforeUpdateSchema() {
        base.UpdateDatabaseBeforeUpdateSchema();
        //if(CurrentDBVersion < new Version("1.1.0.0") && CurrentDBVersion > new Version("0.0.0.0")) {
        //    RenameColumn("DomainObject1Table", "OldColumnName", "NewColumnName");
        //}
    }
    private EmployeeRole CreateDefaultRole() {
        EmployeeRole defaultRole = ObjectSpace.FirstOrDefault<EmployeeRole>(role => role.Name == "Default");
        if(defaultRole == null) {
            defaultRole = ObjectSpace.CreateObject<EmployeeRole>();
            defaultRole.Name = "Default";

			defaultRole.AddObjectPermissionFromLambda<Employee>(SecurityOperations.Read, cm => cm.Oid == (Guid)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
            defaultRole.AddNavigationPermission(@"Application/NavigationItems/Items/Default/Items/MyDetails", SecurityPermissionState.Allow);
			defaultRole.AddMemberPermissionFromLambda<Employee>(SecurityOperations.Write, "ChangePasswordOnFirstLogon", cm => cm.Oid == (Guid)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
			defaultRole.AddMemberPermissionFromLambda<Employee>(SecurityOperations.Write, "StoredPassword", cm => cm.Oid == (Guid)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
            defaultRole.AddTypePermissionsRecursively<EmployeeRole>(SecurityOperations.Read, SecurityPermissionState.Deny);
            defaultRole.AddTypePermissionsRecursively<ModelDifference>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
            defaultRole.AddTypePermissionsRecursively<ModelDifferenceAspect>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
			defaultRole.AddTypePermissionsRecursively<ModelDifference>(SecurityOperations.Create, SecurityPermissionState.Allow);
            defaultRole.AddTypePermissionsRecursively<ModelDifferenceAspect>(SecurityOperations.Create, SecurityPermissionState.Allow);
        }
        return defaultRole;
    }
}
