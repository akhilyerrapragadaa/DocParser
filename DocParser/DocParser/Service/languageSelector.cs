using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace DocParser.Service
{
    public class languageSelector
    {
        private ConcurrentBag<string> shouldBag = new ConcurrentBag<string>();
        private ConcurrentBag<string> shallBag = new ConcurrentBag<string>();

        public ConcurrentBag<string> shouldSearch(string[] selectedLanguages)
        {
            Parallel.ForEach(selectedLanguages, new ParallelOptions { MaxDegreeOfParallelism = 4 }, eachPara =>
            {
                //All language should demands
                //English
                if (int.Parse(eachPara) == 1)
                    shouldBag.Add("should");
                //Swedish
                if (int.Parse(eachPara) == 2)
                    shouldBag.Add("skall");
                //Spanish
                if (int.Parse(eachPara) == 3)
                    shouldBag.Add("debería");
                //French
                if (int.Parse(eachPara) == 4)
                    shouldBag.Add("devrait");
            });
            return shouldBag;
        }

        public ConcurrentBag<string> shallSearch(string[] selectedLanguages)
        {
            Parallel.ForEach(selectedLanguages, new ParallelOptions { MaxDegreeOfParallelism = 4 }, eachPara =>
            {
                //All language shall demands
                //English
                if (int.Parse(eachPara) == 1)
                    shallBag.Add("shall");
                //Swedish
                if (int.Parse(eachPara) == 2)
                    shallBag.Add("skall");
                //Spanish
                if (int.Parse(eachPara) == 3)
                    shallBag.Add("debería");
                //French
                if (int.Parse(eachPara) == 4)
                    shallBag.Add("doit");
            });
            return shallBag;
        }

    }
}