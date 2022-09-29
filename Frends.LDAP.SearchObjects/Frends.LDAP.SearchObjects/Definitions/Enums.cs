﻿namespace Frends.LDAP.SearchObjects.Definitions;

/// <summary>
/// Search scopes.
/// </summary>
public enum Scopes
{
    /// <summary>
    /// Searches only the base DN.
    /// </summary>
    ScopeBase,

    /// <summary>
    /// Searches only entries under the base DN.
    /// </summary>
    ScopeOne,

    /// <summary>
    /// Searches the base DN and all entries within its subtree.
    /// </summary>
    ScopeSub
}

public enum SearchDereference
{
    /// <summary>
    /// Indicates that aliases are never dereferenced.
    /// </summary>
    DerefNever,

    /// <summary>
    /// Indicates that aliases are are derefrenced when searching the entries beneath the starting point of the search, but not when finding the starting entry..
    /// </summary>
    DerefSearching,

    /// <summary>
    /// Indicates that aliases are dereferenced when finding the starting point for the search, but not when searching under that starting entry.
    /// </summary>
    DerefFinding,

    /// <summary>
    /// Indicates that aliases are always dereferenced, both when finding the starting point for the search, and also when searching the entries beneath the starting entry.
    /// </summary>
    DerefAlways,
}
