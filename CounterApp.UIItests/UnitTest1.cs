using System.IO;
using System.Threading;
using FlaUI.Core;
using FlaUI.UIA3;
using NUnit.Framework;
using FlaUI.Core.AutomationElements;

public class CounterTests
{
    [Apartment(ApartmentState.STA)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(5)]
    [TestCase(1000)]
    public void IncrementButton_IncrementsDisplayedCount(int clickCount)
    {
        // 테스트 폴더에서 연습 프로젝트의 앱 실행 파일 경로를 찾습니다.
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

        using var app = Application.Launch(appPath);
        using var automation = new UIA3Automation();

        var window = app.GetMainWindow(automation, TimeSpan.FromSeconds(10));
        Assert.That(window, Is.Not.Null, "앱 창을 찾지 못했습니다.");

        var button = window.FindFirstDescendant(
            element => element.ByAutomationId("IncrementButton"))?.AsButton();
        var countText = window.FindFirstDescendant(
            element => element.ByAutomationId("CountText"));

        Assert.That(button, Is.Not.Null, "숫자 올리기 버튼을 찾지 못했습니다.");
        Assert.That(countText, Is.Not.Null, "숫자 표시를 찾지 못했습니다.");
        var incrementButton = button!;
        var countLabel = countText!;
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