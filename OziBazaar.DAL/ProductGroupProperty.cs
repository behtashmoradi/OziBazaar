//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OziBazaar.DAL
{
    using System;
    using System.Collections.Generic;
    
    public partial class ProductGroupProperty
    {
        public ProductGroupProperty()
        {
            this.ProductProperties = new HashSet<ProductProperty>();
        }
    
        public int ProductGroupPropertyID { get; set; }
        public int ProductGroupID { get; set; }
        public int PropertyID { get; set; }
        public string InitialValue { get; set; }
        public Nullable<short> TabOrder { get; set; }
        public byte[] Version { get; set; }
        public Nullable<bool> IsMandatory { get; set; }
    
        public virtual ProductGroup ProductGroup { get; set; }
        public virtual ICollection<ProductProperty> ProductProperties { get; set; }
        public virtual Property Property { get; set; }
    }
}
