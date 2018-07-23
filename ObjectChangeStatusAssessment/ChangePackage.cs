using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ObjectChangeStatusAssessment
{
    public class ChangePackage<TKey>
    {
        public List<IChangeTrackable<TKey>> OwnedEntities { get; set; } = new List<IChangeTrackable<TKey>>();

        public List<ChangedRelationShip<TKey>> Relationships { get; set; } = new List<ChangedRelationShip<TKey>>();


    }
    public class OwnershipActions
    {
        public bool Add { get; set; }
        public bool Delete { get; set; }
        public bool Update { get; set; }
    }

    public class ChangedRelationShip<TKey>
    {
        public ChangedRelationShip(IChangeTrackable<TKey> parent, PropertyInfo property, OwnershipActions ownershipActions )
        {
            Parent = parent;
            Property = property;
            OwnershipActions = ownershipActions ?? new OwnershipActions();
        }
        /// <summary>
        /// When the record is new and is added as a relationship it will be flagged as added.
        /// This is in the case of a new entity that is added through the parent, but is not owned by the parent
        /// </summary>
        public OwnershipActions OwnershipActions { get; private set; }
        public IChangeTrackable<TKey> Parent { get; private set; }
        public PropertyInfo Property { get; private set; }
        public IChangeTrackable<TKey> Value { get; private set; }
        public ChangeType ChangeType { get; private set; }
        public ChangedRelationShip<TKey> CloneFor(IChangeTrackable<TKey> value, ChangeType changeType)
        {
            return new ChangedRelationShip<TKey>(Parent, Property, OwnershipActions)
            {
                ChangeType = changeType,
                Value = value
            };
        }
    }
}
