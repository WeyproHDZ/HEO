//------------------------------------------------------------------------------
// <auto-generated>
//     這個程式碼是由範本產生。
//
//     對這個檔案進行手動變更可能導致您的應用程式產生未預期的行為。
//     如果重新產生程式碼，將會覆寫對這個檔案的手動變更。
// </auto-generated>
//------------------------------------------------------------------------------

namespace HeO.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Members
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Members()
        {
            this.Feedbackrecord = new HashSet<Feedbackrecord>();
            this.Memberauthorization = new HashSet<Memberauthorization>();
            this.Memberloginrecord = new HashSet<Memberloginrecord>();
            this.Order = new HashSet<Order>();
            this.Orderfaceooklist = new HashSet<Orderfaceooklist>();
            this.Viprecord = new HashSet<Viprecord>();
        }
    
        public int Isenable { get; set; }
        public System.Guid Memberid { get; set; }
        public Nullable<System.Guid> Levelid { get; set; }
        public string Facebookid { get; set; }
        public string Name { get; set; }
        public string Account { get; set; }
        public string Password { get; set; }
        public string Facebooklink { get; set; }
        public int Facebookstatus { get; set; }
        public double Feedbackmoney { get; set; }
        public string Useragent_com { get; set; }
        public string Useragent_phone { get; set; }
        public bool Isreal { get; set; }
        public int Logindate { get; set; }
        public int Lastdate { get; set; }
        public System.DateTime Createdate { get; set; }
        public Nullable<System.DateTime> Updatedate { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Feedbackrecord> Feedbackrecord { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Memberauthorization> Memberauthorization { get; set; }
        public virtual Memberlevel Memberlevel { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Memberloginrecord> Memberloginrecord { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Order> Order { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Orderfaceooklist> Orderfaceooklist { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Viprecord> Viprecord { get; set; }
    }
}
