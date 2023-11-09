using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Checkers
{
    public partial class Game : Form
    {
        Menu menu;

        int S, W, H;
        int offsetX;
        int offsetY;

        Player curPlayer;

        List<Player> players = new List<Player>();
        List<List<Cell>> map = new List<List<Cell>>();

        List<Cell> attactCheckers = new List<Cell>();
        List<Cell> freeCells = new List<Cell>();
        List<Cell> deadCells = new List<Cell>();

        Point Cell = new Point(-1, -1);

        public Game(Menu myMenu)
        {
            InitializeComponent();

            menu = myMenu;
        }

        private void Game_Shown(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Maximized;
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.Sizable;

            H = 8;
            W = 8;

            S = ClientSize.Height / H;
            offsetX = (ClientSize.Width - S * W) / 2;
            offsetY = (ClientSize.Height % S) / 2;

            for (int i = 0; i < H; i++)
            {
                List<Cell> line = new List<Cell>();
                for (int j = 0; j < W; j++)
                {
                    line.Add(new Cell(j, i));
                }
                map.Add(line);
            }

            players.Add(new Player(1, Brushes.DarkOrange, Pens.DarkOrange));
            players.Add(new Player(2, Brushes.DarkGreen, Pens.DarkGreen));

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < W; j++)
                {
                    if ((j + i) % 2 != 0)
                    {
                        map[i][j].Value = 2;
                    }
                    else
                    {
                        map[H - i - 1][j].Value = 1;
                    }
                }
            }

            curPlayer = players[0];
        }
        private void Game_FormClosed(object sender, FormClosedEventArgs e)
        {
            menu.Show();
        }

        private void Game_Paint(object sender, PaintEventArgs e)
        {
            Pen pen = new Pen(Brushes.Black, 3);
            for (int i = 0; i < map.Count; i++)
            {
                for (int j = 0; j < map[i].Count; j++)
                {
                    if ((j + i) % 2 != 0)
                    {
                        e.Graphics.FillRectangle(Brushes.Gray, j * S + offsetX,
                                                               i * S + offsetY, S, S);
                    }
                    if (map[i][j].StatusChecked > 0)
                    {
                        e.Graphics.FillEllipse(Brushes.DarkRed, j * S + offsetX,
                                                                i * S + offsetY, S, S);
                    }

                    e.Graphics.DrawRectangle(pen, j * S + offsetX,
                                                  i * S + offsetY, S, S);

                    if (map[i][j].Value != 0)
                    {
                        e.Graphics.FillEllipse(players[map[i][j].Value - 1].Brush, j * S + offsetX,
                                                                            i * S + offsetY, S, S);
                        if (map[i][j].TypeChecker == 1)
                        {
                            e.Graphics.FillEllipse(Brushes.Black, j * S + offsetX + S / 4,
                                                   i * S + offsetY + S / 4, S / 2, S / 2);
                        }
                    }
                }
            }

            if (Cell.X >= 0 && Cell.Y >= 0)
            {
                pen = new Pen(Brushes.Blue, 5);
                e.Graphics.DrawEllipse(pen, Cell.X * S + offsetX,
                                            Cell.Y * S + offsetY, S, S);
            }


            if (curPlayer.Ind == 1)
            {
                e.Graphics.DrawString($"Ход Белых", new Font("Times New Roman", 36, FontStyle.Bold),
                      Brushes.Black, new PointF(W * S + S / 2 + offsetX, S / 2 + S * (H / 2 - 1) + offsetY));
            }
            else
            {
                e.Graphics.DrawString($"Ход Черных", new Font("Times New Roman", 36, FontStyle.Bold),
                   Brushes.Black, new PointF(W * S + S / 2 + offsetX, S / 2 + S * (H / 2 - 1) + offsetY));
            }
        }

        private void Game_MouseClick(object sender, MouseEventArgs e)
        {
            int x = (e.X - offsetX);
            int y = (e.Y - offsetY);
            if (x < 0 || x >= W * S || y < 0 || y >= H * S) { return; }

            x /= S;
            y /= S;

            if (e.Button == MouseButtons.Left)
            {
                if (map[y][x].Value == curPlayer.Ind)
                {
                    Cell.X = x;
                    Cell.Y = y;
                    UncheckedFreeCells();

                    if (map[y][x].TypeChecker == 0)
                    {
                        if (attactCheckers.Count <= 0)
                        {
                            AddCells();
                        }
                        else
                        {
                            AttackCells();
                        }
                    }
                    else
                    {
                        if (attactCheckers.Count <= 0)
                        {
                            AddAllCells();
                        }
                        else
                        {
                            AttackAllCells();
                        }
                    }
                    Invalidate();
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                if (map[y][x].StatusChecked == 0) { return; }
                if (Cell.X < 0 || Cell.X >= W || Cell.Y < 0 || Cell.Y >= H) { return; }

                if (map[y][x].StatusChecked == 1)
                {
                    map[Cell.Y][Cell.X].Value = 0;
                    map[y][x].Value = curPlayer.Ind;

                    map[y][x].TypeChecker = map[Cell.Y][Cell.X].TypeChecker;
                    map[Cell.Y][Cell.X].TypeChecker = 0;

                    Cell.X = x;
                    Cell.Y = y;
                    LevelUpChecker();

                    ChangeTurn();
                }
                else if (map[y][x].StatusChecked == 2)
                {
                    int koefX = 1;
                    int koefY = 1;
                    if (x - Cell.X < 0) { koefX = -1; }
                    if (y - Cell.Y < 0) { koefY = -1; }

                    Point attacted = FindAttactedCell(koefX, koefY);

                    map[attacted.Y][attacted.X].Dead = true;
                    deadCells.Add(map[attacted.Y][attacted.X]);

                    map[Cell.Y][Cell.X].Value = 0;
                    map[y][x].Value = curPlayer.Ind;

                    map[y][x].TypeChecker = map[Cell.Y][Cell.X].TypeChecker;
                    map[Cell.Y][Cell.X].TypeChecker = 0;

                    Cell.X = x;
                    Cell.Y = y;
                    LevelUpChecker();

                    UncheckedAttackCells();
                    UncheckedFreeCells();

                    if (map[Cell.Y][Cell.X].TypeChecker == 0 && AttackCells())
                    {
                        attactCheckers.Add(map[Cell.Y][Cell.X]);
                        map[Cell.Y][Cell.X].CheckedAttack = true;
                    }
                    else if (map[Cell.Y][Cell.X].TypeChecker == 1 && AttackAllCells())
                    {
                        attactCheckers.Add(map[Cell.Y][Cell.X]);
                        map[Cell.Y][Cell.X].CheckedAttack = true;
                    }
                    else
                    {
                        ChangeTurn();
                    }
                }

                Invalidate();
            }
        }

        private Point FindAttactedCell(int koefX, int koefY)
        {
            Point cell;
            int x = Cell.X;
            int y = Cell.Y;
            while (true)
            {
                x += koefX;
                y += koefY;
                if (map[y][x].Value != 0)
                {
                    cell = new Point(x, y);
                    break;
                }
            }
            return cell;
        }

        private bool AddCells()
        {
            bool res = false;
            if (curPlayer.Ind == 1)
            {
                if (AddFreeCell(-1, -1)) { res = true; }
                if (AddFreeCell(1, -1)) { res = true; }
            }
            else
            {
                if (AddFreeCell(-1, 1)) { res = true; }
                if (AddFreeCell(1, 1)) { res = true; }
            }
            return res;
        }
        private bool AttackCells()
        {
            bool res = false;
            if (AddAttackFreeCell(-1, 1)) { res = true; }
            if (AddAttackFreeCell(1, 1)) { res = true; }
            if (AddAttackFreeCell(-1, -1)) { res = true; }
            if (AddAttackFreeCell(1, -1)) { res = true; }

            return res;
        }
        private bool AddAttackFreeCell(int koefX, int koefY)
        {
            int x = Cell.X + koefX;
            int y = Cell.Y + koefY;
            if (x < 0 || x >= W || y < 0 || y >= H) { return false; }

            if (map[y][x].Value != curPlayer.Ind && map[y][x].Value != 0 &&
                !map[y][x].Dead)
            {
                x += koefX;
                y += koefY;
                if (x < 0 || x >= W || y < 0 || y >= H || map[y][x].Value != 0) { return false; }

                freeCells.Add(map[y][x]);
                map[y][x].StatusChecked = 2;
                return true;
            }

            return false;
        }
        private bool AddFreeCell(int koefX, int koefY)
        {
            int x = Cell.X + koefX;
            int y = Cell.Y + koefY;

            if (x >= 0 && x < W && y >= 0 && y < H)
            {
                if (map[y][x].Value == 0)
                {
                    freeCells.Add(map[y][x]);
                    map[y][x].StatusChecked = 1;
                    return true;
                }
            }
            return false;
        }

        private bool AddAllCells()
        {
            bool res = false;
            if (AddAllFreeCells(-1, 1)) { res = true; };
            if (AddAllFreeCells(1, 1)) { res = true; };
            if (AddAllFreeCells(-1, -1)) { res = true; };
            if (AddAllFreeCells(1, -1)) { res = true; };
            return res;
        }
        private bool AttackAllCells()
        {
            bool res = false;
            if (AddAllAttackCells(-1, 1)) { res = true; }
            if (AddAllAttackCells(1, 1)) { res = true; }
            if (AddAllAttackCells(-1, -1)) { res = true; }
            if (AddAllAttackCells(1, -1)) { res = true; }

            return res;
        }

        private bool AddAllFreeCells(int koefX, int koefY)
        {
            bool res = false;
            int x = Cell.X;
            int y = Cell.Y;

            while (true)
            {
                x += koefX;
                y += koefY;

                if (x < 0 || x >= W || y < 0 || y >= H) { break; }
                if (map[y][x].Value != 0) { break; }

                freeCells.Add(map[y][x]);
                map[y][x].StatusChecked = 1;
                res = true;
            }
            return res;
        }
        private bool AddAllAttackCells(int koefX, int koefY)
        {
            bool res = false;
            int x = Cell.X;
            int y = Cell.Y;
            List<Cell> cells = ForAllAttackCells(koefX, koefY, x, y);
            if (cells.Count > 0) { res = true; }

            int invX = 1;
            int invY = 1;
            if (koefX > 0) { invX = -1; }
            if (koefY > 0) { invY = -1; }

            bool checkNext = true;
            bool next;
            for (int i = 0; i < cells.Count; i++)
            {
                next = false;
                x = cells[i].X;
                y = cells[i].Y;
                if ((invX != -1 || invY != 1) && ForAllAttackCells(-1, 1, x, y).Count > 0) { next = true; }
                if ((invX != 1 || invY != 1) && ForAllAttackCells(1, 1, x, y).Count > 0)   { next = true; }
                if ((invX != -1 || invY != -1) && ForAllAttackCells(-1, -1, x, y).Count > 0) { next = true; }
                if ((invX != 1 || invY != -1) && ForAllAttackCells(1, -1, x, y).Count > 0)  { next = true; }

                if (next)
                {
                    cells[i].StatusChecked = 2;
                    freeCells.Add(cells[i]);
                    checkNext = false;
                }
            }
            if (checkNext)
            {
                for (int i = 0; i < cells.Count; i++)
                {
                    cells[i].StatusChecked = 2;
                    freeCells.Add(cells[i]);
                }
            }

            return res;
        }
        private List<Cell> ForAllAttackCells(int koefX, int koefY, int X, int Y)
        {
            int x = X;
            int y = Y;
            bool enemy = false;
            List<Cell> cells = new List<Cell>();
            while (true)
            {
                x += koefX;
                y += koefY;

                if (x < 0 || x >= W || y < 0 || y >= H) { break; }
                if (map[y][x].Value == curPlayer.Ind) { break; }
                if (map[y][x].Value == 0)
                {
                    if (enemy)
                    {
                        cells.Add(map[y][x]);
                    }
                    continue;
                }
                if (map[y][x].Dead || enemy) { break; }

                x += koefX;
                y += koefY;
                if (x < 0 || x >= W || y < 0 || y >= H || map[y][x].Value != 0) { break; }

                cells.Add(map[y][x]);
                enemy = true;
            }

            return cells;
        }

        private void UncheckedFreeCells()
        {
            foreach (Cell cell in freeCells)
            {
                cell.StatusChecked = 0;
            }
            freeCells.Clear();
        }
        private void UncheckedAttackCells()
        {
            foreach (Cell cell in attactCheckers)
            {
                cell.CheckedAttack = false;
            }
            attactCheckers.Clear();
        }
        private void UndeadCells()
        {
            curPlayer.Score += deadCells.Count();
            foreach (Cell dead in deadCells)
            {
                dead.Value = 0;
                dead.TypeChecker = 0;
                dead.Dead = false;
            }
            deadCells.Clear();
        }

        private bool CheckAttack()
        {
            bool res = false;
            for (int i = 0; i < H; i++)
            {
                for (int j = 0; j < W; j++)
                {
                    if (map[i][j].Value == curPlayer.Ind)
                    {
                        Cell.X = j; Cell.Y = i;
                        if (map[i][j].TypeChecker == 0)
                        {
                            if (AttackCells())
                            {
                                attactCheckers.Add(map[Cell.Y][Cell.X]);
                                map[Cell.Y][Cell.X].CheckedAttack = true;
                                res = true;
                            }
                        }
                        else
                        {
                            if (AttackAllCells())
                            {
                                attactCheckers.Add(map[Cell.Y][Cell.X]);
                                map[Cell.Y][Cell.X].CheckedAttack = true;
                                res = true;
                            }
                        }
                    }
                }
            }
            return res;
        }
        private bool CheckMove()
        {
            for (int i = 0; i < H; i++)
            {
                for (int j = 0; j < W; j++)
                {
                    if (map[i][j].Value == curPlayer.Ind)
                    {
                        Cell.X = j; Cell.Y = i;
                        if (map[i][j].TypeChecker == 0)
                        {
                            if (AddCells())
                            {
                                return true;
                            }
                        }
                        else
                        {
                            if (AddAllCells())
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private void LevelUpChecker()
        {
            if (curPlayer.Ind == 1)
            {
                if (Cell.Y == 0 && map[Cell.Y][Cell.X].TypeChecker == 0)
                {
                    map[Cell.Y][Cell.X].TypeChecker = 1;
                }
            }
            else
            {
                if (Cell.Y == H - 1 && map[Cell.Y][Cell.X].TypeChecker == 0)
                {
                    map[Cell.Y][Cell.X].TypeChecker = 1;
                }
            }
        }

        private bool CheckWin()
        {
            if (players[0].Score == 12)
            {
                MessageBox.Show("White Win");
                return true;
            }
            else if (players[1].Score == 12)
            {
                MessageBox.Show("Black Win");
                return true;
            }
            if (CheckMove()) { return false; }
            else if (CheckAttack()) { return false; }
            else
            {
                if (curPlayer.Ind == 2)
                {
                    MessageBox.Show("White Win");
                    return true;
                }
                else if (curPlayer.Ind == 1)
                {
                    MessageBox.Show("Black Win");
                    return true;
                }
            }

            return false;
        }
        private void ChangeTurn()
        {
            UncheckedFreeCells();
            UncheckedAttackCells();
            UndeadCells();

            if (curPlayer.Ind == players[0].Ind)
            {
                curPlayer = players[1];
            }
            else { curPlayer = players[0]; }

            if (CheckWin())
            {
                Cell.X = -1;
                Cell.Y = -1;
                Close();
                return;
            }
            UncheckedFreeCells();
            UncheckedAttackCells();

            CheckAttack();
            Cell.X = -1;
            Cell.Y = -1;
        }
    }
}
