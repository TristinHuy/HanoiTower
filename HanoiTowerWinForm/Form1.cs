using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace HanoiTowerWinForm
{
    public partial class Form1 : Form
    {
        // Tower state: 'A', 'B', 'C' -> Stacks of disks
        private Dictionary<char, Stack<int>> towers;
        // Total number of disks
        private int totalDisks;
        // Default solving method: true for Recursive, false for Iterative
        private bool useRecursiveMethod = true;
        public class Timing
        {
            private Stopwatch stopwatch;

            public Timing()
            {
                stopwatch = new Stopwatch();
            }

            public void StartTime()
            {
                stopwatch.Restart();
            }

            public void StopTime()
            {
                stopwatch.Stop();
            }

            public TimeSpan Result()
            {
                return stopwatch.Elapsed;
            }
        }
        public Form1()
        {
            InitializeComponent();
        }

        #region UI Logic: Drawing Towers and Disks

        // Paint event: draw rods and disks
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(Color.DimGray);

            // Rod properties
            int rodWidth = 10;
            int rodHeight = 350;
            int baseY = pictureBox1.Height - 50;

            // Rod positions (match logical names A, B, C)
            var rodPositions = new Dictionary<char, int>
            {
                { 'A', 50 },  // Rod A position
                { 'B', 250 }, // Rod B position
                { 'C', 450 }  // Rod C position
            };

            // Draw rods
            foreach (var rod in rodPositions.Values)
            {
                g.FillRectangle(Brushes.WhiteSmoke, rod - rodWidth / 2, baseY - rodHeight, rodWidth, rodHeight);
            }

            // Draw disks
            if (towers != null)
            {
                int diskHeight = 20;
                int maxDiskWidth = 100;
                int minDiskWidth = 20;
                int widthStep = totalDisks > 1 ? (maxDiskWidth - minDiskWidth) / (totalDisks - 1) : 0;

                foreach (var rod in rodPositions)
                {
                    char rodKey = rod.Key;
                    int rodX = rod.Value;

                    if (towers[rodKey].Count == 0) continue;

                    // Reverse stack to draw bottom to top
                    int[] disks = towers[rodKey].ToArray();
                    Array.Reverse(disks);

                    int y = baseY - diskHeight;
                    foreach (int disk in disks)
                    {
                        int diskWidth = minDiskWidth + (disk - 1) * widthStep;
                        int x = rodX - diskWidth / 2;
                        g.FillRectangle(Brushes.Black, x, y, diskWidth, diskHeight);

                        // Draw disk number
                        string diskNumber = disk.ToString();
                        SizeF textSize = g.MeasureString(diskNumber, this.Font);
                        float textX = x + diskWidth / 2 - textSize.Width / 2;
                        float textY = y + diskHeight / 2 - textSize.Height / 2;
                        g.DrawString(diskNumber, this.Font, Brushes.White, textX, textY);

                        y -= diskHeight;
                    }
                }
            }
        }

        // Refresh PictureBox to redraw towers
        private void PrintTowers()
        {
            pictureBox1.Invoke(new Action(() =>
            {
                pictureBox1.Refresh();
            }));
            Thread.Sleep(10); // Delay for visualization
        }

        #endregion

        #region Tower of Hanoi Algorithms

        // Recursive Tower of Hanoi solution
        private void SolveHanoiRecursive(int n, char from, char to, char aux)
        {
            if (n == 1)
            {
                MoveDisk(from, to);
                return;
            }

            SolveHanoiRecursive(n - 1, from, aux, to);
            MoveDisk(from, to);
            SolveHanoiRecursive(n - 1, aux, to, from);
        }

        // Iterative Tower of Hanoi solution
        private void SolveHanoiIterative(int n, char from, char to, char aux)
        {
            var stack = new Stack<Tuple<int, char, char, char>>();
            stack.Push(Tuple.Create(n, from, to, aux));

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                int disk = current.Item1;
                char src = current.Item2;
                char dest = current.Item3;
                char helper = current.Item4;

                if (disk == 1)
                {
                    MoveDisk(src, dest);
                }
                else
                {
                    stack.Push(Tuple.Create(disk - 1, helper, dest, src));
                    stack.Push(Tuple.Create(1, src, dest, helper));
                    stack.Push(Tuple.Create(disk - 1, src, helper, dest));
                }
            }
        }

        // Move a disk from one rod to another
        private void MoveDisk(char from, char to)
        {
            // Pause if needed
            pauseEvent.Wait();

            // Cancel if requested
            if (cancellationTokenSource.Token.IsCancellationRequested)
                return;
            int disk = towers[from].Pop();
            towers[to].Push(disk);
            PrintTowers();
        }

        //Muốn đo thuần thuật toán thì phải viết lại nhưng vẫn giữ nguyên logic
        // Code thuật toán cũ quan hệ mật thiết với UI, code mới chạy không in UI
        private void SolveHanoiRecursiveWithoutUI(int n, char from, char to, char aux)
        {
            var localTowers = new Dictionary<char, Stack<int>>
        {
        { 'A', new Stack<int>() },
        { 'B', new Stack<int>() },
        { 'C', new Stack<int>() }
        };

            for (int i = n; i >= 1; i--)
                localTowers['A'].Push(i);

            void Move(char f, char t)
            {
                int d = localTowers[f].Pop();
                localTowers[t].Push(d);
            }

            void RecursiveSolve(int m, char f, char t, char a)
            {
                if (m == 1)
                {
                    Move(f, t);
                    return;
                }

                RecursiveSolve(m - 1, f, a, t);
                Move(f, t);
                RecursiveSolve(m - 1, a, t, f);
            }

            RecursiveSolve(n, from, to, aux);
        }
        private void SolveHanoiIterativeWithoutUI(int n, char from, char to, char aux)
        {
            var localTowers = new Dictionary<char, Stack<int>>
        {
        { 'A', new Stack<int>() },
        { 'B', new Stack<int>() },
        { 'C', new Stack<int>() }
        };

            for (int i = n; i >= 1; i--)
                localTowers['A'].Push(i);

            void Move(char f, char t)
            {
                int d = localTowers[f].Pop();
                localTowers[t].Push(d);
            }

            var stack = new Stack<Tuple<int, char, char, char>>();
            stack.Push(Tuple.Create(n, from, to, aux));

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                int disk = current.Item1;
                char src = current.Item2;
                char dest = current.Item3;
                char helper = current.Item4;

                if (disk == 1)
                {
                    Move(src, dest);
                }
                else
                {
                    stack.Push(Tuple.Create(disk - 1, helper, dest, src));
                    stack.Push(Tuple.Create(1, src, dest, helper));
                    stack.Push(Tuple.Create(disk - 1, src, helper, dest));
                }
            }
        }

        #endregion

        #region Event Handlers

        // NumericUpDown value changed
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            cancellationTokenSource?.Cancel(); // Cancel ongoing process
            pauseEvent.Set(); // Ensure it's not stuck in pause
            InitializeTowers((int)SoDia.Value);
        }

        // Initialize towers with disks on rod A
        private void InitializeTowers(int numberOfDisks)
        {
            totalDisks = numberOfDisks;

            towers = new Dictionary<char, Stack<int>>
            {
                { 'A', new Stack<int>() },
                { 'B', new Stack<int>() },
                { 'C', new Stack<int>() }
            };

            for (int i = numberOfDisks; i >= 1; i--)
            {
                towers['A'].Push(i);
            }

            pictureBox1.Refresh();
        }

        // Recursive method radio button checked
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
                useRecursiveMethod = true;
        }

        // Iterative method radio button checked
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
                useRecursiveMethod = false;
        }

        // Start button clicked
        private void button1_Click(object sender, EventArgs e)
        {
            int numberOfDisks = (int)SoDia.Value;

            if (numberOfDisks <= 0)
            {
                MessageBox.Show("Số đĩa không hợp lệ!");
                return;
            }

            // Proper initialization of cancellationTokenSource
            cancellationTokenSource = new CancellationTokenSource();

            Task.Run(() =>
            {
                try
                {
                    timing.StartTime(); // Start timing

                    if (useRecursiveMethod)
                        SolveHanoiRecursive(numberOfDisks, 'A', 'C', 'B');
                    else
                        SolveHanoiIterative(numberOfDisks, 'A', 'C', 'B');

                    timing.StopTime(); // Stop timing

                    // Update the UI with the execution time
                    label1.Invoke(new Action(() =>
                    {
                        label1.Text = $"Thời gian hoàn thành: {timing.Result().TotalMilliseconds} ms";
                    }));
                }
                catch (OperationCanceledException)
                {
                    // Handle the task being canceled
                }
            }, cancellationTokenSource.Token);

        }
        private void SolveHanoiWithoutUI(int n, char from, char to, char aux)
        {
            // Không dùng stack gốc của giao diện
            var localTowers = new Dictionary<char, Stack<int>>
    {
        { 'A', new Stack<int>() },
        { 'B', new Stack<int>() },
        { 'C', new Stack<int>() }
    };

            for (int i = n; i >= 1; i--)
                localTowers['A'].Push(i);

            void Move(char f, char t)
            {
                int d = localTowers[f].Pop();
                localTowers[t].Push(d);
            }

            void RecursiveSolve(int m, char f, char t, char a)
            {
                if (m == 1)
                {
                    Move(f, t);
                    return;
                }

                RecursiveSolve(m - 1, f, a, t);
                Move(f, t);
                RecursiveSolve(m - 1, a, t, f);
            }

            RecursiveSolve(n, from, to, aux);
        }

        #endregion

        private void button2_Click(object sender, EventArgs e)
        {

            pauseEvent.Reset(); // Pause the process
        }



        private void button3_Click(object sender, EventArgs e)
        {

            pauseEvent.Set(); // Resume the process
        }



        private Timing timing = new Timing(); // Khai báo đối tượng Timing toàn cục

        private void button1_Click_1(object sender, EventArgs e)
        {
            int numberOfDisks = (int)SoDia.Value;

            if (numberOfDisks <= 0)
            {
                MessageBox.Show("Please enter a valid number of disks!");
                return;
            }

            cancellationTokenSource = new CancellationTokenSource(); // Reset token hủy bỏ

            Task.Run(() =>
            {
                try
                {
                    timing.StartTime(); // Bắt đầu đo thời gian

                    if (useRecursiveMethod)
                        SolveHanoiRecursive(numberOfDisks, 'A', 'C', 'B');
                    else
                        SolveHanoiIterative(numberOfDisks, 'A', 'C', 'B');

                    timing.StopTime(); // Kết thúc đo thời gian

                    // Cập nhật UI (vì đang chạy trên thread khác, nên cần dùng Invoke)
                    label1.Invoke(new Action(() =>
                    {
                        label1.Text = $"Thời gian thực thi: {timing.Result().TotalMilliseconds} ms";
                    }));
                }
                catch (OperationCanceledException)
                {
                    // Xử lý khi thuật toán bị hủy
                }
            }, cancellationTokenSource.Token);
        }


        private CancellationTokenSource cancellationTokenSource;
        private ManualResetEventSlim pauseEvent = new ManualResetEventSlim(true);

        private void pictureBox3_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            int numberOfDisks = (int)SoDia.Value;

            if (numberOfDisks <= 0)
            {
                MessageBox.Show("Số đĩa không hợp lệ!");
                return;
            }

            Task.Run(() =>
            {
                var stopwatch = Stopwatch.StartNew();

                if (useRecursiveMethod)
                    SolveHanoiRecursiveWithoutUI(numberOfDisks, 'A', 'C', 'B');
                else
                    SolveHanoiIterativeWithoutUI(numberOfDisks, 'A', 'C', 'B');

                stopwatch.Stop();

                label1.Invoke(new Action(() =>
                {
                    label1.Text = $"Thời gian không UI: {stopwatch.ElapsedMilliseconds} ms";
                }));
            });
        }
    }
 
}