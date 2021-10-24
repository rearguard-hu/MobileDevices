using System;
using System.Linq;

namespace MobileDevices.iOS
{
    /// <summary>
    /// Represents the version number of an Apple product.
    /// </summary>
    /// <seealso href="http://blog.joemoreno.com/2007/11/apple-software-build-numbers.html"/>
    [Serializable]
    public class AppleVersion : ICloneable, IComparable, IComparable<AppleVersion>, IEquatable<AppleVersion>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppleVersion"/> class.
        /// </summary>
        /// <param name="major">
        /// The major version number.
        /// </param>
        /// <param name="minor">
        /// The minor version number.
        /// </param>
        /// <param name="build">
        /// The build number.
        /// </param>
        public AppleVersion(int major, char minor, int build)
            : this(major, minor, 0, build, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppleVersion"/> class.
        /// </summary>
        /// <param name="major">
        /// The major version number.
        /// </param>
        /// <param name="minor">
        /// The minor version number.
        /// </param>
        /// <param name="architecture">
        /// The architecture which this version targets. Can be <c>0</c> for universal
        /// applications.
        /// </param>
        /// <param name="build">
        /// The build number.
        /// </param>
        /// <param name="revision">
        /// The build revision.
        /// </param>
        public AppleVersion(int major, char minor, int architecture, int build, char? revision)
        {
            if (major < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(major));
            }

            if (architecture < 0 || architecture >= 10)
            {
                throw new ArgumentOutOfRangeException(nameof(architecture));
            }

            if (build > 1000 || build < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(build));
            }

            if (!char.IsLetter(minor) || !char.IsUpper(minor))
            {
                throw new ArgumentOutOfRangeException(nameof(minor));
            }

            if (revision != null && !(char.IsLetter(revision.Value) && char.IsLower(revision.Value)))
            {
                throw new ArgumentOutOfRangeException(nameof(revision));
            }

            this.Major = major;
            this.Minor = minor;
            this.Architecture = architecture;
            this.Build = build;
            this.Revision = revision;
        }

        /// <summary>
        /// Gets or sets the major version number.
        /// </summary>
        public int Major
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets or sets the minor version number.
        /// </summary>
        public char Minor
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets or sets the architecture number. <c>0</c> indicates universal builds;
        /// <c>2</c> indicates 32-bit Intel builds, <c>4</c> indicates 64-bit Intel builds,
        /// <c>3</c> indicates ARM builds, <c>5</c> indicates 64-bit ARM builds.
        /// </summary>
        /// <remarks>
        /// At least for Xcode, the Architecture number appears to be also in use to mark
        /// patch versions, see https://en.wikipedia.org/wiki/Xcode#1.x_series.</remarks>
        public int Architecture
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets or sets the build number.
        /// </summary>
        public int Build
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the long build number; this includes the architecture number if available.
        /// </summary>
        public int LongBuild
        {
            get
            {
                return (this.Architecture * 1000) + this.Build;
            }
        }

        /// <summary>
        /// Gets or sets the revision number.
        /// </summary>
        public char? Revision
        {
            get;
            protected set;
        }

        /// <summary>
        /// Determines whether two specified <see cref="AppleVersion"/> objects are equal.
        /// </summary>
        /// <param name="v1">
        /// The first <see cref="AppleVersion"/> object.
        /// </param>
        /// <param name="v2">
        /// The second <see cref="AppleVersion"/> object.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="v1"/> equals <paramref name="v2"/>;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator ==(AppleVersion v1, AppleVersion v2)
        {
            if (object.ReferenceEquals(v1, null))
            {
                return object.ReferenceEquals(v2, null);
            }

            return v1.Equals(v2);
        }

        /// <summary>
        /// Determines whether two specified <see cref="AppleVersion"/> objects are not equal.
        /// </summary>
        /// <param name="v1">
        /// The first <see cref="AppleVersion"/> object.
        /// </param>
        /// <param name="v2">
        /// The second <see cref="AppleVersion"/> object.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="v1"/> does not equal <paramref name="v2"/>;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator !=(AppleVersion v1, AppleVersion v2)
        {
            return !(v1 == v2);
        }

        /// <summary>
        /// Determines whether the first specified <see cref="AppleVersion"/> object is
        /// less than the second specified <see cref="AppleVersion"/> object.
        /// </summary>
        /// <param name="v1">
        /// The first <see cref="AppleVersion"/> object.
        /// </param>
        /// <param name="v2">
        /// The second <see cref="AppleVersion"/> object.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="v1"/> is less than <paramref name="v2"/>;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator <(AppleVersion v1, AppleVersion v2)
        {
            if (v1 == null)
            {
                // null < any value, except null.
                return v2 != null;
            }

            return v1.CompareTo(v2) < 0;
        }

        /// <summary>
        /// Determines whether the first specified <see cref="AppleVersion"/> object is
        /// less than or equal to the second specified <see cref="AppleVersion"/> object.
        /// </summary>
        /// <param name="v1">
        /// The first <see cref="AppleVersion"/> object.
        /// </param>
        /// <param name="v2">
        /// The second <see cref="AppleVersion"/> object.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="v1"/> is less or equal to than <paramref name="v2"/>;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator <=(AppleVersion v1, AppleVersion v2)
        {
            if (v1 == null)
            {
                return true;
            }

            return v1.CompareTo(v2) <= 0;
        }

        /// <summary>
        /// Determines whether the first specified <see cref="AppleVersion"/> object is
        /// greater than the second specified <see cref="AppleVersion"/> object.
        /// </summary>
        /// <param name="v1">
        /// The first <see cref="AppleVersion"/> object.
        /// </param>
        /// <param name="v2">
        /// The second <see cref="AppleVersion"/> object.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="v1"/> is greater than <paramref name="v2"/>;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator >(AppleVersion v1, AppleVersion v2)
        {
            return v2 < v1;
        }

        /// <summary>
        /// Determines whether the first specified <see cref="AppleVersion"/> object is
        /// greater than or equal to the second specified <see cref="AppleVersion"/> object.
        /// </summary>
        /// <param name="v1">
        /// The first <see cref="AppleVersion"/> object.
        /// </param>
        /// <param name="v2">
        /// The second <see cref="AppleVersion"/> object.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="v1"/> is greater than or equal to<paramref name="v2"/>;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator >=(AppleVersion v1, AppleVersion v2)
        {
            return v2 <= v1;
        }

        /// <summary>
        /// Determines whether two versions represent the same major and minor version. This method also checks
        /// on architecture and revision (used to indicate beta releases). Build numbers can change.
        /// </summary>
        /// <param name="v1">
        /// The first version to compare.
        /// </param>
        /// <param name="v2">
        /// The second version to compare.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="v1"/> and <paramref name="v2"/> differ in build number only;
        /// in all other cases, <see langword="false"/>.
        /// </returns>
        public static bool DifferByBuildNumberOnly(AppleVersion v1, AppleVersion v2)
        {
            if (v1 == null || v2 == null)
            {
                return false;
            }

            // We ignore the build number, that is the only thing that is allowed to change.
            return v1.Major == v2.Major
                && v1.Minor == v2.Minor
                && v1.Architecture == v2.Architecture
                && v1.Revision == v1.Revision;
        }

        /// <summary>
        /// Converts the <see cref="string"/> representation of a version number to an equivalent
        /// <see cref="AppleVersion"/> object.
        /// </summary>
        /// <param name="value">
        /// A <see cref="string"/> that contains a version number to convert.
        /// </param>
        /// <returns>
        /// An object that is equivalent to the version number specified in the
        /// <paramref name="value"/> parameter.
        /// </returns>
        public static AppleVersion Parse(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            int numerics = value.Count(c => char.IsNumber(c));
            int majorLetters = value.Count(c => char.IsLetter(c) && char.IsUpper(c));
            int revisionLetters = value.Count(c => char.IsLetter(c) && char.IsLower(c));

            if (numerics < 2 || majorLetters != 1 || revisionLetters > 1 || numerics + majorLetters + revisionLetters != value.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            char minor = value.Single(c => char.IsLetter(c) && char.IsUpper(c));
            int minorIndex = value.IndexOf(minor);

            if (minorIndex == 0 || minorIndex == value.Length - 1)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            char revision = value.SingleOrDefault(c => char.IsLetter(c) && char.IsLower(c));
            int revisionIndex = value.IndexOf(revision);

            if (revisionIndex != -1 && revisionIndex != value.Length - 1)
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            int major = int.Parse(value.Substring(0, minorIndex));

            int build = 0;

            if (revisionIndex > 0)
            {
                build = int.Parse(value.Substring(minorIndex + 1, revisionIndex - minorIndex - 1));
            }
            else
            {
                build = int.Parse(value.Substring(minorIndex + 1));
            }

            int architecture = 0;

            if (build >= 1000)
            {
                architecture = build / 1000;
                build = build - (1000 * architecture);
            }

            return new AppleVersion(major, minor, architecture, build, revisionIndex == -1 ? (char?)null : revision);
        }

        /// <summary>
        /// Compares the current <see cref="AppleVersion"/> object to a specified object and returns
        /// an indication of their relative values.
        /// </summary>
        /// <param name="obj">
        /// An object to compare, or <see langword="null"/>.
        /// </param>
        /// <returns>
        /// A signed integer that indicates the relative values of the two objects.
        /// </returns>
        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }

            AppleVersion v = obj as AppleVersion;
            if (v == null)
            {
                throw new ArgumentOutOfRangeException(nameof(obj));
            }

            return this.CompareTo(v);
        }

        /// <summary>
        /// Compares the current <see cref="AppleVersion"/> object to a specified <see cref="AppleVersion"/> object and returns
        /// an indication of their relative values.
        /// </summary>
        /// <param name="obj">
        /// An object to compare, or <see langword="null"/>.
        /// </param>
        /// <returns>
        /// A signed integer that indicates the relative values of the two objects.
        /// </returns>
        public int CompareTo(AppleVersion obj)
        {
            if (obj == null)
            {
                return 1;
            }

            if (this.Major != obj.Major)
            {
                if (this.Major > obj.Major)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }

            if (this.Minor != obj.Minor)
            {
                if (this.Minor > obj.Minor)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }

            if (this.Build != obj.Build)
            {
                if (this.Build > obj.Build)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }

            if (this.Revision != null || obj.Revision != null)
            {
                if (this.Revision == null && obj.Revision != null)
                {
                    return 1;
                }
                else if (this.Revision != null && obj.Revision == null)
                {
                    return -1;
                }
                else if (this.Revision != obj.Revision)
                {
                    if (this.Revision > obj.Revision)
                    {
                        return 1;
                    }
                    else
                    {
                        return -1;
                    }
                }
            }

            return 0;
        }

        /// <summary>
        /// Returns a new <see cref="AppleVersion"/> object whose value is the same as the current
        /// <see cref="AppleVersion"/> object.
        /// </summary>
        /// <returns>
        /// A new <see cref="AppleVersion"/> object whose value is the same as the current
        /// <see cref="AppleVersion"/> object.
        /// </returns>
        public object Clone()
        {
            return new AppleVersion(this.Major, this.Minor, this.Architecture, this.Build, this.Revision);
        }

        /// <summary>
        /// Returns a value indicating whether the current <see cref="AppleVersion"/> object is equal
        /// to a specified object.
        /// </summary>
        /// <param name="obj">
        /// An object to compare with the current <see cref="AppleVersion"/> object, or <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="null"/> if the current <see cref="AppleVersion"/> object and <paramref name="obj"/>
        /// are both <see cref="AppleVersion"/> objects, and every component of the current <see cref="AppleVersion"/>
        /// object matches the corresponding component of <paramref name="obj"/>; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool Equals(object obj)
        {
            AppleVersion v = obj as AppleVersion;
            if (v == null)
            {
                return false;
            }

            return this.Equals(v);
        }

        /// <summary>
        /// Returns a value indicating whether the current <see cref="AppleVersion"/> object and a
        /// specified <see cref="AppleVersion"/> object represent the same value.
        /// </summary>
        /// <param name="other">
        /// A <see cref="AppleVersion"/> object to compare to the current <see cref="AppleVersion"/> object,
        /// or <see langword="null"/>.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if every component of the current <see cref="AppleVersion"/> object
        /// matches the corresponding component of the <paramref name="other"/> parameter; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool Equals(AppleVersion other)
        {
            if (other == null)
            {
                return false;
            }

            // check that major, minor, build & revision numbers match
            return (this.Major == other.Major) &&
                (this.Minor == other.Minor) &&
                (this.Build == other.Build) &&
                (this.Revision == other.Revision);
        }

        /// <summary>
        /// Returns a hash code for the current <see cref="AppleVersion"/> object.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer hash code.
        /// </returns>
        public override int GetHashCode()
        {
            // Let's assume that most version numbers will be pretty small and just
            // OR some lower order bits together.
            int accumulator = 0;

            accumulator |= ((this.Major + this.Architecture) & 0x000000FF) << 24;
            accumulator |= (this.Minor & 0x000000FF) << 16;
            accumulator |= (this.Build & 0x000000FF) << 8;
            accumulator |= (this.Revision == null ? '\0' : this.Revision.Value) & 0x000000FF;

            return accumulator;
        }

        /// <summary>
        /// Converts the value of the current <see cref="AppleVersion"/> object to its equivalent
        /// <see cref="string"/> representation.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/> representation of the values of the <see cref="Major"/>,
        /// <see cref="Minor"/> and <see cref="Build"/> components of the current
        /// <see cref="AppleVersion"/> object.
        /// </returns>
        public override string ToString()
        {
            if (this.Revision == null)
            {
                return $"{this.Major}{this.Minor}{this.LongBuild}";
            }
            else
            {
                return $"{this.Major}{this.Minor}{this.LongBuild}{this.Revision}";
            }
        }
    }
}
