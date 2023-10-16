using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecurityUser.Module.BusinessObjects {
    [DefaultClassOptions, ImageName("BO_Task")]
    public class EmployeeTask : BaseObject {
        public EmployeeTask(Session session)
            : base(session) { }
        private Employee owner;
        [Association("Employee-Task")]
        public Employee Owner {
            get { return owner; }
            set { SetPropertyValue(nameof(Owner), ref owner, value); }
        }
        string _subject;
        public string Subject {
            get {
                return _subject;
            }
            set {
                SetPropertyValue(nameof(Subject), ref _subject, value);
            }
        }
    }
}
