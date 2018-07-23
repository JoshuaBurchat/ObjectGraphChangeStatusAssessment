using DeepEqual;
using DeepEqual.Syntax;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ObjectChangeStatusAssessment
{

    /// <summary>
    /// The change assessor is used to assess an object trees changes and flag the object accordingly.
    /// (Updated, Deleted, Added, None)
    /// This can be helpful when comparing your crud update processes against your persisted models.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class ChangeAssessor<TKey>
    {

        /// <summary>
        /// Configured list if property names to ignore on any type if found, used if you have standard naming conventions
        /// </summary>
        private HashSet<string> _ignoreAllPropertiesByName = new HashSet<string>();
        /// <summary>
        /// Configurated list of types to have all properties ignored, for example maybe you want created by and created timestamp to always be ignored
        /// because they are always set on update. So you can have an interface for IChangeTracking { Cby, CTimestamp etc }
        /// </summary>
        private List<Type> _ignoreAllPropertiesFromTypes = new List<Type>();
        /// <summary>
        /// Type information sorted for easy access when executing the comparison
        /// </summary>
        protected Dictionary<Type, MappedTypeDetails> _mappedTypeDetails = new Dictionary<Type, MappedTypeDetails>();

        public ChangeAssessor()
        {
            //Always ignore the change track required fields as they are used as flags and should not be compared
            _ignoreAllPropertiesFromTypes.Add(typeof(IChangeTrackable<TKey>));
        }

        //TODO maybe allow the configuration of specific properties from specific types
        /// <summary>
        /// Configurated a type to have all properties ignored, for example maybe you want created by and created timestamp to always be ignored
        /// because they are always set on update. So you can have an interface for IChangeTracking { Cby, CTimestamp etc }
        /// </summary>
        /// <typeparam name="T">type to have its properties ignored</typeparam>
        /// <returns></returns>
        public ChangeAssessor<TKey> IgnoreFields<T>() where T : class
        {
            _ignoreAllPropertiesFromTypes.Add(typeof(T));
            return this;
        }




        /// <summary>
        /// Configured list if property names to ignore on any type if found, used if you have standard naming conventions
        /// </summary>
        public ChangeAssessor<TKey> IgnoreFields(params string[] fields)
        {
            foreach (var field in fields)
                _ignoreAllPropertiesByName.Add(field);
            return this;
        }

        /// <summary>
        /// Contains a list of mappings indicating that that parent object in the tree has access to add, delete and update 
        /// the child elements
        /// </summary>
        private Dictionary<Tuple<Type, PropertyInfo>, OwnershipActions> _ownerMappings = new Dictionary<Tuple<Type, PropertyInfo>, OwnershipActions>();

        private PropertyInfo ResolvePropertyInfo<T>(Expression<Func<T, object>> property)
        {
            var body = property.Body;
            if (body.NodeType == ExpressionType.Convert)
                body = ((UnaryExpression)body).Operand;
            var propertyInfo = (body as MemberExpression).Member as PropertyInfo;
            return propertyInfo;
        }

        /// <summary>
        /// By adding an owner mapping you will be changing how elements get flagged.
        /// By being the owner the parent object in the tree can flag sub properties as added, removed or deleted
        /// </summary>
        public ChangeAssessor<TKey> AddOwnerMapping<T>(Expression<Func<T, object>> property, bool add = true, bool update = true, bool delete = true) where T : class
        {
            var propertyInfo = ResolvePropertyInfo<T>(property);
            OwnershipActions ownershipActions = null;
            var key = new Tuple<Type, PropertyInfo>(typeof(T), propertyInfo);
            if (!_ownerMappings.TryGetValue(key, out ownershipActions))
            {
                ownershipActions = new OwnershipActions()
                {
                    Add = add,
                    Delete = delete,
                    Update = update
                };
                _ownerMappings.Add(key, ownershipActions);
            }
            return this;
        }

        private ChangedRelationShip<TKey> BuildChangeRelationshipTemplate(IChangeTrackable<TKey> parent, PropertyInfo propertyInfo)
        {
            OwnershipActions ownershipActions = null;
            var key = new Tuple<Type, PropertyInfo>(parent.GetType(), propertyInfo);
            _ownerMappings.TryGetValue(key, out ownershipActions);
            return new ChangedRelationShip<TKey>(parent, propertyInfo, ownershipActions);
        }



        /// <summary>
        /// Will compare the source (potential updated object) and the destination (persisted object)
        /// If the object differs, or its sub properties are changed they will have the proper ChangeType assigned
        /// (Added, Updated, Deleted, None)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">(potential updated object)</param>
        /// <param name="destination">(persisted object)</param>
        /// <returns>Returns an array of all changed objects from the source objects tree</returns>
        public ChangePackage<TKey> SetChangeStatus<T>(T source, T destination) where T : IChangeTrackable<TKey>
        {
            this.SetupConfigurationForType(typeof(T));
            //Execute right away with array, TODO maybe I should get rid of the yields as we would want this to execute right away

            ChangePackage<TKey> changes = new ChangePackage<TKey>();
            SetChangeStatusInternal(
                new ChangedRelationShip<TKey>(null, null, new OwnershipActions()
                {
                    Add = true,
                    Update = true,
                    Delete = true
                }),
                source,
                destination,
                new HashSet<object>(),
                changes);
            return changes;
        }

        /// <summary>
        /// Will compare the source (potential updated object) and the destination (persisted object)
        /// If the object differs, or its sub properties are changed they will have the proper ChangeType assigned
        /// (Added, Updated, Deleted, None)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">(potential updated object)</param>
        /// <param name="destination">(persisted object)</param>
        /// <returns>Returns an array of all changed objects from the source objects tree</returns>
        public ChangePackage<TKey> SetChangeStatusList<T>(IList<T> list, IList<T> compareWith) where T : IChangeTrackable<TKey>
        {
            this.SetupConfigurationForType(typeof(T));
            ChangePackage<TKey> changes = new ChangePackage<TKey>();
            GetListChangesInternal(
                new ChangedRelationShip<TKey>(null, null, new OwnershipActions()
                {
                    Add = true,
                    Update = true,
                    Delete = true
                }),
                ((IEnumerable<IChangeTrackable<TKey>>)list ?? new IChangeTrackable<TKey>[0]).Cast<IChangeTrackable<TKey>>(),
                ((IEnumerable<IChangeTrackable<TKey>>)compareWith ?? new IChangeTrackable<TKey>[0]).Cast<IChangeTrackable<TKey>>(), new HashSet<object>(),
                changes);
            return changes;
        }


        /// <summary>
        /// Internally called in order to pass around a list of already checked objects. This will help avoid circular references
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="main"></param>
        /// <param name="compareWith"></param>
        /// <param name="alreadyChecked"></param>
        /// <returns></returns>
        private void SetChangeStatusInternal<T>(ChangedRelationShip<TKey> relationshipTemplate, T main, T compareWith, HashSet<object> alreadyChecked, ChangePackage<TKey> currentPackage) where T : IChangeTrackable<TKey>
        {

            //TODO break this function up... alot
            if (alreadyChecked.Contains(main)) return;

            alreadyChecked.Add(main);

            //TODO debating if this is meaningful, and whether or not this is the correct approach to a removed single object property
            //For example currently if you have a sub property 
            //source { c : {} }, destination { c : null }, this is considered an add
            //source { c : null }, destination { c : {} }, this is considered a delete
            //But this might be a foreign key and shouldnt be flagged as deleted?? or maybe just in regards to the relationship? 
            //There may need to be configuration for this relationship check per property
            if (main == null && compareWith == null)
            {
                return;
            }
            else if (main != null && compareWith != null)
            {
                //When the id of a single property changes its both a delete and an add
                //To avoid this results the user of the API should ignore object properties if they simply need to
                // check the foreign key
                if (relationshipTemplate.Parent != null && !_keyEqualsDelegate(main.Id, compareWith.Id))
                {
                    if (relationshipTemplate.OwnershipActions.Delete)
                    {
                        compareWith.ChangeType = ChangeType.Deleted;
                        currentPackage.OwnedEntities.Add(compareWith);
                        currentPackage.Relationships.Add(relationshipTemplate.CloneFor(compareWith, ChangeType.Deleted));
                    }
                    else
                    {
                        currentPackage.Relationships.Add(relationshipTemplate.CloneFor(compareWith, ChangeType.Deleted));
                    }
                    if (relationshipTemplate.OwnershipActions.Add)
                    {
                        main.ChangeType = ChangeType.Added;
                        currentPackage.OwnedEntities.Add(main);
                        currentPackage.Relationships.Add(relationshipTemplate.CloneFor(main, ChangeType.Added));
                    }
                    else
                    {
                        currentPackage.Relationships.Add(relationshipTemplate.CloneFor(main, ChangeType.Added));
                    }
                }
            }
            else if (main == null && compareWith != null)
            {
                if (relationshipTemplate.OwnershipActions.Delete)
                {
                    compareWith.ChangeType = ChangeType.Deleted;
                    currentPackage.OwnedEntities.Add(compareWith);
                    currentPackage.Relationships.Add(relationshipTemplate.CloneFor(compareWith, ChangeType.Deleted));
                }
                else
                {
                    currentPackage.Relationships.Add(relationshipTemplate.CloneFor(compareWith, ChangeType.Deleted));
                }
                return;
            }
            else if (compareWith == null && main != null)
            {
                if (relationshipTemplate.OwnershipActions.Add)
                {
                    main.ChangeType = ChangeType.Added;
                    currentPackage.OwnedEntities.Add(main);
                    currentPackage.Relationships.Add(relationshipTemplate.CloneFor(main, ChangeType.Added));
                }
                else
                {
                    currentPackage.Relationships.Add(relationshipTemplate.CloneFor(main, ChangeType.Added));
                }
                return;
            }

            var details = _mappedTypeDetails[main.GetType()];

            //Only check the object if the parent owns this property and can make changes to it
            if (relationshipTemplate.OwnershipActions.Update)
            {
                //TODO most of the other fields will be non-object non complex fields, so I can probably replace deep equals with a custom property check of my own
                var compareSyntax = main.WithDeepEqual(compareWith);

                HashSet<string> ignoreNames = new HashSet<string>();
                foreach (var ignoreProperty in details.IgnoreProperties) ignoreNames.Add(ignoreProperty.Name);
                compareSyntax.IgnoreProperty((c) =>
                {
                    return ignoreNames.Contains(c.Name);
                });
                if (!compareSyntax.Compare())
                {
                    main.ChangeType = ChangeType.Updated;
                    currentPackage.OwnedEntities.Add(main);
                }
                foreach (var property in details.ObjectProperties)
                {
                    var mainValue = property.GetValue(main, null) as IChangeTrackable<TKey>;
                    var compareWithValue = property.GetValue(compareWith, null) as IChangeTrackable<TKey>;
                    this.SetChangeStatusInternal(BuildChangeRelationshipTemplate(main, property), mainValue, compareWithValue, alreadyChecked, currentPackage);
                }
            }
            foreach (var property in details.ListProperties)
            {
                var mainListValue = ((property.GetValue(main, null) as IList) ?? new IChangeTrackable<TKey>[0]).Cast<IChangeTrackable<TKey>>().ToList();
                var compareWithListValue = ((property.GetValue(compareWith, null) as IList) ?? new IChangeTrackable<TKey>[0]).Cast<IChangeTrackable<TKey>>().ToList();

                GetListChangesInternal(BuildChangeRelationshipTemplate(main, property), mainListValue, compareWithListValue, alreadyChecked, currentPackage);

            }
        }
        private readonly Func<TKey, TKey, bool> _keyEqualsDelegate = EqualityComparer<TKey>.Default.Equals;


        private void GetListChangesInternal(ChangedRelationShip<TKey> relationshipTemplate, IEnumerable<IChangeTrackable<TKey>> list, IEnumerable<IChangeTrackable<TKey>> compareWith, HashSet<object> alreadyChecked, ChangePackage<TKey> currentPackage)
        {

            //Note delayed execution
            var removalQuery = compareWith.Where(a => !list.Any(b => _keyEqualsDelegate(a.Id, b.Id)));
            if (relationshipTemplate.OwnershipActions.Delete)
            {
                foreach (var remove in removalQuery)
                {
                    remove.ChangeType = ChangeType.Deleted;
                    currentPackage.OwnedEntities.Add(remove);
                    currentPackage.Relationships.Add(relationshipTemplate.CloneFor(remove, ChangeType.Deleted));
                }
            }
            else
            {
                foreach (var remove in removalQuery)
                {
                    currentPackage.Relationships.Add(relationshipTemplate.CloneFor(remove, ChangeType.Deleted));
                }
            }

            foreach (var mainListItem in list)
            {
                bool found = false;
                foreach (var compareWithItem in compareWith)
                {
                    if (_keyEqualsDelegate(compareWithItem.Id, mainListItem.Id))
                    {
                        found = true;
                        //Will check for update if the parent is an owner
                        if (relationshipTemplate.OwnershipActions.Update)
                        {
                            SetChangeStatusInternal(relationshipTemplate, mainListItem, compareWithItem, alreadyChecked, currentPackage);
                        }
                        break;
                    }
                }
                if (!found)
                {
                    if (relationshipTemplate.OwnershipActions.Add)
                    {
                        mainListItem.ChangeType = ChangeType.Added;
                        currentPackage.OwnedEntities.Add(mainListItem);
                        currentPackage.Relationships.Add(relationshipTemplate.CloneFor(mainListItem, ChangeType.Added));
                    }
                    else
                    {
                        currentPackage.Relationships.Add(relationshipTemplate.CloneFor(mainListItem, ChangeType.Added));
                    }
                }
            }
        }

        private void SetupConfigurationForType(Type type)
        {
            if (!_mappedTypeDetails.ContainsKey(type))
            {

                var matchingIgnoreTypes = this._ignoreAllPropertiesFromTypes.Where(i => type.IsSubclassOf(i) || type.GetInterfaces().Contains(i) || i == type).SelectMany(i => i.GetProperties()).ToDictionary(c => c.Name);

                MappedTypeDetails details = new MappedTypeDetails() { Type = type };
                _mappedTypeDetails.Add(type, details);
                foreach (var property in type.GetProperties())
                {
                    if (matchingIgnoreTypes.ContainsKey(property.Name)
                        || this._ignoreAllPropertiesByName.Contains(property.Name)
                    )
                    {
                        details.IgnoreProperties.Add(property);
                    }

                    //TODO make this work for not only IList but any iterator
                    else if (typeof(IList).IsAssignableFrom(property.PropertyType))
                    {
                        if (property.PropertyType.IsArray)
                        {
                            if (property.PropertyType.GetElementType().GetInterfaces().Contains(typeof(IChangeTrackable<TKey>)))
                            {
                                details.IgnoreProperties.Add(property);
                                details.ListProperties.Add(property);
                            }
                        }
                        else
                        {
                            details.IgnoreProperties.Add(property);
                            if (property.PropertyType.GetGenericArguments()[0].GetInterfaces().Contains(typeof(IChangeTrackable<TKey>)))
                            {
                                details.ListProperties.Add(property);
                            }
                        }
                    }
                    //Question should we be checking sub object properties if if they are not IChangeTrackable<TKey>?
                    //If they have change trackable children???
                    else if (property.PropertyType.GetInterfaces().Contains(typeof(IChangeTrackable<TKey>)))
                    {
                        details.IgnoreProperties.Add(property);
                        details.ObjectProperties.Add(property);
                    }
                }
                foreach (var property in details.ListProperties)
                {
                    SetupConfigurationForType(property.PropertyType.GetGenericArguments()[0]);
                }
                foreach (var property in details.ObjectProperties)
                {
                    SetupConfigurationForType(property.PropertyType);
                }
            }
        }


        //TODO In the future I think this should be its own Object Configuration module...thing
        public class MappedTypeDetails
        {
            public Type Type { get; set; }
            public ObservableCollection<PropertyInfo> ObjectProperties { get; set; } = new ObservableCollection<PropertyInfo>();
            public ObservableCollection<PropertyInfo> ListProperties { get; set; } = new ObservableCollection<PropertyInfo>();
            public ObservableCollection<PropertyInfo> IgnoreProperties { get; set; } = new ObservableCollection<PropertyInfo>();

            public MappedTypeDetails()
            {
                IgnoreProperties.CollectionChanged += IgnoreProperties_CollectionChanged;
            }

            private void IgnoreProperties_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (var added in e.NewItems.OfType<PropertyInfo>())
                    {
                        IgnorePropertyNames.Add(added.Name);
                    }
                }
                else if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    foreach (var removed in e.OldItems.OfType<PropertyInfo>())
                    {
                        IgnorePropertyNames.Remove(removed.Name);
                    }
                }
            }

            public HashSet<string> IgnorePropertyNames { get; set; } = new HashSet<string>();

        }
    }
}
