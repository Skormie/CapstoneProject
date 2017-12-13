using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Media;
using System.IO;
using WMPLib;

namespace CapstoneProject
{

    // **************************************************
    //
    // Title: Capstone Project
    // Description: Randomly Generated Rouge-Like Dungeon Crawler.
    // Application Type: Win Forms
    // Author: Jason Luckhardt
    // Dated Created: 12/02/2017
    // Last Modified: 12/10/2017
    //
    // **************************************************

    public partial class Form1 : Form
    {
        Mob attacker;
        bool gameOver = false;
        Random rng = new Random();
        List<Button> attackButtons;
        Mob[] mobIndex = new Mob[4];
        byte[,] maze = new byte[3, 3];
        Attack[] equippedAttacks = new Attack[4];
        float timeCounter, atkCounter, winCounter = 255;
        List<Attack> playerAttacks = new List<Attack>();
        Player player = new Player("Grapes", 100, 20, 200, 1, 0, 0, 3);
        int[,] direction = new int[4, 2] { { 0, 1 }, { -1, 0 }, { 0, -1 }, { 1, 0 } }; // Right, Up, Left, Down
        WindowsMediaPlayer bgmPlayer = new WindowsMediaPlayer();

        public Form1()
        {
            InitializeComponent();
            chkBGM.AccessibleDescription = "theme.wav";
            chkBGM_CheckedChanged(null, EventArgs.Empty);
            bgmPlayer.settings.setMode("loop", true);
            CreateMonsters();
            CreateDrops();
            CreateAttacks();
            timerMain.Start();
            //Win32.AllocConsole(); // Debug Console.
            DisplayBattleHUD(false);
            SetPlayer();
            Generate();
            tableBackgroundPanel.BackgroundImage = DisplayUpdateScene();
            GetDrop(mobIndex[0].atk_drops[0].drop);
            lblLevel.Text = "Level "+player.level;
        }

        public void CreateMonsters()
        {
            mobIndex[0] = new Mob("Water Sprite", 100, 100, 5000, Properties.Resources.mobWater, 10, new SoundPlayer(Properties.Resources.screech3));
            mobIndex[1] = new Mob("Fire Sprite", 100, 100, 3000, Properties.Resources.mobFire, 20, new SoundPlayer(Properties.Resources.screech4_1));
            mobIndex[2] = new Mob("Light Sprite", 100, 100, 2000, Properties.Resources.mobLight, 30, new SoundPlayer(Properties.Resources.monster_screech));
            mobIndex[3] = new Mob("Earth Sprite", 100, 100, 1000, Properties.Resources.mobEarth, 40, new SoundPlayer(Properties.Resources.grunt));
        }

        public void CreateDrops()
        {
            //mobIndex[0].Drops(new Attack("Drop Kick", 100, 1, 100), new Attack("Round House Kick", 1, 1, 1000));
            mobIndex[0].Drops(new Drop("Name", new Attack("Roundhouse Kick", 0, 0, 10, new SoundPlayer(Properties.Resources.atk_flurry)), 10));
            //mobIndex[1].Drops(new Attack("Double Kick", 100, 1, 100), new Attack("...", 1, 1, 1000), new Attack("Doom Fist", 1000, 1, 1000));
        }

        public void CreateAttacks()
        {
            playerAttacks.Add(new Attack("Weak Kick", 0, 100, 10, new SoundPlayer(Properties.Resources.atk1)));
            playerAttacks.Add(new Attack("Weak Punch", 0, 50, 5, new SoundPlayer(Properties.Resources.atk2)));
            listBox1.Items.AddRange(new string[] { playerAttacks[0].name, playerAttacks[1].name });
            attackButtons = new List<Button>() { button1, button2, button3, button4 };
            listBox1.SetSelected(0, true);
            listBox1.SetSelected(1, true);
        }

        public void GetDrop( object drop )
        {
            if (drop.GetType() == typeof(Attack))
            {
                Attack dropped = (Attack)drop;
                playerAttacks.Add(dropped);
                listBox1.Items.Add(dropped.name);
            }
        }

        public void SetPlayer()
        {
            player.x = 0;
            player.y = 0;
            hpPlayer.Maximum = player.maxHP;
            hpPlayer.Value = player.hp;
            trkBarPlayer.Maximum = player.attackTime;
            trkBarPlayer.Value = player.attackTime;
        }

        public bool CanMove()
        {
            if (gameOver) return false;
            int testX = player.x + direction[player.direction, 1];
            int testY = player.y + direction[player.direction, 0];
            if (testX >= 0 && testX < maze.GetLength(1)
                && testY >= 0 && testY < maze.GetLength(0))
            {
                int Byte = maze[testY, testX];
                switch (player.direction)
                {
                    case 0: // Face East
                        if ((Byte & 4) == 4)
                            return false;
                        break;
                    case 1: // Face North
                        if ((Byte & 8) == 8)
                            return false;
                        break;
                    case 2: // Face West
                        if ((Byte & 1) == 1)
                            return false;
                        break;
                    case 3: // Face South
                        if ((Byte & 2) == 2)
                            return false;
                        break;
                    default:
                        break;
                }
            }
            else // Deadend
                return false;
            return true;
        }

        public Image DisplayUpdateScene()
        {
            bool canMove = CanMove();

            int testX = player.x + direction[player.direction, 1];
            int testY = player.y + direction[player.direction, 0];

            if (testX >= 0 && testX < maze.GetLength(1)
                && testY >= 0 && testY < maze.GetLength(0))
            {
                int Byte = maze[testY, testX];

                if ((Byte & 32) > 0 && canMove)
                    pictureBox1.Image = Properties.Resources.exit;
                else
                    pictureBox1.Image = null;

                if (!canMove)
                    return Properties.Resources.face_wall;

                switch (player.direction)
                {
                    case 0: // Face East
                        if ((Byte & 11) == 11)
                            return Properties.Resources.dead_end;
                        else if ((Byte & 3) == 3)
                            return Properties.Resources.right;
                        else if ((Byte & 10) == 10)
                            return Properties.Resources.straight;
                        else if ((Byte & 9) == 9)
                            return Properties.Resources.left;
                        else if ((Byte & 2) == 2)
                            return Properties.Resources.straight_right;
                        else if ((Byte & 1) == 1)
                            return Properties.Resources.left_right;
                        else
                            return Properties.Resources.left_straight;
                    case 1: // Face North
                        if ((Byte & 7) == 7)
                            return Properties.Resources.dead_end;
                        else if ((Byte & 6) == 6)
                            return Properties.Resources.right;
                        else if ((Byte & 5) == 5)
                            return Properties.Resources.straight;
                        else if ((Byte & 3) == 3)
                            return Properties.Resources.left;
                        else if ((Byte & 4) == 4)
                            return Properties.Resources.straight_right;
                        else if ((Byte & 2) == 2)
                            return Properties.Resources.left_right;
                        else
                            return Properties.Resources.left_straight;
                    case 2: // Face West
                        if ((Byte & 14) == 14)
                            return Properties.Resources.dead_end;
                        else if ((Byte & 12) == 12)
                            return Properties.Resources.right;
                        else if ((Byte & 10) == 10)
                            return Properties.Resources.straight;
                        else if ((Byte & 6) == 6)
                            return Properties.Resources.left;
                        else if ((Byte & 8) == 8)
                            return Properties.Resources.straight_right;
                        else if ((Byte & 4) == 4)
                            return Properties.Resources.left_right;
                        else
                            return Properties.Resources.left_straight;
                    case 3: // Face South
                        if ((Byte & 13) == 13)
                            return Properties.Resources.dead_end;
                        if ((Byte & 9) == 9)
                            return Properties.Resources.right;
                        if ((Byte & 5) == 5)
                            return Properties.Resources.straight;
                        if ((Byte & 12) == 12)
                            return Properties.Resources.left;
                        if((Byte & 1) == 1)
                            return Properties.Resources.straight_right;
                        if((Byte & 8) == 8)
                            return Properties.Resources.left_right;
                        if((Byte & 4) == 4)
                            return Properties.Resources.left_straight;
                        break;
                    default:
                        break;
                }
            }
            else // Deadend
                return Properties.Resources.face_wall; // Should make a flat dead end.
            return null;
        }

        // Main Timer Responsible for Attacks and Updating Player/Mob HP.
        private void timerMain_Tick(object sender, EventArgs e)
        {
            int rngAtk;
            int damage;

            if (attacker != null)
            {
                hpMob.Value = attacker.hp;
                timeCounter += 100;
                if(timeCounter % attacker.attackTime == 0)
                {
                    rngAtk = rng.Next(0,13);
                    if(rngAtk == 11)
                    {
                        damage = attacker.damage * 2 + rngAtk - 1;
                        lblBattleText.Text = "Critical Hit! You took "+ damage +"!!!";
                    }
                    else if(rngAtk == 12)
                    {
                        damage = 0;
                        lblBattleText.Text = attacker.name+" missed!!!";
                    }
                    else
                    {
                        damage = attacker.damage = rngAtk;
                        lblBattleText.Text = attacker.name + " hit you for " + damage + " damage.";
                    }

                    if (damage >= hpPlayer.Value)
                        hpPlayer.Value = 0;
                    else
                    {
                        hpPlayer.Value -= damage;
                        if (chkSFX.Checked)
                            attacker.atkSFX.Play();
                    }
                }

                trkBarMobAtk.Value = (int)(timeCounter % attacker.attackTime / attacker.attackTime * 100);
                if (hpMob.Value == 0)
                {
                    attacker = null;
                    imgMob.BackgroundImage = null;
                    DisplayBattleHUD(false);
                    chkBGM.AccessibleDescription = "theme.wav";
                    chkBGM_CheckedChanged(null, EventArgs.Empty);
                }
                if (hpPlayer.Value <= 0)
                {
                    attacker = null;
                    gameOver = true;
                    DisplayBattleHUD(false);
                    lblBattleText.Visible = true;
                    tableBackgroundPanel.BackgroundImage = null;
                    tableBackgroundPanel.BackColor = Color.Black;
                    chkBGM.AccessibleDescription = "gameover.wav";
                    chkBGM_CheckedChanged(null, EventArgs.Empty);
                    lblBattleText.Text = "Looks like this is the end!";
                }
            }  
            if (trkBarPlayer.Value < trkBarPlayer.Maximum)
                trkBarPlayer.Value+= 2;
        }

        // Button Click Forward Moves the player Forward if able and creates battles.
        private void btnForward_Click(object sender, EventArgs e)
        {
            if (attacker != null)
                return;
            if (rng.Next(10) == 0)
                InstantiateBattle();
            else if (CanMove())
            {
                if(chkSFX.Checked)
                    player.footSound.Play();
                player.x += direction[player.direction, 1];
                player.y += direction[player.direction, 0];
                maze[player.y, player.x] |= 16;
                if ((maze[player.y, player.x] & 32) > 0)
                { // Win Stage
                    gameOver = true;
                    timerWin.Start();
                    tableBackgroundPanel.BackgroundImage = null;
                    pictureBox1.Image = null;
                    chkBGM.AccessibleDescription = "fanfare.wav";
                    chkBGM_CheckedChanged(null, EventArgs.Empty);
                }
                else
                    tableBackgroundPanel.BackgroundImage = DisplayUpdateScene();
            } 
        }

        // Turns the battle HUD off and ON.
        public void DisplayBattleHUD(bool display)
        {
            lblMonster.Visible = display;
            hpMob.Visible = display;
            trkBarMobAtk.Visible = display;
            lblBattleText.Visible = display;
            hpPlayer.Visible = display;
            trkBarPlayer.Visible = display;
            lblBattleText.BackColor = display ? Color.FromArgb(128, 0, 0, 0) : Color.Transparent;
        }

        // Instantiates the battle scene and selects a random monster.
        private void InstantiateBattle()
        {
            attacker = mobIndex[rng.Next(mobIndex.Length)].Copy();
            imgMob.BackgroundImage = attacker.img;
            lblMonster.Text = attacker.name;
            DisplayBattleHUD(true);
            lblBattleText.Text = "You encountered a " + attacker.name + ".";
            chkBGM.AccessibleDescription = "battletheme.wav";
            chkBGM_CheckedChanged(chkBGM, EventArgs.Empty);
        }
        
        // Generates my random Maze.
        public void Generate()
        {
            for (int r = 0; r < maze.GetLength(0); r++)
                for (int c = 0; c < maze.GetLength(1); c++)
                    maze[r, c] = 15;

            int visitedCells = 0;
            int[] currentXY = new int[] { 0, 0 };
            Stack<int[]> cells = new Stack<int[]>();
            while (visitedCells < maze.Length)
            {
                List<int[]> adjacentCells = GetAdjacentCellWalls(maze, currentXY[0], currentXY[1]);
                if (adjacentCells.Count > 0)
                {
                    int[] randomCell = adjacentCells[rng.Next(adjacentCells.Count)];
                    int[] direction = new int[] { randomCell[0] - currentXY[0], randomCell[1] - currentXY[1] };
                    int[] direction2 = new int[] { currentXY[0] - randomCell[0], currentXY[1] - randomCell[1] };
                    maze[currentXY[0], currentXY[1]] ^= KnickWall(direction);
                    maze[randomCell[0], randomCell[1]] ^= KnickWall(direction2);
                    cells.Push(currentXY);
                    currentXY = randomCell;
                    visitedCells++;
                }
                else if (cells.Count > 0)
                    currentXY = cells.Pop();
                else
                    break;
            }
            maze[player.y, player.x] |= 16;
            maze[rng.Next(1, maze.GetLength(0)), rng.Next(1, maze.GetLength(1))] |= 32;
        }

        // Destory Walls
        public byte KnickWall(int[] Direction)
        {
            byte[,] walls = new byte[,] { {0, 2, 0},
                                          {4, 0, 1},
                                          {0, 8, 0} };
            return walls[1 + Direction[0], 1 + Direction[1]];
        }

        // List Adjacent Cells that have all 4 Walls
        public List<int[]> GetAdjacentCellWalls(byte[,] Maze, int CurrentX, int CurrentY)
        {
            List<int[]> adjacentCells = new List<int[]>();

            for (int i = 0; i < direction.GetLength(0); i++)
            {
                int testX = CurrentX + direction[i, 0];
                int testY = CurrentY + direction[i, 1];
                if (testX >= 0 && testX < maze.GetLength(0)
                    && testY >= 0 && testY < maze.GetLength(1))
                {
                    if (Maze[testX, testY] == 15)
                    {
                        adjacentCells.Add(new int[] { testX, testY });
                    }
                }
            }

            return adjacentCells;
        }

        // Select Attacks from the Attacks Tab.
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox attacklist = (ListBox)sender;
            for (int i = 0; i < attackButtons.Count; i++)
            {
                if (i < listBox1.SelectedItems.Count)
                {
                    attackButtons[i].Text = listBox1.SelectedItems[i].ToString();
                    equippedAttacks[i] = playerAttacks[listBox1.SelectedIndices[i]];
                }
                else
                    attackButtons[i].Text = "";
            }
        }

        // Method that controls attacks.
        private void AtkButton_Click(object sender, EventArgs e)
        {
            Button atkButton = (Button)sender;
            int atk = int.Parse(atkButton.AccessibleDescription);
            if (trkBarPlayer.Value >= trkBarPlayer.Maximum
                && atkButton.Text != ""
                && attacker != null) {
                timerAtk.Start();
                if (equippedAttacks[atk].damage >= attacker.hp)
                    attacker.hp = 0;
                else
                {
                    if(chkSFX.Checked)
                        equippedAttacks[atk].atkSound.Play();
                    attacker.hp -= equippedAttacks[atk].damage;
                }
                trkBarPlayer.Value -= equippedAttacks[atk].attackTime;
            }
        }

        // Method rotates the players view right.
        private void btnRight_Click(object sender, EventArgs e)
        {
            if (gameOver || attacker != null) return;
            switch (player.direction)
            {
                case 0: player.direction = 3; break;
                case 1: player.direction = 0; break;
                case 2: player.direction = 1; break;
                case 3: player.direction = 2; break;
                default:
                    break;
            }
            tableBackgroundPanel.BackgroundImage = DisplayUpdateScene();
        }

        // Method rotates the players view left.
        private void btnLeft_Click(object sender, EventArgs e)
        {
            if (gameOver || attacker != null) return;
            switch (player.direction)
            {
                case 0: player.direction = 1; break;
                case 1: player.direction = 2; break;
                case 2: player.direction = 3; break;
                case 3: player.direction = 0; break;
                default:
                    break;
            }
            tableBackgroundPanel.BackgroundImage = DisplayUpdateScene();
        }

        // Draw the mini-map and pointer along with shaded areas.
        private void imgMiniMap_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            if (gameOver||!chkMiniMap.Checked) return;
            Graphics g = e.Graphics;

            float rowLines = (imgMiniMap.Bottom * ((100 / (float)maze.GetLength(0)) / 100))-1;
            float colLines = (imgMiniMap.Right * ((100 / (float)maze.GetLength(1)) / 100))-1;
            float colLineOffset = colLines;
            float rowLineOffset = rowLines;
            float colPointOffset = colLines * .15f;
            float rowPointOffset = rowLines * .15f;

            for (int r = 0; r < maze.GetLength(0); r++)
            {
                for (int c = 0; c < maze.GetLength(1); c++)
                {
                    if (r == player.x && c == player.y)
                    {
                        float cCenter = r * colLines + (colLineOffset / 2);
                        float rCenter = c * rowLines + (rowLineOffset / 2);

                        PointF[] pointRight = { new PointF(cCenter + colPointOffset, rCenter), new PointF(cCenter - colPointOffset, rCenter + rowPointOffset), new PointF(cCenter - colPointOffset, rCenter - rowPointOffset) }; // Point Right
                        PointF[] pointUp = { new PointF(cCenter, rCenter - rowPointOffset), new PointF(cCenter + colPointOffset, rCenter + rowPointOffset), new PointF(cCenter - colPointOffset, rCenter + rowPointOffset) }; // Point Up
                        PointF[] pointLeft = { new PointF(cCenter - colPointOffset, rCenter), new PointF(cCenter + colPointOffset, rCenter + rowPointOffset), new PointF(cCenter + colPointOffset, rCenter - rowPointOffset) }; // Point Left
                        PointF[] pointDown = { new PointF(cCenter, rCenter + rowPointOffset), new PointF(cCenter + colPointOffset, rCenter - rowPointOffset), new PointF(cCenter - colPointOffset, rCenter - rowPointOffset) }; // Point Down
                        List<PointF[]> points = new List<PointF[]>() { pointRight, pointUp, pointLeft, pointDown };
                        g.FillPolygon( new SolidBrush(Color.FromArgb(128, 0, 0, 0)), points[player.direction]);
                    }

                    if ((maze[c,r] & 16) == 16)
                    {
                        if ((maze[c, r] & 1) == 1)
                            g.DrawLine(System.Drawing.Pens.Red, r * colLines + colLineOffset, c * rowLineOffset, r * colLines + colLineOffset, c * rowLineOffset + rowLineOffset); // Right Wall
                        if ((maze[c, r] & 2) == 2)
                            g.DrawLine(System.Drawing.Pens.Red, r * colLines, c * rowLineOffset, r * colLines + colLineOffset, c * rowLineOffset); // Top Wall
                        if ((maze[c, r] & 4) == 4)
                            g.DrawLine(System.Drawing.Pens.Red, r * colLines, c * rowLineOffset, r * colLines, c * rowLineOffset + rowLineOffset); // Left Wall
                        if ((maze[c, r] & 8) == 8)
                            g.DrawLine(System.Drawing.Pens.Red, r * colLines, c * rowLineOffset + rowLineOffset, r * colLines + colLineOffset, c * rowLineOffset + rowLineOffset); // Bottom Wall
                    }
                    else
                        g.FillRectangle( new SolidBrush(Color.FromArgb(128, 0, 0, 255)), r * colLines + 1, c * rowLines + 1, colLines -1, rowLines -1);
                }
            }
        }

        // Gets W, A, S keys when pressed.
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyData)
            {
                case Keys.Right:
                case Keys.D:
                    btnRight_Click(sender, e);
                    break;
                case Keys.Left:
                case Keys.A:
                    btnLeft_Click(sender, e);
                    break;
                case Keys.Up:
                case Keys.W:
                    btnForward_Click(sender, e);
                    break;
                default:
                    break;
            }
        }

        private void chkBGM_CheckedChanged(object sender, EventArgs e)
        {
            if (chkBGM.Checked)
            {
                bgmPlayer.URL = Directory.GetCurrentDirectory() + @"\sound\" + chkBGM.AccessibleDescription;
                bgmPlayer.controls.play();
            }
            else
                bgmPlayer.controls.stop();
        }

        private void lblJSLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(lblJSLink.Text);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(linkLabel1.Text);
        }

        // Timer to play the win animation.
        private void timerWin_Tick(object sender, EventArgs e)
        {
            if(winCounter > 0)
            {
                tableBackgroundPanel.BackColor = Color.FromArgb((int)winCounter,0,0,0);
                winCounter-=10;
            }
            else
            {
                tableBackgroundPanel.BackColor = Color.Transparent;
                timerWin.Stop();
                winCounter = 255;
                gameOver = false;
                player.level++;
                maze = new byte[3 * player.level, 3 * player.level];
                SetPlayer();
                Generate();
                tableBackgroundPanel.BackgroundImage = DisplayUpdateScene();
                lblLevel.Text = "Level " + player.level;
                chkBGM.AccessibleDescription = "theme.wav";
                chkBGM_CheckedChanged(null, EventArgs.Empty);
            }
        }

        // Timer to play the attack animation.
        private void timerAtk_Tick(object sender, EventArgs e)
        {
            List<Bitmap> hitEffect = new List<Bitmap> { Properties.Resources.hit1, Properties.Resources.hit2, Properties.Resources.hit3, Properties.Resources.hit4, Properties.Resources.hit5, Properties.Resources.hit6, Properties.Resources.hit7, Properties.Resources.hit8, Properties.Resources.hit9, Properties.Resources.hit10, Properties.Resources.hit11, Properties.Resources.hit12, Properties.Resources.hit13, Properties.Resources.hit14, Properties.Resources.hit15, Properties.Resources.hit16 };
            if (atkCounter < 16)
            {
                imgMob.Image = hitEffect[(int)atkCounter % 16];
                atkCounter++;
            }
            else
            {
                imgMob.Image = null;
                timerAtk.Stop();
                atkCounter = 0;
            }
        }
    }

    // Mob Class
    class Mob
    {
        public string name { get; set; }
        public int hp { get; set; }
        public int maxHP { get; set; }
        public int mp { get; set; }
        public int maxMP { get; set; }
        public int attackTime { get; set; }
        public Image img { get; set; }
        public int damage { get; set; }
        public SoundPlayer atkSFX { get; set; }

        public List<Drop> atk_drops = new List<Drop>();

        public List<Drop> MyProperty
        {
            get { return atk_drops = new List<Drop>(); }
            set { atk_drops = value; }
        }


        public Mob Copy()
        {
            Mob attacker = (Mob)MemberwiseClone();
            attacker.hp = maxHP;
            attacker.mp = maxMP;
            attacker.name = name;
            attacker.attackTime = attackTime;
            attacker.img = img;
            attacker.damage = damage;
            attacker.atkSFX = atkSFX;
            return attacker;
        }

        public Mob( string Name, int MaxHP, int MaxMP, int AttackTime, Image Img, int Damage, SoundPlayer AtkSFX )
        {
            hp = MaxHP;
            mp = MaxMP;
            maxHP = MaxHP;
            maxMP = MaxMP;
            name = Name;
            attackTime = AttackTime;
            img = Img;
            damage = Damage;
            atkSFX = AtkSFX;
        }

        public void Drops(params Drop[] drops)
        {
            for (int i = 0; i < drops.Length; i++)
                atk_drops.Add(drops[i]);
        }
    }

    // Player Class
    class Player
    {
        public SoundPlayer footSound = new SoundPlayer(Properties.Resources.footstep);

        public string name { get; set; }
        public int hp { get; set; }
        public int maxHP { get; set; }
        public int mp { get; set; }
        public int maxMP { get; set; }
        public int attackTime { get; set; }
        public Image img { get; set; }
        public int level { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public int direction { get; set; }
        //public SoundPlayer footSound { get; set; }

        public Player(string Name, int MaxHP, int MaxMP, int AttackTime, int Level, int X, int Y, int Direction)
        {
            hp = MaxHP;
            mp = MaxMP;
            maxHP = MaxHP;
            maxMP = MaxMP;
            name = Name;
            attackTime = AttackTime;
            level = Level;
            x = X;
            y = Y;
            direction = Direction;
        }
    }

    // Attacks Class
    class Attack
    {
        public string name { get; set; }
        public int mp { get; set; }
        public int attackTime { get; set; }
        public int damage { get; set; }
        public SoundPlayer atkSound { get; set; }

        public Attack(string Name, int MP, int AttackTime, int Damage, SoundPlayer AtkSound)
        {
            mp = MP;
            name = Name;
            attackTime = AttackTime;
            damage = Damage;
            atkSound = AtkSound;
        }
    }

    // Drops Class
    public class Drop
    {
        public string name { get; set; }
        public object drop { get; set; }
        public int probability { get; set; }

        public Drop(string Name, object Drop, int Probability)
        {
            name = Name;
            drop = Drop;
            probability = Probability;
        }
    }

    // Debug Console
    public class Win32
    {
        [DllImport("kernel32.dll")]
        public static extern Boolean AllocConsole();
        [DllImport("kernel32.dll")]
        public static extern Boolean FreeConsole();
    }
}
