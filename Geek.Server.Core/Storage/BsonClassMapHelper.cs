using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Options;
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;

namespace Geek.Server.Core.Storage
{
    public class DictionaryRepresentationConvention : ConventionBase, IMemberMapConvention
    {
        private readonly DictionaryRepresentation _dictionaryRepresentation;

        public DictionaryRepresentationConvention(DictionaryRepresentation dictionaryRepresentation = DictionaryRepresentation.ArrayOfDocuments)
        {
            _dictionaryRepresentation = dictionaryRepresentation;
        }

        public void Apply(BsonMemberMap memberMap)
        {
            var serializer = memberMap.GetSerializer();
            if (serializer is IDictionaryRepresentationConfigurable dictionaryRepresentationConfigurable)
            {
                var reconfiguredSerializer = dictionaryRepresentationConfigurable.WithDictionaryRepresentation(_dictionaryRepresentation);
                memberMap.SetSerializer(reconfiguredSerializer);
            }
        }
    }

    public class EmptyContainerSerializeMethodConvention : ConventionBase, IMemberMapConvention
    {
        public void Apply(BsonMemberMap memberMap)
        {
            if (memberMap.MemberType.IsGenericType)
            {
                var genType = memberMap.MemberType.GetGenericTypeDefinition();
                if (genType == typeof(List<>))
                {
                    memberMap.SetShouldSerializeMethod(o =>
                    {
                        var value = memberMap.Getter(o);
                        if (value is IList list)
                        {
                            return list != null && list.Count > 0;
                        }
                        return true;
                    });
                } 
                 
                else if (genType == typeof(ConcurrentDictionary<,>) || genType == typeof(Dictionary<,>) )
                {
                    memberMap.SetShouldSerializeMethod(o =>
                    {
                        if (o != null)
                        {
                            var value = memberMap.Getter(o);
                            if (value != null)
                            {
                                PropertyInfo countProperty = value.GetType().GetProperty("Count");
                                if (countProperty != null)
                                {
                                    int count = (int)countProperty.GetValue(value, null);
                                    return count > 0;
                                }
                            }
                        }
                        return true;
                    });
                }
            }
        }
    }

    public static class BsonClassMapHelper
    {
        static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();
        static public void SetConvention()
        {
            ConventionRegistry.Register("DictionaryRepresentationConvention",
                new ConventionPack { new DictionaryRepresentationConvention(DictionaryRepresentation.ArrayOfDocuments) }, _ => true);

            ConventionRegistry.Register("EmptyContainerSerializeMethodConvention",
                new ConventionPack { new EmptyContainerSerializeMethodConvention() }, _ => true);
        }

        //提前注册,简化多态类型处理
        static public void RegisterAllClass(Assembly assembly)
        {
            var types = assembly.GetTypes();
            foreach (var t in types)
            {
                try
                {
                    if (!BsonClassMap.IsClassMapRegistered(t))
                    {
                        var bsonClassMap = new BsonClassMap(t);
                        bsonClassMap.AutoMap();
                        bsonClassMap.SetIgnoreExtraElements(true);
                        bsonClassMap.SetIgnoreExtraElementsIsInherited(true);
                        BsonClassMap.RegisterClassMap(bsonClassMap);
                    }
                }
                catch (Exception e)
                {
                    //LOGGER.Error(e);
                }
            }
        }
    }
}
