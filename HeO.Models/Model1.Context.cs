﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class HeOEntities : DbContext
    {
        public HeOEntities()
            : base("name=HeOEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<AdminLims> AdminLims { get; set; }
        public virtual DbSet<Admins> Admins { get; set; }
        public virtual DbSet<Feedbackproduct> Feedbackproduct { get; set; }
        public virtual DbSet<Guide> Guide { get; set; }
        public virtual DbSet<Lims> Lims { get; set; }
        public virtual DbSet<Memberlevelcooldown> Memberlevelcooldown { get; set; }
        public virtual DbSet<Returnstatus> Returnstatus { get; set; }
        public virtual DbSet<Setting> Setting { get; set; }
        public virtual DbSet<Term> Term { get; set; }
        public virtual DbSet<Memberauthorization> Memberauthorization { get; set; }
        public virtual DbSet<sysdiagrams> sysdiagrams { get; set; }
        public virtual DbSet<News> News { get; set; }
        public virtual DbSet<Feedbackdetail> Feedbackdetail { get; set; }
        public virtual DbSet<Orderfaceooklist> Orderfaceooklist { get; set; }
        public virtual DbSet<Newslang> Newslang { get; set; }
        public virtual DbSet<Vipdetail> Vipdetail { get; set; }
        public virtual DbSet<Viprecord> Viprecord { get; set; }
        public virtual DbSet<Thread> Thread { get; set; }
        public virtual DbSet<Guidelang> Guidelang { get; set; }
        public virtual DbSet<Termlang> Termlang { get; set; }
        public virtual DbSet<Servicelog> Servicelog { get; set; }
        public virtual DbSet<Memberlevel> Memberlevel { get; set; }
        public virtual DbSet<Order> Order { get; set; }
        public virtual DbSet<Feedbackrecord> Feedbackrecord { get; set; }
        public virtual DbSet<Members> Members { get; set; }
    }
}
