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
    
    public partial class Memberauthorization
    {
        public System.Guid Id { get; set; }
        public Nullable<System.Guid> Memberid { get; set; }
        public Nullable<System.Guid> Feedbackproductid { get; set; }
        public bool Checked { get; set; }
    
        public virtual Feedbackproduct Feedbackproduct { get; set; }
        public virtual Members Members { get; set; }
    }
}