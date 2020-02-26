using System;
using System.Collections.Generic;
using System.Linq;


namespace DocParser.Service
{
    public class IndexGenerator
    {
        private List<int> levels = new List<int> { 1 };

        public void DownOneLevel()
        {
            levels.Add(0);
        }

        public void UpLevels(int numLevels)
        {
            if (levels.Count < numLevels + 1)
                throw new InvalidOperationException(
                    "Attempt to ascend beyond the top level.");

            for (int i = 0; i < numLevels; i++)
                levels.RemoveAt(levels.Count - 1);
            MoveNext();
        }

        public void MoveNext()
        {
            levels[levels.Count - 1]++;
        }

        public void DecreNext()
        {
            levels[levels.Count - 1]--;
        }

        public string Current
        {
            get
            {
                return new string(' ', (levels.Count - 1) * 2)
                     + string.Join(".", levels.Select(l => l.ToString()));
            }
        }

    }
}