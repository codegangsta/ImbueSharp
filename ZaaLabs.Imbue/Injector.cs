using System;
using System.Collections.Generic;
using System.Reflection;

namespace ZaaLabs.Imbue
{
    public class Injector : IInjector
    {
        //_____________________________________________________________________
        //	IInjector Implementation
        //_____________________________________________________________________
        public void MapValue(Object value, Type type)
        {
            // add the mapped value to the corresponding type in mapped values
            MappedValues.Add(type,value);
        }

        public void Apply(Object value)
        {
            var type = value.GetType();
            var targets = GetInjectionTargets(type);

            // loop through all of the injection targets and apply it to the target
            foreach (var injectionTarget in targets)
            {
                ApplyToTarget(injectionTarget,value);
            }
        }

        //_____________________________________________________________________
        //	Constructor
        //_____________________________________________________________________
        public Injector()
        {
            // initialize the Mapped values
            MappedValues = new Dictionary<Type, object>();
        }

        //_____________________________________________________________________
        //	Protected Properties
        //_____________________________________________________________________
        protected Dictionary<Type, Object> MappedValues;

        //_____________________________________________________________________
        //	Protected Static Properties
        //_____________________________________________________________________
        protected static Dictionary<Type, List<InjectionTarget>> InjectionTargetCache = new Dictionary<Type, List<InjectionTarget>>();
        protected static Dictionary<Type, Dictionary<String, FieldInfo>> FieldInfoCache = new Dictionary<Type, Dictionary<string, FieldInfo>>();

        //_____________________________________________________________________
        //	Protected Methods
        //_____________________________________________________________________
        protected void ApplyToTarget(InjectionTarget target, Object value)
        {
            if (MappedValues.ContainsKey(target.Type))
            {
                var injectedValue = MappedValues[target.Type];
                var field = GetFieldInfo(value.GetType(), target.Property);
                if (field != null && injectedValue != null)
                {
                    field.SetValue(value, injectedValue);
                }
            }
            else
            {
                throw new Exception("No rule for injecting " + target.Type.ToString() + " into " + value.GetType().ToString());
            }
        }

        //_____________________________________________________________________
        //	Protected Static Methods
        //_____________________________________________________________________
        /**
         * Gets a PropertyInfo from a cache of propertyInfos
         */
        protected static FieldInfo GetFieldInfo(Type type, string property)
        {
            // set up the property info dictionary
            if(!FieldInfoCache.ContainsKey(type))
            {
                FieldInfoCache.Add(type,new Dictionary<string, FieldInfo>());
            }

            var fieldInfos = FieldInfoCache[type];

            // if we don't have the property we will get that too
            if(!fieldInfos.ContainsKey(property))
            {
                fieldInfos.Add(property,type.GetField(property));
            }

            // now that we have everything taken care of, we can properly return
            // the propertyinfo
            return fieldInfos[property];
        }

        protected static List<InjectionTarget> GetInjectionTargets(Type type)
        {
            // see if we have it in the injection target cache
            if (InjectionTargetCache.ContainsKey(type))
            {
                return InjectionTargetCache[type];
            }

            // initialize injection targets
            var targets = new List<InjectionTarget>();

            // otherwise we will create injection targets
            var fieldInfos = type.GetFields();
            foreach (var fieldInfo in fieldInfos)
            {
                var attributes = fieldInfo.GetCustomAttributes(typeof(InjectAttribute), false);
                // if we have an inject attribute, this is considered a target
                if (attributes.Length > 0)
                {
                    var target = new InjectionTarget();
                    target.Type = fieldInfo.FieldType;
                    target.Property = fieldInfo.Name;
                    targets.Add(target);
                }
            }

            // fill the cache
            InjectionTargetCache[type] = targets;

            return targets;
        }

        //_____________________________________________________________________
        //	Structs/Classes
        //_____________________________________________________________________
        protected struct InjectionTarget
        {
            public Type Type;
            public String Property;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class InjectAttribute : Attribute
    {
        // we can leave this blank, sice we don't have anything
        // to process on the tag
    }
}
