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
    private Button? _decrementButton;
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
            _decrementButton = window.FindFirstDescendant(
                element => element.ByAutomationId("DecrementButton"))?.AsButton();
            _resetButton = window.FindFirstDescendant(
                element => element.ByAutomationId("ResetButton"))?.AsButton();
            _countLabel = window.FindFirstDescendant(
                element => element.ByAutomationId("CountText"));

            Assert.That(_incrementButton, Is.Not.Null, "숫자 올리기 버튼을 찾지 못했습니다.");
            Assert.That(_decrementButton, Is.Not.Null, "숫자 내리기 버튼을 찾지 못했습니다.");
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

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(100)]
    [TestCase(500)]
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

    [TestCase(0, 1, 0, TestName = "0에서 내리면 0 유지")]
    [TestCase(1, 1, 0, TestName = "최솟값 1에서 내리면 0")]
    [TestCase(2, 1, 1, TestName = "최솟값 바로 위에서 1 감소")]
    [TestCase(1, 2, 0, TestName = "0 아래로 내려가지 않음")]
    public void DecrementButton_RespectsZeroBoundary(
        int startingCount,
        int decrementCount,
        int expectedCount)
    {
        var incrementButton = _incrementButton!;
        var decrementButton = _decrementButton!;
        var countLabel = _countLabel!;

        for (var count = 0; count < startingCount; count++)
        {
            incrementButton.Invoke();
        }

        var initialized = SpinWait.SpinUntil(
            () => countLabel.Name == $"현재 숫자: {startingCount}",
            TimeSpan.FromSeconds(5));
        Assert.That(initialized, Is.True,
            $"테스트 시작값 {startingCount}을 만들지 못했습니다. 현재 표시: {countLabel.Name}");

        for (var click = 1; click <= decrementCount; click++)
        {
            decrementButton.Invoke();

            var expectedAtStep = Math.Max(0, startingCount - click);
            var expectedText = $"현재 숫자: {expectedAtStep}";
            var updated = SpinWait.SpinUntil(
                () => countLabel.Name == expectedText,
                TimeSpan.FromSeconds(5));

            Assert.That(updated, Is.True,
                $"{click}회 내린 뒤 기대한 문구 '{expectedText}'가 표시되지 않았습니다. " +
                $"현재 표시: {countLabel.Name}");
        }

        Assert.That(countLabel.Name, Is.EqualTo($"현재 숫자: {expectedCount}"));
    }
}
