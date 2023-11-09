using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Checkers
{
    class Player
    {
        public int Ind { get; set; }
        public Brush Brush { get; set; }
        public Pen Pen { get; set; }
        public int Score { get; set; } = 0;

        public Player(int Ind, Brush Brush, Pen Pen)
        {
            this.Ind = Ind;
            this.Brush = Brush;
            this.Pen = Pen;
        }
    }
}
