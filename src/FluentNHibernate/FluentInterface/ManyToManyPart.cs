using System;
using System.Reflection;
using FluentNHibernate.MappingModel;
using FluentNHibernate.MappingModel.Collections;

namespace FluentNHibernate.FluentInterface
{
    public class ManyToManyPartOld<PARENT, CHILD> : IDeferredCollectionMapping
    {
        private readonly PropertyInfo _info;
        private readonly AttributeStore<ICollectionMapping> _attributes;

        private Func<ICollectionMapping> _collectionBuilder;

        public ManyToManyPartOld(PropertyInfo info)
        {
            _info = info;
            _attributes = new AttributeStore<ICollectionMapping>();
            AsBag();   
        }

        public ManyToManyPartOld<PARENT, CHILD> AsBag()
        {
            _collectionBuilder = () => new BagMapping();
            return this;
        }

        public ManyToManyPartOld<PARENT, CHILD> AsSet()
        {
            _collectionBuilder = () => new SetMapping();
            return this;
        }

        public ICollectionMapping ResolveCollectionMapping()
        {
            var collection = _collectionBuilder();
            _attributes.CopyTo(collection.Attributes);

            collection.PropertyInfo = _info;
            collection.Key = new KeyMapping();
            collection.Contents = new ManyToManyMapping { ChildType = typeof(CHILD) };

            return collection;
        }
    }
}