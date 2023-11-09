using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers
{
    class Cell
    {
        public int Value { get; set; } = 0;
        public int StatusChecked { get; set; } = 0;
        public int TypeChecker { get; set; } = 0;
        public bool CheckedAttack { get; set; } = false;
        public bool Dead { get; set; } = false;

        public int X { get; set; }
        public int Y { get; set; }
        public Cell(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
