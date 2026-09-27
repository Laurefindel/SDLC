using System;
using System.Drawing;
using System.Windows.Forms;

namespace InternetLifeApp;

public class InternetLifeModel
{
    public event EventHandler? ModelChanged;

    private double _hoursPerDay;
    public double HoursPerDay => _hoursPerDay;

    public double YearsOnline { get; private set; }
    public long MemesSeen { get; private set; }
    public long AdsScrolled { get; private set; }

    private const double LifespanYears = 75; 
    private const int MemesPerHour = 45;     
    private const int AdsPerHour = 30;       

    public void UpdateData(double hoursPerDay)
    {
        if (hoursPerDay < 0 || hoursPerDay > 24)
            throw new ArgumentException("Часы должны быть в диапазоне от 0 до 24.");

        _hoursPerDay = hoursPerDay;

        double totalDaysInLife = LifespanYears * 365.25;
        double totalHoursOnline = totalDaysInLife * _hoursPerDay;

        YearsOnline = (_hoursPerDay / 24.0) * LifespanYears;
        MemesSeen = (long)(totalHoursOnline * MemesPerHour);
        AdsScrolled = (long)(totalHoursOnline * AdsPerHour);

        ModelChanged?.Invoke(this, EventArgs.Empty);
    }
}

public class InternetLifeController
{
    private readonly InternetLifeModel _model;

    public InternetLifeController(InternetLifeModel model)
    {
        _model = model;
    }

    public void HandleEnterDataRequest(IWin32Window owner, double currentHours)
    {
        using var inputForm = new InputForm(currentHours);
        
        if (inputForm.ShowDialog(owner) == DialogResult.OK)
        {
            if (double.TryParse(inputForm.InputText, out double hours) && hours >= 0 && hours <= 24)
            {
                _model.UpdateData(hours);
            }
            else
            {
                MessageBox.Show(
                    "Некорректные данные!\nПожалуйста, введите число часов от 0 до 24.", 
                    "Ошибка ввода", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
            }
        }
    }
}

public class MainForm : Form
{
    private readonly InternetLifeModel _model;
    private readonly InternetLifeController _controller;

    private Button _btnEnterData;
    private Label _lblYears;
    private Label _lblMemes;
    private Label _lblAds;

    public MainForm(InternetLifeModel model, InternetLifeController controller)
    {
        _model = model;
        _controller = controller;

        InitializeComponent();

        _model.ModelChanged += OnModelChanged;
    }

    private void InitializeComponent()
    {
        this.Text = "Сколько ты проживёшь в интернете";
        this.Size = new Size(420, 250);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Font = new Font("Segoe UI", 10F);

        _btnEnterData = new Button { Text = "Ввести данные", Location = new Point(120, 20), Size = new Size(160, 40) };
        _btnEnterData.Click += BtnEnterData_Click;

        _lblYears = new Label { Location = new Point(30, 80), AutoSize = true, Text = "Лет в интернете: ?" };
        _lblMemes = new Label { Location = new Point(30, 120), AutoSize = true, Text = "Просмотрено мемов: ?" };
        _lblAds = new Label { Location = new Point(30, 160), AutoSize = true, Text = "Пролистано рекламы: ?" };

        this.Controls.Add(_btnEnterData);
        this.Controls.Add(_lblYears);
        this.Controls.Add(_lblMemes);
        this.Controls.Add(_lblAds);
    }

    private void BtnEnterData_Click(object? sender, EventArgs e)
    {
        _controller.HandleEnterDataRequest(this, _model.HoursPerDay);
    }

    private void OnModelChanged(object? sender, EventArgs e)
    {
        _lblYears.Text = $"Лет в интернете: {_model.YearsOnline:F2} (из 75 лет жизни)";
        _lblMemes.Text = $"Просмотрено мемов: {_model.MemesSeen:N0}";
        _lblAds.Text = $"Пролистано рекламы: {_model.AdsScrolled:N0}";
    }
}

public class InputForm : Form
{
    private TextBox _txtHours;
    private Button _btnOk;

    public string InputText => _txtHours.Text;

    public InputForm(double currentHours)
    {
        this.Text = "Ввод информации";
        this.Size = new Size(360, 150); 
        this.Font = new Font("Segoe UI", 10F); 
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;

        var label = new Label { Text = "Часов онлайн в день:", Location = new Point(20, 25), AutoSize = true };

        _txtHours = new TextBox { Location = new Point(200, 22), Width = 110 };

        if (currentHours > 0)
        {
            _txtHours.Text = currentHours.ToString();
        }

        _btnOk = new Button { Text = "OK", Location = new Point(120, 65), Size = new Size(100, 30), DialogResult = DialogResult.OK };
        this.AcceptButton = _btnOk;

        this.Controls.Add(label);
        this.Controls.Add(_txtHours);
        this.Controls.Add(_btnOk);
    }
}

public static class Program
{
    [STAThread]
    public static void Main()
    {
        ApplicationConfiguration.Initialize();

        var model = new InternetLifeModel();
        var controller = new InternetLifeController(model);
        var mainForm = new MainForm(model, controller);

        Application.Run(mainForm);
    }
}