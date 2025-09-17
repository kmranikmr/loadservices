using DataAnalyticsPlatform.Shared;
using DataAnalyticsPlatform.Shared.DataAccess;
using System;
using System.Collections.Generic;

namespace DataAnalyticsPlatform.Common
{
    public interface IBaseModel
    {
        string ModelName { get; set; }
        string[] Props { get; set; }
        object[] Values { get; set; }
    }

    public interface IModelMap
    {
        void MapIt();
        List<BaseModel> GetModels();
    }

    public abstract class Entity
    {
        // Base entity class
    }

    public class BaseModel : IBaseModel
    {
        public string ModelName { get; set; }
        public long RecordId { get; set; }
        public long FileId { get; set; }
        public string[] Props { get; set; }
        public object[] Values { get; set; }
    }

    public partial class OriginalRecord
    {
        // Base class for original records
    }

    // Model Map classes for XML serialization
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class ModelMap
    {
        private ModelMapRecord recordField;
        private byte versionField;

        [System.Xml.Serialization.XmlElementAttribute()]
        public ModelMapRecord record
        {
            get { return this.recordField; }
            set { this.recordField = value; }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public byte version
        {
            get { return this.versionField; }
            set { this.versionField = value; }
        }
    }

    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ModelMapRecord
    {
        private ModelMapRecordModel[] modelField;
        private string nameField;

        [System.Xml.Serialization.XmlElementAttribute("Model")]
        public ModelMapRecordModel[] Model
        {
            get { return this.modelField; }
            set { this.modelField = value; }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get { return this.nameField; }
            set { this.nameField = value; }
        }
    }

    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ModelMapRecordModel
    {
        private ModelMapRecordModelProp[] propField;
        private string nameField;

        [System.Xml.Serialization.XmlElementAttribute("prop")]
        public ModelMapRecordModelProp[] prop
        {
            get { return this.propField; }
            set { this.propField = value; }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get { return this.nameField; }
            set { this.nameField = value; }
        }
    }

    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class ModelMapRecordModelProp
    {
        private string transformField;
        private string nameField;

        public string transform
        {
            get { return this.transformField; }
            set { this.transformField = value; }
        }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name
        {
            get { return this.nameField; }
            set { this.nameField = value; }
        }
    }
}
