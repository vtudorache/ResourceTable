using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Tuvl.Util
{
    // The abstract class ResourceTable models a resource table, a mapping where
    // keys are strings and values are objects of any type.
    // This class is useful as a base for classes defining locale-specific resources,
    // isolating data objects from program code.
    // The resource table is loaded by its owner when needed. The owner should hold
    // a reference to the table, in order to avoid performance penalties resulting
    // from calls of GetTable(...) when searching for resources.
    public abstract class ResourceTable
    {
        // The culture of this resource table.
        protected abstract CultureInfo Culture { get; }

        // Returns an object contained only in this resource table, for the given
        // key. Returns null if the key isn't present.
        protected abstract object GetObject(string key);

        // Returns the keys contained only in this resource table.
        protected abstract IEnumerable<string> GetKeys();

        // The parent table of this resource table.
        protected ResourceTable Parent = null;

        // The object contained in this resource table or a parent table, for the
        // given key. Returns null if the key isn't present.
        public object this[string key]
        {
            get
            {
                object value = null;
                ResourceTable current = this;
                while ((value is null) && !(current is null))
                {
                    value = current.GetObject(key);
                    current = current.Parent;
                }
                return value;
            }
        }

        // The keys contained in this resource table and its parent tables.
        public IEnumerable<string> Keys
        {
            get
            {
                HashSet<string> keySet = new HashSet<string>();
                ResourceTable current = this;
                while (!(current is null))
                {
                    keySet.UnionWith(current.GetKeys());
                    current = current.Parent;
                }
                return keySet;
            }
        }

        // The name passed to GetTable(...) when creating this resource table.
        public string BaseName { get; private set; }

        // Gets a ResourceTable object with the given baseName for the given culture,
        // thisType being the Type of the object where GetTable(...) is called from
        // (obtained with this.GetType()). The given baseName is the name of a Type
        // derived from ResourceTable, including its namespace.
        // The search starts in the namespace of thisType. For example, given the
        // baseName NS.MyTab, calling GetTable(...) from an object having a namespace
        // A.Test, the method searches for a Type (derived from ResourceTable) having
        // the name A.Test.NS.MyTab. If such Type exists, an instance is created as a
        // ResourceTable. Then, the culture's name (obtained with culture.Name) is
        // split by '-' and for each part a new baseName is obtained by appending '_'
        // and the part to the previous baseName. For each baseName, the method
        // searches a Type derived from ResourceTable. If one exists, an instance is
        // created as a ResourceTable and its Parent is set to the previously created
        // ResourceTable object if such an object exists.
        // Returns the last ResourceTable object to the caller, that must hold a
        // reference to it, in order to avoid performance penalties resulting from
        // repeated searches.
        public static ResourceTable GetTable(Type thisType, string baseName,
            CultureInfo culture)
        {
            ResourceTable current = null;
            Dictionary<string, Type> tableTypes = new Dictionary<string, Type>();
            Type[] types = null;
            try
            {
                types = thisType.Assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types;
            }
            foreach (Type t in types)
            {
                if ((t is null) || t.BaseType != typeof(ResourceTable))
                {
                    continue;
                }
                tableTypes.Add(string.Format("{0}.{1}", t.Namespace, t.Name), t);
            }
            string typeName = string.Format("{0}.{1}", thisType.Namespace, baseName);
            if (tableTypes.ContainsKey(typeName))
            {
                Type t = tableTypes[typeName];
                current = Activator.CreateInstance(t) as ResourceTable;
                current.BaseName = baseName;
            }
            foreach (string part in culture.Name.Split('-'))
            {
                typeName = string.Format("{0}_{1}", typeName, part);
                if (!tableTypes.ContainsKey(typeName))
                {
                    continue;
                }
                Type t = tableTypes[typeName];
                ResourceTable child = Activator.CreateInstance(t) as ResourceTable;
                child.BaseName = baseName;
                child.Parent = current;
                current = child;
            }
            return current;
        }

        // Gets a ResourceTable object for the given baseName and the current
        // culture.
        public static ResourceTable GetTable(Type thisType, string baseName)
        {
            return GetTable(thisType, baseName, CultureInfo.CurrentCulture);
        }
   }
}