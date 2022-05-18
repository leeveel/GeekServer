using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Geek.Server.Logic.Bag
{

    public class BagState : DBState
    {
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfDocuments)]
        public virtual StateMap<int, long> ItemMap { get; set; } = new StateMap<int, long>();

        public virtual StateList<int> ItemList { get; set; } = new StateList<int>();
    }


    public class BagComp : StateComponent<BagState>
    {
    }

}
