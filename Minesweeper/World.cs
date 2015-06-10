using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Graphics;
using Fusion.Input;
using Fusion.Mathematics;

namespace Minesweeper
{
    public class World : GameService
    {
        public enum gameStatus {LOST = -1, CONTINUES = 0, WON = 1}

        Texture2D hex;
        Texture2D bomb;
        Texture2D flag;
        Texture2D question;
        Texture2D loser;
        int loserWidth = 275;
        int loserHeight = 230;
        Texture2D winner;
        int winnerWidth = 261;
        int winnerHeight = 229;

        SpriteFont font;

        const int rMax = 4;
        const int cMax = 4;
        const double bombProbability = 0.2;
        const float d = 50.0f;

        int bombCounter;

        static float stepX = 3 * d;
        static float stepY = (float)Math.Sqrt(3.0) * d;
        static float hexW = 2 * d;
        static float hexH = (float)Math.Sqrt(3.0) * d;

        public gameStatus status;

        public enum cellStatus {EMPTY = 0, FLAG = 1, QUESTION = 2, OPENED = 3}

        public class Cell
        {
            public int r;
            public int c;
            public bool hasBomb;
            public int bombNeighboursCount;
            public cellStatus mark;
            public Cell(int c_, int r_, bool hasBomb_) { c = c_; r = r_; hasBomb = hasBomb_; mark = cellStatus.EMPTY; bombNeighboursCount = 0; }
            public void cartCoord(out Vector2 coord) { coord.X = c * stepX / 2; coord.Y = r * stepY / 2; }
        }

        public List<Cell> Field;

        public World(Game game) : base(game) { }

        public void updateNeighbourCount(int c, int r) {
            Cell f = Field.Find(t => (t.c == c && t.r == r));
            if (f != null) f.bombNeighboursCount++;
        }

        public void InputDevice_KeyDown(object sender, InputDevice.KeyEventArgs e)
        {
            if (e.Key == Keys.F2)
            {
                NewGame();
            }
            if (status == gameStatus.CONTINUES)
            {
                Vector2 position = Game.InputDevice.MousePosition;
                if (e.Key == Keys.LeftButton)
                {
                    open(indexOfCell(position));
                }
                if (e.Key == Keys.RightButton)
                {
                    cellChangeState(position);
                }
            }
        }
        
        public void InputDevice_KeyUp(object sender, InputDevice.KeyEventArgs e) { }

        public override void Initialize()
        {
            base.Initialize();
            LoadContent();
            NewGame();
        }

        // protected override void LoadContent()
        protected void LoadContent()
        {
            hex = Game.Content.Load<Texture2D>("hex");
            bomb = Game.Content.Load<Texture2D>("bomb");
            flag = Game.Content.Load<Texture2D>("flag");
            question = Game.Content.Load<Texture2D>("question");
            font = Game.Content.Load<SpriteFont>("segoe40");
            winner = Game.Content.Load<Texture2D>("winner");
            loser = Game.Content.Load<Texture2D>("loser");
            // base.LoadContent();
        }

        public void NewGame() {
            bombCounter = 0;
            Field = new List<Cell>();
            var rand = new Random();
            for (var i = -cMax; i < cMax + 1; i++)
            {
                for (var j = -rMax; j < rMax + 1; j++)
                {
                    if ((i - j) % 2 != 0) continue;
                    var hasBomb = rand.NextDouble() <= bombProbability;
                    if (hasBomb) bombCounter++;
                    Field.Add(new Cell(i, j, hasBomb));
                }
            }
            foreach (Cell cell in Field)
            {
                if (!cell.hasBomb) continue;
                updateNeighbourCount(cell.c + 1, cell.r + 1);
                updateNeighbourCount(cell.c, cell.r + 2);
                updateNeighbourCount(cell.c - 1, cell.r + 1);
                updateNeighbourCount(cell.c - 1, cell.r - 1);
                updateNeighbourCount(cell.c, cell.r - 2);
                updateNeighbourCount(cell.c + 1, cell.r - 1);
            }
            status = gameStatus.CONTINUES;
        }

        public int indexOfCell(Vector2 coord)
        {
            coord -= (new Vector2(Game.GraphicsDevice.DisplayBounds.Width / 2, Game.GraphicsDevice.DisplayBounds.Height / 2));
            var uEven = (int)Math.Round(coord.X / stepX);
            var vEven = (int)Math.Round(coord.Y / stepY);
            var uOdd = (int)Math.Round(coord.X / stepX - 0.5f);
            var vOdd = (int)Math.Round(coord.Y / stepY - 0.5f);
            var coordEven = new Vector2(stepX * uEven, stepY * vEven);
			var coordOdd = new Vector2(stepX * uOdd + 0.5f * stepX, stepY * vOdd + 0.5f * stepY);
            int r, c;
            if ((coord - coordEven).Length() < (coord - coordOdd).Length())
            {
                // значит, ближайший центр - чётный.
                c = 2 * uEven;
                r = 2 * vEven;
            }
            else
            {
                // значит, ближайший центр - нечётный.
                c = 2 * uOdd + 1;
                r = 2 * vOdd + 1;
            }
            return Field.IndexOf(Field.Find(t => (t.c == c && t.r == r)));
        }

		public void cellChangeState(Vector2 coord)
        {
            var i = indexOfCell(coord);
            if (i != -1) {
                switch (Field[i].mark) {
                    case cellStatus.EMPTY:
                        Field[i].mark = cellStatus.FLAG;
                        break;
                    case cellStatus.FLAG:
                        Field[i].mark = cellStatus.QUESTION;
                        break;
                    case cellStatus.QUESTION:
                        Field[i].mark = cellStatus.EMPTY;
                        break;
                }
            }
        }

        public void open(int index) {
            if (index == -1) return;
            var cell = Field[index];
            if (cell.hasBomb) {
                status = gameStatus.LOST;
                return;
            }
            if (cell.mark == cellStatus.OPENED) return;
            cell.mark = cellStatus.OPENED;
            if (Field.IndexOf(Field.Find(t => (!t.hasBomb && t.mark != cellStatus.OPENED))) == -1) {
                status = gameStatus.WON;
                return;
            }
            if (cell.bombNeighboursCount == 0)
            {
                open(Field.IndexOf(Field.Find(t => (t.c == cell.c + 1 && t.r == cell.r + 1))));
                open(Field.IndexOf(Field.Find(t => (t.c == cell.c && t.r == cell.r + 2))));
                open(Field.IndexOf(Field.Find(t => (t.c == cell.c - 1 && t.r == cell.r + 1))));
                open(Field.IndexOf(Field.Find(t => (t.c == cell.c - 1 && t.r == cell.r - 1))));
                open(Field.IndexOf(Field.Find(t => (t.c == cell.c && t.r == cell.r - 2))));
                open(Field.IndexOf(Field.Find(t => (t.c == cell.c + 1 && t.r == cell.r - 1))));
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (status == gameStatus.CONTINUES)
            {
                var ds = Game.GetService<DebugStrings>();
                ds.Add(Color.Yellow,"Total number of bombs on the map: {0}",bombCounter);
            }
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime, StereoEye stereoEye)
        {
            int w = Game.GraphicsDevice.DisplayBounds.Width;
            int h = Game.GraphicsDevice.DisplayBounds.Height;

            var sb = Game.GetService<SpriteBatch>();
            var rand = new Random();

            // Game.GraphicsDevice.ClearBackbuffer(Color4.Black);
			Game.GraphicsDevice.ClearBackbuffer(Color4.White);

            sb.Begin();

            foreach(var cell in Field){
				var coord = new Vector2();
                cell.cartCoord(out coord);
                var xTopLeft = w / 2 + coord.X - hexW / 2;
                var yTopLeft = h / 2 + coord.Y - hexH / 2;
                sb.Draw(hex, xTopLeft, yTopLeft, hexW, hexH, (cell.mark == cellStatus.OPENED) ? new Color(132, 204, 129) : new Color(75, 70, 105));
                switch (cell.mark) {
                    case cellStatus.FLAG:
                        sb.Draw(flag, xTopLeft + 0.2f * hexW, yTopLeft + 0.2f * hexH, hexW * 0.6f, hexH * 0.6f, Color.White);
                        break;
                    case cellStatus.QUESTION:
                        sb.Draw(question, xTopLeft, yTopLeft, hexW, hexH, Color.White);
                        break;
                    case cellStatus.OPENED:
                        // if (cell.bombNeighboursCount > 0) font.DrawString(sb, cell.bombNeighboursCount.ToString(), xTopLeft + 0.5f * (hexW - font.MeasureString(cell.bombNeighboursCount.ToString()).Width), yTopLeft + 0.5f * (hexH + font.AverageCapitalLetterHeight), Color.Black);
                        if (cell.bombNeighboursCount > 0) font.DrawString(sb, cell.bombNeighboursCount.ToString(), xTopLeft + 0.5f * (hexW - font.MeasureString(cell.bombNeighboursCount.ToString()).Width), yTopLeft + 0.5f * (hexH + 10), Color.Black);
                        break;
                }
                if (status != gameStatus.CONTINUES && cell.hasBomb) {
                    sb.Draw(bomb, xTopLeft + 0.2f * hexW, yTopLeft + 0.2f * hexH, hexW * 0.6f, hexH * 0.6f, Color.White);
                }
            }

            if (status != gameStatus.CONTINUES) {
                if (status == gameStatus.LOST)
                {
                    sb.Draw(loser, (w - loserWidth) / 2, (h - loserHeight) / 2, loserWidth, loserHeight, Color.White);
                }
                else
                {
                    sb.Draw(winner, (w - winnerWidth) / 2, (h - winnerHeight) / 2, winnerWidth, winnerHeight, Color.White);
                }
            }

            sb.End();

            base.Draw(gameTime, stereoEye);
        }
    }
}
