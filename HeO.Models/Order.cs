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
    
    public partial class Order
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Order()
        {
            this.Orderfaceooklist = new HashSet<Orderfaceooklist>();
        }
    
        public System.Guid Orderid { get; set; }
        public string Ordernumber { get; set; }
        public Nullable<System.Guid> Memberid { get; set; }
        public string HDZoredernumber { get; set; }
        public string Url { get; set; }
        public bool Isreal { get; set; }
        public string Service { get; set; }
        public string Message { get; set; }
        public Nullable<int> Count { get; set; }
        public Nullable<int> Remains { get; set; }
        public double Cost { get; set; }
        public int OrderStatus { get; set; }
        public Nullable<System.DateTime> Duedate { get; set; }
        public Nullable<System.DateTime> Updatedate { get; set; }
        public System.DateTime Createdate { get; set; }
    
        public virtual Members Members { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Orderfaceooklist> Orderfaceooklist { get; set; }
    }
}
