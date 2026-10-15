using System.IO;
using System.Threading;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;

[TestFixture]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
public class CounterTests
{
    private Application? _app;
    private UIA3Automation? _automation;
    private Button? _incrementButton;
    private Button? _resetButton;
    private AutomationElement? _countLabel;

    [OneTimeSetUp]
    public void LaunchApp()
    {
        var projectRoot = Path.GetFullPath(Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "..", "..", "..", ".."));

        var appPath = Path.Combine(
            projectRoot,
            "CounterApp",
            "bin",
            "Debug",
            "net10.0-windows",
            "CounterApp.exe");

        Assert.That(File.Exists(appPath), Is.True,
            $"앱 실행 파일을 찾을 수 없습니다: {appPath}");

        try
        {
            _automation = new UIA3Automation();
            _app = Application.Launch(appPath);

            var window = _app.GetMainWindow(_automation, TimeSpan.FromSeconds(10));
            Assert.That(window, Is.Not.Null, "앱 창을 찾지 못했습니다.");

            _incrementButton = window!.FindFirstDescendant(
                element => element.ByAutomationId("IncrementButton"))?.AsButton();
            _resetButton = window.FindFirstDescendant(
                element => element.ByAutomationId("ResetButton"))?.AsButton();
            _countLabel = window.FindFirstDescendant(
                element => element.ByAutomationId("CountText"));

            Assert.That(_incrementButton, Is.Not.Null, "숫자 올리기 버튼을 찾지 못했습니다.");
            Assert.That(_resetButton, Is.Not.Null, "초기화 버튼을 찾지 못했습니다.");
            Assert.That(_countLabel, Is.Not.Null, "숫자 표시를 찾지 못했습니다.");
        }
        catch
        {
            _app?.Dispose();
            _automation?.Dispose();
            throw;
        }
    }

    [OneTimeTearDown]
    public void CloseApp()
    {
        _app?.Dispose();
        _automation?.Dispose();
    }

    [TearDown]
    public void ResetCounter()
    {
        if (_resetButton is null || _countLabel is null)
        {
            return;
        }

        _resetButton.Invoke();
        var reset = SpinWait.SpinUntil(
            () => _countLabel.Name == "현재 숫자: 0",
            TimeSpan.FromSeconds(5));

        Assert.That(reset, Is.True,
            $"테스트 후 카운터 초기화에 실패했습니다. 현재 표시: {_countLabel.Name}");
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(5)]
    [TestCase(1000)]
    public void IncrementButton_IncrementsDisplayedCount(int clickCount)
    {
        var incrementButton = _incrementButton!;
        var countLabel = _countLabel!;

        Assert.That(countLabel.Name, Is.EqualTo("현재 숫자: 0"));

        for (var count = 1; count <= clickCount; count++)
        {
            incrementButton.Invoke();

            var expectedText = $"현재 숫자: {count}";
            var updated = SpinWait.SpinUntil(
                () => countLabel.Name == expectedText,
                TimeSpan.FromSeconds(5));

            Assert.That(updated, Is.True,
                $"{count}회 클릭 후 기대한 문구 '{expectedText}'가 표시되지 않았습니다. " +
                $"현재 표시: {countLabel.Name}");
        }
    }
}
