using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UniStack
{
    class Americanise
    {
        private const int minWordLen = 4;
        private const int minWordLenReg = 6;
        private const RegexOptions regOpts = RegexOptions.Compiled | RegexOptions.CultureInvariant;
        private static Regex[] patterns = new[]
        {
            new Regex("ise(d|s)?$", regOpts),
            new Regex("aemia$", regOpts),
            new Regex("haem(at)?o", regOpts),
            new Regex("([lL])eukaem", regOpts),
            new Regex("programme(s?)$", regOpts),
            new Regex("^([a-z]{3,})our(s?)$", regOpts)
        };
        private static string[] replacements = new[]
        {
            "ize$1", "hem$1o", "emia", "$1eukem", "program$1", "$1or$2"
        };
        // Using a collection of words (rather than a regex,
        // as used in the original implementation) is approximately
        // 10x faster to identify a word to be ignore.
        private static List<string[]> exceptions = new List<string[]>
        {
            null, null, null, null, null,
            new[]
            {
                "abatjour",
                "beflour",
                "bonjour",
                "calambour",
                "carrefour",
                "cornflour",
                "contour",
                "detour",
                "devour",
                "dortour",
                "dyvour",
                "downpour",
                "giaour",
                "glamour",
                "holour",
                "inpour",
                "outpour",
                "pandour",
                "paramour",
                "pompadour",
                "recontour",
                "repour",
                "ryeflour",
                "sompnour",
                "tambour",
                "troubadour",
                "tregetour",
                "velour"
             }
        };
        private static Dictionary<string, string> commonWords = new Dictionary<string, string>
        {
            ["anaesthetic"] = "anesthetic",
            ["analogue"] = "analog",
            ["analogues"] = "analogs",
            ["analyse"] = "analyze",
            ["analyses"] = "analyzes",
            ["analysed"] = "analyzed",
            ["analysing"] = "analyzing",
            ["armoured"] = "armored",
            ["cancelled"] = "canceled",
            ["cancelling"] = "canceling",
            ["candour"] = "candor",
            ["capitalisation"] = "capitalization",
            ["centre"] = "center",
            ["chimaeric"] = "chimeric",
            ["clamour"] = "clamor",
            ["coloured"] = "colored",
            ["colouring"] = "coloring",
            ["colourful"] = "colorful",
            ["defence"] = "defense",
            ["Defence"] = "Defense",
            ["dialogue"] = "dialog",
            ["dialogues"] = "dialogs",
            ["discolour"] = "discolor",
            ["discolours"] = "discolors",
            ["discoloured"] = "discolored",
            ["discolouring"] = "discoloring",
            ["encyclopaedia"] = "encyclopedia",
            ["endeavour"] = "endeavor",
            ["endeavours"] = "endeavors",
            ["endeavoured"] = "endeavored",
            ["endeavouring"] = "endeavoring",
            ["fervour"] = "fervor",
            ["favour"] = "favor",
            ["favours"] = "favors",
            ["favoured"] = "favored",
            ["favouring"] = "favoring",
            ["favourite"] = "favorite",
            ["favourites"] = "favorites",
            ["fibre"] = "fiber",
            ["fibres"] = "fibers",
            ["finalise"] = "finalize",
            ["finalised"] = "finalized",
            ["finalising"] = "finalizing",
            ["flavour"] = "flavor",
            ["flavours"] = "flavors",
            ["flavoured"] = "flavored",
            ["flavouring"] = "flavoring",
            ["glamour"] = "glamour",
            ["grey"] = "gray",
            ["harbour"] = "harbor",
            ["harbours"] = "harbors",
            ["homologue"] = "homolog",
            ["homologues"] = "homologs",
            ["honour"] = "honor",
            ["honours"] = "honors",
            ["honoured"] = "honored",
            ["honouring"] = "honoring",
            ["honourable"] = "honorable",
            ["humour"] = "humor",
            ["humours"] = "humors",
            ["humoured"] = "humored",
            ["humouring"] = "humoring",
            ["kerb"] = "curb",
            ["labelled"] = "labeled",
            ["labelling"] = "labeling",
            ["labour"] = "labor",
            ["Labour"] = "Labor",
            ["labours"] = "labors",
            ["laboured"] = "labored",
            ["labouring"] = "laboring",
            ["leant"] = "leaned",
            ["learnt"] = "learned",
            ["manoeuvre"] = "maneuver",
            ["manoeuvres"] = "maneuvers",
            ["maximising"] = "maximizing",
            ["meagre"] = "meager",
            ["minimising"] = "minimizing",
            ["modernising"] = "modernizing",
            ["misdemeanour"] = "misdemeanor",
            ["misdemeanours"] = "misdemeanors",
            ["neighbour"] = "neighbor",
            ["neighbours"] = "neighbors",
            ["neighbourhood"] = "neighborhood",
            ["neighbourhoods"] = "neighborhoods",
            ["oestrogen"] = "estrogen",
            ["oestrogens"] = "estrogens",
            ["organisation"] = "organization",
            ["organisations"] = "organizations",
            ["popularising"] = "popularizing",
            ["practise"] = "practice",
            ["practised"] = "practiced",
            ["pressurising"] = "pressurizing",
            ["realising"] = "realizing",
            ["recognising"] = "recognizing",
            ["rumoured"] = "rumored",
            ["rumouring"] = "rumoring",
            ["savour"] = "savor",
            ["savours"] = "savors",
            ["savoured"] = "savored",
            ["savouring"] = "savoring",
            ["splendour"] = "splendor",
            ["splendours"] = "splendors",
            ["tokenising"] = "tokenizing",
            ["theatre"] = "theater",
            ["theatres"] = "theaters",
            ["titre"] = "titer",
            ["titres"] = "titers",
            ["travelled"] = "traveled",
            ["travelling"] = "traveling"
        };

        public static string Apply(string word)
        {
            if (word.Length < minWordLen) return word;
            if (commonWords.ContainsKey(word)) return commonWords[word];
            if (word.Length < minWordLenReg) return word;

            for (int i = 0; i < patterns.Length; i++)
            {
                if (patterns[i].IsMatch(word))
                {
                    if (exceptions[i] != null && exceptions[i].Contains(word)) continue;

                    return patterns[i].Replace(word, replacements[i]);
                }
            }

            return word;
        }
    }
}
