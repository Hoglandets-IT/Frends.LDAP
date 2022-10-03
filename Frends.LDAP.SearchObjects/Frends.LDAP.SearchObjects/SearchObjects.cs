﻿using Frends.LDAP.SearchObjects.Definitions;
using System.ComponentModel;
using Novell.Directory.Ldap;
using System;
using System.Collections.Generic;

namespace Frends.LDAP.SearchObjects;

/// <summary>
/// LDAP task.
/// </summary>
public class LDAP
{
    /// <summary>
    /// Search objects from Active Directory.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.LDAP.SearchObjects)
    /// </summary>
    /// <param name="input">Input parameters.</param>
    /// <param name="connection">Connection parameters.</param>
    /// <returns>Object { bool Success, string Error, string CommonName, List&lt;SearchResult&gt; SearchResult }</returns>
    public static Result SearchObjects([PropertyTab] Input input, [PropertyTab] Connection connection)
    {
        if (string.IsNullOrWhiteSpace(connection.Host) || string.IsNullOrWhiteSpace(connection.User) || string.IsNullOrWhiteSpace(connection.Password))
            throw new Exception("Connection parameters missing.");

        var conn = new LdapConnection();
        var defaultPort = connection.SecureSocketLayer ? 636 : 389;
        var atr = new List<string>();
        var searchResults = new List<SearchResult>();
        var searchConstraints = new LdapSearchConstraints(
                input.MsLimit, 
                input.ServerTimeLimit, 
                SetSearchDereference(input), 
                input.MaxResults,
                false,
                input.BatchSize,
                null,
                0);
        
        if(input.Attributes != null)
            foreach (var i in input.Attributes)
                atr.Add(i.Key.ToString());

        try
        {
            conn.SecureSocketLayer = connection.SecureSocketLayer;
            conn.Connect(connection.Host, connection.Port == 0 ? defaultPort : connection.Port);
            if (connection.TLS) conn.StartTls();
            conn.Bind(connection.User, connection.Password);

            LdapSearchQueue queue = conn.Search(
                input.SearchBase, SetScope(input), 
                input.Filter,
                atr.ToArray(), 
                input.TypesOnly, 
                null, 
                searchConstraints);

            LdapMessage message;
            while ((message = queue.GetResponse()) != null)
            {
                if (message is LdapSearchResult ldapSearchResult)
                {
                    var entry = ldapSearchResult.Entry;
                    var attributeList = new List<AttributeSet>();
                    var getAttributeSet = entry.GetAttributeSet();
                    var ienum = getAttributeSet.GetEnumerator();

                    while (ienum.MoveNext())
                    {
                        LdapAttribute attribute = ienum.Current;
                        var attributeName = attribute.Name;
                        var attributeVal = attribute.StringValue;
                        attributeList.Add(new AttributeSet { Key = attributeName, Value = attributeVal });
                    }

                    searchResults.Add(new SearchResult() { DistinguishedName = entry.Dn, AttributeSet = attributeList });
                }
            }
            return new Result(true, null, searchResults);
        }
        catch (LdapException ex)
        {
            return new Result(false, ex.Message, null);
        }
        catch (Exception ex)
        {
            throw new Exception($"SearchObjects error: {ex}");
        }
        finally
        {
            if (connection.TLS) conn.StopTls();
            conn.Disconnect();
        }
    }

    internal static int SetScope(Input input)
    {
        return input.Scope switch
        {
            Scopes.ScopeBase => 0,
            Scopes.ScopeOne => 1,
            Scopes.ScopeSub => 2,
            _ => throw new Exception("SetScope error: Invalid scope."),
        };
    }

    internal static int SetSearchDereference(Input input)
    {
        return input.SearchDereference switch
        {
            SearchDereference.DerefNever => 0,
            SearchDereference.DerefSearching => 1,
            SearchDereference.DerefFinding => 2,
            SearchDereference.DerefAlways => 3,
            _ => throw new Exception("SetSearchConstraint error: Invalid search constraint."),
        };
    }
}