using System;
using System.Collections.Generic;
using System.Reflection;

namespace Server
{
    [AttributeUsage(AttributeTargets.Property)]
    public class HueAttribute : Attribute
    {
        public HueAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class BodyAttribute : Attribute
    {
        public BodyAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class PropertyObjectAttribute : Attribute
    {
        public PropertyObjectAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class NoSortAttribute : Attribute
    {
        public NoSortAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class CallPriorityAttribute : Attribute
    {
        private int m_Priority;

        public int Priority
        {
            get
            {
                return this.m_Priority;
            }
            set
            {
                this.m_Priority = value;
            }
        }

        public CallPriorityAttribute(int priority)
        {
            this.m_Priority = priority;
        }
    }

    public class CallPriorityComparer : IComparer<MethodInfo>
    {
        public int Compare(MethodInfo x, MethodInfo y)
        {
            if (x == null && y == null)
                return 0;

            if (x == null)
                return 1;

            if (y == null)
                return -1;

            return this.GetPriority(x) - this.GetPriority(y);
        }

        private int GetPriority(MethodInfo mi)
        {
            object[] objs = mi.GetCustomAttributes(typeof(CallPriorityAttribute), true);

            if (objs == null)
                return 0;

            if (objs.Length == 0)
                return 0;

            CallPriorityAttribute attr = objs[0] as CallPriorityAttribute;

            if (attr == null)
                return 0;

            return attr.Priority;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class TypeAliasAttribute : Attribute
    {
        private readonly string[] m_Aliases;

        public string[] Aliases
        {
            get
            {
                return this.m_Aliases;
            }
        }

        public TypeAliasAttribute(params string[] aliases)
        {
            this.m_Aliases = aliases;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class ParsableAttribute : Attribute
    {
        public ParsableAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum)]
    public class CustomEnumAttribute : Attribute
    {
        private readonly string[] m_Names;

        public string[] Names
        {
            get
            {
                return this.m_Names;
            }
        }

        public CustomEnumAttribute(string[] names)
        {
            this.m_Names = names;
        }
    }

    [AttributeUsage(AttributeTargets.Constructor)]
    public class ConstructableAttribute : Attribute
    {
        private AccessLevel m_AccessLevel;

        public AccessLevel AccessLevel
        {
            get
            {
                return this.m_AccessLevel;
            }
            set
            {
                this.m_AccessLevel = value;
            }
        }

        public ConstructableAttribute() : this(AccessLevel.Player)//Lowest accesslevel for current functionality (Level determined by access to [add)
        {
        }

        public ConstructableAttribute(AccessLevel accessLevel)
        {
            this.m_AccessLevel = accessLevel;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class CommandPropertyAttribute : Attribute
    {
        private readonly AccessLevel m_ReadLevel;

        private readonly AccessLevel m_WriteLevel;

        private readonly bool m_ReadOnly;

        public AccessLevel ReadLevel
        {
            get
            {
                return this.m_ReadLevel;
            }
        }

        public AccessLevel WriteLevel
        {
            get
            {
                return this.m_WriteLevel;
            }
        }

        public bool ReadOnly
        {
            get
            {
                return this.m_ReadOnly;
            }
        }

        public CommandPropertyAttribute(AccessLevel level, bool readOnly)
        {
            this.m_ReadLevel = level;
            this.m_ReadOnly = readOnly;
        }

        public CommandPropertyAttribute(AccessLevel level) : this(level, level)
        {
        }

        public CommandPropertyAttribute(AccessLevel readLevel, AccessLevel writeLevel)
        {
            this.m_ReadLevel = readLevel;
            this.m_WriteLevel = writeLevel;
        }
    }
}