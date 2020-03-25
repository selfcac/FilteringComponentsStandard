using System;
using System.Collections.Generic;
using System.Linq;

namespace HTTPProtocolFilter
{
    public enum BlockPhraseType // CONTENT \ EP
    {
        CONTAIN = 0, EXACTWORD, WORDCONTAINING, REGEX
    }

    public enum BlockPhraseScope
    {
        URL= 0, BODY, ANY
    }

    public enum AllowEPType //  EP
    {
        // NO EXACT because always can have params (?a=b&c=d)
        CONTAIN = 0, STARTWITH, REGEX
    }

    public enum AllowDomainType // DOMAIN
    {
        EXACT = 0, SUBDOMAINS
    }

    public enum WorkingMode
    {
        ENFORCE = 0, MAPPING
    }

    public class PhraseFilter
    {
        public BlockPhraseScope Scope;
        public BlockPhraseType Type;
        public string Phrase;

        public override string ToString()
        {
            return string.Format("[PHRASE] \"{0}\", {1} in {2}", Phrase, Type, Scope);
        }
    }

    public class EPPolicy
    {
        public AllowEPType Type;
        public string EpFormat;

        public override string ToString()
        {
            return string.Format("[EP] \"{0}\", {1}", EpFormat, Type);
        }
    }

    public class DomainPolicy
    {
        public bool DomainBlocked = false; // only if true, checke if allowed ep, if 0 just check blocked, o.w check allow then block
        public bool AllowRefering = false;
        public bool Trusted = false; // Will bypass our proxy? for ssl pinning

        public AllowDomainType Type;
        public string DomainFormat;

        public List<EPPolicy> AllowEP = new List<EPPolicy>();
        public List<EPPolicy> BlockEP = new List<EPPolicy>();

        public static implicit operator DomainPolicy (string input)
        {
            return new DomainPolicy() {
                DomainFormat = input,
                Type = ((input[0] == '.') ? AllowDomainType.SUBDOMAINS : AllowDomainType.EXACT)
                };
        }

        public override string ToString()
        {
            return string.Format("[D] \"{0}\", Allow:{1}/Block:{2}, {3}, Blocked? {4}"
                , DomainFormat, AllowEP.Count, BlockEP.Count , Type, DomainBlocked);
        }
    }

    public interface IHTTPFilter
    {
        WorkingMode getMode();

        bool isWhitelistedURL(Uri uri, out string reason);
        bool isWhitelistedURL(string host, string pathAndQuery, out string reason);

        DomainPolicy findAllowedDomain(string host);
        bool isWhitelistedHost(string host);
        bool isTrustedHost(string host);

        bool isWhitelistedEP(DomainPolicy domainObj, string ep, out string reason);

        PhraseFilter findBlockingPhrase(string Content, BlockPhraseScope scope, out string context);
        bool isContentAllowed(string Content, BlockPhraseScope scope, out string reason);

        void reloadPolicy(string jsonContent);
        string savePolicy();
    }
}
