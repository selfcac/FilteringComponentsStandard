using HTTPProtocolFilter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace HTTPFilterTest
{
    [TestClass]
    public class UnitTest1
    {
        void areTrue(object actual) { Assert.AreEqual(true, actual); }
        void areFalse(object actual) { Assert.AreEqual(false, actual); }

        [TestMethod()]
        public void getWordsTest()
        {
            string text = "<input value=\"gogo\"></input>";
            Assert.AreEqual(true, FilterPolicy.getWords(text).Contains("gogo"));

            text = "go home \"gogo\" g";
            Assert.AreEqual(true, FilterPolicy.getWords(text).Contains("gogo"));

            text = "first second thidr122";
            var words = FilterPolicy.getWords(text);
            areTrue(words.Contains("first"));
            areTrue(words.Contains("second"));
            areTrue(words.Contains("thidr"));
            areFalse(words.Contains("thidr122"));
        }

        [TestMethod()]
        public void checkPhraseFoundSimpleTest()
        {
            string BodyContent = "This text has baDword and \"2wronGword\" and nonOword3 wroNgworK";
            string context = "", allContexts = "";

            areFalse(FilterPolicy.checkPhraseFoundSimple(BodyContent, new PhraseFilter()
            {
                Phrase = "text",
                Type = BlockPhraseType.EXACTWORD // Not supported in checkPhraseFoundSimple
            }, out context)); allContexts += context + '\n';

            areFalse(FilterPolicy.checkPhraseFoundSimple("", new PhraseFilter()
            {
                Phrase = "notfound",
                Type = BlockPhraseType.CONTAIN
            }, out context)); allContexts += context + '\n';

            areFalse(FilterPolicy.checkPhraseFoundSimple("", new PhraseFilter()
            {
                Phrase = "notfound",
                Type = BlockPhraseType.REGEX
            }, out context)); allContexts += context + '\n';

            areFalse(FilterPolicy.checkPhraseFoundSimple(BodyContent, new PhraseFilter()
            {
                Phrase = "notfound",
                Type = BlockPhraseType.CONTAIN
            }, out context)); allContexts += context + '\n';

            areTrue(FilterPolicy.checkPhraseFoundSimple(BodyContent, new PhraseFilter()
            {
                Phrase = "badword",
                Type = BlockPhraseType.CONTAIN
            }, out context)); allContexts += context + '\n';

            areTrue(FilterPolicy.checkPhraseFoundSimple(BodyContent, new PhraseFilter()
            {
                Phrase = "wrongword",
                Type = BlockPhraseType.CONTAIN
            }, out context)); allContexts += context + '\n';

            areTrue(FilterPolicy.checkPhraseFoundSimple(BodyContent, new PhraseFilter()
            {
                Phrase = "nonoword",
                Type = BlockPhraseType.CONTAIN
            }, out context)); allContexts += context + '\n';

            areTrue(FilterPolicy.checkPhraseFoundSimple(BodyContent, new PhraseFilter()
            {
                Phrase = "wrongwor[tk]",
                Type = BlockPhraseType.REGEX
            }, out context)); allContexts += context + '\n';

            //Console.Write(allContexts);
        }

        [TestMethod()]
        public void checkPhraseFoundWordTest()
        {
            string context = "", allContexts = "";
            string BodyContent = "This text has baDword and \"2wronGword\" and nonOword3 wroNgworK";
            List<string> words = FilterPolicy.getWords(BodyContent);

            areFalse(FilterPolicy.checkPhraseFoundWord(words, new PhraseFilter()
            {
                Phrase = "text",
                Type = BlockPhraseType.CONTAIN // Not supported in checkPhraseFoundWord
            }, out context)); allContexts += context + '\n';

            areFalse(FilterPolicy.checkPhraseFoundWord(words, new PhraseFilter()
            {
                Phrase = "",
                Type = BlockPhraseType.WORDCONTAINING
            }, out context)); allContexts += context + '\n';

            areFalse(FilterPolicy.checkPhraseFoundWord(words, new PhraseFilter()
            {
                Phrase = "",
                Type = BlockPhraseType.EXACTWORD
            }, out context)); allContexts += context + '\n';

            areFalse(FilterPolicy.checkPhraseFoundWord(words, new PhraseFilter()
            {
                Phrase = "notfound",
                Type = BlockPhraseType.WORDCONTAINING
            }, out context)); allContexts += context + '\n';

            areTrue(FilterPolicy.checkPhraseFoundWord(words, new PhraseFilter()
            {
                Phrase = "badword",
                Type = BlockPhraseType.EXACTWORD
            }, out context)); allContexts += context + '\n';

            areTrue(FilterPolicy.checkPhraseFoundWord(words, new PhraseFilter()
            {
                Phrase = "wrongword",
                Type = BlockPhraseType.EXACTWORD
            }, out context)); allContexts += context + '\n';

            areTrue(FilterPolicy.checkPhraseFoundWord(words, new PhraseFilter()
            {
                Phrase = "nonoword",
                Type = BlockPhraseType.EXACTWORD
            }, out context)); allContexts += context + '\n';

            areTrue(FilterPolicy.checkPhraseFoundWord(words, new PhraseFilter()
            {
                Phrase = "rongwor",
                Type = BlockPhraseType.WORDCONTAINING
            }, out context)); allContexts += context + '\n';

            //Console.Write(allContexts);
        }

        [TestMethod()]
        public void checkEPRuleTest()
        {
            areTrue(FilterPolicy.checkEPRuleMatch(new EPPolicy()
            {
                Type = AllowEPType.STARTWITH,
                EpFormat = "/r/collect"
            }, "/r/coLleCt"));

            areTrue(FilterPolicy.checkEPRuleMatch(new EPPolicy()
            {
                Type = AllowEPType.CONTAIN,
                EpFormat = "&safe=1"
            }, "/searcH?q=anyword&safe=1"));

            areTrue(FilterPolicy.checkEPRuleMatch(new EPPolicy()
            {
                Type = AllowEPType.REGEX,
                EpFormat = "\\/search\\?q=.*&safe=1"
            }, "/searCh?q=anyword&safe=1"));

            areFalse(FilterPolicy.checkEPRuleMatch(new EPPolicy()
            {
                Type = AllowEPType.REGEX,
                EpFormat = "\\/search\\?q=.*&safe=1"
            }, "/search?q=anyword"));

            areFalse(FilterPolicy.checkEPRuleMatch(new EPPolicy()
            {
                Type = AllowEPType.REGEX,
                EpFormat = "\\/search\\?q=.*&safe=1"
            }, ""));


        }

        [TestMethod()]
        public void isWhitelistedEPTest()
        {
            string reason_throwaway = "";

            IHTTPFilter filter = new FilterPolicy()
            {
                BlockedPhrases = new List<PhraseFilter>()
                {
                    new PhraseFilter()
                    {
                        Type = BlockPhraseType.WORDCONTAINING ,
                        Phrase= "bad",
                        Scope = BlockPhraseScope.ANY
                    },
                    new PhraseFilter()
                    {
                        Type = BlockPhraseType.REGEX,
                        Phrase = "wor[dk]",
                        Scope = BlockPhraseScope.ANY
                    }
                }

                ,
                AllowedDomains = new List<DomainPolicy>()
                {
                    new DomainPolicy()
                    {
                        DomainBlocked = false,
                        DomainFormat = "e.com",
                        Type = AllowDomainType.EXACT,
                        AllowEP = new List<EPPolicy>()
                        {
                           new EPPolicy() {
                                Type = AllowEPType.STARTWITH,
                                 EpFormat = "/i-am-whitelisted"
                           }
                        }
                    }
                }
            };

            string ep1 = "/search?q=verybadword";
            string ep2 = "/i-am-whitelisted";
            string ep3 = "/i-am-whitelisted/badword";

            // any ep except bad phrases:
            areTrue(filter.isWhitelistedEP(new DomainPolicy()
            {
                DomainBlocked = false,
                DomainFormat = "",
                Type = AllowDomainType.EXACT,
                AllowEP = new List<EPPolicy>()
            }, ep1, out reason_throwaway));
            areFalse(filter.isContentAllowed(ep1, BlockPhraseScope.URL, out reason_throwaway));

            // only ep that are whitelisted
            var domain1 = ((FilterPolicy)filter).AllowedDomains[0];

            areTrue(filter.isWhitelistedEP(domain1, ep2, out reason_throwaway));
            areFalse(filter.isWhitelistedEP(domain1, "/not-whitelisted", out reason_throwaway));

            areTrue(filter.isWhitelistedEP(domain1, ep2 + "/work", out reason_throwaway));
            areFalse(filter.isContentAllowed(ep2 + "/work", BlockPhraseScope.URL, out reason_throwaway));
            areFalse(filter.isWhitelistedURL(new Uri("http://e.com" + ep2 + "/work"), out reason_throwaway)); // does both checks

            areFalse(filter.isContentAllowed(ep3, BlockPhraseScope.URL, out reason_throwaway));
            areFalse(filter.isWhitelistedURL(new Uri("http://e.com" + ep3), out reason_throwaway)); // does both checks

            areFalse(filter.isWhitelistedEP(null, "", out reason_throwaway));
        }

        [TestMethod()]
        public void URLTest()
        {
            string reason_throwaway = "";

            FilterPolicy filter = new FilterPolicy()
            {
                BlockedPhrases = new List<PhraseFilter>()
                {
                    new PhraseFilter()
                    {
                        Type = BlockPhraseType.WORDCONTAINING ,
                        Phrase= "bad",
                        Scope = BlockPhraseScope.URL
                    },
                    new PhraseFilter()
                    {
                        Type = BlockPhraseType.REGEX,
                        Phrase = "wor[dk]",
                        Scope = BlockPhraseScope.URL
                    }
                }
            };

            filter.AllowedDomains = new List<DomainPolicy>()
            {
                new DomainPolicy()
                {
                    DomainBlocked = false,
                    DomainFormat = "go.com",
                    Type = AllowDomainType.SUBDOMAINS,
                    AllowEP = new List<EPPolicy>()
                    {
                       new EPPolicy() {
                            Type = AllowEPType.STARTWITH,
                             EpFormat = "/i-am-whitelisted"
                       }
                    }
                }
            };

            string ep1 = "/search?q=verybadword";
            string ep2 = "/i-am-whitelisted";
            string ep3 = "/i-am-whitelisted/badword";

            areTrue(filter.isWhitelistedHost("g.go.com"));
            areTrue(filter.isWhitelistedURL("go.com", ep2, out reason_throwaway));
            areFalse(filter.isWhitelistedURL("go.com", "/not-ok", out reason_throwaway));
            areFalse(filter.isWhitelistedURL("go-go.com", ep2, out reason_throwaway));

            areTrue(filter.isWhitelistedHost("go.com"));
            areFalse(filter.isWhitelistedURL("g.go.com", ep2 + "/work", out reason_throwaway));

        }

        [TestMethod()]
        public void nullAtInit()
        {
            IHTTPFilter filter = new FilterPolicy();
            try
            {
                filter.isWhitelistedHost("try.com");
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.ToString());
            }
        }

        [TestMethod()]
        public void emptyContent()
        {
            FilterPolicy filter = new FilterPolicy()
            {
                BlockedPhrases = new List<PhraseFilter>()
                {
                    new PhraseFilter()
                    {
                        Type = BlockPhraseType.WORDCONTAINING ,
                        Phrase= "bad"
                    },
                    new PhraseFilter()
                    {
                        Type = BlockPhraseType.REGEX,
                        Phrase = "wor[dk]"
                    }
                }
            };


            Assert.AreEqual(null, filter.findBlockingPhrase("", BlockPhraseScope.ANY, out _));
        }

        [TestMethod()]
        public void newBlockFeatures()
        {
            var epAllowList = new List<EPPolicy>();

            FilterPolicy filter = new FilterPolicy()
            {
                BlockedPhrases = new List<PhraseFilter>()
                {
                    new PhraseFilter()
                    {
                        Type = BlockPhraseType.WORDCONTAINING ,
                        Phrase= "ssearch",
                        Scope = BlockPhraseScope.URL // only search url will blocked
                    },
                    new PhraseFilter()
                    {
                        Type = BlockPhraseType.EXACTWORD,
                        Phrase = "veryHTMLTerm",
                        Scope = BlockPhraseScope.BODY // like html
                    },
                    new PhraseFilter()
                    {
                        Type = BlockPhraseType.WORDCONTAINING,
                        Phrase = "veryBadTerm",
                        Scope = BlockPhraseScope.ANY // just block it -  Very bad term!
                    },
                },

                AllowedDomains = new List<DomainPolicy>()
                {
                    new DomainPolicy()
                    {
                        DomainBlocked = true,
                        DomainFormat = "777.com",
                        Type = AllowDomainType.SUBDOMAINS
                    },
                    new DomainPolicy()
                    {
                        DomainBlocked = false,
                        DomainFormat = "666.com",
                        Type = AllowDomainType.SUBDOMAINS,

                        AllowEP = epAllowList,
                        BlockEP = new List<EPPolicy>()
                        {
                            new EPPolicy()
                            {
                                EpFormat = "\\/search(\\/{0,1})",
                                Type = AllowEPType.REGEX
                            }
                        }
                    }
                }

            };
            string totalReason = "";
            string reason = "";

            // blocked by url\any but not body scope
            areFalse(filter.isWhitelistedURL("a.666.com", "/ssearch", out reason)); totalReason += reason + '\n';
            areFalse(filter.isWhitelistedURL("a.666.com", "/ok?q=veryBadTerm", out reason)); totalReason += reason + '\n';
            areTrue(filter.isWhitelistedURL("a.666.com", "/ok?q=veryHTMLTerm", out reason)); totalReason += "<Allowed>\n";

            areFalse(filter.isContentAllowed("<veryHTMLTerm>", BlockPhraseScope.BODY, out reason)); totalReason += reason + '\n';
            areTrue(filter.isContentAllowed("<ssearch>", BlockPhraseScope.BODY, out reason)); totalReason += "<Allowed>\n";
            areFalse(filter.isContentAllowed("<veryBadTerm>", BlockPhraseScope.BODY, out reason)); totalReason += reason + '\n';

            areTrue(filter.isContentAllowed("<veryHTMLTerm>", BlockPhraseScope.URL, out reason)); totalReason += "<Allowed>\n";
            areFalse(filter.isContentAllowed("<ssearch>", BlockPhraseScope.URL, out reason)); totalReason += reason + '\n';
            areFalse(filter.isContentAllowed("<veryBadTerm>", BlockPhraseScope.URL, out reason)); totalReason += reason + '\n';

            areFalse(filter.isContentAllowed("<veryHTMLTerm>", BlockPhraseScope.ANY, out reason)); totalReason += reason + '\n';
            areFalse(filter.isContentAllowed("<ssearch>", BlockPhraseScope.ANY, out reason)); totalReason += reason + '\n';
            areFalse(filter.isContentAllowed("<veryBadTerm>", BlockPhraseScope.ANY, out reason)); totalReason += reason + '\n';


            // Blocked 777 by "DomainBlocked"
            areFalse(filter.isWhitelistedURL("a.777.com", "/some-ep", out reason)); totalReason += reason + '\n';

            // Allowed EP in 666 by default
            areTrue(filter.isWhitelistedURL("a.666.com", "/some-ep", out reason)); totalReason += "<Allowed>\n";

            // Blocked EP in 666 by blocke EP
            areFalse(filter.isWhitelistedURL("a.666.com", "/some-ep/search/", out reason)); totalReason += reason + '\n';
            areFalse(filter.isWhitelistedURL("a.666.com", "/some-ep/search", out reason)); totalReason += reason + '\n';
            areFalse(filter.isWhitelistedURL("a.666.com", "/search/gggg", out reason)); totalReason += reason + '\n';

            epAllowList.Add(new EPPolicy()
            {
                EpFormat = "/img",
                Type = AllowEPType.STARTWITH
            });

            // Blocked EP in 66 when not default - no t in whitelist
            areFalse(filter.isWhitelistedURL("a.666.com", "/some-ep", out reason)); totalReason += reason + '\n';

            // Allowed EP in 66 when w.l. and not blocked by ep
            areTrue(filter.isWhitelistedURL("a.666.com", "/img?a=b&b=c", out reason)); totalReason += "<Allowed>\n";

            // Blocked EP in 66 when w.l. and blocked by ep
            areFalse(filter.isWhitelistedURL("a.666.com", "/img/search?a=b&b=c", out reason)); totalReason += reason;

            Console.Write(totalReason);

        }

        [TestMethod()]
        public void getWordSurroundingTest()
        {
            string testResult = "";
            string Content = "this are very tight text";

            string word = "this";
            testResult += FilterPolicy.getWordSurrounding(Content, Content.IndexOf(word), word.Length) + '\n';

            word = "are";
            testResult += FilterPolicy.getWordSurrounding(Content, Content.IndexOf(word), word.Length) + '\n';

            word = "ver";
            testResult += FilterPolicy.getWordSurrounding(Content, Content.IndexOf(word), word.Length) + '\n';

            word = "text";
            testResult += FilterPolicy.getWordSurrounding(Content, Content.IndexOf(word), word.Length) + '\n';


            Content = "long long long long long long this are very tight text long long long long long long";

            word = "this";
            testResult += FilterPolicy.getWordSurrounding(Content, Content.IndexOf(word), word.Length) + '\n';

            word = "are";
            testResult += FilterPolicy.getWordSurrounding(Content, Content.IndexOf(word), word.Length) + '\n';

            word = "ver";
            testResult += FilterPolicy.getWordSurrounding(Content, Content.IndexOf(word), word.Length) + '\n';

            word = "text";
            testResult += FilterPolicy.getWordSurrounding(Content, Content.IndexOf(word), word.Length);

            Console.Write(testResult);

        }


        [TestMethod()]
        public void regexContext()
        {
            FilterPolicy filter = new FilterPolicy()
            {
                BlockedPhrases = new List<PhraseFilter>()
                {
                    new PhraseFilter()
                    {
                        Type = BlockPhraseType.REGEX,
                        Phrase = "la[tp]ex",
                        Scope = BlockPhraseScope.ANY // just block it -  Very bad term!
                    },
                    new PhraseFilter()
                    {
                        Type = BlockPhraseType.REGEX,
                        Phrase = "^(?=.*search)(?!.*google\\.com).*\\/search.*[\\?&]q\\=",
                        Scope = BlockPhraseScope.URL,
                    }
                },
                AllowedDomains = new List<DomainPolicy>()
                {
                    new DomainPolicy()
                    {
                         DomainBlocked = false,
                         DomainFormat = ".google.com",
                         Type = AllowDomainType.SUBDOMAINS
                    }
                }
            };

            string reason = "";
            areFalse(filter.isContentAllowed("translateX: -50%", BlockPhraseScope.BODY, out reason));

            areTrue(filter.isWhitelistedURL(new Uri(
                "https://www.google.com/search?q=normal%20search&oq=normal%20search&aqs=chrome..69i57.2865j0j1&sourceid=chrome&ie=UTF-8&safe=active")
                , out reason));


            Console.Write(reason);
        }



        [TestMethod()]
        public void blockSubdomainAfterDomain()
        {
            FilterPolicy filter = new FilterPolicy()
            {
                BlockedPhrases = new List<PhraseFilter>() { },
                AllowedDomains = new List<DomainPolicy>()
                {
                    new DomainPolicy()
                    {
                         DomainBlocked = true,
                         DomainFormat = "aaa.maariv.co.il",
                         Type = AllowDomainType.EXACT
                    },
                    new DomainPolicy()
                    {
                         DomainBlocked = false,
                         DomainFormat = ".maariv.co.il",
                         Type = AllowDomainType.SUBDOMAINS
                    },
                    new DomainPolicy()
                    {
                         DomainBlocked = true,
                         DomainFormat = "tmi.maariv.co.il",
                         Type = AllowDomainType.EXACT
                    },
                }
            };


            string reason = "";

            areFalse(filter.isWhitelistedURL(new Uri(
                "https://aaa.maariv.co.il/sss?a=b&c=d")
                , out reason));

            areTrue(filter.isWhitelistedURL(new Uri(
                "https://www.maariv.co.il/sss?a=b&c=d")
                , out reason));

            areTrue(filter.isWhitelistedURL(new Uri(
                "https://maariv.co.il/sss?a=b&c=d")
                , out reason));

            areTrue(filter.isWhitelistedURL(new Uri(
                "https://abc.maariv.co.il/sss?a=b&c=d")
                , out reason));

            areFalse(filter.isWhitelistedURL(new Uri(
                "https://tmi.maariv.co.il/sss?a=b&c=d")
                , out reason));

            Console.WriteLine(reason);
        }

        //[TestMethod()]
        //public void googleBodyPPVC()
        //{
        //    string content = File.ReadAllText(@"C:\Users\Yoni\Desktop\selfcac\UiTests\ppvc google body.html");
        //    areTrue(FilterPolicy.getWords(content).Contains("pvc"));

        //}

        [TestMethod()]
        public void PhraseBug()
        {
            string url = "https://www.youtube.com/results?search_query=fetish&pbj=1";
            areTrue(FilterPolicy.getWords(url).Contains("fetish"));

        }

        [TestMethod()]
        public void checkEPRuleMatchTest_LowerText()
        {
            areTrue(FilterPolicy.checkEPRuleMatch(new EPPolicy() { EpFormat = "/LOWER", Type = AllowEPType.STARTWITH }, "/lower"));
            areTrue(FilterPolicy.checkEPRuleMatch(new EPPolicy() { EpFormat = "LOWER", Type = AllowEPType.CONTAIN }, "/lower-some"));
        }

        [TestMethod()]
        public void trustedDomains()
        {
            FilterPolicy filter = new FilterPolicy()
            {
                BlockedPhrases = new List<PhraseFilter>() { },
                AllowedDomains = new List<DomainPolicy>()
                {
                    new DomainPolicy()
                    {
                         DomainBlocked = true,
                         DomainFormat = "www.shalom.co.il",
                         Type = AllowDomainType.EXACT,
                         Trusted = true,
                    },
                    new DomainPolicy()
                    {
                         DomainBlocked = false,
                         DomainFormat = ".maariv.co.il",
                         Type = AllowDomainType.SUBDOMAINS,
                         Trusted = true,
                    },
                    new DomainPolicy()
                    {
                         DomainBlocked = false,
                         DomainFormat = ".ynet.co.il",
                         Type = AllowDomainType.SUBDOMAINS
                    },
                }
            };

            Assert.IsTrue(filter.isTrustedHost("www.shalom.co.il"));
            Assert.IsFalse(filter.isTrustedHost("wwshalom.co.il"));

            Assert.IsTrue(filter.isTrustedHost("maariv.co.il"));
            Assert.IsFalse(filter.isTrustedHost("submaariv.co.il"));
            Assert.IsTrue(filter.isTrustedHost("sub.maariv.co.il"));

            Assert.IsFalse(filter.isTrustedHost("ynet.co.il"));
            Assert.IsFalse(filter.isTrustedHost("sub.ynet.co.il"));
        }



        [TestMethod()]
        public void MitmEndpointsFeatures()
        {
            FilterPolicy filter = new FilterPolicy()
            {
                BlockedPhrases = new List<PhraseFilter>() { },
                AllowedDomains = new List<DomainPolicy>()
                {
                    new DomainPolicy()
                    {
                         DomainBlocked = true,
                         DomainFormat = "aaa.maariv.co.il",
                         Type = AllowDomainType.EXACT
                    },
                    new DomainPolicy()
                    {
                         DomainBlocked = false,
                         DomainFormat = ".maariv.co.il",
                         Type = AllowDomainType.SUBDOMAINS
                    },
                    new DomainPolicy()
                    {
                         DomainBlocked = true,
                         DomainFormat = "tmi.maariv.co.il",
                         Type = AllowDomainType.EXACT
                    },
                }
            };

            string newDomain1 = "add.com";
            string newDomain2 = ".addmore.com";
            string newDomain3 = "extra.com";

            // Add whitelist
            Assert.IsFalse(filter.isWhitelistedHost(newDomain1));
            filter.addWhitelistDomain(newDomain1);
            Assert.IsTrue(filter.isWhitelistedHost(newDomain1));
            Assert.IsTrue(filter.isWhitelistedURL(newDomain1,"/anyEp", out _));

            // Add whitelist sub+domain
            Assert.IsFalse(filter.isWhitelistedHost(newDomain2));
            filter.addWhitelistDomain(newDomain2);
            Assert.IsTrue(filter.isWhitelistedHost("sub" + newDomain2));
            Assert.IsTrue(filter.isWhitelistedURL("sub2" + newDomain2, "/anyEp", out _));

            // Block domain by find
            filter.findAllowedDomain(newDomain1).DomainBlocked = true;
            Assert.IsFalse(filter.isWhitelistedURL(newDomain1, "/anyEp", out _));

            // Add EP (allow \\ block)
            filter.addWhitelistDomain(newDomain3);
            filter.findAllowedDomain(newDomain3).AddEndpoint(true, "/ok", AllowEPType.STARTWITH);
            Assert.IsFalse(filter.isWhitelistedURL(newDomain3, "/", out _));
            Assert.IsTrue(filter.isWhitelistedURL(newDomain3,"/ok_1", out _));
            Assert.IsFalse(filter.isWhitelistedURL(newDomain3, "/not-ok_1", out _));

            filter.findAllowedDomain(newDomain3).AddEndpoint(false, "bad", AllowEPType.CONTAIN);
            Assert.IsFalse (filter.isWhitelistedURL(newDomain3, "/ok_1_but_bad", out _));
            


        }


    }
}
