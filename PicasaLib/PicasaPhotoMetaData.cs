﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Deze code is gegenereerd met een hulpprogramma.
//     Runtime-versie:2.0.50727.5456
//
//     Als u wijzigingen aanbrengt in dit bestand, kan dit onjuist gedrag veroorzaken wanneer
//     de code wordt gegenereerd.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by xsd, Version=2.0.50727.1432.
// 
namespace PicasaLib {
    using System.Xml.Serialization;
    
    
    /// <opmerkingen/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.1432")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.w3.org/2005/Atom")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://www.w3.org/2005/Atom", IsNullable=false)]
    public partial class entry {
        
        private string titleField;
        
        private string summaryField;
        
        private entryCategory categoryField;
        
        /// <opmerkingen/>
        public string title {
            get {
                return this.titleField;
            }
            set {
                this.titleField = value;
            }
        }
        
        /// <opmerkingen/>
        public string summary {
            get {
                return this.summaryField;
            }
            set {
                this.summaryField = value;
            }
        }
        
        /// <opmerkingen/>
        public entryCategory category {
            get {
                return this.categoryField;
            }
            set {
                this.categoryField = value;
            }
        }
    }
    
    /// <opmerkingen/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.1432")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://www.w3.org/2005/Atom")]
    public partial class entryCategory {
        
        private string schemeField;
        
        private string termField;
        
        /// <opmerkingen/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string scheme {
            get {
                return this.schemeField;
            }
            set {
                this.schemeField = value;
            }
        }
        
        /// <opmerkingen/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string term {
            get {
                return this.termField;
            }
            set {
                this.termField = value;
            }
        }
    }
}
