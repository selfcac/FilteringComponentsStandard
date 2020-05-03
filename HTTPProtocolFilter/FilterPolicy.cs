using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HTTPProtocolFilter.Utils;
using CommonStandard;
using Newtonsoft.Json;

namespace HTTPProtocolFilter
{
    public class FilterPolicy : JSONBaseClass, IHTTPFilter
    {
        public WorkingMode proxyMode = WorkingMode.ENFORCE;

        public WorkingMode getMode()
        {
            return proxyMode;
        }

        #region DomainsFilter

        private Utils.Trie<DomainPolicy> allowedDomainsTrie;
        private List<DomainPolicy> _allDomains = new List<DomainPolicy>();

        private void initDomains(List<DomainPolicy> newDomains)
        {
            // Fast search

            allowedDomainsTrie = new Utils.Trie<DomainPolicy>();
            foreach (DomainPolicy domain in newDomains)
            {
                allowedDomainsTrie.InsertDomain(domain);
            }

            _allDomains = newDomains;
        }

        public List<DomainPolicy> AllowedDomains
        {
            get
            {
                return _allDomains;
            }
            set
            {
                initDomains(value);
            }
        }

        public FilterPolicy()
        {
            AllowedDomains = new List<DomainPolicy>();
        }

        public DomainPolicy findAllowedDomain(string host)
        {
            return allowedDomainsTrie.CheckDomain(host)?.Tag;
        }

        #endregion

        #region Phrases

        public List<PhraseFilter> BlockedPhrases = new List<PhraseFilter>();

        public static List<string> getWords(string text)
        {
            if (string.IsNullOrEmpty(text))
                return new List<string>();

            text = text.ToLower(); // all compares are done in little case

            List<string> words = new List<string>();
            bool insideWord = char.IsLetter(text[0]);
            StringBuilder currentWord = new StringBuilder();

            for (int i = 0; i < text.Length; i++)
            {
                if (char.IsLetter(text[i]))
                {
                    if (insideWord)
                    {
                    }
                    else
                    {
                        insideWord = true;
                    }
                    currentWord.Append(text[i]);
                }
                else // Not char
                {
                    if (insideWord)
                    {
                        insideWord = false;
                        if (currentWord.Length > 0)
                            words.Add(currentWord.ToString());
                        currentWord.Clear();
                    }
                    else
                    {
                        // Nothing
                    }
                }
            }

            if (insideWord) // Word at the end of text
            {
                if (currentWord.Length > 0)
                    words.Add(currentWord.ToString());
            }

            return words;
        }

        public static string getWordSurrounding(string content, int index, int tryLength, int expand = 10)
        {
            if (string.IsNullOrEmpty(content))
                return "";

            int before = Math.Max(index - expand, 0);
            int after = Math.Min(index + tryLength + expand, content.Length);

            return content.Substring(before, (index - before)) + "_<" +
                   content.Substring(index, tryLength) + ">_" +
                   content.Substring(index + tryLength, (after - (index + tryLength)) );
        }

        public static bool checkPhraseFoundSimple(string Content, PhraseFilter filter, out string context)
        {
            context = "";

            if ((filter.Phrase ?? "") == "")
                return false;

            bool found = false;
            Content = Content.ToLower();
            switch (filter.Type)
            {
                case BlockPhraseType.CONTAIN:
                    var index = Content.IndexOf(filter.Phrase);
                    if (index > -1)
                        context = getWordSurrounding(Content, index, filter.Phrase.Length);
                    found = index > -1;
                    break;
                case BlockPhraseType.REGEX:
                    var match = Regex.Match(Content, filter.Phrase);
                    if (match.Groups[0].Length > 0)
                        context = getWordSurrounding(Content, match.Groups[0].Index, match.Groups[0].Length);
                    found = match.Groups[0].Length > 0 ;
                    break;
            }

            return found;
        }

        public static bool checkPhraseFoundWord(List<string> words, PhraseFilter filter, out string  context)
        {
            context = "";

            if ((filter.Phrase ?? "") == "")
                return false;

            bool found = false;
            int index = 0;
            switch (filter.Type)
            {
                case BlockPhraseType.EXACTWORD:
                    index = words.IndexOf(filter.Phrase.ToLower());
                    if (index > -1)
                        context = words[index];
                    found = index > -1;
                    break;
                case BlockPhraseType.WORDCONTAINING:
                    index = words.FindIndex((word) => word.Contains(filter.Phrase.ToLower())) ;
                    if (index > -1)
                        context = words[index];
                    found = index > -1;
                    break;
            }

            context = "_<" + context + ">_";
            return found;
        }

        /// <summary>
        /// Check if content clean of bad words
        /// </summary>
        /// <param name="Content"></param>
        /// <returns>True if content is allowed under policy</returns>
        public bool isContentAllowed(string Content, BlockPhraseScope scope, out string reason)
        {
            string phraseContext = "";
            var phrase = findBlockingPhrase(Content, scope, out phraseContext);
            if (phrase == null)
            {
                reason = "content allowed, no phrase found in scope " + scope;
                return true;
            }
            else
            {
                reason = "content blocked because scope <*" + scope 
                    + "*> equal phrase <*" + phrase.ToString()
                    + "*>, context: " + phraseContext;
                return false;
            }
        }

        /// <summary>
        /// Find the phrase that the content is blocked from using.
        /// </summary>
        /// <param name="Content">string chunk</param>
        /// <returns>Null if no phrase rule is applicabalbe (allowed)</returns>
        public PhraseFilter findBlockingPhrase(string Content, BlockPhraseScope scope , out string context)
        {
            PhraseFilter result = null;
            List<string> Words = getWords(Content);
            context = "";

            for (int i = 0; i < BlockedPhrases.Count && (result == null); i++)
            {
                if (
                    // Any of the scope is ALL
                    scope == BlockPhraseScope.ANY
                    || BlockedPhrases[i].Scope == BlockPhraseScope.ANY

                    // Scopes are equal:
                    || BlockedPhrases[i].Scope == scope
                    )
                {
                    switch (BlockedPhrases[i].Type)
                    {
                        case BlockPhraseType.CONTAIN:
                        case BlockPhraseType.REGEX:
                            if (checkPhraseFoundSimple(Content, BlockedPhrases[i], out context))
                            {
                                result = BlockedPhrases[i];
                            }
                            break;

                        case BlockPhraseType.EXACTWORD:
                        case BlockPhraseType.WORDCONTAINING:
                            if (checkPhraseFoundWord(Words, BlockedPhrases[i], out context))
                            {
                                result = BlockedPhrases[i];
                            }
                            break;
                    }
                }
            }

            return result;
        }

        #endregion 

       
        public static bool checkEPRuleMatch(EPPolicy ep, string epPath)
        {
            bool match = false;
            epPath = epPath.ToLower();

            switch(ep.Type)
            {
                case AllowEPType.CONTAIN:
                    match = epPath.IndexOf(ep.EpFormat.ToLower()) > -1; // not using contain cause HTTP is ASCII only
                    break;

                case AllowEPType.REGEX:
                    match = Regex.IsMatch(epPath, ep.EpFormat);
                    break;

                case AllowEPType.STARTWITH:
                    match = epPath.StartsWith(ep.EpFormat.ToLower());
                    break;
            }

            return match;
        }

        /// <summary>
        /// Check if ep is in policy
        /// </summary>
        /// <returns>true if allowed</returns>
        public bool isWhitelistedEP(DomainPolicy domainObj, string ep, out string reason)
        {
            if (domainObj == null)
            {
                reason = "domain object is null";
                return false;
            }

            if (domainObj.DomainBlocked)
            {
                reason = "Domain <*" + domainObj.DomainFormat + "*> in block mode";
                return false;
            }

            bool allowed = false;
            reason = "block <*" + ep + "*> because not in <*" + domainObj.DomainFormat + "*> (not whitelisted) ";


            // check if ep in domain
            if (domainObj.AllowEP.Count > 0 )
            {
                // Whitelist : All EP are blocked unless some ep allow (and no block ep later).
                for (int i=0;i<domainObj.AllowEP.Count;i++)
                {
                    if (checkEPRuleMatch(domainObj.AllowEP[i], ep))
                    {
                        allowed = true;
                        reason = domainObj.DomainFormat + ep + " is allowed by " + domainObj.AllowEP[i].ToString();
                        break;
                    }
                }
            }
            else
            {
                // All EP allowed unless found by block ep
                allowed = true;
            }

            if (allowed)
            {
                for (int i = 0; i < domainObj.BlockEP.Count; i++)
                {
                    if (checkEPRuleMatch(domainObj.BlockEP[i], ep))
                    {
                        allowed = false;
                        reason = "<*" + domainObj.DomainFormat + ep + "*> is blocked by <*" + domainObj.BlockEP[i].ToString() + "*>";
                        break;
                    }
                }
            }


            return allowed;
        }

        /// <summary>
        /// Check if Host (domain) is in policy
        /// </summary>
        /// <returns>true if allowed</returns>
        public bool isWhitelistedHost(string host)
        {
            DomainPolicy domain = allowedDomainsTrie.CheckDomain(host)?.Tag;
            return domain != null;
        }

        public bool isTrustedHost(string host)
        {
            DomainPolicy domain = allowedDomainsTrie.CheckDomain(host)?.Tag;
            return domain != null && domain.Trusted;
        }

        /// <summary>
        /// Check if complete URL is in policy (Host+EP+Phrase checks)
        /// </summary>
        /// <returns>true if allowed</returns>
        public bool isWhitelistedURL(Uri uri, out string reason)
        {
            return isWhitelistedURL(uri.Host, uri.PathAndQuery, out reason);
        }

        public bool isWhitelistedURL(string host, string pathAndQuery, out string reason)
        {
            bool allowed = false;
            DomainPolicy domainRule = findAllowedDomain(host ?? "");
            if (domainRule != null)
            {
                allowed = isWhitelistedEP(domainRule, pathAndQuery ?? "", out reason);

                if (allowed) // Finally check for banned phrases. 
                {
                    string phrase_reason = "";
                    allowed = isContentAllowed(host + pathAndQuery, BlockPhraseScope.URL, out phrase_reason);
                    if (!allowed)
                    {
                        reason = host + pathAndQuery + ", "+   phrase_reason ;
                    }
                }
            }
            else
            {
                reason = "Domain <*" + host + "*> is not whitelisted";
            }

            return allowed;
        }

        public bool isBodyBlocked(string content, out string reason)
        {
            return !isContentAllowed(content, BlockPhraseScope.BODY, out reason);
        }

        public void reloadPolicy(string jsonContent)
        {
            FilterPolicy newPolicy = JSONBaseClass.FromJSONString<FilterPolicy>(jsonContent,defValue: null);
            if (newPolicy != null)
            {
                proxyMode = newPolicy.proxyMode;
                BlockedPhrases = newPolicy.BlockedPhrases;
                AllowedDomains = newPolicy.AllowedDomains;
            }
        }

        public string savePolicy()
        {
            return ToJSON();
        }

        public void addWhitelistDomain(string host)
        {
            if (string.IsNullOrEmpty(host))
                return;

            AllowedDomains.Add((DomainPolicy)host); // will add subdomain type if startWith '.'
            AllowedDomains = AllowedDomains; // Refresh the Trie structure
        }

        public string getPhrasesJson()
        {
            return JsonConvert.SerializeObject(BlockedPhrases);
        }
    }
}
