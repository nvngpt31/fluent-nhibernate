﻿using System.Diagnostics;
using System.Linq.Expressions;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Data;
using FluentNHibernate.FluentInterface;
using FluentNHibernate.Mapping;
using FluentNHibernate.Testing.Fixtures;
using NHibernate;
using NUnit.Framework;

namespace FluentNHibernate.Testing.DomainModel
{
    [TestFixture]
    public class ConnectedTester
    {
        private ISessionSource _source;

        [SetUp]
        public void SetUp()
        {
            var properties = new SQLiteConfiguration()
                .UseOuterJoin()
                //.ShowSql()
                .InMemory()
                .ToProperties();

            //var properties = MsSqlConfiguration
            //    .MsSql2005
            //    .ConnectionString
            //    .Server(".")
            //    .Database("FluentNHibernate")
            //    .TrustedConnection
            //    .CreateProperties
            //    .ToProperties();

            _source = new SingleConnectionSessionSourceForSQLiteInMemoryTesting(properties, new TestPersistenceModel());
            _source.BuildSchema();
        }

        [Test]
        public void Spin_up_the_Linq_stuff()
        {
            ISession session = _source.CreateSession();

            session.SaveOrUpdate(new Record{Name = "Jeremy", Age = 34});
            session.SaveOrUpdate(new Record{Name = "Jessica", Age = 29});
            session.SaveOrUpdate(new Record{Name = "Natalie", Age = 25});
            session.SaveOrUpdate(new Record{Name = "Hank", Age = 29});
            session.SaveOrUpdate(new Record{Name = "Darrell", Age = 34});
            session.SaveOrUpdate(new Record{Name = "Bill", Age = 34});
            session.SaveOrUpdate(new Record{Name = "Tim", Age = 35});
            session.SaveOrUpdate(new Record{Name = "Greg", Age = 36});

            //ISession session2 = _source.CreateSession();
            //var query = from record in session2.Linq<Record>() where record.Age < 30 select record;
            //foreach (Record record in query.ToList())
            //{
            //    Debug.WriteLine(record.Name);
            //}

            //return;

            Repository repository = new Repository(_source.CreateSession());
            Record[] records = repository.Query<Record>(record => record.Age == 29);
            //records.Length.ShouldEqual(2);

            foreach (var record in records)
            {
                Debug.WriteLine(record.Name);
            }
        }

        [Test]
        public void QueryBy_test()
        {
            ISession session = _source.CreateSession();

            session.SaveOrUpdate(new Record { Name = "Jeremy", Age = 34 });
            session.SaveOrUpdate(new Record { Name = "Jessica", Age = 29 });
            session.SaveOrUpdate(new Record { Name = "Natalie", Age = 25 });
            session.SaveOrUpdate(new Record { Name = "Hank", Age = 29 });
            session.SaveOrUpdate(new Record { Name = "Darrell", Age = 34 });
            session.SaveOrUpdate(new Record { Name = "Bill", Age = 34 });
            session.SaveOrUpdate(new Record { Name = "Tim", Age = 35 });
            session.SaveOrUpdate(new Record { Name = "Greg", Age = 36 });

            Repository repository = new Repository(_source.CreateSession());
            Record record = repository.FindBy<Record, string>(r => r.Name, "Hank");
            record.Name.ShouldEqual("Hank");
        }

        [Test]
        public void Query_by_filters()
        {
            ISession session = _source.CreateSession();

            session.SaveOrUpdate(new Record { Name = "Jeremy", Age = 34 });
            session.SaveOrUpdate(new Record { Name = "Jeremy", Age = 35 });
            session.SaveOrUpdate(new Record { Name = "Jessica", Age = 29 });
            session.SaveOrUpdate(new Record { Name = "Natalie", Age = 25 });
            session.SaveOrUpdate(new Record { Name = "Hank", Age = 29 });
            session.SaveOrUpdate(new Record { Name = "Darrell", Age = 34 });
            session.SaveOrUpdate(new Record { Name = "Bill", Age = 34 });
            session.SaveOrUpdate(new Record { Name = "Tim", Age = 35 });
            session.SaveOrUpdate(new Record { Name = "Greg", Age = 36 });

            Repository repository = new Repository(_source.CreateSession());
            Record record = repository.FindBy<Record>(r => r.Age == 34 && r.Name == "Jeremy");
            record.Name.ShouldEqual("Jeremy");
            record.Age.ShouldEqual(34);
        }

        [Test]
        public void Query_with_multiple_expressions()
        {
            ISession session = _source.CreateSession();

            session.SaveOrUpdate(new Record { Name = "Jeremy", Age = 34 });
            session.SaveOrUpdate(new Record { Name = "Jeremy", Age = 35 });

            Repository repository = new Repository(_source.CreateSession());
            Record[] records = repository.Query<Record>(r => r.Age >= 35 && r.Name == "Jeremy");

            records.ShouldHaveCount(1);
            records[0].Name.ShouldEqual("Jeremy");
            records[0].Age.ShouldEqual(35);
        }

        [Test]
        public void Save_and_find()
        {
            Repository repository1 = new Repository(_source.CreateSession());
            var record1 = new Record { Name = "Jeremy", Age = 34 };
            repository1.Save(record1);

            Repository repository2 = new Repository(_source.CreateSession());
            Record record2 = repository2.Find<Record>(record1.Id);

            record1.ShouldNotBeTheSameAs(record2);
            record2.Name.ShouldEqual("Jeremy");
        }

        [Test]
        public void MappingTest1()
        {
            new PersistenceSpecification<Record>(_source)
                .CheckProperty(r => r.Age, 22)
                .CheckProperty(r => r.Name, "somebody")
                .CheckProperty(r => r.Location, "somewhere")
                .VerifyTheMappings();
        }

        [Test]
        public void Mapping_test_with_arrays()
        {
            new PersistenceSpecification<BinaryRecord>(_source)
                .CheckProperty(r => r.BinaryValue, new byte[] { 1, 2, 3 })
                .VerifyTheMappings();
        }
        [Test]
        public void CanWorkWithNestedSubClasses()
        {
            new PersistenceSpecification<Child2Record>(_source)
                .CheckProperty(r => r.Name, "Foxy")
                .CheckProperty(r => r.Another, "Lady")
                .CheckProperty(r => r.Third, "Yeah")
                .VerifyTheMappings();
        }
    }

    public class RecordMap : ClassMap<Record>
    {
        public RecordMap()
        {
            UseIdentityForKey(x => x.Id, "id");
            Map(x => x.Name);
            Map(x => x.Age);
            Map(x => x.Location);
        }
    }

    public class NestedSubClassMap : ClassMap<SuperRecord>
    {
        public NestedSubClassMap()
        {
            Id(x => x.Id);
            Map(x => x.Name);
            DiscriminateSubClassesOnColumn<string>("Type")
                .SubClass<ChildRecord>(sc =>
                {
                    sc.Map(x => x.Another);
                    sc.SubClass<Child2Record>(sc2 =>
                        sc2.Map(x => x.Third));
                });
        }
    }

    public class SuperRecord  : Entity
    {
        public virtual string Name { get; set; }
    }

    public class ChildRecord : SuperRecord
    {
        public virtual string Another { get; set; }
    }

    public class Child2Record : ChildRecord
    {
        public virtual string Third { get; set; }
    }

    public class Record : Entity
    {
        public virtual string Name { get; set; }
        public virtual int Age { get; set; }
        public virtual string Location { get; set; }
    }

    public class BinaryRecordMap : ClassMap<BinaryRecord>
    {
        public BinaryRecordMap()
        {
            UseIdentityForKey(x => x.Id, "id");
            Map(x => x.BinaryValue).Not.Nullable();
        }
    }

    public class BinaryRecord : Entity
    {
        public virtual byte[] BinaryValue { get; set; }
    }
}
