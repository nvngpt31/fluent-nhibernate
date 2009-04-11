﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Xml;
using FluentNHibernate.FluentInterface;
using FluentNHibernate.Mapping;
using FluentNHibernate.Utils;
using NHibernate.Properties;
using NHibernate.SqlTypes;
using NHibernate.Type;
using NHibernate.UserTypes;
using NUnit.Framework;
using FluentNHibernate;
using PropertyMap=FluentNHibernate.Mapping.PropertyMap;

namespace FluentNHibernate.Testing.DomainModel.Mapping
{
    [TestFixture]
    public class PropertyMapTester
    {
        [Test]
        public void SetAttributeOnColumnElement()
        {
            PropertyInfo property = ReflectionHelper.GetProperty<PropertyTarget>(x => x.Name);
            var map = new PropertyMap(property, false, typeof(PropertyTarget));
            map.SetAttributeOnColumnElement("unique", "true");

            var document = new XmlDocument();
            XmlElement classElement = document.CreateElement("root");
            map.Write(classElement, new MappingVisitor());

            var columnElement = (XmlElement) classElement.SelectSingleNode("property/column");
            columnElement.AttributeShouldEqual("unique", "true");
        }

        [Test]
        public void Map_WithoutColumnName_UsesPropertyNameFor_PropertyColumnAttribute()
        {
            new MappingTester<PropertyTarget>()
                .ForMapping(m => m.Map(x => x.Name))
                .Element("class/property[@name='Name']/column")
                    .HasAttribute("name", "Name");
        }

        [Test]
        public void Map_WithoutColumnName_UsesPropertyNameFor_ColumnNameAttribute()
        {
            var classMap = new ClassMap<PropertyTarget>();

            classMap.Map(x => x.Name);

            var document = classMap.CreateMapping(new MappingVisitor());

            // attribute on property
            var classElement = document.DocumentElement.SelectSingleNode("class");
            var columnElement = (XmlElement)classElement.SelectSingleNode("property/column");
            columnElement.AttributeShouldEqual("name", "Name");
        }


        [Test]
        public void Map_WithColumnName_UsesColumnNameFor_PropertyColumnAttribute()
        {
            new MappingTester<PropertyTarget>()
                .ForMapping(m => m.Map(x => x.Name, "column_name"))
                .Element("class/property[@name='Name']/column")
                    .HasAttribute("name", "column_name");
        }

        [Test]
        public void Map_WithColumnName_UsesColumnNameFor_ColumnNameAttribute()
        {
            var classMap = new ClassMap<PropertyTarget>();

            classMap.Map(x => x.Name, "column_name");

            var document = classMap.CreateMapping(new MappingVisitor());

            // attribute on property
            var classElement = document.DocumentElement.SelectSingleNode("class");
            var columnElement = (XmlElement)classElement.SelectSingleNode("property/column");
            columnElement.AttributeShouldEqual("name", "column_name");
        }
        
        [Test]
        public void Map_WithFluentColumnName_UsesColumnNameFor_ColumnNameAttribute()
        {
            var classMap = new ClassMap<PropertyTarget>();

            classMap.Map(x => x.Name)
                .ColumnNames.Add("column_name");

            var document = classMap.CreateMapping(new MappingVisitor());

            // attribute on property
            var classElement = document.DocumentElement.SelectSingleNode("class");
            var columnElement = (XmlElement)classElement.SelectSingleNode("property/column");
            columnElement.AttributeShouldEqual("name", "column_name");
        }

        private MappingTester<T> Model<T>(Action<ClassMap<T>> mapping)
        {
            return new MappingTester<T>()
                .ForMapping(mapping);
        }

        [Test]
        public void ShouldAddOnlyOneColumnWhenNotSpecified()
        {
            Model<PropertyTarget>(m => m.Map(x => x.Name))
                .Element("class/property[@name='Name']").HasThisManyChildNodes(1);
        }

        [Test]
        public void ShouldAddAllColumns()
        {
            Model<PropertyTarget>(m => m.Map(x => x.Name).ColumnNames.Add("one", "two", "three"))
                .Element("class/property[@name='Name']").HasThisManyChildNodes(3)
                .Element("class/property[@name='Name']/column[@name='one']").Exists()
                .Element("class/property[@name='Name']/column[@name='two']").Exists()
                .Element("class/property[@name='Name']/column[@name='three']").Exists();
        }

        [Test]
        public void ColumnName_IsPropertyName_WhenNoColumnNameGiven()
        {
            Model<PropertyTarget>(m => m.Map(x => x.Name))
                .Element("class/property[@name='Name']/column")
                .HasAttribute("name", "Name");
        }

        [Test]
        public void ColumnName_IsColumnName_WhenColumnNameGiven()
        {
            Model<PropertyTarget>(m => m.Map(x => x.Name, "column_name"))
                .Element("class/property[@name='Name']/column")
                .HasAttribute("name", "column_name");
        }

        [Test]
        public void ColumnName_IsColumnName_WhenColumnNameFluentGiven()
        {
            Model<PropertyTarget>(m => m.Map(x => x.Name).ColumnNames.Add("column_name"))
                .Element("class/property[@name='Name']/column")
                .HasAttribute("name", "column_name");
        }

        [Test]
        public void Map_WithFluentLength_OnString_UsesWithLengthOf_PropertyColumnAttribute()
        {
            new MappingTester<PropertyTarget>()
                .ForMapping(m => m.Map(x => x.Name).WithLengthOf(20))
                .Element("class/property[@name='Name']/column").HasAttribute("length", "20");
        }

		[Test]
		public void Map_WithFluentLength_OnDecimal_UsesWithLengthOf_PropertyColumnAttribute()
		{
		    new MappingTester<PropertyTarget>()
		        .ForMapping(m => m.Map(x => x.DecimalProperty).WithLengthOf(1))
		        .Element("class/property[@name='DecimalProperty']/column").HasAttribute("length", "1");
		}

        [Test]
        public void Map_WithFluentLength_AllowOnAnything_PropertyColumnAttribute()
        {
            new MappingTester<PropertyTarget>()
                .ForMapping(m => m.Map(x => x.Id).WithLengthOf(20))
                .Element("class/property[@name='Id']/column").HasAttribute("length", "20");
        }

        [Test]
        public void Map_UsesCanNotBeNull_PropertyColumnAttribute()
        {
            new MappingTester<PropertyTarget>()
                .ForMapping(m => m.Map(x => x.Name).Not.Nullable())
                .Element("class/property[@name='Name']").DoesntHaveAttribute("not-null")
                .Element("class/property[@name='Name']/column").HasAttribute("not-null", "true");
        }

        [Test]
        public void Map_UsesAsReadOnly_PropertyColumnAttribute()
        {
            var classMap = new ClassMap<PropertyTarget>();

            classMap.Map(x => x.Name)
                .ReadOnly();

            var document = classMap.CreateMapping(new MappingVisitor());

            // attribute on property
            var classElement = document.DocumentElement.SelectSingleNode("class");
            var propertyElement = (XmlElement)classElement.SelectSingleNode("property");
            propertyElement.AttributeShouldEqual("insert", "false");
            propertyElement.AttributeShouldEqual("update", "false");
        }

        [Test]
        public void Map_UsesUniqueKey_PropertyColumnAttribute()
        {
            var classMap = new ClassMap<PropertyTarget>();

            classMap.Map(x => x.Name)
                .UniqueKey("uniqueKey");

            var document = classMap.CreateMapping(new MappingVisitor());

            // attribute on property
            var classElement = document.DocumentElement.SelectSingleNode("class");
            var propertyElement = (XmlElement)classElement.SelectSingleNode("property");
            propertyElement.AttributeShouldEqual("unique-key", "uniqueKey");
        }

        [Test]
        public void Map_UsesNotReadOnly_PropertyColumnAttribute()
        {
            var classMap = new ClassMap<PropertyTarget>();

            classMap.Map(x => x.Name)
                .Not.ReadOnly();

            var document = classMap.CreateMapping(new MappingVisitor());

            // attribute on property
            var classElement = document.DocumentElement.SelectSingleNode("class");
            var propertyElement = (XmlElement)classElement.SelectSingleNode("property");
            propertyElement.AttributeShouldEqual("insert", "true");
            propertyElement.AttributeShouldEqual("update", "true");
        }

        [Test]
        public void Map_WithFluentFormula_UsesFormula() 
        {
            new MappingTester<PropertyTarget>()
                .ForMapping(m => m.Map(x => x.Name).FormulaIs("foo(bar)"))
                .Element("class/property").HasAttribute("formula", "foo(bar)");
        }

        [Test]
        public void CanSpecifyCustomType()
        {
            new MappingTester<PropertyTarget>()
                .ForMapping(c=>c.Map(x => x.Data).CustomTypeIs("BinaryBlob"))
                .Element("class/property").HasAttribute("type", "BinaryBlob");
        }

        [Test]
        public void CanSpecifyCustomTypeAsDotNetTypeGenerically()
        {
            new MappingTester<PropertyTarget>()
                .ForMapping(c => c.Map(x => x.Data).CustomTypeIs<custom_type_for_testing>())
                .Element("class/property").HasAttribute("type", typeof(custom_type_for_testing).AssemblyQualifiedName);
        }

        [Test]
        public void CanSpecifyCustomTypeAsDotNetType()
        {
            new MappingTester<PropertyTarget>()
                .ForMapping(c=>c.Map(x => x.Data).CustomTypeIs(typeof (custom_type_for_testing)))
                .Element("class/property").HasAttribute("type", typeof(custom_type_for_testing).AssemblyQualifiedName);
        }

        [Test]
        public void CanSpecifyCustomSqlType()
        {
            var classMap = new ClassMap<PropertyTarget>();
            var propertyMap = classMap.Map(x => x.Data)
                .CustomSqlTypeIs("image");

            new MappingTester<PropertyTarget>()
                .ForMapping(classMap)
                .Element("class/property/column").HasAttribute("sql-type", "image");
        }

        #region Custom IUserType impl for testing
        public class custom_type_for_testing : IUserType
        {
            public bool Equals(object x, object y)
            {
                throw new System.NotImplementedException();
            }

            public int GetHashCode(object x)
            {
                throw new System.NotImplementedException();
            }

            public object NullSafeGet(IDataReader rs, string[] names, object owner)
            {
                throw new System.NotImplementedException();
            }

            public void NullSafeSet(IDbCommand cmd, object value, int index)
            {
                throw new System.NotImplementedException();
            }

            public object DeepCopy(object value)
            {
                throw new System.NotImplementedException();
            }

            public object Replace(object original, object target, object owner)
            {
                throw new System.NotImplementedException();
            }

            public object Assemble(object cached, object owner)
            {
                throw new System.NotImplementedException();
            }

            public object Disassemble(object value)
            {
                throw new System.NotImplementedException();
            }

            public SqlType[] SqlTypes
            {
                get { throw new System.NotImplementedException(); }
            }

            public Type ReturnedType
            {
                get { throw new System.NotImplementedException(); }
            }

            public bool IsMutable
            {
                get { throw new System.NotImplementedException(); }
            }
        }
        #endregion
    }

    public class PropertyTarget
    {
        public string Name { get; set; }
        public PropertyReferenceTarget Reference { get; set; }
        public IList<PropertyReferenceTarget> References { get; set; }
        public ComponentTarget Component { get; set; }
        public IList<ComponentTarget> Components { get; set; }
        public int Id { get; set; }
        public byte[] Data { get; set; }
		public decimal DecimalProperty { get; set; }
        public IDictionary ExtensionData { get; set; }
    }

    public class PropertyReferenceTarget {}
    
    public class ComponentTarget
    {
        public PropertyTarget MyParent { get; set; }
        public object Name { get; set; }
    }

    public class FakePropertyAccessor : IPropertyAccessor
    {
        public IGetter GetGetter(Type theClass, string propertyName)
        {
            throw new NotImplementedException();
        }

        public ISetter GetSetter(Type theClass, string propertyName)
        {
            throw new NotImplementedException();
        }

        public bool CanAccessTroughReflectionOptimizer
        {
            get { throw new NotImplementedException(); }
        }

        public bool CanAccessThroughReflectionOptimizer
        {
            get { throw new NotImplementedException(); }
        }

    }
}
