using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Mapping;
using System.Linq;

namespace Reinforced.Stroke
{
    /// <summary>
    /// Mapping cache that holds EntitySetMapping and ready-to use names
    /// </summary>
    internal class MappingCacheEntry
    {

        /// <summary>
        /// Cached type informations
        /// </summary>
        private readonly Dictionary<Type, TypeCacheEntry> _typesCache = new Dictionary<Type, TypeCacheEntry>();
        private readonly object _typesCacheLocker = new object();

        private TypeCacheEntry GetCacheEntry(Type t, EntitySetMapping mapping)
        {
            if (!_typesCache.ContainsKey(t))
            {
                lock (_typesCacheLocker)
                {
                    if (!_typesCache.ContainsKey(t))
                    {
                        if (mapping == null) return null;
                        // Find the storage entity set (table) that the entity is mapped
                        var tableEntitySet = mapping
                            .EntityTypeMappings.Single()
                            .Fragments.Single()
                            .StoreEntitySet;

                        // Return the table name from the storage entity set
                        var tableName = tableEntitySet.MetadataProperties["Table"].Value ?? tableEntitySet.Name;
                        _typesCache[t] = new TypeCacheEntry()
                        {
                            TableName = tableName.ToString()
                        };
                    }
                }
            }
            return _typesCache[t];
        }

        /// <summary>
        /// Retrieves cached table name for type
        /// </summary>
        /// <param name="t">EF entity type</param>
        /// <param name="mapping">Entity set maping (optional)</param>
        /// <returns>String table name or null if not cached. If null returned - please ty again specifying Mapping</returns>
        public string GetTableName(Type t, EntitySetMapping mapping = null)
        {
            var entry = GetCacheEntry(t, mapping);
            if (entry == null) return null;
            return entry.TableName;
        }

        /// <summary>
        /// Retrieves cached DB field name for property of particular type
        /// </summary>
        /// <param name="t">EF entity type</param>
        /// <param name="propertyName">Type's property name</param>
        /// <param name="mapping">Entity set maping (optional)</param>
        /// <returns>String field name. Table name has to be retrieved separately.  If null returned - please ty again specifying Mapping</returns>
        public string GetFieldName(Type t, string propertyName, EntitySetMapping mapping = null)
        {
            var entry = GetCacheEntry(t, mapping);
            if (entry == null) return null;
            if (!entry.FieldNames.ContainsKey(propertyName))
            {
                if (mapping == null) return null;
                lock (entry._typeEntitiesLocker)
                {
                    if (!entry.FieldNames.ContainsKey(propertyName))
                    {
                        // Find the storage property (column) that the property is mapped
                        var columnName = mapping
                            .EntityTypeMappings.Single()
                            .Fragments.Single()
                            .PropertyMappings
                            .OfType<ScalarPropertyMapping>()
                            .Single(m => m.Property.Name == propertyName)
                            .Column
                            .Name;
                        entry.FieldNames[propertyName] = columnName;
                    }
                }
            }
            return entry.FieldNames[propertyName];
        }
    }
}
