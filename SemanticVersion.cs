using System.Text.RegularExpressions;

namespace MoanMod;

/// <summary>
/// Represents a <a href="https://semver.org">semantic version</a>
/// </summary>
public class SemanticVersion : IComparable<SemanticVersion>
{
    private static readonly Regex s_SemVerRegex = new Regex(
        @"^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-((?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+([0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$",
        RegexOptions.Compiled
    );

    /// <summary>
    /// The Major part of this semver
    /// </summary>
    public int Major { get; init; }
    /// <summary>
    /// The Minor part of this semver
    /// </summary>
    public int Minor { get; init; }
    /// <summary>
    /// The Patch part of this semver
    /// </summary>
    public int Patch { get; init; }
    /// <summary>
    /// The Prerelease part of this semver, may be null or empty
    /// </summary>
    public string Prerelease { get; init; } = "";
    /// <summary>
    /// The BuildMetadata part of this semver, may be null or empty
    /// </summary>
    public string BuildMetadata { get; init; } = "";

    /// <summary>
    /// If this SemVer is a prerelease
    /// </summary>
    public bool IsPrerelease => !string.IsNullOrEmpty(Prerelease);
    /// <summary>
    /// If this SemVer has build metadata
    /// </summary>
    public bool HasBuildMetadata => !string.IsNullOrEmpty(BuildMetadata);

    /// <summary>
    /// Try parse a semantic version string
    /// </summary>
    /// <param name="versionString">The semantic version string</param>
    /// <param name="result">The variable to store the result in</param>
    /// <returns>True if the parsing was successful, false otherwise</returns>
    public static bool TryParse(string versionString, out SemanticVersion result)
    {
        try
        {
            result = new SemanticVersion(versionString);
            return true;
        }
        catch (Exception)
        {
            result = null;
            return false;
        }
    }
    /// <summary>
    /// Constructs a Semantic version from three ints
    /// </summary>
    /// <param name="major">Major part of the version</param>
    /// <param name="minor">Minor part of the version</param>
    /// <param name="patch">Patch part of the version</param>
    public SemanticVersion(int major, int minor, int patch)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
    }

    /// <summary>
    /// Constructs a SemanticVersion object from a string
    /// </summary>
    /// <param name="versionString"></param>
    /// <exception cref="ArgumentNullException">If <paramref name="versionString"/> is null</exception>
    /// <exception cref="ArgumentException">If <paramref name="versionString"/> is not valid semver</exception>
    public SemanticVersion(string versionString)
    {
        if (string.IsNullOrWhiteSpace(versionString))
            throw new ArgumentNullException(nameof(versionString));

        versionString = versionString.TrimStart('v', 'V');

        Match match = s_SemVerRegex.Match(versionString);
        if (!match.Success) throw new ArgumentException($"Invalid semver 2.0.0: {versionString}");

        Major = int.Parse(match.Groups[1].Value);
        Minor = int.Parse(match.Groups[2].Value);
        Patch = int.Parse(match.Groups[3].Value);

        Prerelease = match.Groups[4].Value;
        BuildMetadata = match.Groups[5].Value;
    }

    /// <summary>
    /// Compares this to <paramref name="other"/>.
    /// </summary>
    /// <param name="other">The version to compare against</param>
    /// <returns>-1 if this is older than <paramref name="other"/>, 0 if they're equal, and 1 if this is newer than <paramref name="other"/></returns>
    public int CompareTo(SemanticVersion other)
    {
        if (other == null) return 1;

        if (Major != other.Major) return Major.CompareTo(other.Major);
        if (Minor != other.Minor) return Minor.CompareTo(other.Minor);
        if (Patch != other.Patch) return Patch.CompareTo(other.Patch);

        if (!IsPrerelease && other.IsPrerelease) return 1;
        if (IsPrerelease && !other.IsPrerelease) return -1;
        if (!IsPrerelease && !other.IsPrerelease) return 0;

        return Prerelease.CompareTo(other.Prerelease); // wrong according to semver, but kept because CBA
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        var version = $"{Major}.{Minor}.{Patch}";
        if (IsPrerelease) version += $"-{Prerelease}";
        if (HasBuildMetadata) version += $"+{BuildMetadata}";
        return version;
    }

    /// <summary>
    /// Returns true if the current Major and Minor are equal to <paramref name="other"/>'s Major and Minor.
    /// 
    /// Useful to check for compatibility and such
    /// </summary>
    /// <param name="other">The object to check against</param>
    /// <returns>true if Major and Minor are equal to <paramref name="other"/>'s Major and Minor, false instead</returns>
    public bool MajorMinorEquals(SemanticVersion other) => Major == other?.Major && Minor == other?.Minor;

    /// <inheritdoc/>
    public override bool Equals(object obj) => CompareTo(obj as SemanticVersion) == 0;

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Major, Minor, Patch, Prerelease);


    /// <summary>
    /// Checks if the LHS version is newer than the RHS version
    /// </summary>
    /// <param name="left">LHS version</param>
    /// <param name="right">RHS version</param>
    /// <returns>true if left is newer than right, false instead</returns>
    public static bool operator >(SemanticVersion left, SemanticVersion right) => left.CompareTo(right) > 0;

    /// <summary>
    /// Checks if the LHS version is older than the RHS version
    /// </summary>
    /// <param name="left">LHS version</param>
    /// <param name="right">RHS version</param>
    /// <returns>true if left is older than right, false instead</returns>
    public static bool operator <(SemanticVersion left, SemanticVersion right) => left.CompareTo(right) < 0;

    /// <summary>
    /// Checks if the LHS version is newer or equal to the RHS version
    /// </summary>
    /// <param name="left">LHS version</param>
    /// <param name="right">RHS version</param>
    /// <returns>true if left is newer or equal to right, false instead</returns>
    public static bool operator >=(SemanticVersion left, SemanticVersion right) => left.CompareTo(right) >= 0;

    /// <summary>
    /// Checks if the LHS version is older or equal to the RHS version
    /// </summary>
    /// <param name="left">LHS version</param>
    /// <param name="right">RHS version</param>
    /// <returns>true if left is older or equal to right, false instead</returns>
    public static bool operator <=(SemanticVersion left, SemanticVersion right) => left.CompareTo(right) <= 0;
}
