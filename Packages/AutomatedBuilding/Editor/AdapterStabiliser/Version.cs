using System;
using System.Linq;

namespace LionStudios.Editor.AutoBuilder.AdapterStabilizer
{
    
    public class Version : IComparable, IComparable<Version>, IEquatable<Version>
    {
        
        private int[] elements;

        public Version(params int[] elements)
        {
            this.elements = elements;
        }
        
        public Version(string version)
            : this(version.Split('.').Select(s => int.Parse(s)).ToArray()) { }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            if (obj is Version otherVersion)
                return CompareTo(otherVersion);

            throw new ArgumentException("Object is not a Version");
        }

        public int CompareTo(Version other)
        {
            if (other == null) return 1;

            int minLength = Math.Min(elements.Length, other.elements.Length);

            for (int i = 0; i < minLength; i++)
            {
                int result = elements[i].CompareTo(other.elements[i]);
                if (result != 0)
                    return result;
            }

            return elements.Length.CompareTo(other.elements.Length);
        }

        public bool Equals(Version other)
        {
            if (other == null) return false;

            if (elements.Length != other.elements.Length) return false;

            for (int i = 0; i < elements.Length; i++)
            {
                if (elements[i] != other.elements[i])
                    return false;
            }

            return true;
        }

        public override bool Equals(object obj) 
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            return Equals((Version)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                foreach (int element in elements)
                    hash = hash * 31 + element.GetHashCode();
                return hash;
            }
        }

        public override string ToString()
        {
            return string.Join(".", elements);
        }

        public static bool operator ==(Version version1, Version version2)
        {
            if (ReferenceEquals(version1, version2))
                return true;

            if (ReferenceEquals(version1, null) || ReferenceEquals(version2, null))
                return false;

            return version1.Equals(version2);
        }

        public static bool operator !=(Version version1, Version version2)
        {
            return !(version1 == version2);
        }

        public static bool operator <(Version version1, Version version2)
        {
            if (ReferenceEquals(version1, null))
                throw new ArgumentNullException(nameof(version1));

            return version1.CompareTo(version2) < 0;
        }

        public static bool operator <=(Version version1, Version version2)
        {
            if (ReferenceEquals(version1, null))
                throw new ArgumentNullException(nameof(version1));

            return version1.CompareTo(version2) <= 0;
        }

        public static bool operator >(Version version1, Version version2)
        {
            return !(version1 <= version2);
        }

        public static bool operator >=(Version version1, Version version2)
        {
            return !(version1 < version2);
        }
        
    }
    
}