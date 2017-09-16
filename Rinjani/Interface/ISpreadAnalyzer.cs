using System.Collections.Generic;

namespace Rinjani
{
    public interface ISpreadAnalyzer
    {
        SpreadAnalysisResult Analyze(IList<Quote> quotes);
    }
}