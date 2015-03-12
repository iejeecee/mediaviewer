// Updating a entity:
// http://stackoverflow.com/questions/15336248/entity-framework-5-updating-a-record
// Debug generated SQL, add the following line
// context.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);
#pragma warning disable 693
using MediaViewer.Infrastructure.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MediaDatabase.DbCommands
{
    class DbCommands<T> : IDisposable
    {
        
        const int nrRetriesOnOptimisticConcurrencyException = 100;

        bool usingExistingContext;

        protected bool UsingExistingContext
        {
            get { return usingExistingContext; }
            set { usingExistingContext = value; }
        }
        MediaDatabaseContext db;

        public MediaDatabaseContext Db
        {
            get { return db; }
            set { db = value; }
        }

        protected DbCommands(MediaDatabaseContext existingContext = null)
        {
            if (existingContext == null)
            {
                Db = new MediaDatabaseContext();
                UsingExistingContext = false;
            }
            else
            {
                Db = existingContext;
                UsingExistingContext = true;
            }
        }

        ~DbCommands()
        {
            if (!UsingExistingContext && Db != null)
            {
                Db.Dispose();
                Db = null;
            }
        }

        public virtual T update(T entity)
        {
            bool retry = true;
            T result = default(T);

            for (int i = 0; i < nrRetriesOnOptimisticConcurrencyException && retry == true; i++)
            {                
                try
                {
                    result = updateFunc(entity);

                    retry = false;
                }
                catch (DbUpdateConcurrencyException e)
                {
                    if (i == nrRetriesOnOptimisticConcurrencyException - 1)
                    {
                        Logger.Log.Error("Concurrencyexception while updating entity, nr retries exhausted: " + e.Message);
                        throw;
                    }
                    else
                    {
                        foreach (DbEntityEntry conflictingEntity in e.Entries)
                        {
                            // reload the conflicting entity (database wins)
                            conflictingEntity.Reload();
                        }
                                               
                        Logger.Log.Warn("Concurrencyexception while updating entity: " + e.Message);
                        retry = true;
                    }
                   
                }

            }
                

            return (result);
        }

        protected virtual T updateFunc(T entity)
        {
            throw new NotImplementedException();        
        }

        public virtual T create(T entity)
        {            
            T result = default(T);
        
            try
            {
                result = createFunc(entity);                                   
            }
            catch (DbUpdateConcurrencyException e)
            {
                Logger.Log.Error("Concurrencyexception while creating entity: " + e.Message);

                throw;
            }
                           
            return (result);
        }


        protected virtual T createFunc(T entity)
        {
            throw new NotImplementedException();     
        }

        public virtual void delete(T entity)
        {
                     
            try
            {
                deleteFunc(entity);              
            }
            catch (DbUpdateConcurrencyException e)
            {
                Logger.Log.Error("Concurrencyexception while deleting entity: " + e.Message);

                throw;
            }

            
        }
  
        protected virtual void deleteFunc(T entity)
        {
            throw new NotImplementedException();     
        }

        public void Dispose()
        {
            if (!UsingExistingContext && Db != null)
            {
                Db.Dispose();
                Db = null;
            }
        }

        public virtual void clearAll()
        {
            throw new NotImplementedException();  
        }


        private readonly ICollection<Type> validPropertyTypes = new List<Type> { typeof(string), typeof(Enum), typeof(DateTime), typeof(Guid),typeof(Byte[]) };

        //https://lowrymedia.com/2014/06/30/understanding-entity-framework-and-sql-indexes/
        /// <summary>
        /// Creates a non-clustered index and includes the remaining columns of the type (Entity Framework returns all columns).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyExpressions"></param>
        /// <returns></returns>
        public string CreateIndex<T>(params Expression<Func<T, object>>[] propertyExpressions)
        {
            var tableName = this.GetTableName(typeof(T));

            var indexName = new StringBuilder();
            var indexedProperties = new StringBuilder();
            foreach (var propertyExpression in propertyExpressions)
            {
                indexName.AppendFormat("{0}_", this.GetPropertyName(propertyExpression));
                indexedProperties.AppendFormat("[{0}] ASC,", this.GetPropertyName(propertyExpression));
            }

            var includedProperties = new StringBuilder();
            foreach (var property in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                if (!(property.CanRead && property.CanWrite))
                {
                    continue;
                }

                var isPrimitive = property.PropertyType.IsPrimitive;
                var isValidNonPrimitive = validPropertyTypes.Contains(property.PropertyType) || validPropertyTypes.Contains(property.PropertyType.BaseType);
                var isNullable = property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>);
                if (!(isPrimitive || isValidNonPrimitive || isNullable))
                {
                    continue;
                }

                var propertyUsedInIndex = propertyExpressions.Any(x => this.GetPropertyName(x) == property.Name);
                if (propertyUsedInIndex)
                {
                    continue;
                }

                System.Attribute[] attrs = System.Attribute.GetCustomAttributes(property);
                bool isNotMapped = false;
                foreach (System.Attribute attr in attrs)
                {
                    if (attr is NotMappedAttribute)
                    {
                        isNotMapped = true;
                    }
                }

                if (isNotMapped) continue;

                includedProperties.AppendFormat("[{0}],", property.Name);
            }

            if (includedProperties.Length > 0)
            {
                return string.Format("CREATE NONCLUSTERED INDEX [IX_{0}_{1}] ON [dbo].[{0}] ({2}) INCLUDE ({3}); ", tableName, indexName.ToString().TrimEnd('_'), indexedProperties.ToString().TrimEnd(','), includedProperties.ToString().TrimEnd(','));
            }

            return string.Format("CREATE NONCLUSTERED INDEX [IX_{0}_{1}] ON [dbo].[{0}] ({2}); ", tableName, indexName.ToString().TrimEnd('_'), indexedProperties.ToString().TrimEnd(','));
        }

        protected virtual string GetPropertyName<T>(Expression<Func<T, object>> propertyExpression)
        {
            return this.GetPropertyExpressionMember(propertyExpression).Member.Name;
        }

        protected virtual MemberExpression GetPropertyExpressionMember<T>(Expression<Func<T, object>> propertyExpression)
        {
            if (propertyExpression.Body is UnaryExpression)
            {
                return (MemberExpression)((UnaryExpression)propertyExpression.Body).Operand;
            }

            return (MemberExpression)propertyExpression.Body;
        }

        protected virtual string GetTableName(Type type)
        {
            //return ("BaseMediaSet");

            var metadata = ((IObjectContextAdapter)this.db).ObjectContext.MetadataWorkspace;
            var objectItemCollection = ((ObjectItemCollection)metadata.GetItemCollection(DataSpace.OSpace));
            var entityType = metadata.GetItems<EntityType>(DataSpace.OSpace).Single(e => objectItemCollection.GetClrType(e) == type);
            var entitySet = metadata.GetItems(DataSpace.CSpace).Where(x => x.BuiltInTypeKind == BuiltInTypeKind.EntityType).Cast<EntityType>().Single(x => x.Name == entityType.Name);
            var entitySetMappings = metadata.GetItems<EntityContainerMapping>(DataSpace.CSSpace).Single().EntitySetMappings.ToList();

            EntitySet table;
            var mapping = entitySetMappings.SingleOrDefault(x => x.EntitySet.Name == entitySet.Name);
            if (mapping != null)
            {
                table = mapping.EntityTypeMappings.Single().Fragments.Single().StoreEntitySet;
            }
            else
            {
                mapping = entitySetMappings.SingleOrDefault(x => x.EntityTypeMappings.Where(y => y.EntityType != null).Any(y => y.EntityType.Name == entitySet.Name));
                if (mapping != null)
                {
                    table = mapping.EntityTypeMappings.Where(x => x.EntityType != null).Single(x => x.EntityType.Name == entityType.Name).Fragments.Single().StoreEntitySet;
                }
                else
                {
                    var entitySetMapping = entitySetMappings.Single(x => x.EntityTypeMappings.Any(y => y.IsOfEntityTypes.Any(z => z.Name == entitySet.Name)));
                    table = entitySetMapping.EntityTypeMappings.First(x => x.IsOfEntityTypes.Any(y => y.Name == entitySet.Name)).Fragments.Single().StoreEntitySet;
                }
            }

            return (string)table.MetadataProperties["Table"].Value ?? table.Name;
            
        }
    
    }
}
